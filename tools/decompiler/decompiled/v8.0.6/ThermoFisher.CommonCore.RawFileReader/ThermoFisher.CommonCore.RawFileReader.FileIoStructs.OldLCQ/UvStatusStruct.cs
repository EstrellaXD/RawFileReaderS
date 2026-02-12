using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// UV Detector Status
/// </summary>
internal struct UvStatusStruct
{
	internal int Status;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	internal short[] Wavelength;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	internal float[] Absorbance;
}
