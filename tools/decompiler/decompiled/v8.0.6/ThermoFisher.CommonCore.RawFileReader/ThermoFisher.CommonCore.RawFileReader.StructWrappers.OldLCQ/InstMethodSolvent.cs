using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The instrument method solvent, from legacy LCQ files.
/// </summary>
internal sealed class InstMethodSolvent : IRawObjectBase
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
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		viewer.ReadStringExt(ref startPos);
		viewer.ReadStringExt(ref startPos);
		viewer.ReadStringExt(ref startPos);
		viewer.ReadStringExt(ref startPos);
		return startPos - dataOffset;
	}
}
