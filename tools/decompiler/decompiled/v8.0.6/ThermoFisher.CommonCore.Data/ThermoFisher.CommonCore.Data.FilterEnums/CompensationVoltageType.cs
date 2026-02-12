namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Specifies compensation voltage type.
/// </summary>
public enum CompensationVoltageType
{
	/// <summary>
	/// No numeric value: e.g. SID
	/// </summary>
	NoValue,
	/// <summary>
	/// A single value: e.g. SID=40
	/// </summary>
	SingleValue,
	/// <summary>
	/// A ramp: e.g. SID=40-50
	/// </summary>
	Ramp,
	/// <summary>
	/// SIM: e.g. SIM [100@40, 200@50]
	/// </summary>
	SIM,
	/// <summary>
	/// Accept any value
	/// </summary>
	Any
}
