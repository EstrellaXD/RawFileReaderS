namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines ion ratio window, as found in Xcalibur PMD files
/// </summary>
public enum XcaliburIonRatioWindowType
{
	/// <summary>
	/// Test relative to target percent
	/// </summary>
	Relative,
	/// <summary>
	/// Absolute window around target percent
	/// </summary>
	Absolute
}
