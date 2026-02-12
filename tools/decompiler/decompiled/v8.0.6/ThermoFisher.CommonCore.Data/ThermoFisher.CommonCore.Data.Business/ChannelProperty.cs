using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a property of a channel
/// </summary>
public class ChannelProperty : IChannelPropertyAccess
{
	/// <summary>
	/// Gets or sets the property name
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the string value of a proerty
	/// </summary>
	public string StringValue { get; set; }

	/// <summary>
	/// Gets or set the numeric value of a property
	/// </summary>
	public double NumericValue { get; set; }

	/// <summary>
	/// Gets or sets the decimal places for formatting the numeric value
	/// </summary>
	public int Digits { get; set; }

	/// <summary>
	/// Gets or sets the unit of the property.
	/// </summary>
	public string Units { get; set; }
}
