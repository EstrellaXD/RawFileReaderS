namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Determines how an element limit is applied
/// </summary>
public enum ElementConditions
{
	/// <summary>
	/// Element count must be greater than a specified value
	/// </summary>
	GreaterThan,
	/// <summary>
	///  Element count must be less than a specified value
	/// </summary>
	LessThan,
	/// <summary>
	/// Element count must equal the supplied value
	/// </summary>
	Equals
}
