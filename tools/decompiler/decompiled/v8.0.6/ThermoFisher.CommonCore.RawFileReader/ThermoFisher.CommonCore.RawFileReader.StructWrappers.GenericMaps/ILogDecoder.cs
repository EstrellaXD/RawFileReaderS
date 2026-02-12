namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// The log decoder interface is used to decode values for all generic logs.
/// </summary>
internal interface ILogDecoder
{
	/// <summary>
	/// Gets the set of descriptors for this log
	/// </summary>
	DataDescriptors Descriptors { get; }

	/// <summary>
	/// Get the available bytes
	/// </summary>
	long Available { get; }

	/// <summary>
	/// Get the value of a field, with minimal decoding
	/// </summary>
	/// <param name="dataOffset">offset into map</param>
	/// <param name="dataDescriptor">definition of type</param>
	/// <returns>The value as an object</returns>
	object GetValue(ref long dataOffset, DataDescriptor dataDescriptor);

	/// <summary>
	/// Decipher a value.
	/// </summary>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="dataDescriptor">
	/// The data descriptor.
	/// </param>
	/// <returns>
	/// The value, as an object
	/// </returns>
	object DecipherValue(ref long dataOffset, DataDescriptor dataDescriptor);
}
