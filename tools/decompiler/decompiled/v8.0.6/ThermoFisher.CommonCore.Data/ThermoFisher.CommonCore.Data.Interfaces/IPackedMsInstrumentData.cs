namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Data for a scan, converted into byte arrays
/// </summary>
public interface IPackedMsInstrumentData
{
	/// <summary>
	/// Gets or sets the converted scan event
	/// </summary>
	byte[] PackedScanEvent { get; set; }

	/// <summary>
	/// Gets or sets the converted scan stats
	/// </summary>
	byte[] PackedScanStats { get; }

	/// <summary>
	/// Gets or sets the converted scan data
	/// </summary>
	byte[] ScanData { get; }

	/// <summary>
	/// Gets the number of profile index records
	/// </summary>
	int ProfilePaketCount { get; }

	/// <summary>
	/// Gets the profile index records
	/// </summary>
	byte[] ProfileArray { get; }
}
