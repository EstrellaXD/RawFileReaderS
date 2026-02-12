namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read individual constraints
/// See NIST documentation for details
/// </summary>
public interface IIndividualConstraintAccess
{
	/// <summary>
	/// Gets the condition on this element (greater, less or equal to value)
	/// </summary>
	ElementConditions ElementCondition { get; }

	/// <summary>
	/// Gets the comparison value for this element constraint.
	/// Used in a a test as per "ElementCondition"
	/// </summary>
	int Value { get; }

	/// <summary>
	/// Gets the element to constrain
	/// </summary>
	string Element { get; }
}
