namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// values are used in the type member of the STATSTG structure to indicate the type of the storage element. A storage element is a storage object,
///  a stream object, or a byte-array object (LOCKBYTES).
/// </summary>
internal enum StgType
{
	/// <summary>
	/// Indicates that the storage element is a byte-array object.
	/// </summary>
	LockBytes = 3,
	/// <summary>
	/// Indicates that the storage element is a property storage object.
	/// </summary>
	Property = 4,
	/// <summary>
	/// Indicates that the storage element is a storage object.
	/// </summary>
	Storage = 1,
	/// <summary>
	/// Indicates that the storage element is a stream object.
	/// </summary>
	Stream = 2
}
