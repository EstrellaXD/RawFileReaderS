namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time spectrum label struct, from legacy LCQ files
/// </summary>
internal struct RtSpecLabelStruct
{
	internal bool LabelMass;

	internal int LabelDecimals;

	internal bool LabelFlags;

	internal bool LabelOffset;

	internal bool Rotate;

	internal bool Boxed;

	internal double Digits;

	internal double LabelThreshold;
}
