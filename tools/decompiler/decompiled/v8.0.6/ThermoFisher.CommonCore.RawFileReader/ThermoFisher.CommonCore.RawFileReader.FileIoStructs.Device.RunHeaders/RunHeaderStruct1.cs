using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;

/// <summary>
///     The run header struct version 1.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct RunHeaderStruct1
{
	/// <summary>
	///     software revision level
	/// </summary>
	internal short Revision;

	internal int DataSetID;

	internal int FirstSpectrum;

	internal int LastSpectrum;

	/// <summary>
	///     # of status log records written in this file
	/// </summary>
	internal int NumStatusLog;

	/// <summary>
	///     # of error log records written in this file
	/// </summary>
	internal int NumErrorLog;

	internal int FileFlag;

	/// <summary>
	///     offset of where the scan indexes starts in the virtual controller data
	/// </summary>
	internal int SpectPos32Bit;

	/// <summary>
	///     offset of where the virtual data starts in the virtual controller data
	/// </summary>
	internal int PacketPos32Bit;

	/// <summary>
	///     offset of where the status log starts in the virtual controller data
	/// </summary>
	internal int StatusLogPos32Bit;

	/// <summary>
	///     offset of where the error log starts in the virtual controller data
	/// </summary>
	internal int ErrorLogPos32Bit;

	internal short MaxPacket;

	internal double MaxIntegIntensity;

	internal double LowMass;

	internal double HighMass;

	internal double StartTime;

	internal double EndTime;

	internal int MaxIntensity;

	internal int InstrumentID;

	internal short Inlet;

	internal short ErrorFlag;

	internal double SampleVolume;

	internal double SampleAmount;

	internal short VialNumber;

	internal double InjVolume;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	internal byte[] Flags;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
	internal string AcqFile;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
	internal string InstDesc;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
	internal string AcqDate;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
	internal string Operator;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
	internal string Comment1;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	internal string Comment2;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
	internal string SampleVolUnits;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
	internal string SampleAmountUnits;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
	internal string InjAmountUnits;

	internal bool IsInAcquisition;
}
