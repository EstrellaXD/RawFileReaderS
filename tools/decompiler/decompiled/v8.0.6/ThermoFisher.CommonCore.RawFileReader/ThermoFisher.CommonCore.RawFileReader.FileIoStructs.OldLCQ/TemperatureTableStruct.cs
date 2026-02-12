namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The temperature table struct, from legacy LCQ files.
/// </summary>
internal struct TemperatureTableStruct
{
	internal double Rate;

	internal double Time;

	internal double Hold;

	internal int StartTemp;

	internal int EndTemp;
}
