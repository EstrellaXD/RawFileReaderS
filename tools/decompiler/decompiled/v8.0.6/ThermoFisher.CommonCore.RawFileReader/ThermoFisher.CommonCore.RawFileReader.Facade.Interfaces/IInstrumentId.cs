using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
///     The Instrument Id interface.
/// </summary>
internal interface IInstrumentId : IRealTimeAccess, IDisposable
{
	/// <summary>
	///     Gets or sets the absorbance unit.
	/// </summary>
	AbsorbanceUnits AbsorbanceUnit { get; set; }

	/// <summary>
	///     Gets the channel labels.
	/// </summary>
	List<KeyValuePair<int, string>> ChannelLabels { get; }

	/// <summary>
	///     Gets the flags.
	/// </summary>
	string Flags { get; }

	/// <summary>
	///     Gets the hardware rev.
	/// </summary>
	string HardwareVersion { get; }

	/// <summary>
	///     Gets a value indicating whether is valid.
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	///     Gets the model.
	/// </summary>
	string Model { get; }

	/// <summary>
	///     Gets the name.
	/// </summary>
	string Name { get; }

	/// <summary>
	///     Gets the serial number.
	/// </summary>
	string SerialNumber { get; }

	/// <summary>
	///     Gets the software rev.
	/// </summary>
	string SoftwareVersion { get; }

	/// <summary>
	///     Gets the x axis.
	/// </summary>
	string AxisLabelX { get; }

	/// <summary>
	///     Gets the y axis.
	/// </summary>
	string AxisLabelY { get; }

	/// <summary>
	/// Gets a value indicating whether this instance is TSQ quantum file.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is TSQ quantum file; otherwise, <c>false</c>.
	/// </value>
	bool IsTsqQuantumFile { get; }
}
