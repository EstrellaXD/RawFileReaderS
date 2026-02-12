using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Methods to read a scan from a file
/// </summary>
public interface IScanReader
{
	/// <summary>
	/// Create a scan object from a file and a scan number.
	/// </summary>
	/// <param name="rawData">
	/// File to read from
	/// </param>
	/// <param name="scanNumber">
	/// Scan number to read
	/// </param>
	/// <returns>
	/// The scan read, or null of the scan number if not valid
	/// </returns>
	Scan GetScanFromScanNumber(IDetectorReaderBase rawData, int scanNumber);

	/// <summary>
	/// Create a scan object from a file and a retention time.
	/// </summary>
	/// <param name="rawData">
	/// File to read from
	/// </param>
	/// <param name="time">
	/// time of Scan number to read
	/// </param>
	/// <returns>
	/// The scan read, or null if no scan was read
	/// </returns>
	Scan GetScanAtTime(IDetectorReaderBase rawData, double time);
}
