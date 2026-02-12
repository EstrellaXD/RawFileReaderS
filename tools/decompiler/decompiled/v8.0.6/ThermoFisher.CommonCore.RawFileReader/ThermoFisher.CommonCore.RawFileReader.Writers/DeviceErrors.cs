using System;
using System.Text;
using System.Threading;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Defines the DeviceErrors type.
/// </summary>
internal class DeviceErrors : IFileError
{
	private readonly StringBuilder _warningMessage = new StringBuilder(500);

	private int _errorCode;

	/// <summary>
	/// Gets the error code number.
	/// Typically this is a windows system error number.
	/// If no number is encoded:
	/// Returns "1" if the error message is null
	/// Returns "2" is there is an error message.
	/// A return of "0" will occur if there has been no error message
	/// </summary>
	public int ErrorCode
	{
		get
		{
			if (_errorCode != 0)
			{
				return _errorCode;
			}
			if (ErrorMessage == null)
			{
				return 1;
			}
			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				return 2;
			}
			return 0;
		}
	}

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage { get; private set; }

	/// <summary>
	/// Gets a value indicating whether has error.
	/// </summary>
	public bool HasError
	{
		get
		{
			if (ErrorCode == 0)
			{
				return !string.IsNullOrEmpty(ErrorMessage);
			}
			return true;
		}
	}

	/// <summary>
	/// Gets a value indicating whether has warning.
	/// </summary>
	public bool HasWarning { get; private set; }

	/// <summary>
	/// Gets the warning message.
	/// </summary>
	public string WarningMessage => _warningMessage.ToString();

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.DeviceErrors" /> class.
	/// </summary>
	public DeviceErrors()
	{
		ClearAllErrorsAndWarnings();
	}

	/// <summary>
	/// Adds an Information message.
	/// Information is added as a line.
	/// </summary>
	/// <param name="information">
	/// The Information.
	/// </param>
	public void AppendInformataion(string information)
	{
		if (string.IsNullOrEmpty(WarningMessage))
		{
			SetInformataion(information);
		}
		else
		{
			AppendMessage(InformationLine(information));
		}
	}

	/// <summary>
	/// Adds an Warning message.
	/// Warning is added as a line.
	/// </summary>
	/// <param name="warning">
	/// The Warning.
	/// </param>
	public void AppendWarning(string warning)
	{
		if (string.IsNullOrEmpty(WarningMessage))
		{
			UpdateWarning(WarningLine(warning));
			return;
		}
		AppendMessage(WarningLine(warning));
		HasWarning = true;
	}

	/// <summary>
	/// The clear all.
	/// </summary>
	public void ClearAllErrorsAndWarnings()
	{
		ClearError();
		ClearWarning();
	}

	/// <summary>
	/// The clear error.
	/// </summary>
	public void ClearError()
	{
		Interlocked.Exchange(ref _errorCode, 0);
		ErrorMessage = string.Empty;
	}

	/// <summary>
	/// Sets an Information message.
	/// This initializes the warning message to the "Information:" + the message
	/// but initialize "HasWarning" to false, so callers know there are no warnings
	/// Information may be used to log progress, and inspected on error.
	/// Information is added as a line.
	/// </summary>
	/// <param name="information">
	/// The Information.
	/// </param>
	public void SetInformataion(string information)
	{
		UpdateWarning(InformationLine(information), hasWarning: false);
	}

	/// <summary>The update error. </summary>
	/// <param name="error">The error message. </param>
	/// <param name="errorCode"> The error code..</param>
	/// <returns>Always false.</returns>
	/// <exception cref="T:System.ArgumentException">The zero value is intended for no error and should not be used for clearing error here.</exception>
	public bool UpdateError(string error, int errorCode = -1)
	{
		if (errorCode == 0)
		{
			throw new ArgumentException("This method is inteneded for setting error code and error message. It's not used for clearing error.");
		}
		Interlocked.Exchange(ref _errorCode, errorCode);
		ErrorMessage = error;
		return false;
	}

	/// <summary>update error.</summary>
	/// <param name="ex">The error exception. </param>
	/// <exception cref="T:System.ArgumentException">The zero value is intended for no error and should not be used for clearing error here.</exception>
	/// <returns>Always false.</returns>
	public bool UpdateError(Exception ex)
	{
		UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
		return false;
	}

	/// <summary>
	/// append error, plus newline of there was an error before
	/// </summary>
	/// <param name="message">
	/// The message.
	/// </param>
	public void AppendError(string message)
	{
		if (!string.IsNullOrWhiteSpace(ErrorMessage))
		{
			ErrorMessage += Environment.NewLine;
		}
		ErrorMessage += message;
	}

	/// <summary>
	/// update warning message and state.
	/// Clears any previous warnings
	/// </summary>
	/// <param name="warning">
	/// The warning.
	/// </param>
	/// <param name="hasWarning">
	/// true if has warning.
	/// </param>
	public void UpdateWarning(string warning, bool hasWarning = true)
	{
		HasWarning = hasWarning;
		_warningMessage.Clear();
		_warningMessage.AppendLine(warning);
	}

	/// <summary>Append an error message.</summary>
	/// <param name="ex">The error exception. </param>
	/// <exception cref="T:System.ArgumentException">The zero value is intended for no error and should not be used for clearing error here.</exception>
	/// <returns>Always false.</returns>
	public bool AppendError(Exception ex)
	{
		return AppendError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
	}

	/// <summary>
	/// Format an information line.
	/// </summary>
	/// <param name="information">
	/// The information.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	private static string InformationLine(string information)
	{
		return "Information: " + information;
	}

	/// <summary>
	/// Format a warning line.
	/// </summary>
	/// <param name="warning">
	/// The information.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	private static string WarningLine(string warning)
	{
		return "Warning: " + warning;
	}

	/// <summary>The update error. </summary>
	/// <param name="error">The error message. </param>
	/// <param name="errorCode"> The error code..</param>
	/// <returns>Always false.</returns>
	/// <exception cref="T:System.ArgumentException">The zero value is intended for no error and should not be used for clearing error here.</exception>
	private bool AppendError(string error, int errorCode)
	{
		Interlocked.Exchange(ref _errorCode, errorCode);
		ErrorMessage += error;
		return false;
	}

	/// <summary>
	/// Append a message, as a line.
	/// </summary>
	/// <param name="text">
	/// The text.
	/// </param>
	private void AppendMessage(string text)
	{
		_warningMessage.AppendLine(text);
	}

	/// <summary>
	/// Clear all warnings.
	/// </summary>
	private void ClearWarning()
	{
		HasWarning = false;
		_warningMessage.Clear();
	}
}
