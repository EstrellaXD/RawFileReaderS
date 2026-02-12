using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Exception for raw data reading. Called when a Device specific method is made
/// without first selecting a Device. For example: requesting a chromatogram
/// </summary>
public class NoSelectedDeviceException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException" /> class. 
	/// Basic analysis exception, no specified reason.
	/// </summary>
	public NoSelectedDeviceException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	public NoSelectedDeviceException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	/// <param name="inner">
	/// Trapped inner exception
	/// </param>
	public NoSelectedDeviceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
