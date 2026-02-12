using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// LC Subsystem Status
/// </summary>
internal struct LcStatusStruct
{
	internal int Status;

	internal float RunTime;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	internal float[] FlowRate;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
	internal float[] Pressure;

	internal float Temperature;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	internal float[] Composition;
}
