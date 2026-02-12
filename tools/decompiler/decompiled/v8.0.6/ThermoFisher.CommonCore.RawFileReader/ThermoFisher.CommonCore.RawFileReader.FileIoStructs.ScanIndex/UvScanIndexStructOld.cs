namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;

/// <summary>
/// The scan index structure - old version.
/// </summary>
internal struct UvScanIndexStructOld
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
}
