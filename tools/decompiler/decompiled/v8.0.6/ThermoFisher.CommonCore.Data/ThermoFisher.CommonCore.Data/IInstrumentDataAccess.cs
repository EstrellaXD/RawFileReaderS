using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to instrument data
/// </summary>
public interface IInstrumentDataAccess
{
	/// <summary>
	/// Gets or sets the name of the instrument
	/// </summary>
	string Name { get; set; }

	/// <summary>
	/// Gets or sets the model of the instrument
	/// </summary>
	string Model { get; set; }

	/// <summary>
	/// Gets or sets the serial number of the instrument
	/// </summary>
	string SerialNumber { get; set; }

	/// <summary>
	/// Gets or sets the software version of the instrument
	/// </summary>
	string SoftwareVersion { get; set; }

	/// <summary>
	/// Gets or sets the hardware version of the instrument
	/// </summary>
	string HardwareVersion { get; set; }

	/// <summary>
	/// Gets or sets the names of the channels, for UV or analog data.
	/// </summary>
	string[] ChannelLabels { get; set; }

	/// <summary>
	/// Gets or sets the units of the Signal, for UV or analog
	/// </summary>
	DataUnits Units { get; set; }

	/// <summary>
	/// Gets or sets the flags.
	/// The purpose of this field is to contain flags separated by ';' that
	/// denote experiment information, etc. For example, if a file is acquired
	/// under instrument control based on an experiment protocol like an ion
	/// mapping experiment, an appropriate flag can be set here.
	/// Legacy LCQ MS flags:
	/// 1. TIM - total ion map
	/// 2. NLM - neutral loss map
	/// 3. PIM - parent ion map
	/// 4. DDZMAP - data dependent zoom map
	/// </summary>
	string Flags { get; set; }

	/// <summary>
	/// Gets or sets the device suggested label of X axis
	/// </summary>
	string AxisLabelX { get; set; }

	/// <summary>
	/// Gets or sets the device suggested label of Y axis (name for units of data, such as "°C")
	/// </summary>
	string AxisLabelY { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether any other properties in this interface contain valid data.
	/// This is to support legacy files only. Early versions of the raw file did not have "instrument data",
	/// Data migration to current formats is automatic in raw file reading tools,
	/// leading to a data structure being returned to a caller with "all defaults" and "empty strings"
	/// plus the IsValid set to false.
	/// </summary>
	bool IsValid { get; set; }
}
