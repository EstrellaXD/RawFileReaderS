using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped error log entry. This implements the public interface on
/// an error logged in a raw file
/// </summary>
internal class WrappedErrorLogEntry : IErrorLogEntry
{
	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Gets the retention time.
	/// </summary>
	public double RetentionTime { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedErrorLogEntry" /> class.
	/// </summary>
	/// <param name="errorLogItem">
	/// The error log item.
	/// </param>
	public WrappedErrorLogEntry(ErrorLogItem errorLogItem)
	{
		if (errorLogItem != null)
		{
			Message = errorLogItem.ErrorText;
			RetentionTime = errorLogItem.RetentionTime;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedErrorLogEntry" /> class.
	/// </summary>
	public WrappedErrorLogEntry()
	{
	}
}
