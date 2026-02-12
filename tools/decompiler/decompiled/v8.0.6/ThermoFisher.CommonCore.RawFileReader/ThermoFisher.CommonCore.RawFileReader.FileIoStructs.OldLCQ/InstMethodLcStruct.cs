namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The instrument method LC struct, from legacy LCQ files.
/// </summary>
internal struct InstMethodLcStruct
{
	internal double TotalRunTime;

	internal double Det1TimeConstant;

	internal double Det2TimeConstant;

	internal double SpargeRate;

	internal double LowPressureUnit;

	internal double HighPressureUnit;

	internal double ColumnTemp;

	internal double ColumnTempLimit;

	internal short SpargeSolventA;

	internal short SpargeSolventB;

	internal short SpargeSolventC;

	internal short SpargeSolventD;

	internal short DetectorAutoZero;
}
