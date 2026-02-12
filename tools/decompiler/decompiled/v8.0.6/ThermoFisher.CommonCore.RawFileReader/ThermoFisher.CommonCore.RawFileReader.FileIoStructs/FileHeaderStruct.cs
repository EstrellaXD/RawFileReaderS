using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
/// The file header structure.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct FileHeaderStruct
{
	/// <summary>
	///     2-byte ID for Finnigan / Enterprise file -- constant for all Enterprise files
	/// </summary>
	internal ushort FinnID;

	/// <summary>
	///     Finnigan signature
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
	internal string FinnSig;

	/// <summary>
	///     file type
	/// </summary>
	internal ushort FileType;

	/// <summary>
	///     file revision - actually, this is the file format version.
	/// </summary>
	internal ushort FileRev;

	/// <summary>
	///     file-creation audit information.
	/// </summary>
	internal UserIdStampStruct Created;

	/// <summary>
	///     file creation 32-bit CRC
	/// </summary>
	internal uint CheckSum;

	/// <summary>
	///     file-change audit information (i.e. when file is closed)
	/// </summary>
	internal UserIdStampStruct Changed;

	/// <summary>
	///     count of times the file was edited
	/// </summary>
	internal ushort TimesEdited;

	/// <summary>
	///     count of times calibrated
	/// </summary>
	internal ushort TimesCalibrated;

	/// <summary>
	///     reserved space in the header, for future use
	///     do not expose access to these values.
	/// </summary>
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
	internal uint[] Reserved;

	/// <summary>
	///     user's narrative description of the file.
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
	internal string FileDescription;

	/// <summary>
	///     end-of-data marker
	/// </summary>
	internal int EndOfData;
}
