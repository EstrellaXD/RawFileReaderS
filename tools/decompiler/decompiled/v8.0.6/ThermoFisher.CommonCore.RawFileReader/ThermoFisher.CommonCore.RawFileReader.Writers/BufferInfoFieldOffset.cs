namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Buffer Info structure
/// The offset where the field begins
/// </summary>
internal enum BufferInfoFieldOffset
{
	/// <summary>
	/// The number of elements.
	/// </summary>
	NumElements = 0,
	/// <summary>
	/// The size of the data block
	/// </summary>
	Size = 24,
	/// <summary>
	/// The reference count.
	/// </summary>
	ReferenceCount = 40
}
