using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// The MS Packet interface.
/// </summary>
internal interface IMsPacket : IPacket
{
	/// <summary>
	/// Gets the label peaks.
	/// </summary>
	LabelPeak[] LabelPeaks { get; }

	/// <summary>
	/// Gets the noise and baselines.
	/// </summary>
	NoiseAndBaseline[] NoiseAndBaselines { get; }

	/// <summary>
	/// Gets the debug data for a scan.
	/// </summary>
	byte[] DebugData { get; }

	/// <summary>
	/// Gets the debug data for a scan.
	/// </summary>
	IExtendedScanData ExtendedData { get; }
}
