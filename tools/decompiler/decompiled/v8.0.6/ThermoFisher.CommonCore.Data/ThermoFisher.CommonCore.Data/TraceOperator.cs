using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Mode of handling multiple chromatogram traces (from the same filtered scans)
/// </summary>
[DataContract]
public enum TraceOperator
{
	/// <summary>
	/// There is only one chromatogram
	/// </summary>
	[EnumMember]
	None,
	/// <summary>
	/// The two chromatograms are summed
	/// </summary>
	[EnumMember]
	Plus,
	/// <summary>
	/// The second chromatogram is subtracted from the first
	/// </summary>
	[EnumMember]
	Minus
}
