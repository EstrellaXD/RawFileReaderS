using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determines is the peak is rising, or inverted
/// </summary>
[DataContract]
public enum PeakDirection
{
	/// <summary>
	/// Peak rises, like a mountain
	/// </summary>
	[EnumMember]
	Positive,
	/// <summary>
	/// Peak is inverted (like a valley)
	/// </summary>
	[EnumMember]
	Negative
}
