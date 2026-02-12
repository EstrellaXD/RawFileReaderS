namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to smoothing settings
/// </summary>
public interface ISmoothingSettingsAccess
{
	/// <summary>
	/// Gets the number of points for smoothing the chromatogram
	/// </summary>
	int SmoothingPoints { get; }

	/// <summary>
	/// Gets the number of times to repeat smoothing
	/// </summary>
	int SmoothRepeat { get; }

	/// <summary>
	/// Gets the envelope shape used by smoothing algorithm
	/// </summary>
	SmoothMethod SmoothMethod { get; }
}
