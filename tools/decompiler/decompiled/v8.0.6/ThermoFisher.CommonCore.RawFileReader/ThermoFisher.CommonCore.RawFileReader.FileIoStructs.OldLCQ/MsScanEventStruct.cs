using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The mass spec scan event struct.
/// </summary>
internal struct MsScanEventStruct
{
	internal ScanFilterEnums.PolarityTypes Polarity;

	internal OldLcqEnums.MsScanMode ScanMode;

	internal OldLcqEnums.MsScanType ScanType;

	internal int DepDataFlag;

	internal int MicroScans;

	internal int StoreDepData;

	internal double Duration;

	internal bool UseSourceCID;

	internal bool TurboScanMode;

	internal double CIDPercent;
}
