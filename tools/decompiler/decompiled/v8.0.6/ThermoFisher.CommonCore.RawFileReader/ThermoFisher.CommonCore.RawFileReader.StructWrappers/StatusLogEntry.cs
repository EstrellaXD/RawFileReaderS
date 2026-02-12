using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
///     The status log entry which consists of the retention time and the value pairs.
/// </summary>
internal sealed class StatusLogEntry
{
	/// <summary>
	/// Gets the retention time.
	/// </summary>
	public double RetentionTime { get; private set; }

	/// <summary>
	/// Gets the value pairs.
	/// </summary>
	public List<LabelValuePair> ValuePairs { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.StatusLogEntry" /> class.
	/// </summary>
	/// <param name="time">
	/// The time.
	/// </param>
	/// <param name="valuePairs">
	/// The value pairs.
	/// </param>
	public StatusLogEntry(double time, List<LabelValuePair> valuePairs)
	{
		RetentionTime = time;
		ValuePairs = valuePairs;
	}
}
