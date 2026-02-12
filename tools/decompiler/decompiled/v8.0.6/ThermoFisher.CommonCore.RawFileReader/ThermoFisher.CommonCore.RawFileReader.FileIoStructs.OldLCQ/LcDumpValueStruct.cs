using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The LC dump value struct.
/// </summary>
internal struct LcDumpValueStruct
{
	internal OldLcqEnums.DivertBetweenRuns BetweenRuns;

	internal bool UseDumpValve;

	internal double StartFlowIntoMS;

	internal double StopFlowIntoMS;
}
