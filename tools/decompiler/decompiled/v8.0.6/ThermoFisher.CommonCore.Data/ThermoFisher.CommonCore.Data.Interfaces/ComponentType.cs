namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Determines if this component is a standard or a target
/// </summary>
public enum ComponentType
{
	/// <summary>
	/// A compound which is being quantitated
	/// </summary>
	TargetCompound,
	/// <summary>
	/// An internal standard reference, to calculate area or height ratios (responses).
	/// </summary>
	ISTD
}
