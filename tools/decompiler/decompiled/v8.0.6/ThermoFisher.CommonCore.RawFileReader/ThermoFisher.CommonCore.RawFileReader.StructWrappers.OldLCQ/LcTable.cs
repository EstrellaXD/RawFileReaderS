using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The LC table, from legacy LCQ files
/// </summary>
internal sealed class LcTable : IRawObjectBase
{
	/// <summary>
	/// Gets the start percent.
	/// </summary>
	public int[] StartPercent { get; private set; }

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
		int count = Marshal.SizeOf(typeof(LcTableStruct));
		viewer.ReadBytesExt(ref startPos, count);
		int num = viewer.ReadIntExt(ref startPos);
		StartPercent = new int[num];
		for (int i = 0; i < num; i++)
		{
			StartPercent[i] = viewer.ReadIntExt(ref startPos);
		}
		return startPos - dataOffset;
	}
}
