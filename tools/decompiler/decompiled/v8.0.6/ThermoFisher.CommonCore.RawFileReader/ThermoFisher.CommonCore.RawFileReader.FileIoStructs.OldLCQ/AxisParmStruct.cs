using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The axis parameters struct, from legacy LCQ files
/// </summary>
internal struct AxisParmStruct
{
	internal OldLcqEnums.ShowAxisLabel ShowLabel;

	internal bool OffsetAxis;

	internal double OffsetAmt;
}
