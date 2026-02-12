using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The MS dependent data struct 2 for legacy LCQ files.
/// </summary>
internal struct MsDependentDataStruct2
{
	internal OldLcqEnums.ModeLargest Mode;

	internal int Largest;

	internal double IsolationWidth;
}
