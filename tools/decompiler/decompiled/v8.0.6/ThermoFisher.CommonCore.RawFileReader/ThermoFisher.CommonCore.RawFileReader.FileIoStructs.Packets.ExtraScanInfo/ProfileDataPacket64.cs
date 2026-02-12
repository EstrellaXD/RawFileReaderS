using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;

/// <summary>
/// The profile data packet at file version 64.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct ProfileDataPacket64
{
	internal uint DataPos;

	internal float LowMass;

	internal float HighMass;

	internal double MassTick;

	internal long DataPosOffSet;
}
