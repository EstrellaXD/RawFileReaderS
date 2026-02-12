using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The non ITCL struct.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
internal struct NonItclStruct
{
	internal double TotalAcqTime;

	internal double TotalScanTime;

	internal double PreSignalThreshold;

	internal OldLcqEnums.DataMode Mode;

	internal double SubtractedMass;

	internal int DefaultChargeState;

	internal double DepCollisionEnergy;

	internal double ParentSignalThreshold;

	internal double DepIsolationWidth;
}
