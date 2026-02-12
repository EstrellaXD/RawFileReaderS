using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The trailer struct, from legacy LCQ files
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct TrailerStruct
{
	internal double IntegIntensity;

	internal float LowMassScan;

	internal float HighMassScan;

	internal int ScanNumber;

	internal int UScanCount;

	internal int StartTime;

	internal int EndTime;

	internal uint PeakIntensity;

	internal float PeakMass;

	internal float IonTime;

	internal byte ScanMode;

	internal byte ScanRate;

	internal byte ScanSegment;

	internal byte ScanEvent;

	internal float SourceCidEnergy;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	internal byte[] SpectFlags;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	internal float[] UVAnalogInput;

	internal byte ChargeState;

	internal byte DependencyType;

	internal byte AveragesDone;

	internal float ElapsedTime;

	internal int Msn;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
	internal float[] SetMass;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
	internal float[] CollisEnergy;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
	internal float[] TickleQFrequency;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
	internal float[] NotchWidth;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	internal float[] UVDigitalInput;

	internal float ApparentMW;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
	internal byte[] Dummy;

	internal uint DataPos;

	internal int NumberPackets;
}
