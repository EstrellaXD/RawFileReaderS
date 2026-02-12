using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
/// The user id structure for the raw file.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct UserIdStampStruct
{
	/// <summary>
	///     Length of logon ID string and user ID
	/// </summary>
	public const int LoginNameLength = 25;

	/// <summary>
	///     Date and time that the file was changed using C++ FILETIME structure (64-bits), precision is to 100 nanoseconds.
	/// </summary>
	internal FILETIME TimeStamp;

	/// <summary>
	///     Windows login name. Two bytes per char (NT default string representation)
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
	internal string WindowsLogin;

	/// <summary>
	///     User (full) name
	/// </summary>
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
	internal string UserName;
}
