using System;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The xcalibur display (peak labeling) flags.
/// </summary>
[Flags]
internal enum XcaliburDisplayFlags : uint
{
	/// <summary>
	/// label with retention time.
	/// </summary>
	LabelWithRetentionTime = 1u,
	/// <summary>
	/// label with scan number.
	/// </summary>
	LabelWithScanNumber = 2u,
	/// <summary>
	/// The label with area.
	/// </summary>
	LabelWithArea = 4u,
	/// <summary>
	/// label with base peak.
	/// </summary>
	LabelWithBasePeak = 8u,
	/// <summary>
	/// label with height.
	/// </summary>
	LabelWithHeight = 0x10u,
	/// <summary>
	/// label with internal standard response.
	/// </summary>
	LabelWithIstdResp = 0x20u,
	/// <summary>
	/// label with signal to noise.
	/// </summary>
	LabelWithSignalToNoise = 0x40u,
	/// <summary>
	/// label with saturation.
	/// </summary>
	LabelWithSaturation = 0x80u,
	/// <summary>
	/// label should be boxed.
	/// </summary>
	LabelBoxed = 0x40000000u,
	/// <summary>
	/// label should be rotated.
	/// </summary>
	LabelRotated = 0x80000000u
}
