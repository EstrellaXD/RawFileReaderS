using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The old LCQ tune data struct
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
internal struct LcqgTuneDataStruct
{
	internal double CapillaryTemp;

	internal double APCIVaporizerTemp;

	internal double SourceVoltage;

	internal double SourceCurrent;

	internal double SheathGasFlow;

	internal double AuxGasFlow;

	internal double CapillaryVoltage;

	internal double TubeLensOffset;

	internal double OctapoleRFAmplifier;

	internal double Octapole1Offset;

	internal double Octapole2Offset;

	internal double InterOctapoleLensVoltage;

	internal double TrapDCOffsetVoltage;

	internal double MultiplierVoltage;

	internal double MaxIonTime;

	internal double IonTime;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
	internal byte[] DataType;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	internal byte[] SourceType;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
	internal byte[] Polarity;

	internal short ZoomMicroScans;

	internal double ZoomAGCTarget;

	internal short FullMicroScans;

	internal double FullAGCTarget;

	internal short SIMMicroScans;

	internal double SIMAGCTarget;

	internal short MSnMicroScans;

	internal double MSnAGCTarget;
}
