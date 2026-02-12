namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanEventInfo;

/// <summary>
///     The scan event info struct version 2.
/// </summary>
internal struct ScanEventInfoStruct2
{
	internal byte IsValid;

	/// <summary>
	///     Set to TRUE if trailer scan event should be used.
	/// </summary>
	internal byte IsCustom;

	internal byte Corona;

	/// <summary>
	///     Set to SFDetectorValid if detector value is valid.
	/// </summary>
	internal byte Detector;

	internal byte Polarity;

	internal byte ScanDataType;

	internal byte MSOrder;

	internal byte ScanType;

	internal byte SourceFragmentation;

	internal byte TurboScan;

	internal byte DependentData;

	internal byte IonizationMode;

	internal double DetectorValue;
}
