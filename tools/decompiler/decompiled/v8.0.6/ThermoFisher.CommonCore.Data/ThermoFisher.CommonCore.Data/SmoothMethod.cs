using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// This specifies a smoothing envelope shape.
/// </summary>
[DataContract]
public enum SmoothMethod
{
	/// <summary>
	/// Smooth with equal weights.
	/// </summary>
	[EnumMember]
	MovingMean,
	/// <summary>
	/// Smooth with a gaussian weighting envelope.
	/// </summary>
	[EnumMember]
	Gaussian
}
