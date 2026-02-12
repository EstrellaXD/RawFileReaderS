using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The instrument configuration struct version 1, from legacy LCQ files.
/// </summary>
internal struct InstrumentConfigStruct1
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	internal bool[] ChannelInUse;
}
