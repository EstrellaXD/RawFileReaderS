using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The GC method, from legacy LCQ files.
/// </summary>
internal sealed class GcMethod : IRawObjectBase
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
		int count = Marshal.SizeOf(typeof(InstMethodGcStruct));
		viewer.ReadBytesExt(ref startPos, count);
		viewer.LoadRawFileObjectExt(() => new TemperatureTable(), fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt(() => new TemperatureTable(), fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt(() => new TemperatureTable(), fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt(() => new TemperatureTable(), fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt(() => new TemperatureTable(), fileRevision, ref startPos);
		viewer.ReadIntsExt(ref startPos);
		viewer.ReadDoublesExt(ref startPos);
		viewer.ReadIntsExt(ref startPos);
		return startPos - dataOffset;
	}
}
