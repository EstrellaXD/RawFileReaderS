using System;
using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to a channel header (for either 2d or 3d data)
/// </summary>
public interface IChannelHeaderBase
{
	/// <summary>
	/// Gets the version of this object.
	/// This can be an empty string, for "default or initial version"
	/// </summary>
	string Version { get; }

	/// <summary>
	/// Gets the Name of the signal,
	/// such as "oven temperature" in "oven temperature °C"
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the unit (such as "°C" for degrees Celsius)
	/// </summary>
	string SignalUnit { get; }

	/// <summary>
	/// Gets suggested digits after the decimal point
	/// </summary>
	int DecimalPlaces { get; }

	/// <summary>
	/// Gets the anticipated range of signals
	/// </summary>
	IRangeAccess SignalRange { get; }

	/// <summary>
	/// Gets the name of the device that provided the signal
	/// </summary>
	string DeviceName { get; }

	/// <summary>
	/// Gets the units over which samples are taken (X axis scale)
	/// </summary>
	string SamplingUnits { get; }

	/// <summary>
	/// Defines additional properties for the channel
	/// </summary>
	IReadOnlyCollection<IChannelPropertyAccess> AdditionalProperties { get; }

	/// <summary>
	/// Gets a value indicating whether this data should be plotted as a continuous trace.
	/// This may not be true for diagnostic values with "On/Off" state.
	/// When not set, the value of the signal remains constant at the previous value sent, and 
	/// jumps instantly to the new value at a time point.
	/// </summary>
	bool IsContinuous { get; }

	/// <summary>
	/// Create the log headers for a generic log, which match this channel header
	/// </summary>
	/// <returns></returns>
	Tuple<IHeaderItem[], object[]> CreateLog();
}
