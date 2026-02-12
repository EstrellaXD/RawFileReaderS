namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The LOCKTYPE enumeration values indicate the type of locking requested
///  for the specified range of bytes.
/// The values are used in the ILockBytes::LockRegion and IStream::LockRegion methods.
/// </summary>
internal enum LockType
{
	/// <summary>
	/// If this lock is granted, writing to the specified range of bytes is
	///  prohibited except by the owner that was granted this lock.
	/// </summary>
	Exclusive = 2,
	/// <summary>
	/// If this lock is granted, no other LOCK_ONLYONCE lock can be obtained on the range. Usually this lock type is an alias for some other lock type.
	///  Thus, specific implementations can have additional behavior associated with this lock type.
	/// </summary>
	OnlyOnce = 4,
	/// <summary>
	/// If this lock is granted, the specified range of bytes can be opened and read any number of times,
	/// but writing to the locked range is prohibited except for the owner that was granted this lock.
	/// </summary>
	Write = 1
}
