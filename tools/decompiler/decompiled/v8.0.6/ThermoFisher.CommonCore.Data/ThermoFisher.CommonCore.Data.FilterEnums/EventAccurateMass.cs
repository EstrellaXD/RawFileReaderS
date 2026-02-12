namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Determines how accurate mass calibration was done.
/// </summary>
public enum EventAccurateMass
{
	/// <summary>
	/// Calibration is internal (calibration compound mixed with injected data).
	/// </summary>
	Internal,
	/// <summary>
	/// Calibration is external (calibration compound used on a previous injection).
	/// </summary>
	External,
	/// <summary>
	/// No recorded accurate mass calibration.
	/// </summary>
	Off
}
