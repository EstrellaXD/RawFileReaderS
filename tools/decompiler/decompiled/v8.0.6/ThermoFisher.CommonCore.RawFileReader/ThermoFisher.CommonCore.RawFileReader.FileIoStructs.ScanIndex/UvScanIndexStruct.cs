namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;

/// <summary>
///     The UV scan index structure - current version.
/// </summary>
internal struct UvScanIndexStruct
{
	internal uint DataOffset32Bit;

	internal int ScanNumber;

	internal int PacketType;

	internal int NumberPackets;

	internal int NumberOfChannels;

	internal int UniformTime;

	internal double Frequency;

	internal double StartTime;

	internal double ShortWavelength;

	internal double LongWavelength;

	internal double TIC;

	internal long DataOffset;
}
