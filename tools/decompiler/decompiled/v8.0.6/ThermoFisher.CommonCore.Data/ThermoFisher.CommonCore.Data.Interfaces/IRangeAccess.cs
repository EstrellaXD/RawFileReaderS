namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to a range
/// </summary>
public interface IRangeAccess
{
	/// <summary>
	/// Gets the Low end of range
	/// </summary>
	double Low { get; }

	/// <summary>
	/// Gets the high end of the range
	/// </summary>
	double High { get; }
}
