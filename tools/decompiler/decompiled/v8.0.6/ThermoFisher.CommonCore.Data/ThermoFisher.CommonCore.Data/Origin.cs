using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Treatment of calibration curve origin
/// </summary>
[DataContract]
public enum Origin
{
	/// <summary>
	/// The origin is included (as an extra point) on the calibration curve.
	/// </summary>
	[EnumMember]
	Include,
	/// <summary>
	/// The origin is not added to the curve
	/// </summary>
	[EnumMember]
	Excluded,
	/// <summary>
	/// The curve is forced through the origin
	/// </summary>
	[EnumMember]
	Force
}
