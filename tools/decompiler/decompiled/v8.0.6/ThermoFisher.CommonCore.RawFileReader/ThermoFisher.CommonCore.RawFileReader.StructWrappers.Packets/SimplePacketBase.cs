using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// Low resolution data never has noise peaks, label data etc.
/// Handle this in a base object
/// </summary>
internal class SimplePacketBase
{
	private readonly LabelPeaks _labelStreamData;

	/// <summary>
	/// Gets or sets the lazy segment peaks.
	/// </summary>
	protected Lazy<List<SegmentData>> LazySegmentPeaks { get; set; }

	/// <summary>
	/// Gets the label peaks.
	/// </summary>
	public LabelPeak[] LabelPeaks => _labelStreamData.Peaks;

	/// <summary>
	/// Gets the debug data for a scan.
	/// Always empty for simple packets
	/// </summary>
	public byte[] DebugData => Array.Empty<byte>();

	/// <summary>
	/// Gets the debug data for a scan.
	/// Always empty for simple packets
	/// </summary>
	public IExtendedScanData ExtendedData => new AdvancedPacketBase.EmptyExtendedScanData();

	/// <summary>
	/// Gets the noise and baselines.
	/// </summary>
	public NoiseAndBaseline[] NoiseAndBaselines => null;

	/// <summary>
	/// Gets the segmented peaks.
	/// </summary>
	public List<SegmentData> SegmentPeaks
	{
		get
		{
			if (LazySegmentPeaks == null)
			{
				return new List<SegmentData>();
			}
			return LazySegmentPeaks.Value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.SimplePacketBase" /> class.
	/// </summary>
	public SimplePacketBase()
	{
		_labelStreamData = new LabelPeaks();
	}
}
