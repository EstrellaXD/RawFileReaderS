using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;

/// <summary>
/// The run header struct.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct RunHeaderStruct2
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

	/// <summary>
	///     Offset of where the run header starts in the virtual controller data. This block is
	///     new for Xcalibur, need to be in shared memory.
	///     if time permits will come up with a scheme to force these names into a separate memory
	///     pool that only this object knows about so the disk file will not contain this data.
	/// </summary>
	internal int RunHeaderPos32Bit;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string ScanEventsFile;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string TuneDataFile;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string DataPktFile;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string SpectrumFile;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string StatusLogFile;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string ErrorLogFile;

	/// <summary>
	///     Start of Xcalibur changes phase 2
	///     1/2 Peak width for current device (e.g. 0.5 for unit resolution)
	/// </summary>
	internal double MassResolution;

	/// <summary>
	///     Expected run time based on experiment method.
	/// </summary>
	internal double ExpectedRunTime;

	/// <summary>
	///     Instrument identification block.
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string InstIDFile;

	/// <summary>
	///     Scan events written during collection (e.g. Tune or data dependent)
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string InstScanEventsFile;

	/// <summary>
	///     Per scan scan event objects
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string TrailerScanEventsFile;

	/// <summary>
	///     Instrument specific scan trailer header data
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string TrailerHeaderFile;

	/// <summary>
	///     Instrument specific scan trailer data (not TRAILER)
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string TrailerExtraFile;

	/// <summary>
	///     Instrument specific status log header data
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string StatusLogHeaderFile;

	/// <summary>
	///     Instrument specific tune data header
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
	internal string TuneDataHeaderFile;

	internal int TrailerScanEventsPos32Bit;

	internal int TrailerExtraPos32Bit;

	internal int NumTrailerScanEvents;

	internal int NumTrailerExtra;

	internal int NumTuneData;

	internal OldVirtualControllerInfo ControllerInfoVer4;
}
