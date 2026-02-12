using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The MS instrument data.
/// This class provides a default implementation of IMsInstrumentData
/// </summary>
public class MsInstrumentData : IMsInstrumentData
{
	/// <summary>
	/// Gets or sets the centroid data. It's a second stream with profile scan.
	/// </summary>
	public CentroidStream CentroidData { get; set; }

	/// <summary>
	/// Gets or sets the event data.
	/// </summary>
	public IScanEvent EventData { get; set; }

	/// <summary>
	/// Gets or sets the frequencies (for LT/FT).
	/// </summary>
	public double[] Frequencies { get; set; }

	/// <summary>
	/// Gets or sets the noise data.
	/// </summary>
	public NoiseAndBaseline[] NoiseData { get; set; }

	/// <summary>
	/// Gets or sets or sets the scan data.
	/// </summary>
	public ISegmentedScanAccess ScanData { get; set; }

	/// <summary>
	/// Gets or sets the statistics data.
	/// </summary>
	public IScanStatisticsAccess StatisticsData { get; set; }

	/// <summary>
	/// Gets or sets additional data blocks
	/// saved with a scan such as "charge envelopes"
	/// </summary>
	public IExtendedScanData ExtendedData { get; set; }

	/// <summary>
	/// Initialize MS Instrument data from a scan and a scan event.
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.MsInstrumentData" />.
	/// </returns>
	public static MsInstrumentData FromScanAndEvent(Scan scan, IScanEvent scanEvent)
	{
		return new MsInstrumentData
		{
			EventData = scanEvent,
			StatisticsData = scan.ScanStatistics,
			ScanData = scan.SegmentedScan,
			CentroidData = scan.CentroidScan,
			NoiseData = scan.GenerateNoiseTable(),
			Frequencies = scan.GenerateFrequencyTable()
		};
	}
}
