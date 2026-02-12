namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The stream seek.
/// </summary>
internal enum StreamSeek
{
	/// <summary>
	/// set position
	/// </summary>
	Set,
	/// <summary>
	/// seek to current position
	/// </summary>
	Current,
	/// <summary>
	/// seek to end.
	/// </summary>
	End
}
