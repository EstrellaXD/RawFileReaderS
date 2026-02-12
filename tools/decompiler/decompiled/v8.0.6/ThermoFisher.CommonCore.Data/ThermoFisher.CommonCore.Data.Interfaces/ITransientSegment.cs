namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a transient segment for a scan
/// </summary>
public interface ITransientSegment
{
	/// <summary>
	/// Gets the transient segment header, which need to be used by instrument specific code.
	/// This header is defined as "32 1 bit flags" and not a countable (integer) value.
	/// </summary>
	int Header { get; }

	/// <summary>
	/// Gets the transient data, as defined for the instrument.
	/// </summary>
	int[] Data { get; }
}
