namespace Rfsmart.Phoenix.Licensing.Models
{
    public class FeatureTrackingRecord
    {
        /// <summary>
        /// The name of the feature being tracked.
        /// </summary>
        public required string FeatureName { get; set; }

        /// <summary>
        /// The users currently assigned to a feature.
        /// </summary>
        public required string[] Users { get; set; }

        /// <summary>
        /// This is the time recorded was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Count of all users consuming a license for this feature.
        /// </summary>
        public int UserCount => Users.Length;
    }

    public enum FeatureRecordSort
    {
        Created,
        FeatureName,
    }
}
 
