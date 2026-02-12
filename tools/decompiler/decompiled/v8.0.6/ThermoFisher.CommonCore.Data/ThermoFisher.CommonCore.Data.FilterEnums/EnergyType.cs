namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Specifies precursor(collision) energy validation type.
/// </summary>
public enum EnergyType
{
	/// <summary>
	/// Energy value is valid
	/// </summary>
	Valid,
	/// <summary>
	/// Not valid (accept any when filtering)
	/// </summary>
	Any
}
