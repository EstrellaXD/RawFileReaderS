namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;

/// <summary>
/// The scan index structure - current version.
/// </summary>
internal struct ScanIndexStruct
{
	internal uint DataSize;

	internal int TrailerOffset;

	/// <summary>
	///     HIWORD == segment, LOWORD == scan type
	/// </summary>
	internal int ScanTypeIndex;

	internal int ScanNumber;

	/// <summary>
	///     HIWORD == SIScanData (optional), LOWORD == scan type
	/// </summary>
	internal uint PacketType;

	internal int NumberPackets;

	internal double StartTime;

	internal double TIC;

	internal double BasePeakIntensity;

	internal double BasePeakMass;

	internal double LowMass;

	internal double HighMass;

	internal long DataOffset;

	/// <summary>
	///     Cycle number used to associate events within a scan event cycle.
	///     For example, on the first cycle of scan events, all the events
	///     would set this to '1'. On the second cycle, all the events would
	///     set this to '2'. This field must be set by devices if supporting
	///     compound names for filtering. However, it may be set in all
	///     acquisitions to help processing algorithms.
	/// </summary>
	internal int CycleNumber;
}
