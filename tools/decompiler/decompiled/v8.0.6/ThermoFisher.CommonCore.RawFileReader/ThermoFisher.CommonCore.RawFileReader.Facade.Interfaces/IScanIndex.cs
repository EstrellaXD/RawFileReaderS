using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
///     The Scan Index interface.
/// </summary>
internal interface IScanIndex : ISimpleScanHeader
{
	/// <summary>
	///     Gets the number packets.
	/// </summary>
	int NumberPackets { get; }

	/// <summary>
	///     Gets the packet type.
	/// </summary>
	SpectrumPacketType PacketType { get; }

	/// <summary>
	///     Gets the data offset.
	/// </summary>
	long DataOffset { get; }

	/// <summary>
	///     Gets the start time.
	/// </summary>
	double StartTime { get; }

	/// <summary>
	///     Gets the Tic value.
	/// </summary>
	double Tic { get; }
}
