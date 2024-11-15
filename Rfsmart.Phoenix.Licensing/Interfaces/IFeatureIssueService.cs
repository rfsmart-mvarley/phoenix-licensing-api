using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Interfaces
{
    public interface IFeatureIssueService
    {
        Task<bool> IssueFeature(FeatureIssueRequest request);
        Task<FeatureIssueRecord> GetCurrentFeatureIssuance(string featureName);
        Task<FeatureIssueRecord[]> GetAllFeatureIssuances(string featureName);
        Task<FeatureIssueRecord[]> GetAllFeatureIssuances();
    }
}
