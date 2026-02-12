using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time spec info struct.
/// </summary>
internal struct RealTimeSpecInfoStruct
{
	internal OldLcqEnums.SpectrumStyle SpectrumStyle;

	internal bool Split;

	internal int Splits;
}
