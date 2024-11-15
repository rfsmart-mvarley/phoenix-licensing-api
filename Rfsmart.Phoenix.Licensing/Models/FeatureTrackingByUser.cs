using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Models
{
    public class FeatureTrackingByUserRequest
    {
        public required string User { get; set; }
    }

    public class FeatureTrackingByUserResponse
    {
        /// <summary>
        /// The name of the user.
        /// </summary>
        public required string User { get; set; }

        /// <summary>
        /// The features assigned to the user.
        /// </summary>
        public required List<string> Features { get; set; }

        /// <summary>
        /// The count of currently consumed features for this user.
        /// </summary>
        public int LicensedFeatures => Features.Count;
    }
}
