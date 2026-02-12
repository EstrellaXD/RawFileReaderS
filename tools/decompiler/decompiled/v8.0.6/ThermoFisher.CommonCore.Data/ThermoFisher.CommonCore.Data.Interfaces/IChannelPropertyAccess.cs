namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a property of a channel
/// </summary>
public interface IChannelPropertyAccess
{
	/// <summary>
	/// Gets the property name
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the string value of a proerty
	/// </summary>
	string StringValue { get; }

	/// <summary>
	/// Gets the numeric value of a property
	/// </summary>
	double NumericValue { get; }

	/// <summary>
	/// Gets the decimal places for formatting the numeric value
	/// </summary>
	int Digits { get; }

	/// <summary>
	/// Gets the unit of the property.
	/// </summary>
	string Units { get; }
}
