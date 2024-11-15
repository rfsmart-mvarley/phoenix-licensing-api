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
        public async Task<FeatureIssueRecord[]> GetAllFeatureIssuances(string featureName)
        {
            var featureDefinition = await featureDefinitionRepository.Get(featureName);

            if (featureDefinition is null)
            {
                throw new ArgumentException($"Feature definition does not exist for {featureName}");
            }

            return await featureIssueRepository.GetAll(featureName);
        }

        public async Task<FeatureIssueRecord[]> GetAllFeatureIssuances()
        {
            return await featureIssueRepository.GetAll();
        }

        public async Task<FeatureIssueRecord> GetCurrentFeatureIssuance(string featureName)
        {
            var featureDefinition = await featureDefinitionRepository.Get(featureName);

            if (featureDefinition is null)
            {
                throw new ArgumentException($"Feature definition does not exist for {featureName}");
            }

            var resp = await featureIssueRepository.Get(featureName);

            if (resp is null)
            {
                throw new ArgumentException($"Feature issuance does not exist for {featureName}");
            }

            return resp;
        }

        public async Task<bool> IssueFeature(FeatureIssueRequest featureIssueRecord)
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
