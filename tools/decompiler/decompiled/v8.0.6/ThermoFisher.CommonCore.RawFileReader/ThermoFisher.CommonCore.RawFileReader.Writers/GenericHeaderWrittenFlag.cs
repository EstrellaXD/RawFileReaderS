namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Enumeration of generic header.
/// The flag indicates which generic header has been written
/// </summary>
internal enum GenericHeaderWrittenFlag
{
	/// <summary>
	/// The written status log header
	/// </summary>
	WrittenStatusLogHeader,
	/// <summary>
	/// The written trailer extra header
	/// </summary>
	WrittenTrailerExtraHeader,
	/// <summary>
	/// The written tune header
	/// </summary>
	WrittenTuneHeader
}
