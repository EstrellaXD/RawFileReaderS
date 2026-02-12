namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Log of a particular status value, over time
/// </summary>
public interface ISingleValueStatusLog
{
	/// <summary>
	/// Gets the retention times for each value (x values to plot)
	/// </summary>
	double[] Times { get; }

	/// <summary>
	/// Gets the values logged for each time (the trended data, y values to plot).
	/// </summary>
	string[] Values { get; }
}
