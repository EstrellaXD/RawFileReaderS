namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The instrument method GC struct from legacy LCQ files
/// </summary>
internal struct InstMethodGcStruct
{
	internal int DetectorTemp;

	internal int MaxColumnTemp;

	internal int MaxXferTemp;

	internal int MaxAux1Temp;

	internal int MaxAux2Temp;

	internal int MaxDetTemp;

	internal double StabilizationTime;

	internal double CoolantTimeout;

	internal int CoolantToCool;

	internal int CoolantToInj;
}
