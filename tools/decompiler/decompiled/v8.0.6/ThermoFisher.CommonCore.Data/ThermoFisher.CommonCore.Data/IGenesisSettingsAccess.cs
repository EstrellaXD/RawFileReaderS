namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// read only access to Genesis Settings
/// </summary>
public interface IGenesisSettingsAccess
{
	/// <summary>
	/// Gets a value indicating whether to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	bool ConstrainPeak { get; }

	/// <summary>
	/// Gets the minimum width that a peak is expected to have (seconds)
	/// if valley detection is enabled. The property is expressed as a window.
	/// With valley detection enabled,
	/// any valley points nearer than  [expected width]/2
	/// to the top of the peak are ignored.
	/// If a valley point is found outside the expected peak width,
	/// Genesis terminates the peak at that point.
	/// Genesis always terminates a peak when the signal reaches the baseline,
	/// independent of the value set for the ExpectedPeakWidth.
	/// </summary>
	double ExpectedPeakWidth { get; }

	/// <summary>
	/// Gets the percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the <c>ConstrainPeak</c> is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	double PeakHeightPercent { get; }

	/// <summary>
	/// Gets the Signal To Noise Threshold.
	/// A peak is considered ended if the following condition is met:
	/// <c>height &lt;= (BaseNoise * SignalToNoiseThreshold))</c>
	/// Where BaseNoise is the calculated noise on the fitted baseline,
	/// and height is the height above baseline.
	/// </summary>
	double SignalToNoiseThreshold { get; }

	/// <summary>
	/// Gets the Tailing Factor.
	/// This controls how Genesis integrates the tail of a peak.
	/// This factor is the maximum ratio of the trailing edge to the leading side of a constrained peak.
	/// This applies only when the <see cref="P:ThermoFisher.CommonCore.Data.IGenesisSettingsAccess.ConstrainPeak" /> is true.
	/// The valid range is 0.5 through 9.0. 
	/// </summary>
	double TailingFactor { get; }

	/// <summary>
	/// Gets a value indicating whether Valley Detection is performed. This parameter must be set to true when performing base to base integration
	/// </summary>
	bool ValleyDetection { get; }

	/// <summary>
	/// Gets the Peak Signal To Noise Ratio Cutoff.
	/// The peak edge is set to values below this defined S/N. 
	/// This test assumes an edge of a peak is found when the baseline adjusted height of the edge is less than
	/// the ratio of the baseline adjusted apex height and the peak S/N cutoff ratio. 
	/// If the S/N at the apex is 500 and the peak S/N cutoff value is 200,
	/// Genesis defines the right and left edges of the peak when the S/N reaches a value less than 200.
	/// Range: 50.0 to 10000.0. 
	/// Technical equation:<c>if height &lt; (1/PeakSignalToNoiseRatioCutoff)*height(apex) =&gt; valley here</c>
	/// </summary>
	double PeakSignalToNoiseRatioCutoff { get; }

	/// <summary>
	/// Gets the percentage of the valley bottom
	/// that the peak trace can rise above a baseline (before or after the peak). 
	/// If the trace exceeds RisePercent,
	/// Genesis applies valley detection peak integration criteria. 
	/// This method drops a vertical line from the apex of the valley between unresolved
	/// peaks to the baseline.
	/// The intersection of the vertical line and the baseline defines the end of the first
	/// peak and the beginning of the second peak. 
	/// This test is applied to both the left and right edges of the peak. 
	/// The RisePercent criteria is useful for integrating peaks with long tails.
	/// Useful range: 0.1 to 50
	/// </summary>
	double RisePercent { get; }

	/// <summary>
	/// Gets the S/N range is 1.0 to 100.0. for valley detection.
	/// Technical equation:<c>height(here +/- VALLEY_WIDTH) &gt; ValleyDepth*SNR+height(here) =&gt; valley here </c>
	/// </summary>
	double ValleyDepth { get; }

	/// <summary>
	/// Gets a value indicating whether noise is calculated using RMS.
	/// If not set, noise is calculated peak to peak.
	/// </summary>
	bool CalculateNoiseAsRms { get; }

	/// <summary>
	/// Gets the Baseline Noise Tolerance which controls how the baseline is drawn in the noise data.
	/// The higher the baseline noise tolerance value,
	/// the higher the baseline is drawn through the noise data.
	/// The valid range is 0.0 to 100.0
	/// </summary>
	double BaselineNoiseTolerance { get; }

	/// <summary>
	/// Gets the minimum number of scans that Genesis uses to calculate a baseline.
	/// A larger number includes more data in determining an averaged baseline.
	/// The valid range is 2 to 100.0.
	/// </summary>
	int MinScansInBaseline { get; }

	/// <summary>
	/// Gets the Baseline Noise Rejection Factor
	/// This factor controls the width of the RMS noise band above and below the peak detection baseline
	/// and is applied to the raw RMS noise values to raise the effective RMS noise during peak detection.
	/// The left and right peak boundaries are assigned above the noise and, therefore,
	/// closer to the peak apex value in minutes. 
	/// This action effectively raises the peak integration baseline above the RMS noise level. 
	/// Range: 0.1 to 10.0.
	/// Default: 2.0.
	/// </summary>
	double BaselineNoiseRejectionFactor { get; }

	/// <summary>
	/// Gets the number of minutes between background scan recalculations.
	/// Baseline is refitted each time this interval elapses. 
	/// </summary>
	double BackgroundUpdateRate { get; }

	/// <summary>
	/// Gets the Base (minimum) Signal To Noise Ratio.
	/// Peaks are rejected if they have a lower signal to noise ratio than this.
	/// </summary>
	double BaseSignalToNoiseRatio { get; }

	/// <summary>
	/// Gets the lowest acceptable percentage of the largest peak.
	/// Do not return peaks which are less than this % of the highest peak above baseline.
	/// </summary>
	double PercentLargestPeak { get; }

	/// <summary>
	/// Gets a value indicating whether to enable filtering of peaks by relative signal height
	/// </summary>
	bool FilterByRelativePeakHeight { get; }
}
