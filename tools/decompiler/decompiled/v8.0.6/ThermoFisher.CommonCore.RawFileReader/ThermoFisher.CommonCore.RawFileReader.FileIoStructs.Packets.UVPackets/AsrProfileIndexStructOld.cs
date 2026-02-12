namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.UVPackets;

/// <summary>
///     The Adjustable Scan Rate profile packet index structure.
/// </summary>
internal struct AsrProfileIndexStructOld
{
	internal int IsValidScan;

	internal uint WavelengthStart;

	internal uint WavelengthEnd;

	internal uint WavelengthStep;

	internal double TimeWavelengthStart;

	internal double TimeWavelengthEnd;

	internal double TimeWavelengthStep;

	internal double TimeWavelengthExpected;

	internal double AUOffset;

	internal double AUScale;

	internal uint NumberOfPackets;

	internal int DataPosition;
}
