using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

/// <summary>
/// The Simplified FT centroid packet. Just mass/intensity data as needed for chromatogram generation.
/// </summary>
internal class SimplifiedFtCentroidPacket : AdvancedPacketBase, ISimpleMsPacket
{
	public double[] Mass { get; }

	public double[] Intensity { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.FtCentroidPacket" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="offset">offset from start of the memory reader for this scan</param>
	/// <param name="fileRevision">Raw file revision</param>
	/// <param name="includeRefPeaks">The include ref peaks.</param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required</param>
	public SimplifiedFtCentroidPacket(IMemoryReader viewer, long offset, int fileRevision, bool includeRefPeaks = false, PacketFeatures packetScanDataFeatures = PacketFeatures.All)
		: base(viewer, offset, fileRevision, includeRefPeaks, packetScanDataFeatures, expandLabels: false)
	{
		(Mass, Intensity) = ExpandSimplifiedCentroidData();
	}
}
