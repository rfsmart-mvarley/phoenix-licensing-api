using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Interfaces
{
    internal interface IFeatureIssueRepository
    {
        Task<FeatureIssueRecord> Insert(FeatureIssueRecord request);
        Task<FeatureIssueRecord?> Get(string featureName);
    }
}
