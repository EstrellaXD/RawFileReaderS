namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The LC table struct from legacy LCQ files
/// </summary>
internal struct LcTableStruct
{
	internal double Time;

	internal double FlowRate;

	internal int Curve;
}
