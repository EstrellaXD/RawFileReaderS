namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines additional raw data access methods, including diagnostic data in FT format scans.
/// </summary>
public interface IRawDataExtensions
{
	/// <summary>
	/// Gets additional (binary) data from a scan.
	/// The format of this data is custom (per instrument) and can be decoded into
	/// objects by a specific decoder for the detector type.
	/// <seealso cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataExtensions.GetExtendedScanData(System.Int32)" />
	/// </summary>
	/// <param name="scan">Scan whose data is needed</param>
	byte[] GetAdditionalScanData(int scan);

	/// <summary>
	/// Gets additional data from a scan, formatted as a series of data blocks.
	/// The format of this data is custom (per instrument) and can be decoded into
	/// objects by a specific decoder for the detector type.
	/// If the data is not in a detectable "data block" format, then this data is returned as "Not valid".
	/// Data of a format not known to this interface may be retrieved by using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataExtensions.GetExtendedScanData(System.Int32)" />
	/// </summary>
	/// <param name="scan">Scan whose data is needed</param>
	IExtendedScanData GetExtendedScanData(int scan);

	/// <summary>
	/// Gets the (raw) status log data at a given index in the sorted log.
	/// This form of the log removes duplicate and out of order times
	/// Designed for efficiency, this method does not convert logs to display string format.
	/// </summary>
	/// <param name="index">Index  (from 0 to "GetStatusLogEntriesCount() -1")</param>
	/// <returns>Log data at the given index</returns>
	IStatusLogEntry GetSortedStatusLogEntry(int index);
}
