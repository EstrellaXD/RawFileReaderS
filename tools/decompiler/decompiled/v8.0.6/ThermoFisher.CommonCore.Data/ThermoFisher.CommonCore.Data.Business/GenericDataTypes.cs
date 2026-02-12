namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// enumeration for data type for the fields used by
/// records of TuneData, StatusLog, TrailerExtra
/// These are upper case names, so that they don't clash with standard
/// type names.
/// </summary>
public enum GenericDataTypes
{
	/// <summary> Null data type. No data is available (just a label) </summary>
	NULL = 0,
	/// <summary> character data type (1 byte) </summary>
	CHAR = 1,
	/// <summary> true/false data type/ Similar to boolean, 1 byte of data displayed as True or False</summary>
	TRUEFALSE = 2,
	/// <summary>
	/// Alternate name for TRUEFALSE (0= False, 1 = True).
	/// This alternate suggested for consistent use of Bool type in logs.
	/// </summary>
	Bool = 2,
	/// <summary> Yes/No data type. 1 byte of data displayed as Yes or No</summary>
	YESNO = 3,
	/// <summary> ON/OFF data type. 1 byte of data displayed as On or Off </summary>
	ONOFF = 4,
	/// <summary> unsigned char data type (unsigned byte)</summary>
	UCHAR = 5,
	/// <summary> short data type </summary>
	SHORT = 6,
	/// <summary>unsigned short data type </summary>
	USHORT = 7,
	/// <summary>
	/// <para>long data type</para>
	/// Note: this is referred as a long (4-byte) in C++. It's not the same size as long in C#.
	/// </summary>
	LONG = 8,
	/// <summary>
	/// 32 bit integer (same as legacy type code LONG)
	/// </summary>
	Int = 8,
	/// <summary>
	/// <para>unsigned log data type </para>
	/// Note: this is referred as an unsigned long (4-byte) in C++. It's not the same size as unsigned long in C#.
	/// </summary>
	ULONG = 9,
	/// <summary>
	/// 32 bit unsigned int (same as legacy type code UNLONG).
	/// </summary>
	Uint = 9,
	/// <summary>float data type </summary>
	FLOAT = 10,
	/// <summary>double data type </summary>
	DOUBLE = 11,
	/// <summary>string data type (single byte chars) </summary>
	CHAR_STRING = 12,
	/// <summary>string data type (wide chars, unicode)</summary>
	WCHAR_STRING = 13
}
