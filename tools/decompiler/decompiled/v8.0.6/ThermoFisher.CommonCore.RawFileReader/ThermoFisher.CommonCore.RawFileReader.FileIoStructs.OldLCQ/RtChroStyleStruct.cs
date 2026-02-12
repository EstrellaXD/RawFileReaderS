using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time chromatogram style struct, from legacy LCQ files
/// </summary>
internal struct RtChroStyleStruct
{
	internal bool Overlay;

	internal double Elevation;

	internal double Skew;

	internal bool DrawBackdrop;

	internal OldLcqEnums.Style ChroStyle;
}
