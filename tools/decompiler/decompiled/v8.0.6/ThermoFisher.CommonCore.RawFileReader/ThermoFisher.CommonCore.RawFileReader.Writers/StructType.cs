namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The structure type.
/// </summary>
internal enum StructType
{
	/// <summary>
	/// The generic data item structure type.
	/// </summary>
	GenericDataItemStructType,
	/// <summary>
	/// The raw file info structure type.
	/// </summary>
	RawFileInfoStructType,
	/// <summary>
	/// The run header structure type.
	/// </summary>
	RunHeaderStructType,
	/// <summary>
	/// The file header structure type.
	/// </summary>
	FileHeaderStructType,
	/// <summary>
	/// The sequence row info structure type.
	/// </summary>
	SeqRowInfoStructType,
	/// <summary>
	/// The UV scan index structure type.
	/// </summary>
	UvScanIndexStructType,
	/// <summary>
	/// The instrument id info structure type.
	/// </summary>
	InstIdInfoStructType,
	/// <summary>
	/// The auto sampler config structure type.
	/// </summary>
	AutoSamplerConfigStructType,
	/// <summary>
	/// The ASR profile index structure type.
	/// </summary>
	AsrProfileIndexStructType,
	/// <summary>
	/// The profile data packet 64 type.
	/// </summary>
	ProfileDataPacket64Type,
	/// <summary>
	/// The mass spec reaction structure type
	/// </summary>
	MsReactionStructType,
	/// <summary>
	/// The mass spec reaction struct01 type
	/// </summary>
	MsReactionStruct01Type,
	/// <summary>
	/// The mass spec reaction struct02 type
	/// </summary>
	MsReactionStruct02Type,
	/// <summary>
	/// The mass spec reaction struct03 type
	/// </summary>
	MsReactionStruct03Type,
	/// <summary>
	/// The high resource spec type structure type
	/// </summary>
	HighResSpTypeStructType,
	/// <summary>
	/// The low resource spec type structure type
	/// </summary>
	LowResSpTypeStructType,
	/// <summary>
	/// The buffer information structure type
	/// </summary>
	BufferInfoStructType,
	/// <summary>
	/// The sequence information type
	/// </summary>
	SequenceInfoType,
	/// <summary>
	/// The scan event information structure
	/// </summary>
	ScanEventInfoStructType,
	/// <summary>
	/// The scan index structure type
	/// </summary>
	ScanIndexStructType,
	/// <summary>
	/// The noise information packet structure type
	/// </summary>
	NoiseInfoPacketStructType,
	/// <summary>
	/// The packet header structure type
	/// </summary>
	PacketHeaderStructType,
	/// <summary>
	/// The profile segment structure type
	/// </summary>
	ProfileSegmentStructType,
	/// <summary>
	/// The audit data structure type
	/// </summary>
	AuditDataStructType,
	/// <summary>
	/// The standard accuracy struct type.
	/// </summary>
	StandardAccuracyStructType,
	/// <summary>
	/// The virtual controller information structure
	/// </summary>
	VirtualControllerInfoStruct,
	/// <summary>
	/// The end of type.
	/// </summary>
	EndOfType
}
