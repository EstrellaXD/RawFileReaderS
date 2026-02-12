namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// Base of objects which can be loaded from a raw file.
/// Defines ability to load an object.
/// </summary>
internal interface IRawObjectBase
{
	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	long Load(IMemoryReader viewer, long dataOffset, int fileRevision);
}
