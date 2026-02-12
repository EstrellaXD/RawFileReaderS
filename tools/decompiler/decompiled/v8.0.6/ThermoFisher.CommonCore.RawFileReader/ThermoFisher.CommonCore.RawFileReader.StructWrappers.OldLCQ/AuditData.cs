using System;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The audit data, from legacy LCQ files.
/// </summary>
internal sealed class AuditData : IRawObjectBase
{
	private AuditDataStruct _auditDataStructInfo;

	private UserIdStamp _convertedDateTime;

	/// <summary>
	/// Gets or sets the comment.
	/// </summary>
	/// <value>
	/// The comment.
	/// </value>
	public string Comment { get; set; }

	/// <summary>
	/// Gets or sets the what changed.
	/// </summary>
	public long WhatChanged
	{
		get
		{
			return _auditDataStructInfo.WhatChanged;
		}
		set
		{
			_auditDataStructInfo.WhatChanged = value;
		}
	}

	/// <summary>
	/// Gets or sets the time changed.
	/// </summary>
	public DateTime TimeChanged
	{
		get
		{
			return _convertedDateTime.DateAndTime;
		}
		set
		{
			_auditDataStructInfo.Time.TimeStamp = TypeConverters.DateTimeToFileTime(value);
		}
	}

	/// <summary>
	/// Gets the audit data struct.
	/// </summary>
	public AuditDataStruct AuditDataStruct => _auditDataStructInfo;

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">The viewer (memory map into file).</param>
	/// <param name="dataOffset">The data offset (into the memory map).</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>The number of bytes read </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		if (fileRevision < 25)
		{
			AuditDataStruct1 auditDataStruct = viewer.ReadStructureExt<AuditDataStruct1>(ref startPos);
			_auditDataStructInfo.Time = auditDataStruct.Time;
			_convertedDateTime = new UserIdStamp(_auditDataStructInfo.Time);
			_auditDataStructInfo.WhatChanged = auditDataStruct.WhatChanged;
		}
		if (fileRevision >= 66)
		{
			AuditDataStruct auditDataStruct2 = viewer.ReadStructureExt<AuditDataStruct>(ref startPos);
			_auditDataStructInfo.Time = auditDataStruct2.Time;
			_convertedDateTime = new UserIdStamp(_auditDataStructInfo.Time);
			_auditDataStructInfo.WhatChanged = auditDataStruct2.WhatChanged;
		}
		Comment = viewer.ReadStringExt(ref startPos);
		return startPos - dataOffset;
	}
}
