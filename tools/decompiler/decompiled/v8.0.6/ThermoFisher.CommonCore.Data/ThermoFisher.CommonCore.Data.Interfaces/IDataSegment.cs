namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a block of instrument specific data
/// </summary>
public interface IDataSegment
{
	/// <summary>
	/// Get the block header, which needs to be decoded as defined by the instrument.
	/// This will identify the meaning of the data.
	/// </summary>
	int Header { get; }

	/// <summary>
	/// Gets the data within this block
	/// </summary>
	byte[] Bytes { get; }
}
