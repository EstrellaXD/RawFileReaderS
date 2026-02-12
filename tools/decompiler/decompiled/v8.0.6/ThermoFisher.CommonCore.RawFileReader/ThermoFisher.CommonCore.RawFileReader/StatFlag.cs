namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The STATFLAG enumeration values indicate whether the method should try to return
///  a name in the <c>pwcsName</c> member of the STATSTG structure.
///  The values are used in the ILockBytes::Stat, IStorage::Stat, and IStream::Stat methods to
///  save memory when the <c>pwcsName</c> member is not required.
/// </summary>
internal enum StatFlag
{
	/// <summary>
	/// Requests that the statistics include the <c>pwcsName</c> member of the STATSTG structure.
	/// </summary>
	Default,
	/// <summary>
	/// Requests that the statistics not include the <c>pwcsName</c> member of the STATSTG structure.
	///  If the name is omitted, there is no need for the
	///  ILockBytes::Stat, IStorage::Stat, and IStream::Stat methods 
	/// to allocate and free memory for the string value of the name, 
	/// therefore the method reduces time and resources used in an allocation and free operation.
	/// </summary>
	NoName
}
