using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time chromatogram norm struct.
/// </summary>
internal struct RtChroNormStruct
{
	internal double IntensityMin;

	internal double IntensityMax;

	internal OldLcqEnums.NormalizeType NormType;

	internal bool FixScale;
}
