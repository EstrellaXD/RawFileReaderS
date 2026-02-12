namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Settings for genesis integrator (as read from Xcalibur PMD file)
/// </summary>
public interface IGenesisRawSettingsAccess
{
	/// <summary>
	/// Gets a value indicating whether a peak's width (the tail) must be constrained.
	/// This flag allows you to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	bool ConstrainPeak { get; }

	/// <summary>
	/// Gets the width of a typical peak in seconds.
	/// This controls the minimum width that a peak is expected to have
	/// if valley detection is enabled.
	/// Integrator converts this to expectedPeakHalfWidth (minutes) by dividing by 120.
	/// With valley detection enabled,
	/// any valley points nearer than the expectedPeakHalfWidth (which is [expected width]/2)
	/// to the top of the peak are ignored.
	/// If a valley point is found outside the expected peak width,
	/// Genesis terminates the peak at that point.
	/// Genesis always terminates a peak when the signal reaches the baseline,
	/// independent of the value set for the expectedPeakHalfWidth.
	/// </summary>
	double ExpectedPeakWidth { get; }

	/// <summary>
	/// Gets a constraint on peak height.
	/// The percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the <c>ConstrainPeak</c> is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	double PeakHeightPercent { get; }

	/// <summary>
	/// Gets the minimum acceptable signal to noise of a peak.
	/// Genesis ignores all chromatogram peaks that have signal-to-noise values
	/// that are less than the S/N Threshold value
	/// </summary>
	double SignalToNoiseThreshold { get; }

	/// <summary>
	/// Gets the peak tailing factor.
	/// This controls how Genesis integrates the tail of a peak.
	/// This factor is the maximum ratio of the trailing edge to the leading side of a constrained peak.
	/// This applies only when the <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IGenesisRawSettingsAccess.ConstrainPeak" /> is true.
	/// The valid range is 0.5 through 9.0. 
	/// </summary>
	double TailingFactor { get; }

	/// <summary>
	/// Gets a value indicating whether valley detection is performed.
	/// This parameter must be set to true when performing base to base integration
	/// </summary>
	bool ValleyDetection { get; }

	/// <summary>
	/// Gets the multiplier of the valley bottom
	/// that the peak trace can rise above a baseline (before or after the peak). 
	/// If the trace exceeds ValleyThreshold,
	/// Genesis applies valley detection peak integration criteria. 
	/// This method drops a vertical line from the apex of the valley between unresolved
	/// peaks to the baseline.
	/// The intersection of the vertical line and the baseline defines the end of the first
	/// peak and the beginning of the second peak. 
	/// This test is applied to both the left and right edges of the peak. 
	/// The ValleyThreshold criteria is useful for integrating peaks with long tails.
	/// Useful range: 1.001 to 1.5
	/// Note: Appears on product UI converted from factor to percentage as "Rise percentage".
	/// For example: 1.1 = 10%
	/// Code tests similar to the following:<code>
	/// if ((currentSignal-baseline) &gt; ((valleyBottom-baseline) * ValleyThreshold))
	/// {
	///     side of peak has bottomed out, and risen above minimum
	/// }
	/// </code>
	/// </summary>
	double ValleyThreshold { get; }

	/// <summary>
	/// Gets or the S/N range is 1.0 to 100.0. for valley detection.
	/// Technical equation:<c>height(here +/- VALLEY_WIDTH) &gt; ValleyDepth*SNR+height(here) =&gt; valley here </c>
	/// </summary>
	double ValleyDepth { get; }

	/// <summary>
	/// Gets a value indicating whether to enable RMS noise calculation.
	/// If not set, noise is calculated peak to peak.
	/// It is set by default.
	/// </summary>
	bool CalculateNoiseAsRms { get; }

	/// <summary>
	/// Gets a noise limit, where the code stops attempting to find a better baseline.
	/// controls how the baseline is drawn in the noise data.
	/// The higher the baseline noise tolerance value,
	/// the higher the baseline is drawn through the noise data.
	/// The valid range is 0.0 to 1.0.
	/// </summary>
	double BaselineNoiseLimit { get; }

	/// <summary>
	/// Gets the minimum number of scans that Genesis uses to calculate a baseline.
	/// A larger number includes more data in determining an averaged baseline.
	/// The valid range is 2 to 100.
	/// </summary>
	int MinScansInBaseline { get; }

	/// <summary>
	/// Gets a factor which controls the width of the RMS noise band above and below the peak detection baseline
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
	/// Gets a limit for the "baseline signal to noise ratio".
	/// A peak is considered ended if the following condition is met:
	/// <c>height &lt;= (BaseNoise * BaseSignalToNoiseRatio))</c>
	/// Where BaseNoise is the calculated noise on the fitted baseline,
	/// and height is the height above baseline.
	/// </summary>
	double BaseSignalToNoiseRatio { get; }

	/// <summary>
	/// Gets the minimum acceptable percentage of the largest peak.
	/// Do not return peaks which have a height less than this % of the highest peak above baseline.
	/// </summary>
	double PercentLargestPeak { get; }

	/// <summary>
	/// Gets a value indicating whether filtering of peaks is by relative signal height
	/// </summary>
	bool FilterByRelativePeakHeight { get; }

	/// <summary>
	/// Gets the Peak Signal ToNoise Ratio Cutoff.
	/// The peak edge is set to values below this defined S/N. 
	/// This test assumes an edge of a peak is found when the baseline adjusted height of the edge is less than
	/// the ratio of the baseline adjusted apex height and the peak S/N cutoff ratio. 
	/// If the S/N at the apex is 500 and the peak S/N cutoff value is 200,
	/// Genesis defines the right and left edges of the peak when the S/N reaches a value less than 200.
	/// Range: 50.0 to 10000.0. 
	/// Technical equation:<c>if height &lt; (1/PeakSignalToNoiseRatioCutoff)*height(apex) =&gt; valley here</c>
	/// </summary>
	double PeakSignalToNoiseRatioCutoff { get; }
}
