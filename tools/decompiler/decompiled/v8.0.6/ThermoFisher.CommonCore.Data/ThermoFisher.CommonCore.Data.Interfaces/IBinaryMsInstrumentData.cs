namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Data ready to be written to a raw file, with most values in byte array format.
/// Note that scan statistics is provided as an in interface, as properties need to be inspected
/// in order to write the binary data
/// </summary>
public interface IBinaryMsInstrumentData
{
	/// <summary>
	/// Get the definition of how the analyzer is scanned
	/// </summary>
	byte[] PackedScanEvent { get; }

	/// <summary>
	/// Gets general information about about this scan
	/// </summary>
	IScanStatisticsAccess StatisticsData { get; }

	/// <summary>
	/// gets all data ro record for a scan
	/// </summary>
	byte[] ScanData { get; }

	/// <summary>
	/// for certain formats, gets additional index record count
	/// </summary>
	int ProfileIndexCount { get; }

	/// <summary>
	/// gets Index records for some profile formats
	/// </summary>
	byte[] ProfileData { get; }
}
