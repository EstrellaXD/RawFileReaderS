namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Determines whether calibration is performed on concentration or amount
/// </summary>
public enum CalibrateAs
{
	/// <summary>
	/// Calibrate using concentration
	/// </summary>
	Concentration,
	/// <summary>
	/// Calibrate using amount
	/// </summary>
	Amount
}
