using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Interfaces
{
    public interface IFeatureIssueRepository
    {
        Task<FeatureIssueRecord> Insert(FeatureIssueRequest request);
        Task<FeatureIssueRecord?> Get(string featureName);
        Task<FeatureIssueRecord[]> GetAll(string featureName);
        Task<FeatureIssueRecord[]> GetAll();
    }
}
