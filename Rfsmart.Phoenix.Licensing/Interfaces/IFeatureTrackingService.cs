using Rfsmart.Phoenix.Licensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Interfaces
{
    public interface IFeatureTrackingService
    {
        Task<FeatureTrackingByUserResponse> AssignFeaturesToUser(FeaturesRequest request);
        Task<FeatureTrackingByUserResponse> UnassignFeaturesFromUser(FeaturesRequest request);
        Task<FeatureTrackingRecord> Get(FeatureTrackingByFeatureRequest request);
        Task<FeatureTrackingByUserResponse> Get(FeatureTrackingByUserRequest request);
        Task<IEnumerable<FeatureTrackingRecord>> GetConsumption();
        Task<IEnumerable<FeatureTrackingRecord>> GetAll();
        Task<IEnumerable<FeatureTrackingRecord>> GetOveragesByFeature(string feature);
    }
}
