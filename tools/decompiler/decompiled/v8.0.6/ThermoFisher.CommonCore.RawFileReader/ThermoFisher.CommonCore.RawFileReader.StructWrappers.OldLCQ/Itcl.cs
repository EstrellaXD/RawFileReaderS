using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The ITCL settings, for legacy LCQ files.
/// </summary>
internal sealed class Itcl : IRawObjectBase
{
	/// <summary>
	/// Gets the acquisition time.
	/// </summary>
	public double AcquisitionTime { get; private set; }

	/// <summary>
	/// Gets the tune method.
	/// </summary>
	public string TuneMethod { get; private set; }

	/// <summary>
	/// Gets the ITCL procedure.
	/// </summary>
	public string ItclProcedure { get; private set; }

	/// <summary>
	/// Gets the MS segments.
	/// </summary>
	public MsSegment[] MsSegments { get; private set; }

	/// <summary>
	/// Gets the variable lists.
	/// </summary>
	public VariableList[] VariableLists { get; private set; }

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
		AcquisitionTime = viewer.ReadDoubleExt(ref startPos);
		TuneMethod = viewer.ReadStringExt(ref startPos);
		ItclProcedure = viewer.ReadStringExt(ref startPos);
		MsSegments = viewer.LoadRawFileObjectArray<MsSegment>(fileRevision, ref startPos);
		VariableLists = viewer.LoadRawFileObjectArray<VariableList>(fileRevision, ref startPos);
		return startPos - dataOffset;
	}
}
