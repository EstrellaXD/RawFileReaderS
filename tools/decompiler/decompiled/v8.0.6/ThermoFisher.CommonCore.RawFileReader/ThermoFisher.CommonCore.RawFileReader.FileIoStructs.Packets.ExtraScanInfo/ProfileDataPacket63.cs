using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;

/// <summary>
/// The packet header structure for LR SP profile packets.
/// This is defined for structure size only.
/// Data is decoded as individual fields.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct ProfileDataPacket63
{
	private readonly uint dataPos;

	private readonly float lowMass;

	private readonly float highMass;

	private readonly double massTick;
}
