using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time chromatogram other struct, from legacy LCQ files
/// </summary>
internal struct RtChroOtherStruct
{
	internal double TimeRangeStart;

	internal double TimeRangeEnd;

	internal bool SmoothingEnabled;

	internal OldLcqEnums.SmoothingTypes SmoothType;

	internal int SmoothingPts;

	internal uint BackDropColor;
}
