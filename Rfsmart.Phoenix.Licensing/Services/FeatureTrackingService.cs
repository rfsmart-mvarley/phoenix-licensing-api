using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System;

internal class FeatureTrackingService(IFeatureTrackingRepository featureTrackingRepository,
    IFeatureIssueRepository featureIssueRepository) : IFeatureTrackingService
{
    public async Task<bool> AssignFeaturesToUser(AssignFeatureRequest request)
    {
        foreach (var item in request.Features.Distinct())
        {
            var issue = await featureIssueRepository.Get(item);

            if (issue is null)
            {
                throw new ArgumentException($"Feature {item} is not licensed!");
            }
        }

        foreach (var item in request.Features.Distinct())
        {
            var existing = await featureTrackingRepository.Get(item);

            if (existing is null)
            {
                existing = new Rfsmart.Phoenix.Licensing.FeatureTrackingRecord
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
