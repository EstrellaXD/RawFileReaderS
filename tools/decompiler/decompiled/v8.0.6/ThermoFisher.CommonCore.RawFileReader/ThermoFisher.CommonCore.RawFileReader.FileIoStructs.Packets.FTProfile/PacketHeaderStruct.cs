namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.FTProfile;

/// <summary>
/// The packet header structure for FT profile packets.
/// </summary>
internal struct PacketHeaderStruct
{
	internal uint NumSegments;

	internal uint NumProfileWords;

	internal uint NumCentroidWords;

	internal uint DefaultFeatureWord;

	internal uint NumNonDefaultFeatureWords;

	internal uint NumExpansionWords;

	internal uint NumNoiseInfoWords;

	internal uint NumDebugInfoWords;
}
