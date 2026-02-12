using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.FilterInfo;

/// <summary>
/// The filter info struct 50.
/// </summary>
internal struct FilterInfoStruct50
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
}
