namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to Peak Purity Settings
/// </summary>
public interface IPeakPuritySettingsAccess
{
	/// <summary>
	/// Gets the % of the detected baseline for which we want to compute PeakPurity
	/// </summary>
	double DesiredPeakCoverage { get; }

	/// <summary>
	/// Gets a value indicating whether we want to compute Peak Purity
	/// </summary>
	bool EnableDetection { get; }

	/// <summary>
	/// Gets a value indicating whether we want to use
	/// the enclosed wavelength range, not the total scan
	/// </summary>
	bool LimitWavelengthRange { get; }

	/// <summary>
	/// Gets the high limit of the scan over which to compute
	/// </summary>
	double MaximumWavelength { get; }

	/// <summary>
	/// Gets the low limit of the scan over which to compute
	/// </summary>
	double MinimumWavelength { get; }

	/// <summary>
	/// Gets the max of a scan must be greater than this to be included
	/// </summary>
	int ScanThreshold { get; }
}
