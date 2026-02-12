namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The filter rule for accurate mass.
/// </summary>
public enum FilterAccurateMass
{
	/// <summary>
	/// Accurate mass mode off
	/// </summary>
	Off,
	/// <summary>
	/// Accurate mass mode on
	/// </summary>
	On,
	/// <summary>
	/// Accurate mass mode is internal (reference or lock peak in this scan)
	/// </summary>
	Internal,
	/// <summary>
	/// Accurate mass mode is external (no reference in this scan)
	/// </summary>
	External,
	/// <summary>
	/// Accept any accurate mass mode
	/// </summary>
	Any
}
