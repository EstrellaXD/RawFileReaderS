using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.HRLRSPPackets;

/// <summary>
/// The high resolution spectrum type struct.
/// This struct is never access "as a struct" and
/// is decoded one field at a time from memory.
/// It is only declared so that it's size can be calculated.
/// </summary>
internal struct HighResSpTypeStruct
{
	private readonly byte scaleFlag;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
	private readonly byte[] intensity;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	private readonly byte[] mass;

	private readonly byte peakWidth;

	private readonly byte flag;

	private readonly int time;
}
