using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The LC method (from legacy LCQ files)
/// </summary>
internal sealed class LcMethod : IRawObjectBase
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
		int count = Marshal.SizeOf(typeof(InstMethodLcStruct));
		viewer.ReadBytesExt(ref startPos, count);
		viewer.LoadRawFileObjectArray<LcTable>(fileRevision, ref startPos);
		viewer.LoadRawFileObjectArray<LcDetector>(fileRevision, ref startPos);
		viewer.LoadRawFileObjectArray<LcEvent>(fileRevision, ref startPos);
		return startPos - dataOffset;
	}
}
