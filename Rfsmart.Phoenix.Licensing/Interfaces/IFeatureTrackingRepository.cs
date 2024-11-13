using Rfsmart.Phoenix.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Interfaces
{
    internal interface IFeatureTrackingRepository
    {
        Task<FeatureTrackingRecord?> Get(string featureName);
        Task<FeatureTrackingRecord> Insert(FeatureTrackingRecord request);
        Task<PagedResult<FeatureTrackingRecord>> List(BaseListRequest<FeatureRecordSort> request);
        Task<int> Delete(DeleteFeatureRecordsRequest request);
    }
}
