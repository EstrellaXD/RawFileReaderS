using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.HRLRSPPackets;

/// <summary>
/// The low resolution spectrum type struct.
/// </summary>
internal struct LowResSpTypeStruct
{
	private readonly byte _flag;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	private readonly byte[] _intensity;

	private readonly ushort _intergralVal;

	private readonly ushort _fractionalVal;
}
