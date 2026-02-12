using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanEventInfo;

/// <summary>
/// The scan event info struct version 54.
/// </summary>
internal struct ScanEventInfoStruct54
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

	/// <summary>
	///     Indicates how source fragmentation values are interpreted
	/// </summary>
	internal byte SourceFragmentationType;

	/// <summary>
	///     Scan Type Index indicates the segment/scan event for this filter scan event.
	///     HIWORD == segment, LOWORD == scan type
	/// </summary>
	internal int ScanTypeIndex;

	internal byte Wideband;

	/// <summary>
	///     Will be translated to Scan Filter's Accurate Mass enumeration.
	/// </summary>
	internal ScanFilterEnums.ScanEventAccurateMassTypes AccurateMassTypesType;

	internal byte MassAnalyzerType;

	internal byte SectorScan;

	internal byte Lock;

	internal byte FreeRegion;

	internal byte Ultra;

	internal byte Enhanced;

	internal byte MultiPhotonDissociationType;

	internal double MultiPhotonDissociation;

	internal byte ElectronCaptureDissociationType;

	internal double ElectronCaptureDissociation;

	internal byte PhotoIonization;
}
