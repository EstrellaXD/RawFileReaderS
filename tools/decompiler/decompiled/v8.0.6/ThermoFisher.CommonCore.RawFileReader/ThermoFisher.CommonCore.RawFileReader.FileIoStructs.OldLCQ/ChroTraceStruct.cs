using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The chromatogram trace struct.
/// </summary>
internal struct ChroTraceStruct
{
	internal bool Enable;

	internal OldLcqEnums.ChroTraceTypes TraceType;

	internal double RangeScale;

	internal double Delay;

	internal uint TraceColor;

	internal OldLcqEnums.ChroTraceTypes TraceType2;

	internal int Operator;
}
