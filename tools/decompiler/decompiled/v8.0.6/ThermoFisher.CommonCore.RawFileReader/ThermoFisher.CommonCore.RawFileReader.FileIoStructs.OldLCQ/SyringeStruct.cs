using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The syringe struct.
/// </summary>
internal struct SyringeStruct
{
	internal double FlowRate;

	internal double Diameter;

	internal double Volume;

	internal double InnerDiameter;

	internal bool StopPumpAtEndOfRun;

	internal OldLcqEnums.SyringeBrand Brand;

	internal OldLcqEnums.SyringeUnit Unit;
}
