using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
///     The raw file information structure - current version.
/// </summary>
internal struct RawFileInfoStruct
{
	/// <summary>
	///     If true, there is an experiment method in the file.
	/// </summary>
	internal bool IsExpMethodPresent;

	internal SystemTimeStruct TimeStructStamp;

	internal bool IsInAcquisition;

	internal uint VirtualDataOffset32;

	internal int NumberOfVirtualControllers;

	internal int NextAvailableControllerIndex;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	internal OldVirtualControllerInfo[] VirtualControllerInfoVer3;

	internal long VirtualDataOffset;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	internal VirtualControllerInfoStruct[] VirtualControllerInfoStruct;

	internal long BlobOffset;

	internal uint BlobSize;
}
