namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to system suitability settings
/// </summary>
public interface ISystemSuitabilitySettingsAccess
{
	/// <summary>
	/// Gets a value indicating whether resolution checks will be performed
	/// </summary>
	bool EnableResolutionChecks { get; }

	/// <summary>
	/// Gets the Resolution Threshold.
	/// The threshold value determines if a peak's resolution or ok or not.
	/// The default value is 90%.
	/// Resolution is defined as the ratio:
	/// <para>100 × V/P</para>
	/// where:
	/// <para>V = depth of the Valley: the difference in intensity from the chromatogram at the apex of the target peak
	/// to the lowest point in the valley between the target peak and a neighboring peak</para>
	/// <para>P = Peak height: the height of the target peak, above the peak's baseline</para>
	/// </summary>
	double ResolutionThreshold { get; }

	/// <summary>
	/// Gets a value indicating whether peak symmetry checks are to be performed.
	/// Symmetry is determined at a specified peak height
	/// and is a measure of how even-sided a peak is
	/// about a perpendicular dropped from its apex.
	/// </summary>
	bool EnableSymmetryChecks { get; }

	/// <summary>
	/// Gets the Peak Height at which symmetry is measured.
	/// The default value is 50%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	double SymmetryPeakHeight { get; }

	/// <summary>
	/// Gets the Symmetry Threshold.
	/// The SOP defined Symmetry Threshold is &gt; 70% at 50% peak height.
	/// This represents a realistic practical tolerance for capillary GC data.
	/// You can enter any value within the range 0% to 100%.
	/// The default value is 80% at 50% peak height.
	/// The algorithm determines symmetry at the <c>SymmetryPeakHeight</c>
	/// For the purposes of the test, a peak is considered symmetrical if:
	/// (Lesser of L and R) × 100 / (Greater of L and R) &gt; Symmetry Threshold %
	/// where:
	/// <para>L = the distance from the left side of the peak to
	/// the perpendicular dropped from the peak apex</para>
	/// <para>R = the distance from the right side of the peak to
	/// the perpendicular dropped from the peak apex</para>
	/// Measurements of L and R are taken from the raw file without smoothing.
	/// </summary>
	double SymmetryThreshold { get; }

	/// <summary>
	/// Gets a value indicating whether peak classification checks are to be run
	/// </summary>
	bool EnablePeakClassificationChecks { get; }

	/// <summary>
	/// Gets the Peak Height at which the suitability calculator tests the width of target peaks.
	/// You can enter any value within the range 0% to 100%. The default value is 50%. 
	/// </summary>
	double PeakWidthPeakHeight { get; }

	/// <summary>
	/// Gets the minimum peak width, at the specified peak height, for the peak width suitability test.
	/// The default value is 1.8. You can set any value in the range 0 to 30 seconds. 
	/// </summary>
	double MinPeakWidth { get; }

	/// <summary>
	/// Gets the maximum peak width, at the specified peak height, for the peak width suitability test.
	/// The default value is 3.6. You can set any value in the range 0 to 30 seconds. 
	/// </summary>
	double MaxPeakWidth { get; }

	/// <summary>
	/// Gets the Peak Height at which the algorithm measures the tailing of target peaks.
	/// The default SOP value is 10%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	double TailingPeakHeight { get; }

	/// <summary>
	///  Gets the failure threshold for the tailing suitability test.
	///  The default SOP defined failure threshold is %lt 2 at 10% peak height. The valid range is 1 to 50.
	///  Tailing is calculated at the value defined in <see cref="P:ThermoFisher.CommonCore.Data.ISystemSuitabilitySettingsAccess.TailingPeakHeight" />.
	///  For the purposes of the test, a peak is considered to be excessively tailed if:
	///  <code>
	///  R / L &gt; Failure Threshold %
	///  where:
	///  L = the distance from the left side of the peak to the perpendicular dropped from the peak apex
	///  R = the distance from the right side of the peak to the perpendicular dropped from the peak apex
	///  Measurements of L and R are taken from the raw file without smoothing.</code>
	/// </summary>
	double TailingFailureThreshold { get; }

	/// <summary>
	/// Gets the Peak Height at which the algorithm measures column overloading.
	/// The default SOP value is 50%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	double ColumnOverloadPeakHeight { get; }

	/// <summary>
	/// Gets the failure threshold value for the column overload suitability test.
	/// The default SOP defined threshold is 1.5 at 50% peak height. The valid range is 1 to 20.
	/// A peak is considered to be overloaded if:
	/// <code>
	/// L / R &gt; Failure Threshold %
	/// where:
	/// L = the distance from the left side of the peak to the perpendicular dropped from the peak apex
	/// R = the distance from the right side of the peak to the perpendicular dropped from the peak apex
	/// Measurements of L and R are taken from the raw file without smoothing. </code>
	/// </summary>
	double ColumnOverloadFailureThreshold { get; }

	/// <summary>
	/// Gets the Number of Peak Widths for Noise Detection testing parameter for
	/// the baseline clipping system suitability test.
	/// The default value is 1.0 and the permitted range is 0.1 to 10.
	/// A peak is considered to be baseline clipped if there is no signal
	/// (zero intensity) on either side of the peak within the specified
	/// number of peak widths. The range is truncated to the quantitation window
	/// if the specified number of peak widths extends beyond the window’s edge.
	/// </summary>
	double PeakWidthsForNoiseDetection { get; }

	/// <summary>
	/// Gets the threshold for system suitability testing 
	/// of the signal-to-noise ratio. The default value is 20 and the
	/// permitted range is 1 to 500. The algorithm calculates the signal-to-noise ratio 
	/// within the quantitation window using only baseline signal.
	/// Any extraneous, minor, detected peaks are excluded from the calculation. 
	/// </summary>
	double SignalToNoiseRatio { get; }
}
