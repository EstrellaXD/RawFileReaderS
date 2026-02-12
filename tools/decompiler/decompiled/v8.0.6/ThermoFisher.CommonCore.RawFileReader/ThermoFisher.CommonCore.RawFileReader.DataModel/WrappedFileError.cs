using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped file error. Translates error information
/// from a raw file into public interface
/// </summary>
internal class WrappedFileError : IFileError
{
	private readonly IErrors _errors;

	/// <summary>
	/// Gets the error code number.
	/// Typically this is a windows system error number.
	/// </summary>
	public int ErrorCode
	{
		get
		{
			if (_errors == null)
			{
				return 1;
			}
			if (_errors.HasError)
			{
				return 2;
			}
			return 0;
		}
	}

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage
	{
		get
		{
			if (_errors == null)
			{
				return "Unknown error while loading raw file";
			}
			return _errors.ErrorMessage;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	public bool HasError
	{
		get
		{
			if (_errors != null)
			{
				return _errors.HasError;
			}
			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this file has detected a warning.
	/// If this is false: Other warning properties in this interface have no meaning.
	/// </summary>
	public bool HasWarning => false;

	/// <summary>
	/// Gets the warning message.
	/// </summary>
	public string WarningMessage => string.Empty;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedFileError" /> class.
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	public WrappedFileError(IErrors errors)
	{
		_errors = errors;
	}
}
