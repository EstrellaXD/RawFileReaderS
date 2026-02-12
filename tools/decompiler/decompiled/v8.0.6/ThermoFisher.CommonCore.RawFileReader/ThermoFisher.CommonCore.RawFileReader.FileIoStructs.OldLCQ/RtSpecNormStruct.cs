using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The Real Time spectrum normalization struct.
/// </summary>
internal struct RtSpecNormStruct
{
	internal double YMin;

	internal double YMax;

	internal OldLcqEnums.NormalizeType NormType;

	internal bool ScaleAllSame;
}
