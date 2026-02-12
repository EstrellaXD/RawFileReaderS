namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to mass options
/// </summary>
public interface IMassOptionsAccess
{
	/// <summary>
	/// Gets the tolerance value.
	/// </summary>
	/// <value>The tolerance.</value>
	double Tolerance { get; }

	/// <summary>
	/// Gets the units of precision.
	/// </summary>
	/// <value>The precision.</value>
	int Precision { get; }

	/// <summary>
	/// Gets the tolerance units.
	/// </summary>
	/// <value>The tolerance units.</value>
	ToleranceUnits ToleranceUnits { get; }

	/// <summary>
	/// Gets the tolerance string of the current m_toleranceUnits setting.
	/// </summary>
	/// <value>The tolerance string.</value>
	string ToleranceString { get; }

	/// <summary>
	/// Get the tolerance window around a specific mass
	/// </summary>
	/// <param name="mass">
	/// Mass about which window is needed
	/// </param>
	/// <returns>
	/// The distance (in amu) from the mass which is within tolerance.
	/// For example: myWindow=GetToleranceAtMass(myMass);
	/// accept data between "myMass-myWindow" and "myMass+myWindow"
	/// </returns>
	double GetToleranceAtMass(double mass);
}
