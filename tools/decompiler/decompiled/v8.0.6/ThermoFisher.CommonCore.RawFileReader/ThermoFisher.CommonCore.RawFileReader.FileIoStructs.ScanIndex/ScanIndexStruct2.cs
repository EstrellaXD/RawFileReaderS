namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;

/// <summary>
/// The scan index structure - version 2.
/// </summary>
internal struct ScanIndexStruct2
{
	internal uint DataOffset32Bit;

	internal int TrailerOffset;

	/// <summary>
	///     HIWORD == segment, LOWORD == scan type
	/// </summary>
	internal int ScanTypeIndex;

	internal int ScanNumber;

	/// <summary>
	///     HIWORD == SIScanData (optional), LOWORD == scan type
	/// </summary>
	internal int PacketType;

	internal int NumberPackets;

	internal double StartTime;

	internal double TIC;

	internal double BasePeakIntensity;

	internal double BasePeakMass;

	internal double LowMass;

	internal double HighMass;

	internal long DataOffset;
}
