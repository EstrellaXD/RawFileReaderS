namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The LC detector struct, from legacy LCQ files
/// </summary>
internal struct LcDetectorStruct
{
	internal double Time;

	internal double Absorbance;

	internal int Channel;

	internal int Wavelength;
}
