namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// Instrument Status
/// </summary>
internal struct InstStatusStruct
{
	internal AsStatusStruct AutoSamplerStatus;

	internal LcStatusStruct LcStatus;

	internal UvStatusStruct UvStatus;

	internal SpStatusStruct SyringePumpStatus;

	internal TpStatusStruct TurboPumpStatus;

	internal int AnalState;

	internal int NumDevice;

	internal ReadBackStruct ReadBacks;

	internal uint SysStatus;

	internal float RetentionTime;
}
