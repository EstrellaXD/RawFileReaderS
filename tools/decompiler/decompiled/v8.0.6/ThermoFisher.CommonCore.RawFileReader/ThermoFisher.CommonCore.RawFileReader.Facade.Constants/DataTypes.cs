namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
/// The data types.
/// </summary>
internal enum DataTypes
{
	/// <summary>
	/// The data type is null/empty.
	/// </summary>
	Empty,
	/// <summary>
	/// <para>The data type is char.</para>
	/// Note: this is referred as a byte.
	/// </summary>
	Char,
	/// <summary>
	/// The data type is true or false.
	/// </summary>
	TrueFalse,
	/// <summary>
	/// The data type is yes or no.
	/// </summary>
	YesNo,
	/// <summary>
	/// The data type is on or off.
	/// </summary>
	OnOff,
	/// <summary>
	/// The data type is unsigned char.
	/// </summary>
	UnsignedChar,
	/// <summary>
	/// The data type is short.
	/// </summary>
	Short,
	/// <summary>
	/// The data type is unsigned short.
	/// </summary>
	UnsignedShort,
	/// <summary>
	/// <para>The data type is long.</para>
	/// Note: this is referred as a long (4-byte) in C++. It's not the same size as long in C#.
	/// </summary>
	Long,
	/// <summary>
	/// <para>The data type is unsigned long.</para>
	/// Note: this is referred as an unsigned long (4-byte) in C++. It's not the same size as unsigned long in C#.
	/// </summary>
	UnsignedLong,
	/// <summary>
	/// The data type is float.
	/// </summary>
	Float,
	/// <summary>
	/// The data type is double.
	/// </summary>
	Double,
	/// <summary>
	/// The data type is a char string.
	/// </summary>
	CharString,
	/// <summary>
	/// The data type is a wide char string.
	/// </summary>
	WideCharString,
	/// <summary>
	/// The end of data type
	/// </summary>
	EndOfDataType
}
