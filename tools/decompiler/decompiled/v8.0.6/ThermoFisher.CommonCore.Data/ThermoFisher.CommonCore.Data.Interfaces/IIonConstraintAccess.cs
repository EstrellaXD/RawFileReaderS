namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read an ion constraint.
/// See NIST documentation for details.
/// </summary>
public interface IIonConstraintAccess
{
	/// <summary>
	/// Gets the method of ion constraint
	/// </summary>
	IonConstraintTypes Constraint { get; }

	/// <summary>
	/// Gets the mass to charge ratio of the constraint
	/// </summary>
	int MassToCharge { get; }

	/// <summary>
	/// Gets the from value of the constraint
	/// </summary>
	int From { get; }

	/// <summary>
	/// Gets the To value of the constraint
	/// </summary>
	int To { get; }
}
