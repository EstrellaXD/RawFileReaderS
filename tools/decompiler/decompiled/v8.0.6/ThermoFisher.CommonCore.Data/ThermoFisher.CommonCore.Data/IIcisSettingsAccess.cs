namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to ICIS Settings
/// </summary>
public interface IIcisSettingsAccess
{
	/// <summary>
	/// Gets the number of scans in the baseline window.
	/// Each scan is checked to see if it should be considered a baseline scan.
	/// This is determined by looking at a number of scans (BaselineWindow) before
	/// and after the a data point. If it is the lowest point in the group it will be
	/// marked as a "baseline" point.
	/// Range: 1 - 500
	/// Default: 40
	/// </summary>
	int BaselineWindow { get; }

	/// <summary>
	/// Gets a noise level multiplier.
	/// This determines the peak edge after the location of the possible peak,
	/// allowing the peak to narrow or broaden without affecting the baseline. 
	/// Range: 1 - 500
	/// Default multiplier: 5
	/// </summary>
	int AreaNoiseFactor { get; }

	/// <summary>
	/// Gets a noise level multiplier (a minimum S/N ratio).
	/// This determines the potential peak signal threshold. 
	/// Range: 1 - 1000
	/// Default multiplier: 10
	/// </summary>
	int PeakNoiseFactor { get; }

	/// <summary>
	/// Gets a value indicating whether to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	bool ConstrainPeakWidth { get; }

	/// <summary>
	/// Gets the percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the ConstrainPeak is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	double PeakHeightPercentage { get; }

	/// <summary>
	/// Gets the Tailing Factor.
	/// This controls how Genesis integrates the tail of a peak.
	/// This factor is the maximum ratio of the trailing edge to the leading side of a constrained peak.
	/// This applies only when the ConstrainPeak is true.
	/// The valid range is 0.5 through 9.0. 
	/// </summary>
	double TailingFactor { get; }

	/// <summary>
	/// Gets the minimum number of scans required in a peak. 
	/// Range: 0 to 100. 
	/// Default: 3. 
	/// </summary>
	int MinimumPeakWidth { get; }

	/// <summary>
	///  Gets the minimum separation in scans between the apexes of two potential peaks.
	///  This is a criterion to determine if two peaks are resolved.
	///  Enter a larger number in a noisy environment when the signal is bouncing around.
	///  Range: 1 to 500.
	///  Default: 10 scans. 
	/// </summary>
	int MultipletResolution { get; }

	/// <summary>
	/// Gets the number of scans on each side of the peak apex to be allowed. 
	/// Range: 0 to 100.
	/// Default: 0 scans.
	/// 0 specifies that all scans from peak-start to peak-end are to be included in the area integration.
	/// </summary>
	int AreaScanWindow { get; }

	/// <summary>
	/// Gets the number of scans past the peak endpoint to use in averaging the intensity.
	/// Range: 0 to 100. 
	/// Default: 5 scans.
	/// </summary>
	int AreaTailExtension { get; }

	/// <summary>
	/// Gets a value indicating whether noise is calculated using an RMS method
	/// </summary>
	bool CalculateNoiseAsRms { get; }

	/// <summary>
	/// Gets an enum which indicates how the ICIS peak detector determines which signals are noise.
	/// The selected points can  determine a noise level, or be fed into an RMS calculator,
	/// depending on the RMS setting.
	/// </summary>
	IcisNoiseType NoiseMethod { get; }
}
