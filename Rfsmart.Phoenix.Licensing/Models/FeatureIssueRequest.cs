namespace Rfsmart.Phoenix.Licensing.Models
{
    public class FeatureIssueRequest
    {
        /// <summary>
        /// The name of the feature being tracked.
        /// </summary>
        public required string FeatureName { get; set; }

        /// <summary>
        /// The time the feature was enabled.
        /// </summary>
        public required DateTime EnabledTime { get; set; }

        /// <summary>
        /// The time the feature was disabled.
        /// </summary>
        public DateTime? DisabledTime { get; set; }

        /// <summary>
        /// The number of users licensed to use the feature.
        /// </summary>
        public required int LicensedUsers { get; set; }
    }
}
