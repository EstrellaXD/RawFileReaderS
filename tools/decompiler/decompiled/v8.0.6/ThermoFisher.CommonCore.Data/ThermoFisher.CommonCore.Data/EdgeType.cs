using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Defines why the peak detector has determined that a peak has started or ended
/// </summary>
[DataContract]
public enum EdgeType
{
	/// <summary>
	/// edge intensity intercepted baseline + 05.*noise value.
	/// </summary>
	[EnumMember]
	Base,
	/// <summary>
	/// edge fits at least one of the three valley criteria.
	/// </summary>
	[EnumMember]
	Valley,
	/// <summary>
	/// reserved for manually entered integration limits. 
	/// </summary>
	[EnumMember]
	Manual,
	/// <summary>
	/// edge reached the peak constraint percentage height.
	/// </summary>
	[EnumMember]
	Stripe,
	/// <summary>
	/// edge reached the tailing factor RT limit before the STRIPE height.
	/// </summary>
	[EnumMember]
	Tail,
	/// <summary>
	/// Tilt is as in a pinball machine, something bad happened,
	/// such as hitting the edge of the survey window.
	/// </summary>
	[EnumMember]
	Tilt
}
