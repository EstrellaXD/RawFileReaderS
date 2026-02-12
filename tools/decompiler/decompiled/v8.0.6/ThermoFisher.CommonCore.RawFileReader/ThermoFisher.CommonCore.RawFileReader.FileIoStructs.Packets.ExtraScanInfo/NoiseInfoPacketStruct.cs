namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;

/// <summary>
/// The noise information packet structure.
/// Note: this is defined as a simple set of "internal fields" to
/// exactly match the binary struct format.
/// </summary>
internal struct NoiseInfoPacketStruct
{
	/// <summary>
	/// Gets or sets the mass.
	/// </summary>
	internal readonly float Mass;

	/// <summary>
	/// Gets or sets the noise.
	/// </summary>
	internal readonly float Noise;

	/// <summary>
	/// Gets or sets the baseline.
	/// </summary>
	internal readonly float Baseline;
}
