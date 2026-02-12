using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// An algorithm for peak detection.
/// </summary>
[DataContract]
public enum PeakDetector
{
	/// <summary>
	/// The genesis peak detection algorithm.
	/// </summary>
	[EnumMember]
	Genesis,
	/// <summary>
	/// The ICIS peak detection algorithm
	/// </summary>
	[EnumMember]
	ICIS,
	/// <summary>
	/// The avalon peak detection algorithm
	/// </summary>
	[EnumMember]
	Avalon,
	/// <summary>
	/// The parameterless peak detection algorithm
	/// </summary>
	[EnumMember]
	PPD
}
