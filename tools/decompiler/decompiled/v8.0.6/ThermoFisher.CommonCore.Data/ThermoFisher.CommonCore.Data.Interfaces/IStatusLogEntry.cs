namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to define low level access to a status log entry.
/// This interface is intended for operations which need efficient access to
/// the data, without intermediate string formatting.
/// </summary>
public interface IStatusLogEntry
{
	/// <summary>
	/// Gets the time stamp of this log entry.
	/// </summary>
	float Time { get; }

	/// <summary>
	/// Gets the values for this log entry.
	/// Returns the (unformatted) Status Log Data values for
	/// all fields in the log.
	/// The object types depend on the field types, as returned by 
	/// GetStatusLogHeaderInformation.
	/// These values are in the same order as the headers.
	/// Uses for this include efficient copy of data from one file to another, as
	/// it eliminates translation of numeric data to and from strings.
	/// <c>
	/// Numeric values (where the header for this field returns "True" for IsNumeric)
	/// can always be cast up to double.
	/// The integer numeric types SHORT and USHORT are returned as short and ushort.
	/// The integer numeric types LONG and ULONG are returned as int and uint.
	/// All logical values (Yes/No, True/false, On/Off) are returned as "bool",
	/// where "true" implies "yes", "true" or "on".
	/// Char type is returned as "sbyte".
	/// Uchar type is returned as "byte".
	/// String types WCHAR_STRING and CHAR_STRING types are returned as "string".
	/// </c>
	/// </summary>
	object[] Values { get; }
}
