using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The real time chromatogram label options from legacy LCQ files.
/// </summary>
internal sealed class RtChroLabel : IRawObjectBase
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
		if (fileRevision < 15)
		{
			viewer.ReadPreviousRevisionAndConvertExt<RtChroLabelStruct, RtChroLabelStruct1>(ref startPos);
		}
		else if (fileRevision < 52)
		{
			viewer.ReadPreviousRevisionAndConvertExt<RtChroLabelStruct, RtChroLabelStruct51>(ref startPos);
		}
		else
		{
			viewer.ReadStructureExt<RtChroLabelStruct>(ref startPos);
		}
		return startPos - dataOffset;
	}
}
