using Rfsmart.Phoenix.Common.Models;
using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Interfaces
{
    public interface IFeatureTrackingRepository
    {
        Task<IEnumerable<FeatureTrackingRecord>> GetOveragesByFeature(string featureName, int max);
        Task<FeatureTrackingRecord?> Get(FeatureTrackingByFeatureRequest request);
        Task<IEnumerable<FeatureTrackingRecord>> GetCurrentConsumption();
        Task<IEnumerable<FeatureTrackingRecord>> GetAll();  
        Task<FeatureTrackingRecord> Insert(FeatureTrackingRecord request);
        Task<PagedResult<FeatureTrackingRecord>> List(BaseListRequest<FeatureRecordSort> request);
        Task<int> Delete(DeleteFeatureRecordsRequest request);
    }
}
