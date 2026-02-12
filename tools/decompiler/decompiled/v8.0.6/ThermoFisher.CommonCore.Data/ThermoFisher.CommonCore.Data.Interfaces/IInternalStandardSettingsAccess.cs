namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to CalibrationInternalStandardSettings
/// </summary>
public interface IInternalStandardSettingsAccess
{
	/// <summary>
	/// Gets the amount of internal standard. Not used in any calculation yet.
	/// </summary>
	double ISTDAmount { get; }

	/// <summary>
	/// Gets the units for the internal standard
	/// </summary>
	string ISTDUnits { get; }
}
