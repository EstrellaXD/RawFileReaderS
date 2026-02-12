using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The "no data" scan from raw data class, which supplies code needed for the
/// chromatogram batch generator to get scan information from the IRawDataPlus interface.
/// This version returns scan event and index information only.
/// </summary>
internal class NoDataScanFromRawData
{
	private readonly MassSpecDevice _rawFile;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.NoDataScanFromRawData" /> class.
	/// </summary>
	/// <param name="rawData">
	///   The raw data.
	/// </param>
	public NoDataScanFromRawData(MassSpecDevice rawData)
	{
		_rawFile = rawData;
	}

	/// <summary>
	/// The scan reader.
	/// </summary>
	/// <param name="simpleScanHeader">
	/// The scan index, to the available scans.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SimpleScanWithHeader" />.
	/// </returns>
	public SimpleScanWithHeader Reader(ISimpleScanHeader simpleScanHeader)
	{
		ScanIndex scanIndex = simpleScanHeader as ScanIndex;
		ScanEvent scanEvent = _rawFile.ScanEventWithValidScanNumber(scanIndex);
		ScanEventHelper scanEvent2 = ScanEventHelper.ScanEventHelperFactory(scanEvent, scanEvent.ScanTypeLocation);
		return new SimpleScanWithHeader
		{
			Header = simpleScanHeader,
			Scan = new ScanWithSimpleDataLocal
			{
				Data = null,
				ScanEvent = scanEvent2
			}
		};
	}
}
