using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines the format of a log entry, including label (name of the field), data type, and numeric formatting.
/// </summary>
public interface IHeaderItem
{
	/// <summary>
	/// Gets or sets the data type for the header item.
	/// ex. Char, TrueFalse, YesNo, UShort, Long, etc.
	/// </summary>
	/// <value>
	/// The type of the data.
	/// </value>
	GenericDataTypes DataType { get; set; }

	/// <summary>
	/// Gets or sets the header label.
	/// </summary>
	/// <value>
	/// The label.
	/// </value>
	string Label { get; set; }

	/// <summary>
	/// Gets or sets the precision, if the data type is float or double,
	/// or string length of string fields.
	/// </summary>
	/// <value>
	/// The string length or precision.
	/// </value>
	int StringLengthOrPrecision { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether a number should be displayed in scientific notation.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is scientific notation; otherwise, <c>false</c>.
	/// </value>
	bool IsScientificNotation { get; set; }

	/// <summary>
	/// Gets a value indicating whether this is considered numeric data.
	/// This is the same test as performed for <c>StatusLogPlottableData</c>".
	/// Integer types: short and long (signed and unsigned) and
	/// floating types: float and double are defined as numeric.
	/// </summary>
	bool IsNumeric => DataType switch
	{
		GenericDataTypes.UCHAR => true, 
		GenericDataTypes.CHAR => true, 
		GenericDataTypes.SHORT => true, 
		GenericDataTypes.USHORT => true, 
		GenericDataTypes.LONG => true, 
		GenericDataTypes.ULONG => true, 
		GenericDataTypes.FLOAT => true, 
		GenericDataTypes.DOUBLE => true, 
		_ => false, 
	};
}
