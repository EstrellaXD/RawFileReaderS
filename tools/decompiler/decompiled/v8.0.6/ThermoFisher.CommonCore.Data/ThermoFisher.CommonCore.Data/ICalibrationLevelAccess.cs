namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to a calibration level.
/// </summary>
public interface ICalibrationLevelAccess
{
	/// <summary>
	/// Gets the name for this calibration level
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the amount of calibration compound (usually a concentration) for this level
	/// </summary>
	double BaseAmount { get; }
}
