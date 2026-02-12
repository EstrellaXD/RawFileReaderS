using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Features which can be set per peak, such as "reference compound"
/// </summary>
[Flags]
[DataContract]
public enum PeakOptions
{
	/// <summary>No peak flags</summary>
	[EnumMember]
	None = 0,
	/// <summary>Saturation flag (signal over ADC limit)</summary>
	[EnumMember]
	Saturated = 1,
	/// <summary>Fragmentation flag (peak split by centroider)</summary>
	[EnumMember]
	Fragmented = 2,
	/// <summary>Merged flag (peaks combined by centroider)</summary>
	[EnumMember]
	Merged = 4,
	/// <summary>Exception peak flag (part of reference, but not used by calibration)</summary>
	[EnumMember]
	Exception = 8,
	/// <summary>Reference peak flag (hi-res internal reference compound)</summary>
	[EnumMember]
	Reference = 0x10,
	/// <summary>Mathematically modified packet</summary>
	[EnumMember]
	Modified = 0x20,
	/// <summary>High resolution SIM lock mass</summary>
	[EnumMember]
	LockPeak = 0x40
}
