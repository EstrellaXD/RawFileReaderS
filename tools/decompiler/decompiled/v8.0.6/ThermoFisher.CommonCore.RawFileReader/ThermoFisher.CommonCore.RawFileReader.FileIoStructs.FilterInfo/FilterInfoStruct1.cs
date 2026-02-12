using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.FilterInfo;

/// <summary>
/// The filter info struct 1.
/// </summary>
internal struct FilterInfoStruct1
{
	internal ScanFilterEnums.ScanDataTypes ScanData;

	internal ScanFilterEnums.PolarityTypes Polarity;

	internal ScanFilterEnums.MSOrderTypes MSOrder;

	internal ScanFilterEnums.IsDependent Dependent;

	internal ScanFilterEnums.OnOffTypes SourceCID;

	internal ScanFilterEnums.ScanTypes ScanType;

	internal bool IsComplete;
}
