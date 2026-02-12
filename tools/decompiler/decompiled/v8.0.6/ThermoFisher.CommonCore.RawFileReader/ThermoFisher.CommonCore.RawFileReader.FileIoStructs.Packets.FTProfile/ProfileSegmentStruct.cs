namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.FTProfile;

/// <summary>
///     The profile segment structure.
/// </summary>
internal struct ProfileSegmentStruct
{
	internal double BaseAbscissa;

	internal double AbscissaSpacing;

	internal uint NumSubSegments;

	internal uint NumExpandedWords;
}
