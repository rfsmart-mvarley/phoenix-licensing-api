using Amazon.Runtime.Internal;
using Dapper;
using Microsoft.Extensions.Logging;
using Rfsmart.Phoenix.Common.Context;
using Rfsmart.Phoenix.Database.Services;
using Rfsmart.Phoenix.Database.Utility;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Persistence
{
    public class FeatureDefinitionRepository(
        IDbConnectionProvider connectionProvider,
        IContextProvider<UserContext> userContextProvider,
        ILogger<FeatureDefinitionRepository> logger
    ) : GlobalRepositoryBase(connectionProvider, userContextProvider, logger), IFeatureDefinitionRepository
    {
        protected override string DefaultSchema => "licensing";

        private const string SelectColumnList =
            "feature_name,price_per_user,price_on_demand,enforced_from,enforced_until,created,created_by,last_modified,last_modified_by,is_active";

        public async Task<FeatureDefinition> Create(FeatureDefinition request)
        {
            logger.LogInformation("CreateFeature requested for {@Request}", request);

            var sql = $"""
            insert into pricing
            (
                feature_name, 
                price_per_user, 
                price_on_demand, 
                enforced_from, 
                enforced_until, 
                created,
                created_by,
                last_modified,
                last_modified_by,
                is_active
            ) 
            values 
            (
                @FeatureName,
                @PricePerUser,
                @PriceOnDemand,
                @EnforcedFrom,
                @EnforcedUntil,
                CURRENT_TIMESTAMP,
                @Caller,
                CURRENT_TIMESTAMP,
                @Caller,
                @IsActive
            )
            returning {SelectColumnList}
            """;

            var @params = new
            {
                request.FeatureName,
                request.PricePerUser,
                request.PriceOnDemand,
                request.EnforcedFrom,
                request.EnforcedUntil,
                Caller,
                IsActive = true,
            };

            try
            {
                var integration = await Exec(db => db.QuerySingleAsync<FeatureDefinition>(sql, @params));

                return integration
                    ?? throw new DataMisalignedException(
                        "FeatureDefinition could not be retrieved after creation"
                    );
            }
            catch (Exception ex)
            {
                var e = ex.Message;
                throw;
            }
            
        }

        public async Task<FeatureDefinition?> Get(string featureName)
        {
            logger.LogDebug("Get {@Request}", featureName);

            var featureRecord = await Exec(
                db =>
                    db.QuerySingleAsync<FeatureDefinition>(
                        $"""
                        select * from pricing
                        where feature_name = @featureName
                        ORDER BY created DESC LIMIT 1;
                        """,
                        new
                        {
                            featureName,
                        }
                    )
            );

            return featureRecord;
        }
    }
}
