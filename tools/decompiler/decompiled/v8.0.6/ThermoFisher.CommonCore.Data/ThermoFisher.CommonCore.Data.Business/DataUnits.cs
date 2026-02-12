namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Units of data from a UV or analog devices (if known).
/// </summary>
public enum DataUnits
{
	/// <summary>
	/// No units or unknown units
	/// </summary>
	None,
	/// <summary>
	/// straight AU
	/// </summary>
	AbsorbanceUnits,
	/// <summary>
	/// Milli AU
	/// </summary>
	MilliAbsorbanceUnits,
	/// <summary>
	/// micro AU
	/// </summary>
	MicroAbsorbanceUnits,
	/// <summary>
	/// Units are Volts
	/// </summary>
	Volts,
	/// <summary>
	/// Units are Millivolts
	/// </summary>
	MilliVolts,
	/// <summary>
	/// micro volts
	/// </summary>
	MicroVolts
}
