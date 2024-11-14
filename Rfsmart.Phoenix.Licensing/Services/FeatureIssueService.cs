using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Services
{
    public class FeatureIssueService(IFeatureIssueRepository featureIssueRepository,
        IFeatureDefinitionRepository featureDefinitionRepository) : IFeatureIssueService
    {
        public async Task<bool> IssueFeature(FeatureIssueRecord featureIssueRecord)
        {
            var featureDefinition = await featureDefinitionRepository.Get(featureIssueRecord.FeatureName);

            if (featureDefinition is null)
            {
                throw new ArgumentException($"Feature definition does not exist for {featureIssueRecord.FeatureName}");
            }

            await featureIssueRepository.Insert(featureIssueRecord);

            return true;
        }
    }
}
