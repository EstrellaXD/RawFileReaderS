using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Represents the results of an optional test.
/// </summary>
[DataContract]
public enum ResultStatus
{
	/// <summary>
	/// Failed Test
	/// </summary>
	[EnumMember]
	Failed,
	/// <summary>
	/// Passed Test
	/// </summary>
	[EnumMember]
	Passed,
	/// <summary>
	/// Not Tested 
	/// </summary>
	[EnumMember]
	NotTested
}
