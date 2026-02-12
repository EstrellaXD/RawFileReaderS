using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

/// <summary>
/// The Linear Trap centroid packet.
/// </summary>
internal class LinearTrapCentroidPacket : AdvancedPacketBase
{
	/// <summary>
	/// Gets the segmented peaks. retrieve a DataPeak from the profile packet buffer.
	/// </summary>
	public override List<SegmentData> SegmentPeaks => base.SegmentPeakList;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.LinearTrapCentroidPacket" /> class.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="offset">offset from start of the memory reader for this scan</param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <param name="includeRefPeaks">
	/// The include ref peaks.
	/// </param>
	/// <param name="packetScanDataFeatures">
	/// The packet scan data features.
	/// </param>
	public LinearTrapCentroidPacket(IMemoryReader viewer, long offset, int fileRevision, bool includeRefPeaks = false, PacketFeatures packetScanDataFeatures = PacketFeatures.All)
		: base(viewer, offset, fileRevision, includeRefPeaks, packetScanDataFeatures)
	{
		ExpandCentroidData();
	}
}
