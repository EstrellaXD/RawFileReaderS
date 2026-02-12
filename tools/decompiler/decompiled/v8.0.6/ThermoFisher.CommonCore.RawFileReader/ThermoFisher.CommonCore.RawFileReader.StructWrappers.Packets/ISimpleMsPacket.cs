namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// The MS Packet interface.
/// </summary>
internal interface ISimpleMsPacket
{
	/// <summary>
	/// Gets the mass array.
	/// </summary>
	double[] Mass { get; }

	/// <summary>
	/// gets the intensity array
	/// </summary>
	double[] Intensity { get; }
}
