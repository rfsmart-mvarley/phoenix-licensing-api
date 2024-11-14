using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System;

namespace Rfsmart.Phoenix.Licensing.Services
{
    public class FeatureTrackingService(IFeatureTrackingRepository featureTrackingRepository,
    IFeatureIssueRepository featureIssueRepository) : IFeatureTrackingService
    {
        public async Task<bool> AssignFeaturesToUser(AssignFeatureRequest request)
        {
            var distinctFeatures = request.Features.Distinct();

            foreach (var item in distinctFeatures)
            {
                var issue = await featureIssueRepository.Get(item);

                if (issue is null)
                {
                    throw new ArgumentException($"Feature {item} is not licensed!");
                }
            }

            foreach (var item in distinctFeatures)
            {
                var existing = await featureTrackingRepository.Get(item);

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

            return true;
        }
    }

}
