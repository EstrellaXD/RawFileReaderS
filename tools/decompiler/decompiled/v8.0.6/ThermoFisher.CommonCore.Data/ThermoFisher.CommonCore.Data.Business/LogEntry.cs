using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Represents a single log.
/// </summary>
[Serializable]
public class LogEntry : ILogEntryAccess
{
	/// <summary>
	/// Gets or sets the labels in this log.
	/// </summary>
	public string[] Labels { get; set; }

	/// <summary>
	/// Gets or sets the values in this log.
	/// </summary>
	public string[] Values { get; set; }

	/// <summary>
	/// Gets or sets the length of the log.
	/// </summary>
	public int Length { get; set; }

	/// <summary>
	/// Default constructor
	/// </summary>
	public LogEntry()
	{
	}

	/// <summary>
	/// Shallow Clone constructor.
	/// Create a log entry which references the properties of the supplied object
	/// </summary>
	public LogEntry(ILogEntryAccess access)
	{
		if (access == null)
		{
			throw new ArgumentNullException("access");
		}
		Labels = access.Labels;
		Values = access.Values;
		Length = access.Length;
	}
}
