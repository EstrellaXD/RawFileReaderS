namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// Defines a method of packing log records into a byte array
/// </summary>
public interface IGenericDataPack
{
	/// <summary>
	/// convert data entry to byte array.
	/// </summary>
	/// <param name="data">The log entries. </param>
	/// <param name="buffer">The buffer for storing the log entries in byte array.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of log entries do not match the header definitions</exception>
	void ConvertDataEntryToByteArray(object[] data, out byte[] buffer);

	/// <summary>
	/// convert data entry to byte array.
	/// </summary>
	/// <param name="data">The log entries. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of log entries do not match the header definitions</exception>
	/// <returns>the log entries as a byte array</returns>
	byte[] ConvertDataEntryToByteArray(object[] data);
}
