namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to retrieve error messages, which have been
/// trapped by the underlying file reader.
/// </summary>
public interface IFileError
{
	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// Applications should not continue with processing data from any file which indicates an error.
	/// </summary>
	bool HasError { get; }

	/// <summary>
	/// Gets a value indicating whether this file has detected a warning.
	/// If this is false: Other warning properties in this interface have no meaning.
	/// </summary>
	bool HasWarning { get; }

	/// <summary>
	/// Gets the error code number.
	/// Typically this is a windows system error number.
	/// The lowest valid windows error is: 0x00030200
	/// Errors detected within our files will have codes below 100.
	/// </summary>
	int ErrorCode { get; }

	/// <summary>
	/// Gets the error message.
	/// For "unknown exceptions" this may include a stack trace.
	/// </summary>
	string ErrorMessage { get; }

	/// <summary>
	/// Gets the warning message.
	/// </summary>
	string WarningMessage { get; }
}
