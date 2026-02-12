using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determines how the ICIS peak detector  determines which signals are noise.
/// </summary>
[DataContract]
public enum IcisNoiseType
{
	/// <summary>
	/// A single pass algorithm to determine the noise level. 
	/// </summary>
	[EnumMember]
	Incos,
	/// <summary>
	/// A multiple pass algorithm to determine the noise level.
	/// In general, this algorithm is more accurate in analyzing the noise than the INCOS Noise algorithm,
	/// but it takes longer. 
	/// </summary>
	[EnumMember]
	Repetitive
}
