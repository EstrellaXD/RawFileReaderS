using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time spec other struct, for legacy LCQ files.
/// </summary>
internal struct RtSpecOtherStruct
{
	internal bool SmoothingEnabled;

	internal OldLcqEnums.SmoothingTypes SmoothType;

	internal int SmoothingPts;

	internal bool BkgRgn1Active;

	internal double BkgRgn1LoTime;

	internal double BkgRgn1HiTime;

	internal bool BkgRgn2Active;

	internal double BkgRgn2LoTime;

	internal double BkgRgn2HiTime;

	internal bool Average;
}
