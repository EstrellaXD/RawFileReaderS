using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines access to data which can be written to an MS instrument, for one scan.
/// This format will typically be used from an application.
/// </summary>
public interface IMsInstrumentData
{
	/// <summary>
	/// class to define a safe data structure for "no data"
	/// </summary>
	private class EmptyExtendedScanData : IExtendedScanData
	{
		public long Header { get; }

		public ReadOnlyCollection<ITransientSegment> Transients { get; }

		public ReadOnlyCollection<IDataSegment> DataSegments { get; }

		public EmptyExtendedScanData()
		{
			Header = 0L;
			Transients = new ReadOnlyCollection<ITransientSegment>(new ITransientSegment[0]);
			DataSegments = new ReadOnlyCollection<IDataSegment>(new IDataSegment[0]);
		}
	}

	/// <summary>
	/// Gets the event data.
	/// </summary>
	IScanEvent EventData { get; }

	/// <summary>
	/// Gets the statistics data.
	/// </summary>
	IScanStatisticsAccess StatisticsData { get; }

	/// <summary>
	/// Gets the scan data.
	/// </summary>
	ISegmentedScanAccess ScanData { get; }

	/// <summary>
	/// Gets the noise data.
	/// </summary>
	/// <value>
	/// The noise data.
	/// </value>
	NoiseAndBaseline[] NoiseData { get; }

	/// <summary>
	/// Gets the centroid data, it's a second stream with profile scan.
	/// </summary>
	/// <value>
	/// The centroid data.
	/// </value>
	CentroidStream CentroidData { get; }

	/// <summary>
	/// Gets the frequencies (for LT/FT).
	/// </summary>
	/// <value>
	/// The frequencies.
	/// </value>
	double[] Frequencies { get; }

	/// <summary>
	/// Gets or sets additional data blocks
	/// saved with a scan such as "charge envelopes"
	/// </summary>
	IExtendedScanData ExtendedData => new EmptyExtendedScanData();
}
