using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Exception filter string with invalid format
/// </summary>
public class InvalidFilterFormatException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.InvalidFilterFormatException" /> class. 
	/// Basic analysis exception, no specified reason.
	/// </summary>
	public InvalidFilterFormatException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.InvalidFilterFormatException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	public InvalidFilterFormatException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.InvalidFilterFormatException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	/// <param name="inner">
	/// Trapped inner exception
	/// </param>
	public InvalidFilterFormatException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
