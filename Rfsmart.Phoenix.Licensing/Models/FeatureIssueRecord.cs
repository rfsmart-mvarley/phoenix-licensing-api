namespace Rfsmart.Phoenix.Licensing.Models
{
    public record FeatureIssueRecord
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

        /// <summary>
        /// This is the time the feature was was issued.
        /// </summary>
        public required DateTime Created { get; set; }

        /// <summary>
        /// The user that licensed the feature.
        /// </summary>
        public required string CreatedBy { get; set; }

        /// <summary>
        /// The time the record was last updated.
        /// </summary>
        public required DateTime LastUpdated { get; set; }

        /// <summary>
        /// The user that last updated the record.
        /// </summary>
        public required string LastUpdatedBy { get; set; }
    }

    public enum FeatureIssueSort
    {
        Created,
        FeatureName,
    }
}
