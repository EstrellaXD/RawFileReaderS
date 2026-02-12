namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
///     The File Time structure from C++.
///     Represents the number of 100-nanosecond intervals since January 1, 1601. This structure is a 64-bit value.
/// </summary>
internal struct FileTimeStruct
{
	/// <summary>
	///     Specifies the high 32 bits of the FILETIME.
	/// </summary>
	public int HighDateTime;

	/// <summary>
	///     Specifies the low 32 bits of the FILETIME.
	/// </summary>
	public int LowDateTime;
}
