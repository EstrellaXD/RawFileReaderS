using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Exception for data reading. Thrown when a newer file format is detected.
/// This is usually because an appliction has been compiled against an older generation file reader DLL.
/// The application must be upgraded to use newer tools, which can decode this file.
/// </summary>
public class NewerFileFormatException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NewerFileFormatException" /> class. 
	/// Basic analysis exception, no specified reason.
	/// </summary>
	public NewerFileFormatException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NewerFileFormatException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	public NewerFileFormatException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NewerFileFormatException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	/// <param name="inner">
	/// Trapped inner exception
	/// </param>
	public NewerFileFormatException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
