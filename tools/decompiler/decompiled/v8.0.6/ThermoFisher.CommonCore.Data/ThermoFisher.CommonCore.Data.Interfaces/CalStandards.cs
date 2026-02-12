namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Possible ways in which calibration standards are used
/// </summary>
public enum CalStandards
{
	/// <summary>
	/// Internal standards (in the same raw file)
	/// </summary>
	Internal,
	/// <summary>
	/// External standards (in a different raw file)
	/// </summary>
	External
}
