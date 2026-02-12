namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Represents a read-only single log.
/// </summary>
public interface ILogEntryAccess
{
	/// <summary>
	/// Gets or sets the labels in this log.
	/// </summary>
	string[] Labels { get; }

	/// <summary>
	/// Gets or sets the values in this log.
	/// </summary>
	string[] Values { get; }

	/// <summary>
	/// Gets or sets the length of the log.
	/// </summary>
	int Length { get; }
}
