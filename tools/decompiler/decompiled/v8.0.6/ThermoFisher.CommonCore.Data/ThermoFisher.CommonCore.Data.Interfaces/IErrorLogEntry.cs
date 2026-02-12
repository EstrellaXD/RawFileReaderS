namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ErrorLogEntry interface.
/// </summary>
public interface IErrorLogEntry
{
	/// <summary>
	/// Gets the retention time.
	/// </summary>
	double RetentionTime { get; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	string Message { get; }
}
