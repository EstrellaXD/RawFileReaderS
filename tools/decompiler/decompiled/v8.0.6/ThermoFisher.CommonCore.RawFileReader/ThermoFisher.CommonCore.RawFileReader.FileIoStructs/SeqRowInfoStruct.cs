using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
///     The sequence row information structure in the raw file.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct SeqRowInfoStruct
{
	internal int Revision;

	internal int RowNumber;

	internal int SampleType;

	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
	internal string VialName;

	internal double InjectionVolume;

	internal double SampleWeight;

	internal double SampleVolume;

	internal double ISTDAmount;

	internal double DilutionFactor;
}
