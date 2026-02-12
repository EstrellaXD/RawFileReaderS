using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determines how void time is calculated
/// </summary>
[DataContract]
public enum VoidMethod
{
	/// <summary>
	/// A value is specified
	/// </summary>
	[EnumMember]
	VoidTimeByValue,
	/// <summary>
	/// The time of the first peak in the void chromatogram is used.
	/// </summary>
	[EnumMember]
	VoidTimeFirstPeak
}
