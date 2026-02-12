using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The fraction collector struct.
/// </summary>
internal struct FractionCollectorStruct
{
	internal OldLcqEnums.FractionCollectorChoice FcChoice;

	internal int TriggerFractionCollector;

	internal double IntensityThreshold;
}
