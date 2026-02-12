using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The default scan provider.
/// </summary>
internal class DefaultScanProvider : IScanReader
{
	/// <summary>
	/// Get a scan from a scan number.
	/// </summary>
	/// <param name="rawData">
	/// The raw data.
	/// </param>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The scan at "scan number"
	/// </returns>
	public Scan GetScanFromScanNumber(IDetectorReaderBase rawData, int scanNumber)
	{
		if (!(rawData is IDetectorReader detectorReader))
		{
			return Scan.FromFile(rawData as IRawData, scanNumber);
		}
		return Scan.FromDetector(detectorReader, scanNumber);
	}

	/// <summary>
	/// The get scan at time.
	/// </summary>
	/// <param name="rawFile">
	/// The raw file.
	/// </param>
	/// <param name="time">
	/// The time.
	/// </param>
	/// <returns>
	/// The scan nearest to the given time.
	/// </returns>
	public Scan GetScanAtTime(IDetectorReaderBase rawFile, double time)
	{
		return GetScanFromScanNumber(rawFile, rawFile.ScanNumberFromRetentionTime(time));
	}
}
