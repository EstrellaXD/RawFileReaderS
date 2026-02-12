using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.FilterInfo;

/// <summary>
/// The filter info struct 54.
/// </summary>
internal struct FilterInfoStruct54
{
	internal ScanFilterEnums.ScanDataTypes ScanData;

	internal ScanFilterEnums.PolarityTypes Polarity;

	internal ScanFilterEnums.MSOrderTypes MSOrder;

	internal ScanFilterEnums.IsDependent Dependent;

	internal ScanFilterEnums.OnOffTypes SourceCID;

	internal ScanFilterEnums.ScanTypes ScanType;

	internal bool IsComplete;

	internal ScanFilterEnums.OnOffTypes TurboScan;

	internal ScanFilterEnums.IonizationModes IonizationMode;

	internal ScanFilterEnums.OnOffTypes Corona;

	internal ScanFilterEnums.OnAnyOffTypes DetectorState;

	internal double DetectorValue;

	internal ScanFilterEnums.VoltageTypes SourceCIDType;

	internal int ScanTypeIndex;

	internal ScanFilterEnums.OffOnTypes Wideband;

	internal ScanFilterEnums.AccurateMassTypes AccurateMass;

	internal ScanFilterEnums.MassAnalyzerTypes MassAnalyzer;

	internal ScanFilterEnums.SectorScans SectorScan;

	internal ScanFilterEnums.OnOffTypes Lock;

	internal ScanFilterEnums.FreeRegions FreeRegion;

	internal ScanFilterEnums.OnOffTypes Ultra;

	internal ScanFilterEnums.OnOffTypes Enhanced;

	internal ScanFilterEnums.OnAnyOffTypes MultiPhotonDissociationState;

	internal double MultiPhotonDissociationValue;

	internal ScanFilterEnums.OnAnyOffTypes ElectronCaptureDissociationState;

	internal double ElectronCaptureDissociationValue;

	internal ScanFilterEnums.OnOffTypes PhotoIonization;
}
