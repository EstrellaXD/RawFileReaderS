using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defienes common channel prorties (for chroatography domain data)
/// </summary>
public abstract class ChannelHeaderBase : IChannelHeaderBase
{
	/// <summary>
	/// The number of base proerties ina  channel header.
	/// Derived classes may add more.
	/// </summary>
	public const int NumChannelHeaderBaseProperties = 8;

	/// <summary>
	/// Gets or sets the version string. It may be empty.
	/// </summary>
	public string Version { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the Name of the signal,
	/// such as "oven temperature" in "oven temperature °C"
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the unit (such as "°C" for degrees Celsius)
	/// </summary>
	public string SignalUnit { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets suggested digits after the decimal point
	/// </summary>
	public int DecimalPlaces { get; set; }

	/// <summary>
	/// Gets or sets the anticipated range of signals
	/// </summary>
	public IRangeAccess SignalRange { get; set; }

	/// <summary>
	/// Gets or sets the name of the device that provided the signal
	/// </summary>
	public string DeviceName { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the units over which samples are taken (X axis scale)
	/// </summary>
	public string SamplingUnits { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the extended properties
	/// </summary>
	public ChannelProperty[] ExtendedProperties { get; set; }

	/// <inheritdoc />
	public IReadOnlyCollection<IChannelPropertyAccess> AdditionalProperties => (IReadOnlyCollection<IChannelPropertyAccess>)(object)ExtendedProperties;

	/// <summary>
	/// Gets or sets a value indicating whether this data should be plotted as a continuous trace.
	/// This may not be true for diagnostic values with "On/Off" state.
	/// When not set, the value of the signal remains constant at the previous value sent, and 
	/// jumps instantly to the new value at a time point.
	/// </summary>
	public bool IsContinuous { get; set; }

	/// <summary>
	/// Convert a channel header into log record format
	/// </summary>
	/// <returns>Log header and log data, from the channel header</returns>
	public abstract Tuple<IHeaderItem[], object[]> CreateLog();

	/// <summary>Creates the common log items.</summary>
	/// <param name="items">The items.</param>
	/// <param name="values">The values.</param>
	protected void CreateCommonLogItems(List<IHeaderItem> items, List<object> values)
	{
		if (!string.IsNullOrEmpty(Version))
		{
			AddString(items, values, "Version", Version);
		}
		AddString(items, values, "Name", Name ?? string.Empty);
		AddString(items, values, "Sampling units", SamplingUnits ?? string.Empty);
		AddString(items, values, "Signal unit", SignalUnit ?? string.Empty);
		AddInt(items, values, "Decimal places", DecimalPlaces);
		AddDouble(items, values, "Minimum", SignalRange.Low, DecimalPlaces);
		AddDouble(items, values, "Maximum", SignalRange.High, DecimalPlaces);
		AddBool(items, values, "Is continuous", IsContinuous);
		AddString(items, values, "Device name", DeviceName ?? string.Empty);
	}

	/// <summary>Imports the common log items.</summary>
	/// <param name="logHeader">The log header.</param>
	/// <param name="log">The log.</param>
	/// <param name="index">The index.</param>
	/// <returns>
	///   the next item index
	/// </returns>
	protected int ImportCommonLogItems(IHeaderItem[] logHeader, object[] log, int index)
	{
		ValidateString(logHeader[index], "Name");
		Name = (string)log[index++];
		ValidateString(logHeader[index], "Sampling units");
		SamplingUnits = (string)log[index++];
		ValidateString(logHeader[index], "Signal unit");
		SignalUnit = (string)log[index++];
		ValidateInt(logHeader[index], "Decimal places");
		DecimalPlaces = (int)log[index++];
		ValidateDouble(logHeader[index], "Minimum");
		double low = (double)log[index++];
		ValidateDouble(logHeader[index], "Maximum");
		double high = (double)log[index++];
		SignalRange = RangeFactory.Create(low, high);
		ValidateBool(logHeader[index], "Is continuous");
		IsContinuous = (bool)log[index++];
		ValidateString(logHeader[index], "Device name");
		DeviceName = (string)log[index++];
		return index;
	}

	/// <summary>Imports the extended properties.</summary>
	/// <param name="logHeader">The log header.</param>
	/// <param name="log">The log.</param>
	/// <param name="index">The index.</param>
	/// <returns>
	///   Array of channel property
	/// </returns>
	protected static ChannelProperty[] ImportExtendedProperties(IHeaderItem[] logHeader, object[] log, int index)
	{
		if (index < logHeader.Length)
		{
			ValidateInt(logHeader[index], "Additional data");
			int num = (int)log[index++];
			int num2 = num * 5;
			if (num2 + index <= logHeader.Length)
			{
				ChannelProperty[] array = new ChannelProperty[num2];
				for (int i = 0; i < num; i++)
				{
					ChannelProperty channelProperty = new ChannelProperty();
					string text = i + 1 + ": ";
					ValidateString(logHeader[index], text + "Name");
					channelProperty.Name = (string)log[index++];
					ValidateString(logHeader[index], text + "String value");
					channelProperty.StringValue = (string)log[index++];
					ValidateDouble(logHeader[index], text + "Numeric value");
					channelProperty.NumericValue = (double)log[index++];
					ValidateInt(logHeader[index], text + "Digits");
					channelProperty.Digits = (int)log[index++];
					ValidateString(logHeader[index], text + "Units");
					channelProperty.Units = (string)log[index++];
					array[i] = channelProperty;
				}
				return array.ToArray();
			}
		}
		return null;
	}

	/// <summary>Creates the extended log property items.</summary>
	/// <param name="items">The items.</param>
	/// <param name="values">The values.</param>
	/// <param name="extendedProperties">The extended properties.</param>
	protected static void CreateExtendedLogItems(List<IHeaderItem> items, List<object> values, ChannelProperty[] extendedProperties)
	{
		if (extendedProperties.Length != 0)
		{
			AddInt(items, values, "Additional data", extendedProperties.Length);
			for (int i = 0; i < extendedProperties.Length; i++)
			{
				string text = i + 1 + ": ";
				ChannelProperty channelProperty = extendedProperties[i];
				AddString(items, values, text + "Name", channelProperty.Name);
				AddString(items, values, text + "String value", channelProperty.StringValue);
				AddDouble(items, values, text + "Numeric value", channelProperty.NumericValue, channelProperty.Digits);
				AddInt(items, values, text + "Digits", channelProperty.Digits);
				AddString(items, values, text + "Units", channelProperty.Units);
			}
		}
	}

	private protected static void AddString(List<IHeaderItem> items, List<object> values, string label, string value)
	{
		items.Add(new HeaderItem
		{
			DataType = GenericDataTypes.WCHAR_STRING,
			Label = label,
			StringLengthOrPrecision = value.Length
		});
		values.Add(value);
	}

	private protected static void AddBool(List<IHeaderItem> items, List<object> values, string label, bool value)
	{
		items.Add(new HeaderItem
		{
			DataType = GenericDataTypes.TRUEFALSE,
			Label = label
		});
		values.Add(value);
	}

	/// <summary>
	/// Test that a field is a string (16 bit char) with the correct name
	/// </summary>
	/// <param name="item">field definition</param>
	/// <param name="label">Expected name</param>
	protected static void ValidateString(IHeaderItem item, string label)
	{
		if (!IsExpectedFormat(item, GenericDataTypes.WCHAR_STRING, label))
		{
			throw new FormatException(label);
		}
	}

	/// <summary>
	/// Test that a field is an int with the correct name
	/// </summary>
	/// <param name="item">field definition</param>
	/// <param name="label">Expected name</param>
	protected static void ValidateInt(IHeaderItem item, string label)
	{
		if (!IsExpectedFormat(item, GenericDataTypes.LONG, label))
		{
			throw new FormatException(label);
		}
	}

	/// <summary>
	/// Test that a field is a double with the correct name
	/// </summary>
	/// <param name="item">field definition</param>
	/// <param name="label">Expected name</param>
	protected static void ValidateDouble(IHeaderItem item, string label)
	{
		if (!IsExpectedFormat(item, GenericDataTypes.DOUBLE, label))
		{
			throw new FormatException(label);
		}
	}

	/// <summary>
	/// Test that a field is a bool with the correct name
	/// </summary>
	/// <param name="item">field definition</param>
	/// <param name="label">Expected name</param>
	protected static void ValidateBool(IHeaderItem item, string label)
	{
		if (!IsExpectedFormat(item, GenericDataTypes.TRUEFALSE, label))
		{
			throw new FormatException(label);
		}
	}

	private static void AddInt(List<IHeaderItem> items, List<object> values, string label, int value)
	{
		items.Add(new HeaderItem
		{
			DataType = GenericDataTypes.LONG,
			Label = label
		});
		values.Add(value);
	}

	private static void AddDouble(List<IHeaderItem> items, List<object> values, string label, double value, int precision)
	{
		items.Add(new HeaderItem
		{
			DataType = GenericDataTypes.DOUBLE,
			Label = label,
			StringLengthOrPrecision = precision
		});
		values.Add(value);
	}

	/// <summary>
	/// Tests whether a log entry has the expected format
	/// </summary>
	/// <param name="header">the header for the log entry</param>
	/// <param name="dataType">the exepcetd data type</param>
	/// <param name="label">the expected label</param>
	/// <returns></returns>
	private static bool IsExpectedFormat(IHeaderItem header, GenericDataTypes dataType, string label)
	{
		if (header.DataType == dataType)
		{
			return header.Label == label;
		}
		return false;
	}
}
