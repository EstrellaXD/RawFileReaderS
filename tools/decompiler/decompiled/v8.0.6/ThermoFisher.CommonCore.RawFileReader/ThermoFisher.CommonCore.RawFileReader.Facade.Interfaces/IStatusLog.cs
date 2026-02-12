using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The Status Log interface.
/// </summary>
internal interface IStatusLog : IRealTimeAccess, IDisposable
{
	/// <summary>
	///     Gets the number of Status log items.
	/// </summary>
	int Count { get; }

	/// <summary>
	///     Gets the data descriptors.
	/// </summary>
	DataDescriptors DataDescriptors { get; }

	/// <summary>
	/// The method performs a binary search to find the status entry that is closest to the given retention time.
	/// </summary>
	/// <param name="retentionTime">
	/// The retention time.
	/// </param>
	/// <returns>
	/// The status entry containing the <see cref="T:System.Collections.Generic.List`1" /> of label value pairs for the retention time. 
	/// If there are no entries in the log, it will an empty list.
	/// </returns>
	StatusLogEntry GetItem(double retentionTime);

	/// <summary>
	/// The method gets all the log entries' value pair at the specified index.
	/// </summary>
	/// <param name="index">
	/// The index into the "status log header" for this field.
	/// </param>
	/// <returns>
	/// All the log entries' value pair at the specified index.
	/// </returns>
	List<StatusLogEntry> GetItemValues(int index);

	/// <summary>
	/// Gets the status log for a given index into the set of logs.
	/// This returns the log and it's time stamp.
	/// </summary>
	/// <param name="index">Index into table of logs <c>(from 0 to RunHeader.NumStatusLog-1)</c></param>
	/// <returns>The log values for the given index</returns>
	StatusLogEntry GetStatusRecordByIndex(int index);

	/// <summary>
	/// Gets the (raw) status log data at a given index in the log.
	/// Designed for efficiency, this method does not convert logs to display string format.
	/// </summary>
	/// <param name="index">Index (from 0 to "RunHeader.StatusLogCount -1")</param>
	/// <returns>Log data at the given index</returns>
	IStatusLogEntry GetStatusLogEntryByIndex(int index);

	/// <summary>
	/// Gets the (raw) status log data at a given retention time in the log.
	/// Designed for efficiency, this method does not convert logs to display string format.
	/// </summary>
	/// <param name="retentionTime">Retention time</param>
	/// <returns>Log data at the given retention time</returns>
	IStatusLogEntry GetStatusLogEntryByRetentionTime(double retentionTime);

	/// <summary>
	/// Gets the labels and index positions of the status log items which may be plotted.
	/// That is, the numeric items.
	/// </summary>
	/// <returns>Labels names are returned by "Key" and the index into the log is "Value".</returns>
	IEnumerable<KeyValuePair<string, int>> StatusLogPlottableData();

	/// <summary>
	/// Get a status log record from the sorted log
	/// which fixes out of order or duplictae instrument data
	/// </summary>
	/// <param name="index"></param>
	/// <returns>The log entry</returns>
	IStatusLogEntry GetSortedStatusLogEntryByIndex(int index);
}
