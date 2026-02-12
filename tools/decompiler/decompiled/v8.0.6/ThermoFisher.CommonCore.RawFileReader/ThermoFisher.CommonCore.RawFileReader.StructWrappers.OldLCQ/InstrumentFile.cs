using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The instrument file, from old LCQ
/// </summary>
internal sealed class InstrumentFile : IRawObjectBase
{
	/// <summary>
	/// Gets the audit trail information.
	/// </summary>
	public AuditTrail AuditTrailInfo { get; private set; }

	/// <summary>
	/// Gets the instrument configuration.
	/// </summary>
	public InstrumentConfig InstConfig { get; private set; }

	/// <summary>
	/// Gets the MS method information.
	/// </summary>
	public MsMethod MsMethodInfo { get; private set; }

	/// <summary>
	/// Gets the real time chromatogram information.
	/// </summary>
	public RealTimeChroInformation RealTimeChroInfo { get; private set; }

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
		viewer.LoadRawFileObjectExt(() => new FileHeader(), fileRevision, ref startPos);
		InstConfig = viewer.LoadRawFileObjectExt(() => new InstrumentConfig(), fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt(() => new InstrumentRunInfo(), fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt(() => new InletMethods(), fileRevision, ref startPos);
		MsMethodInfo = viewer.LoadRawFileObjectExt(() => new MsMethod(), fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt(() => new RealTimeSpecInformation(), fileRevision, ref startPos);
		RealTimeChroInfo = viewer.LoadRawFileObjectExt(() => new RealTimeChroInformation(), fileRevision, ref startPos);
		RealTimeChroInfo.RtChroNormInfo.FixScale = true;
		AuditTrailInfo = viewer.LoadRawFileObjectExt(() => new AuditTrail(), fileRevision, ref startPos);
		return startPos - dataOffset;
	}
}
