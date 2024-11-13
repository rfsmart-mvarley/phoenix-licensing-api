namespace Rfsmart.Phoenix.Licensing;

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
}

public enum FeatureRecordSort
{
    Created,
    FeatureName,
}
