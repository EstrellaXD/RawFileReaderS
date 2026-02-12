using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped single value status log.
/// Converts internal data to the public interface.
/// </summary>
internal class WrappedSingleValueStatusLog : ISingleValueStatusLog
{
	/// <summary>
	/// Gets the retention times for each value (x values to plot)
	/// </summary>
	public double[] Times { get; private set; }

	/// <summary>
	/// Gets the values logged for each time (the trended data, y values to plot).
	/// </summary>
	public string[] Values { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedSingleValueStatusLog" /> class.
	/// </summary>
	/// <param name="statusLogEntries">
	/// The status log entries.
	/// </param>
	public WrappedSingleValueStatusLog(List<StatusLogEntry> statusLogEntries)
	{
		CopyFrom(statusLogEntries);
	}

	/// <summary>
	/// copy from statusLogEntries
	/// </summary>
	/// <param name="statusLogEntries">
	/// The status log entries.
	/// </param>
	private void CopyFrom(List<StatusLogEntry> statusLogEntries)
	{
		if (statusLogEntries == null || !statusLogEntries.Any())
		{
			Times = Array.Empty<double>();
			Values = Array.Empty<string>();
			return;
		}
		int count = statusLogEntries.Count;
		int num = 0;
		Times = new double[count];
		Values = new string[count];
		foreach (StatusLogEntry statusLogEntry in statusLogEntries)
		{
			Times[num] = statusLogEntry.RetentionTime;
			Values[num] = statusLogEntry.ValuePairs[0].Value.ToString();
			num++;
		}
	}
}
