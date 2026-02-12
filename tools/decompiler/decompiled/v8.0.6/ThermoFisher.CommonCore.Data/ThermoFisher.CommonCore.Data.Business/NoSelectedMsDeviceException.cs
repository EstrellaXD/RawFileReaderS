using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Exception for raw data reading. Called when a MS specific method is made
/// without first selecting the MS data.
/// For example: Requesting "scan filters" from UV data.
/// These should all be handled in the code (never intentionally thrown to caller)
/// </summary>
public class NoSelectedMsDeviceException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException" /> class. 
	/// Basic analysis exception, no specified reason.
	/// </summary>
	public NoSelectedMsDeviceException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	public NoSelectedMsDeviceException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	/// <param name="inner">
	/// Trapped inner exception
	/// </param>
	public NoSelectedMsDeviceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
