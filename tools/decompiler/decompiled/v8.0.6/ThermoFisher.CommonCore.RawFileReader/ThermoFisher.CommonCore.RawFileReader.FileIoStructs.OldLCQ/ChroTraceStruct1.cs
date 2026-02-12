using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The chromatogram trace struct 1, from legacy LCQ files.
/// </summary>
internal struct ChroTraceStruct1
{
	internal bool Enable;

	internal OldLcqEnums.ChroTraceTypes TraceType;

	internal int MassRanges;

	internal double RangeScale;

	internal double Delay;

	internal uint TraceColor;
}
