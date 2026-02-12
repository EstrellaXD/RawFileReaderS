namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// NIST search identity mode
/// Refer to NIST documentation for details.
/// </summary>
public enum IdentityMode
{
	/// <summary>
	/// Normal identity search mode
	/// </summary>
	Normal,
	/// <summary>
	/// Quick identity search mode
	/// </summary>
	Quick,
	/// <summary>
	///  Penalize identity search mode
	/// </summary>
	Penalize
}
