namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to CalibrationSettings
/// </summary>
public interface ICalibrationSettingsAccess
{
	/// <summary>
	/// Gets the target compound settings.
	/// </summary>
	/// <value>The target compound settings.</value>
	ITargetCompoundSettingsAccess TargetCompoundSettings { get; }

	/// <summary>
	/// Gets the internal standard settings.
	/// </summary>
	/// <value>The internal standard settings.</value>
	IInternalStandardSettingsAccess InternalStandardSettings { get; }

	/// <summary>
	/// Gets a value which determines if this component is a target compound or an internal standard
	/// </summary>
	ComponentType ComponentType { get; }
}
