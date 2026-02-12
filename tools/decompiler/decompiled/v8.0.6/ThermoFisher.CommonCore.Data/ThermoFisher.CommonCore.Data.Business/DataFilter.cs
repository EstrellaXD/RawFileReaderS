namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Specifies how positive and negative values are handled
/// </summary>
public enum DataFilter
{
	/// <summary>
	/// Return all data
	/// </summary>
	AllData,
	/// <summary>
	/// Return strictly positive data (not zero)
	/// </summary>
	PositiveOnly,
	/// <summary>
	/// Return zero and above
	/// </summary>
	PositiveAndZero
}
