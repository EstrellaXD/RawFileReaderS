using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determines how much of the the chromatogram is used.
/// Data outside of the calculated window is ignored
/// </summary>
[DataContract]
public enum WindowMode
{
	/// <summary>
	/// Do not restrict the chromatogram
	/// </summary>
	[EnumMember]
	NoWindow,
	/// <summary>
	/// The chromatogram processed is restricted by the baseline and noise window
	/// </summary>
	[EnumMember]
	Baseline
}
