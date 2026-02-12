namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Stores one record of status log values
/// </summary>
public class StatusLogValues
{
	/// <summary>
	/// Gets or sets the RetentionTime for this log entry
	/// </summary>
	public double RetentionTime { get; set; }

	/// <summary>
	/// Gets or sets the array of status log values
	/// </summary>
	public string[] Values { get; set; }
}
