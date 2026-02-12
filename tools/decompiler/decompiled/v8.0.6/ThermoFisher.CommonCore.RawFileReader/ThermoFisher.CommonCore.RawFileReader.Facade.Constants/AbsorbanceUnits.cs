namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
/// The absorbance units.
/// </summary>
internal enum AbsorbanceUnits
{
	/// <summary>
	/// The unknown.
	/// </summary>
	Unknown,
	/// <summary>
	/// The straight Absorbance unit.
	/// </summary>
	Au,
	/// <summary>
	/// The milli Absorbance unit.
	/// </summary>
	MilliAu,
	/// <summary>
	/// The micro Absorbance unit.
	/// </summary>
	MicroAu,
	/// <summary>
	/// Some other Absorbance unit. make sure you fill in Y axis string below
	/// </summary>
	OtherAu,
	/// <summary>
	/// No absorbance data contained in the data object, or no data which scaling
	/// has any meaning for.
	/// </summary>
	None,
	/// <summary>
	/// The milli units.
	/// </summary>
	MilliUnits,
	/// <summary>
	/// The micro units.
	/// </summary>
	MicroUnits,
	/// <summary>
	/// The other straight units.
	/// </summary>
	OtherUnits
}
