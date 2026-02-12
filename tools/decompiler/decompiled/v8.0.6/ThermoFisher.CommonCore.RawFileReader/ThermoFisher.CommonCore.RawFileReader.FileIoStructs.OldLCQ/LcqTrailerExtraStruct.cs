using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The LCQ trailer extra struct.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct LcqTrailerExtraStruct
{
	internal short MicroScanCount;

	internal float IonInjectionTime;

	internal byte ScanSegment;

	internal byte ScanEvent;

	internal float ElapsedScanTime;

	internal float APISourceCIDEnergy;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	internal byte[] Resolution;

	internal byte AverageScanByInst;

	internal byte BackGdSubtractedByInst;

	internal short ChargeState;
}
