using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System;

namespace Rfsmart.Phoenix.Licensing.Services
{
    public class FeatureTrackingService(IFeatureDefinitionRepository featureDefinitionRepository,
        IFeatureTrackingRepository featureTrackingRepository,
    IFeatureIssueRepository featureIssueRepository) : IFeatureTrackingService
    {
        private async Task ConfirmFeautreExistsAndIsLicensed(string featureName)
        {
            var featureDefinition = await featureDefinitionRepository.Get(featureName);

            if (featureDefinition is null)
            {
                throw new ArgumentException($"Feature definition does not exist for {featureName}");
            }

            var issue = await featureIssueRepository.Get(featureName);

            if (issue is null)
            {
                throw new ArgumentException($"Feature {featureName} is not licensed!");
            }
        }

        public async Task<AssignFeatureRequest> AssignFeaturesToUser(AssignFeatureRequest request)
        {
            var distinctFeatures = request.Features.Distinct();

            foreach (var feature in distinctFeatures)
            {
                await ConfirmFeautreExistsAndIsLicensed(feature);
            }

            foreach (var item in distinctFeatures)
            {
                var existing = await featureTrackingRepository.Get(new FeatureTrackingByFeatureRequest
                {
                    FeatureName = item
                });

                if (existing is null)
                {
                    existing = new FeatureTrackingRecord
                    {
                        FeatureName = item,
                        Users = [request.User]
                    };
                }
                else if (existing.Users.Contains(request.User))
                {
                    continue;
                }
                else
                {
                    existing.Users = [.. existing.Users, request.User];
                }

                await featureTrackingRepository.Insert(existing);
            }

            return request;
        }

        public async Task<FeatureTrackingRecord> Get(FeatureTrackingByFeatureRequest request)
        {
            await ConfirmFeautreExistsAndIsLicensed(request.FeatureName);

            var resp = await featureTrackingRepository.Get(request);

            if (resp is null)
            {
                return new FeatureTrackingRecord
                {
                    FeatureName = request.FeatureName,
                    Users = [],
                };
            }

            return resp;
        }

        public async Task<FeatureTrackingByUserResponse> Get(FeatureTrackingByUserRequest request)
        {
            var resp = await featureTrackingRepository.GetFeatureTrackings();

            var usersFeatures = resp.Aggregate(new List<string>(), (agg, x) => 
            {
                if (x.Users.Contains(request.User))
                {
                    agg.Add(x.FeatureName);
                }

                return agg;
            });

            return new FeatureTrackingByUserResponse
            {
                User = request.User,
                Features = usersFeatures,
            };
        }

        public async Task<IEnumerable<FeatureTrackingRecord>> Get()
        {
            return await featureTrackingRepository.GetFeatureTrackings();
        }
    }
}
