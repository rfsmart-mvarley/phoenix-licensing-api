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
        Task<bool> AssignFeaturesToUser(AssignFeatureRequest request);
    }
}
