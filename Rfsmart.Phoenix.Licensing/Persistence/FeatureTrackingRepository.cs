using Amazon.Runtime.Internal;
using Dapper;
using Microsoft.Extensions.Logging;
using Rfsmart.Phoenix.Common.Context;
using Rfsmart.Phoenix.Common.Models;
using Rfsmart.Phoenix.Database.Services;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Persistence
{
    [ExcludeFromCodeCoverage]
    public class FeatureTrackingRepository(
        ILogger<FeatureTrackingRepository> _logger,
        IDbConnectionProvider _connectionProvider,
        IContextProvider<UserContext> _userContextProvider,
        IContextProvider<TenantContext> _tenantContextProvider
    ) : TenantRepositoryBase(_connectionProvider, _userContextProvider, _logger), IFeatureTrackingRepository
    {
        private const string SelectColumnList =
            "feature_name,users,created";

        private static readonly Dictionary<FeatureRecordSort, string> _sortableFields =
            new()
            {
                [FeatureRecordSort.Created] = "created",
                [FeatureRecordSort.FeatureName] = "feature_name",
            };

        public async Task<FeatureTrackingRecord> Insert(FeatureTrackingRecord request)
        {
            _logger.LogDebug("Insert {@Request}", request);

            var featureRecord = await Exec(
                _tenantContextProvider.Context!,
                db =>
                    db.QuerySingleAsync<FeatureTrackingRecord>(
                        $"""
                        insert into feature_tracking
                        (
                            feature_name,
                            users,
                            created
                        ) 
                        values 
                        (
                            @FeatureName,
                            @Users,
                            CURRENT_TIMESTAMP
                        )
                        RETURNING {SelectColumnList};
                        """,
                        new
                        {
                            request.FeatureName,
                            request.Users,
                        }
                    )
            );

            return featureRecord
                ?? throw new DataMisalignedException(
                    "Feature record could not be retrieved after creation"
                );
        }

        public Task<PagedResult<FeatureTrackingRecord>> List(BaseListRequest<FeatureRecordSort> request)
        {
            _logger.LogInformation("ListFeatureRecords");

            var querySql = $"""
                select {SelectColumnList}
                from feature_tracking
                where
                    (
                        @Filter = ''
                        OR search_index @@ to_tsquery('english', @Filter)
                    )
                """;

            return Exec(
                _tenantContextProvider.Context!,
                db =>
                    new SearchHelper<FeatureTrackingRecord, FeatureRecordSort>(
                        querySql,
                        new { request.Filter },
                        _sortableFields,
                        new SearchHelperRequest<FeatureRecordSort>
                        {
                            PageNumber = request.PageNumber,
                            PageSize = request.PageSize,
                            Direction = request.Direction,
                            OrderBy = request.OrderBy,
                        }
                    ).GetPagedResult(db)
            );
        }

        public async Task<int> Delete(DeleteFeatureRecordsRequest request)
        {
            _logger.LogInformation("Delete Feature Tracking Records {@Request}", request);

            var deletedRows = await Exec(
                    _tenantContextProvider.Context!,
                    db =>
                        db.ExecuteAsync(
                            $"""
                            DELETE FROM feature_tracking 
                            WHERE created >= @From AND created <= @To
                            """,
                            new
                            {
                                request.From,
                                request.To,
                            }
                        )
                );

            _logger.LogInformation("Deleted {@deletedRows} feature tracking records", deletedRows);

            return deletedRows;
        }

        public async Task<FeatureTrackingRecord?> Get(FeatureTrackingByFeatureRequest request)
        {
            _logger.LogDebug("Get {@Request}", request);

            var featureRecord = await Exec(
                _tenantContextProvider.Context!,
                db =>
                    db.QueryFirstOrDefaultAsync<FeatureTrackingRecord>(
                        $"""
                        SELECT distinct on (feature_name) * FROM feature_tracking
                        WHERE feature_name = @FeatureName
                        ORDER BY feature_name,created DESC 
                        """,
                        new
                        {
                            request.FeatureName,
                        }
                    )
            );

            return featureRecord;
        }

        public async Task<IEnumerable<FeatureTrackingRecord>> GetFeatureTrackings()
        {
            _logger.LogDebug("Get tracking state");

            var featureRecord = await Exec(
                _tenantContextProvider.Context!,
                db =>
                    db.QueryAsync<FeatureTrackingRecord>(
                        $"""
                        SELECT distinct on (feature_name) * FROM feature_tracking
                        ORDER BY feature_name,created DESC 
                        """
                    )
            );

            return featureRecord;
        }
    }
}
