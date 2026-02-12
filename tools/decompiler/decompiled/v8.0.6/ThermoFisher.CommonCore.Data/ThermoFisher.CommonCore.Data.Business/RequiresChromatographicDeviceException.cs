using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Exception for raw data reading. Called when a Chromatogram reading method is made
/// without first selecting a devices with chromatographic data. For example: requesting a chromatogram
/// from a device of type "Other" (such a an auto sampler).
/// </summary>
public class RequiresChromatographicDeviceException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.RequiresChromatographicDeviceException" /> class. 
	/// Basic analysis exception, no specified reason.
	/// </summary>
	public RequiresChromatographicDeviceException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.RequiresChromatographicDeviceException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	public RequiresChromatographicDeviceException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.RequiresChromatographicDeviceException" /> class. 
	/// Exception, with reason as text, which application can display or log
	/// </summary>
	/// <param name="message">
	/// Reason for exception
	/// </param>
	/// <param name="inner">
	/// Trapped inner exception
	/// </param>
	public RequiresChromatographicDeviceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
