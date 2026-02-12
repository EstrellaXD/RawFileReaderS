namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// enumeration of all initial and Timed events.
/// Note. The Threshold and Bunch Factor parameters are the most important ones
/// in controlling peak detection.
/// </summary>
public enum EventCode
{
	/// <summary>
	/// No peak detector event
	/// </summary>
	NoCode = 0,
	/// <summary>
	/// Directly related to the RMS noise in the chromatogram,
	/// this is Threshold, the fundamental control used for peak detection.
	/// Set the threshold at the start of a peak
	/// </summary>
	StartThreshold = 1,
	/// <summary>
	/// Directly related to the RMS noise in the chromatogram,
	/// this is Threshold, the fundamental control used for peak detection.
	/// Set the threshold at the end of a peak.
	/// </summary>
	EndThreshold = 2,
	/// <summary>
	/// Controls the area cutoff.
	/// Any peaks with a final area less than the area threshold will not be detected.
	/// This control is in units of area for the data. 
	/// </summary>
	AreaThreshold = 3,
	/// <summary>
	/// The peak to peak resolution threshold controls how much peak overlap must be present
	/// before two or more adjacent peaks create a peak cluster.
	/// Peak clusters will have a baseline drop instead of valley to valley baselines.
	/// This is specified as a percent of peak height overlap. 
	/// </summary>
	PPResolution = 5,
	/// <summary>
	/// Permit detection of a negative going peak.
	/// Automatically resets after a negative peak has been found. 
	/// </summary>
	NegativePeaks = 6,
	/// <summary>
	/// The Bunch Factor is the number of points grouped together during peak detection.
	/// It controls the bunching of chromatographic points during integration and does not
	/// affect the final area calculation of the peak.
	/// The Bunch Factor must be an integer between 1 and 6;
	/// a high bunch factor groups peaks into clusters.
	/// </summary>
	BunchFactor = 7,
	/// <summary>
	/// Controls how closely the baseline should follow the overall shape of the chromatogram.
	/// A lower tension traces the baseline to follow changes in the chromatogram more closely.
	/// A high baseline tension follows the baseline less closely,
	/// over longer time intervals. Set in minutes. 
	/// </summary>
	Tension = 8,
	/// <summary>
	/// Using this event, you can tangent skim any peak clusters.
	/// By default, it chooses the tallest peak in a cluster as the parent.
	/// You can also identify which peak in the cluster is the parent.
	/// Tangent skim peaks are detected on either side (or both sides) of the parent peak.
	/// Tangent skim automatically resets at the end of the peak cluster. 
	/// </summary>
	TangentSkim = 10,
	/// <summary>
	/// Allows peak shoulders to be detected (peaks which are separated by an inflection rather than a valley)
	/// Sets a threshold for the derivative.
	/// </summary>
	ShouldersOn = 11,
	/// <summary>
	/// Disables peak shoulder detection.
	/// </summary>
	ShouldersOff = 12,
	/// <summary>
	/// Stop detecting peaks, until the next on event.
	/// </summary>
	AvalonOff = 13,
	/// <summary>
	/// Start detecting peaks again.
	/// </summary>
	AvalonOn = 14,
	/// <summary>
	/// Force the following peaks to be treated as a cluster (single peak).
	/// </summary>
	ForceClusterOn = 15,
	/// <summary>
	/// End the forced clustering of peaks.
	/// </summary>
	ForceClusterOff = 16,
	/// <summary>
	/// Prevent any peaks from being clustered.
	/// </summary>
	DisableClusterOn = 17,
	/// <summary>
	/// Permit clusters to occur again.
	/// </summary>
	DisableClusterOff = 18
}
