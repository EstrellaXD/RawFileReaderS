using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determines how signal to noise is reported
/// </summary>
[DataContract]
public enum NoiseClassification
{
	/// <summary>
	/// No noise value has been calculated
	/// </summary>
	[EnumMember]
	NotAvailable,
	/// <summary>
	/// No detected noise
	/// </summary>
	[EnumMember]
	Infinite,
	/// <summary>
	/// Use SignalToNoise property to get the value
	/// </summary>
	[EnumMember]
	Value
}
