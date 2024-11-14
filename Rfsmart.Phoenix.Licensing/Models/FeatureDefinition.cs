using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rfsmart.Phoenix.Licensing.Models
{
    public class FeatureDefinition
    {
        /// <summary>
        /// The name of the feature.
        /// </summary>
        public required string FeatureName { get; set; }

        /// <summary>
        /// The cost-per-user of a single feature.
        /// </summary>
        public required double PricePerUser { get; set; }

        /// <summary>
        /// The on-demand price of a single feature.
        /// </summary>
        public required double PriceOnDemand { get; set; }

        /// <summary>
        /// The enforcement date of a pricing structure.
        /// </summary>
        public required DateTime EnforcedFrom { get; set; }

        /// <summary>
        /// The end-date for a pricing structure.
        /// </summary>
        public DateTime EnforcedUntil { get; set; }

        /// <summary>
        /// The active flag for the feature.
        /// </summary>
        public required bool IsActive { get; set; }

        /// <summary>
        /// The time the record was created.
        /// </summary>
        internal DateTime Created { get; set; }

        /// <summary>
        /// The user that created the record.
        /// </summary>
        internal string? CreatedBy { get; set; }

        /// <summary>
        /// The time the record was last modified.
        /// </summary>
        internal DateTime LastModified { get; set; }

        /// <summary>
        /// The user that modified the record.
        /// </summary>
        internal string? LastModifiedBy { get; set; }
    }
}
