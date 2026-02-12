namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to specify returning
/// a limited number of "most intense" peaks.
/// </summary>
public interface IPeakLimitsAccess
{
	/// <summary>
	/// Gets a value indicating whether peak limits are enabled
	/// </summary>
	bool IsLimitPeaksEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether to Select top peak by area or height
	/// </summary>
	LimitPeaks LimitPeaks { get; }

	/// <summary>
	/// Gets the number of "top peaks" to select
	/// </summary>
	double NumberOfPeaks { get; }

	/// <summary>
	/// Gets a value indicating whether "relative peak height threshold" is enabled
	/// </summary>
	bool IsRelativePeakEnabled { get; }

	/// <summary>
	/// Gets the percent of the largest peak, which is used for filtering
	/// peak detection results, when "IsRelativePeakEnabled"
	/// </summary>
	double PercentLargestPeak { get; }

	/// <summary>
	/// Gets a the "percent of component peak" (limit)
	/// Only valid when PeakPercent is set to PercentOfComponentPeak
	/// </summary>
	double PercentComponentPeak { get; }

	/// <summary>
	/// Gets a value indicating how peak percentages are specified
	/// (unused in product Xcalibur)
	/// </summary>
	PeakPercent PeakPercent { get; }
}
