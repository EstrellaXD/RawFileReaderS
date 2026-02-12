namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// A method of ion constraint, for NIST library search
/// </summary>
public enum IonConstraintTypes
{
	/// <summary>
	/// Default value
	/// </summary>
	IonDefault = -1,
	/// <summary>
	/// Normal ion constraint
	/// </summary>
	Normal,
	/// <summary>
	/// Constrain by loss of ion
	/// </summary>
	Loss,
	/// <summary>
	/// Constrain by rank
	/// </summary>
	Rank,
	/// <summary>
	/// Constrain by max mass
	/// </summary>
	Maxmass
}
