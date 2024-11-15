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

        public async Task<FeatureTrackingByUserResponse> AssignFeaturesToUser(FeaturesRequest request)
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

            return await Get(new FeatureTrackingByUserRequest { User = request.User });
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
            var resp = await featureTrackingRepository.GetCurrentConsumption();

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

        public async Task<IEnumerable<FeatureTrackingRecord>> GetConsumption()
        {
            return await featureTrackingRepository.GetCurrentConsumption();
        }

        public async Task<IEnumerable<FeatureTrackingRecord>> GetAll()
        {
            return await featureTrackingRepository.GetAll();
        }

        public async Task<FeatureTrackingByUserResponse> UnassignFeaturesFromUser(FeaturesRequest request)
        {
            var distinctFeatures = request.Features.Distinct();

            foreach (var item in distinctFeatures)
            {
                var existing = await featureTrackingRepository.Get(new FeatureTrackingByFeatureRequest
                {
                    FeatureName = item
                });

                if (existing is null || !existing.Users.Contains(request.User, StringComparer.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                else
                {
                    existing.Users = existing.Users.Where(x => !x.Equals(request.User, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                }

                await featureTrackingRepository.Insert(existing);
            }

            return await Get(new FeatureTrackingByUserRequest { User = request.User });
        }
    }
}
