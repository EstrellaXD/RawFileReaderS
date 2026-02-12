using System;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines the format of a log entry, including label (name of the field), data type, and numeric formatting.
/// </summary>
public class HeaderItem : IHeaderItem
{
	private CultureInfo Culture = CultureInfo.InvariantCulture;

	private string ListSeparator = ",";

	private string locale;

	/// <summary>
	/// Gets or sets the display label for the field.
	/// For example: If this a temperature, this label may be "Temperature" and the DataType may be "GenericDataTypes.FLOAT"
	/// </summary>
	[XmlAttribute]
	public string Label { get; set; }

	/// <summary>
	/// Gets or sets the data type for the field
	/// </summary>
	[XmlAttribute]
	public GenericDataTypes DataType { get; set; }

	/// <summary>
	/// Gets or sets the precision, if the data type is float or double,
	/// or string length of string fields.
	/// </summary>
	[XmlAttribute]
	public int StringLengthOrPrecision { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether a number should be displayed in scientific notation
	/// </summary>
	[XmlAttribute]
	public bool IsScientificNotation { get; set; }

	/// <summary>
	/// Gets a value indicating whether this is considered numeric data.
	/// This is the same test as performed for <c>StatusLogPlottableData</c>".
	/// Integer types: short and long (signed and unsigned) and
	/// floating types: float and double are defined as numeric.
	/// </summary>
	public bool IsNumeric
	{
		get
		{
			GenericDataTypes dataType = DataType;
			if (dataType == GenericDataTypes.CHAR || (uint)(dataType - 5) <= 6u)
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// sets the localization.
	/// ISO 639-1 standard language code.
	/// By default "CultureInfo.InvariantCulture" will be used.
	/// The default applies for null, empty or not found cuture.
	/// </summary>
	/// <param name="culture">Culture name</param>
	/// <param name="decimalSeparator">override decimal separator from culture, unless empty (default)</param>
	/// <param name="listSeparator">override list separator from culture, unless empty (default)</param>
	/// <returns>true if requested culture was selected (or was empty). False on exception finding the culture</returns>
	public bool SetCulture(string culture, string listSeparator = "", string decimalSeparator = "")
	{
		bool result = true;
		locale = culture;
		if (!string.IsNullOrEmpty(culture))
		{
			try
			{
				Culture = new CultureInfo(culture);
			}
			catch (CultureNotFoundException)
			{
				result = false;
				Culture = CultureInfo.InvariantCulture;
			}
		}
		else
		{
			Culture = CultureInfo.InvariantCulture;
		}
		if (string.IsNullOrEmpty(listSeparator))
		{
			ListSeparator = Culture.TextInfo.ListSeparator;
		}
		else
		{
			ListSeparator = listSeparator;
		}
		if (!string.IsNullOrEmpty(decimalSeparator))
		{
			Culture.NumberFormat.NumberDecimalSeparator = decimalSeparator;
		}
		return result;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.HeaderItem" /> class.
	/// </summary>
	public HeaderItem()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.HeaderItem" /> class.
	/// </summary>
	/// <param name="label">The label.</param>
	/// <param name="dataType">Type of the data.</param>
	/// <param name="stringLengthOrPrecision">The string length or precision.</param>
	/// <param name="isScientificNotation">This indicates whether a number should be displayed in scientific notation. Optional parameter has a default value = false.</param>
	public HeaderItem(string label, GenericDataTypes dataType, int stringLengthOrPrecision, bool isScientificNotation = false)
	{
		Label = label;
		DataType = dataType;
		StringLengthOrPrecision = stringLengthOrPrecision;
		IsScientificNotation = isScientificNotation;
	}

	/// <summary>
	/// Tests whether this is a variable header.
	/// A "variable header", if present as the first field in a table of
	/// headers, defines that each record has a variable number of valid fields.
	/// The first field in each data record will then be converted to "validity flags"
	/// which determine which of the fields in a data record have valid values.
	/// </summary>
	/// <param name="fields">
	/// The number of fields in the header.
	/// </param>
	/// <returns>
	/// True if this specifies that "variable length" records are used.
	/// </returns>
	public bool IsVariableHeader(int fields)
	{
		if (Label.Length == 1 && Label[0] == '\u0001')
		{
			if (DataType == GenericDataTypes.CHAR_STRING || DataType == GenericDataTypes.WCHAR_STRING)
			{
				return StringLengthOrPrecision >= fields - 1;
			}
			return false;
		}
		return false;
	}

	private string Analyze(string value, string listSeparators)
	{
		StringBuilder result = new StringBuilder(value.Length);
		StringBuilder number = new StringBuilder(value.Length);
		int i = 0;
		bool endOfData = false;
		if (string.IsNullOrEmpty(listSeparators))
		{
			listSeparators = ",";
		}
		for (; i < value.Length; i++)
		{
			char c = value[i];
			if (char.IsDigit(c))
			{
				ParseNumberSeries();
				if (i < value.Length)
				{
					result.Append(value[i]);
				}
			}
			else
			{
				result.Append(c);
			}
		}
		return result.ToString();
		string ParseNumber(out bool converted)
		{
			converted = false;
			endOfData = false;
			number.Clear();
			char value2;
			while (char.IsDigit(value2 = value[i]))
			{
				number.Append(value2);
				i++;
				if (i == value.Length)
				{
					endOfData = true;
					break;
				}
			}
			if (!endOfData)
			{
				char c2 = value[i];
				if (c2 == '.')
				{
					number.Append(c2);
					i++;
				}
				if (i == value.Length)
				{
					endOfData = true;
					return number.ToString();
				}
				int num = 0;
				while (char.IsDigit(value2 = value[i]))
				{
					number.Append(value2);
					i++;
					num++;
					if (i == value.Length)
					{
						endOfData = true;
						break;
					}
				}
				if (num > 0)
				{
					if (double.TryParse(number.ToString(), out var result2))
					{
						converted = true;
						return result2.ToString("f" + num, Culture);
					}
					return number.ToString();
				}
			}
			return number.ToString();
		}
		void ParseNumberSeries()
		{
			while (true)
			{
				bool converted;
				string value2 = ParseNumber(out converted);
				result.Append(value2);
				if (!converted || endOfData)
				{
					break;
				}
				char value3 = value[i];
				if (!listSeparators.Contains(value3))
				{
					break;
				}
				i++;
				if (i == value.Length)
				{
					endOfData = true;
					result.Append(value3);
					break;
				}
				char c2 = value[i];
				if (c2 == ' ')
				{
					i++;
					if (i == value.Length)
					{
						endOfData = true;
						result.Append(value3);
						result.Append(' ');
						break;
					}
				}
				if (!char.IsDigit(value[i]))
				{
					if (c2 == ' ')
					{
						result.Append(", ");
					}
					else
					{
						result.Append(',');
					}
					break;
				}
				result.Append(ListSeparator);
				if (c2 == ' ')
				{
					result.Append(' ');
				}
			}
		}
	}

	/// <summary>
	/// Formats the specified value per the current header's settings.
	/// </summary>
	/// <param name="value">
	/// The value, as a object. Object type must match the expected type for this header.
	/// </param>
	/// <param name="analyzeStrings">If set: reformat numbers found in strings.
	/// Logs contain strings which may have us numeric values in them
	/// look for common forms and reformat in the locale needed.</param>
	/// <param name="listSeparators">When analyzing strings, accept any of these characters as 
	/// list separators, for lists of numbers.
	/// Default: expect comma ","</param>
	/// <returns>
	/// The formatted value.
	/// </returns>
	public string FormatRawValue(object value, bool analyzeStrings = false, string listSeparators = "")
	{
		if (value == null)
		{
			return string.Empty;
		}
		switch (DataType)
		{
		case GenericDataTypes.NULL:
		case GenericDataTypes.CHAR:
		case GenericDataTypes.UCHAR:
		case GenericDataTypes.SHORT:
		case GenericDataTypes.USHORT:
		case GenericDataTypes.LONG:
		case GenericDataTypes.ULONG:
			return value.ToString();
		case GenericDataTypes.CHAR_STRING:
		case GenericDataTypes.WCHAR_STRING:
		{
			string text = value.ToString();
			if (analyzeStrings)
			{
				return Analyze(text, listSeparators);
			}
			return text;
		}
		case GenericDataTypes.TRUEFALSE:
			if (!(bool)value)
			{
				return "False";
			}
			return "True";
		case GenericDataTypes.YESNO:
			if (!(bool)value)
			{
				return "No";
			}
			return "Yes";
		case GenericDataTypes.ONOFF:
			if (!(bool)value)
			{
				return "Off";
			}
			return "On";
		case GenericDataTypes.FLOAT:
		case GenericDataTypes.DOUBLE:
			return FormatNumericObject(value);
		default:
			return value.ToString();
		}
	}

	/// <summary>
	/// Re-formats the specified value per the current header's settings.
	/// </summary>
	/// <param name="value">
	/// The value, as a string.
	/// </param>
	/// <param name="analyzeStrings">If set: reformat numbers found in strings.
	/// Logs contain strings which may have US numeric values in them
	/// look for common forms and reformat in the locale needed.</param>
	/// <param name="listSeparators">When analyzing strings, accept any of these characters as 
	/// list separators, for lists of numbers.
	/// Default: expect comma ","</param>
	/// <returns>
	/// The formatted value.
	/// </returns>
	public string FormatValue(string value, bool analyzeStrings = false, string listSeparators = "")
	{
		switch (DataType)
		{
		case GenericDataTypes.NULL:
		case GenericDataTypes.CHAR:
		case GenericDataTypes.UCHAR:
		case GenericDataTypes.SHORT:
		case GenericDataTypes.USHORT:
		case GenericDataTypes.LONG:
		case GenericDataTypes.ULONG:
			return value;
		case GenericDataTypes.CHAR_STRING:
		case GenericDataTypes.WCHAR_STRING:
			if (analyzeStrings)
			{
				return Analyze(value, listSeparators);
			}
			return value;
		case GenericDataTypes.TRUEFALSE:
			return FormatBoolValue(value, "True", "False");
		case GenericDataTypes.YESNO:
			return FormatBoolValue(value, "Yes", "No");
		case GenericDataTypes.ONOFF:
			return FormatBoolValue(value, "On", "Off");
		case GenericDataTypes.FLOAT:
		case GenericDataTypes.DOUBLE:
			return FormatNumericValue(value);
		default:
			return value;
		}
	}

	/// <summary>
	/// Format a bool string based on the header.
	/// </summary>
	/// <param name="value">
	/// The value to format.
	/// </param>
	/// <param name="trueValue">
	/// The true string value.
	/// </param>
	/// <param name="falseValue">
	/// The false string value.
	/// </param>
	/// <returns>
	/// The formatted bool value.
	/// </returns>
	private static string FormatBoolValue(string value, string trueValue, string falseValue)
	{
		bool result;
		bool flag = bool.TryParse(value, out result);
		if (result && flag)
		{
			return trueValue;
		}
		return falseValue;
	}

	/// <summary>
	/// Format a numeric value.
	/// </summary>
	/// <param name="value">
	/// The value to format.
	/// </param>
	/// <returns>
	/// The formatted numeric value.
	/// </returns>
	private string FormatNumericValue(string value)
	{
		try
		{
			if (IsScientificNotation)
			{
				string text = string.Format(CultureInfo.InvariantCulture, "#.{0}E+0", new string('#', StringLengthOrPrecision));
				return double.Parse(value, CultureInfo.InvariantCulture).ToString(text, Culture);
			}
			string text2 = "F" + StringLengthOrPrecision;
			double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
			return result.ToString(text2, Culture);
		}
		catch (ArgumentNullException)
		{
			return value;
		}
		catch (FormatException)
		{
			return value;
		}
		catch (OverflowException)
		{
			return value;
		}
	}

	/// <summary>
	/// Format a numeric value.
	/// </summary>
	/// <param name="value">
	/// The value to format.
	/// </param>
	/// <returns>
	/// The formatted numeric value.
	/// </returns>
	private string FormatNumericObject(object value)
	{
		if (IsScientificNotation)
		{
			string text = string.Format(CultureInfo.InvariantCulture, "#.{0}E+0", new string('#', StringLengthOrPrecision));
			if (value is float num)
			{
				return num.ToString(text, Culture);
			}
			if (value is double num2)
			{
				return num2.ToString(text, Culture);
			}
			return "0";
		}
		string text2 = "F" + StringLengthOrPrecision;
		if (value is float num3)
		{
			return num3.ToString(text2, Culture);
		}
		if (value is double num4)
		{
			return num4.ToString(text2, Culture);
		}
		return "0";
	}
}
