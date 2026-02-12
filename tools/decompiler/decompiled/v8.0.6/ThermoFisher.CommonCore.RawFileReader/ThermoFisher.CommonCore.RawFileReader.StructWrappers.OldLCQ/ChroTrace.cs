using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The chromatogram trace, from legacy LCQ files.
/// </summary>
internal sealed class ChroTrace : IRawObjectBase
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
		viewer.ReadPreviousRevisionAndConvertExt<ChroTraceStruct, ChroTraceStruct1>(ref startPos);
		MassRangeStruct.LoadArray(viewer, ref startPos);
		viewer.LoadRawFileObjectExt(() => new Filter(), fileRevision, ref startPos);
		if (fileRevision >= 25)
		{
			MassRangeStruct.LoadArray(viewer, ref startPos);
		}
		return startPos - dataOffset;
	}
}
