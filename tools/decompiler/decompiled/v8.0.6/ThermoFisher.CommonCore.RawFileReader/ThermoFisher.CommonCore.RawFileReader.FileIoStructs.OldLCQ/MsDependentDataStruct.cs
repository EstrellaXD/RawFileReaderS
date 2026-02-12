using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The MS dependent data struct.
/// </summary>
internal struct MsDependentDataStruct
{
	internal OldLcqEnums.ModeLargest Mode;

	internal int Largest;

	internal double IsolationWidth;

	internal bool UsePreviousList;
}
