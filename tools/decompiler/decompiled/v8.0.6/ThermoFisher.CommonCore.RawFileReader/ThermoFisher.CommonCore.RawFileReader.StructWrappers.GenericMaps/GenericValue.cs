using System;
using System.Globalization;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// Encapsulates the generic value structure that is read from the raw file .
/// </summary>
internal class GenericValue
{
	private readonly DataTypes _dataType;

	private readonly string _stringFormat;

	/// <summary>
	/// Gets the value.
	/// </summary>
	public object Value { get; }

	public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.GenericValue" /> class. The value is set
	/// to an empty string.
	/// </summary>
	internal GenericValue()
	{
		Value = string.Empty;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.GenericValue" /> class.
	/// </summary>
	/// <param name="descriptor">
	/// The descriptor.
	/// </param>
	/// <param name="value">
	/// The value.
	/// </param>
	internal GenericValue(DataDescriptor descriptor, object value)
	{
		_dataType = descriptor.DataType;
		Value = value;
		_stringFormat = ((_dataType == DataTypes.Float || _dataType == DataTypes.Double) ? string.Format(FormatProvider, "{0}{1}", descriptor.IsScientificNotation ? "e" : "f", descriptor.LengthOrPrecision) : string.Empty);
	}

	/// <summary>
	/// The method returns the string representation of value. For floats and doubles, it uses the
	/// data descriptor's precision value to determine the value format.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.String" /> representation of value.
	/// </returns>
	public override string ToString()
	{
		if (_dataType == DataTypes.Empty || Value == null)
		{
			return string.Empty;
		}
		if (_dataType == DataTypes.Float)
		{
			return ((double)(float)Value).ToString(_stringFormat, FormatProvider);
		}
		if (_dataType == DataTypes.Double)
		{
			return ((double)Value).ToString(_stringFormat, FormatProvider);
		}
		return Value.ToString();
	}

	/// <summary>
	/// The to string.
	/// </summary>
	/// <param name="ifFormatted">
	/// The if formatted.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	internal string ToString(bool ifFormatted)
	{
		if (ifFormatted)
		{
			return ToString();
		}
		if (_dataType == DataTypes.Empty || Value == null)
		{
			return string.Empty;
		}
		if (_dataType == DataTypes.Float)
		{
			return ((double)(float)Value).ToString(FormatProvider);
		}
		if (_dataType == DataTypes.Double)
		{
			return ((double)Value).ToString(FormatProvider);
		}
		return Value.ToString();
	}
}
