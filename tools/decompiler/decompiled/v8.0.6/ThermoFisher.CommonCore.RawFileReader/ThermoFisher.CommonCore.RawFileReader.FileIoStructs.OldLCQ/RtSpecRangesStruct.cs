namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time spectrum ranges struct, from legacy LCQ files.
/// </summary>
internal struct RtSpecRangesStruct
{
	internal double MassRangeLow;

	internal double MassRangeHigh;

	internal bool FixScale;

	internal double ScaleVal;

	internal bool XstartAsterisk;

	internal bool XendAsterisk;
}
