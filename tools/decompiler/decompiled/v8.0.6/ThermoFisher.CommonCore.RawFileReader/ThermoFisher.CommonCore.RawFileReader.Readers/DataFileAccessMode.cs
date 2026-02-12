using System;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// Data File access mode.
/// These are based on modes for "memory mapped" files on windows,
/// but are applied as appropriate to all readers
/// </summary>
[Flags]
public enum DataFileAccessMode
{
	/// <summary>
	/// Open an existing file
	/// </summary>
	Open = 1,
	/// <summary>
	/// Create a new file
	/// </summary>
	Create = 2,
	/// <summary>
	/// Allow to read
	/// </summary>
	Read = 4,
	/// <summary>
	/// Allow to write
	/// </summary>
	Write = 8,
	/// <summary>
	/// Prefixing a local namespace share memory mapped object name with a raw file loader ID
	/// </summary>
	Id = 0x10,
	/// <summary>
	/// For "windows global" objects. May npt apply to other OS or file types.
	/// Prefixing the file mapping object names with "Global\" allows processes to communicate with each other 
	/// even if they are in different terminal server sessions. (use mainly in Acquisition)
	/// This requires that the first process must have the SeCreateGlobalPrivilege privilege.
	/// without prefixing the file mapping object names with "Global\", the sharing named memory is visible to 
	/// local process
	/// </summary>
	Global = 0x20,
	/// <summary>
	/// Allow a data view to include less bytes than requested, when the request would be longer than the files size
	/// Note: Unlike most options, this is not related to any feature in Windows.
	/// If this attribute is not set, and requested data is not available, then an exception should be thrown.
	/// This can be used for non critical data (such as logs). If calling code sets this, it must examine the results
	/// and intelligently handle missing records.
	/// </summary>
	PermitMissingData = 0x40,
	/// <summary>
	/// The open read
	/// </summary>
	OpenRead = 5,
	/// <summary>
	/// The open read write
	/// </summary>
	OpenReadWrite = 0xD,
	/// <summary>
	/// The open create read
	/// </summary>
	OpenCreateRead = 7,
	/// <summary>
	/// The open create read write
	/// </summary>
	OpenCreateReadWrite = 0xF,
	/// <summary>
	/// The open read global
	/// </summary>
	OpenReadGlobal = 0x25,
	/// <summary>
	/// The open read write global
	/// </summary>
	OpenReadWriteGlobal = 0x2D,
	/// <summary>
	/// The open create read global
	/// </summary>
	OpenCreateReadGlobal = 0x27,
	/// <summary>
	/// The open create read write global
	/// </summary>
	OpenCreateReadWriteGlobal = 0x2F,
	/// <summary>
	/// The open read identifier
	/// </summary>
	OpenReadId = 0x15,
	/// <summary>
	/// The open create read loader identifier
	/// </summary>
	OpenCreateReadLoaderId = 0x17,
	/// <summary>
	/// The open create read write loader identifier
	/// </summary>
	OpenCreateReadWriteLoaderId = 0x1F
}
