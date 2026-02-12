namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines how a percentage is used (for filtering)
/// </summary>
public enum PeakPercent
{
	/// <summary>
	/// Percentage of the largest peak
	/// </summary>
	PercentOfLargestPeak,
	/// <summary>
	/// Percentage of the peak identified as a component
	/// </summary>
	PercentOfComponentPeak
}
