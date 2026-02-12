namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Access to the Dilution Target Component (subset of a calibration level).
/// </summary>
public interface IDilutionTargetCompFactorAccess
{
	/// <summary>
	/// Gets Anticipated amount of target component.
	/// </summary>
	double BaseAmount { get; }

	/// <summary>
	/// Gets the target component name.
	/// </summary>
	string TargetComponentName { get; }
}
