using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The audit trail from legacy LCQ files
/// </summary>
internal sealed class AuditTrail : IRawObjectBase
{
	public const string AuditTrailStreamName = "AuditData";

	/// <summary>
	/// Gets or sets the audit data info.
	/// </summary>
	public AuditData[] AuditDataInfo { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.AuditTrail" /> class.
	/// </summary>
	public AuditTrail()
	{
		AuditDataInfo = Array.Empty<AuditData>();
	}

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
		AuditDataInfo = viewer.LoadRawFileObjectArray<AuditData>(fileRevision, ref startPos);
		return startPos - dataOffset;
	}
}
