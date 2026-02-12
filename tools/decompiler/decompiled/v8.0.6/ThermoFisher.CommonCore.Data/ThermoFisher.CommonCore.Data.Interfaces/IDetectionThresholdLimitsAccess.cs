namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to Detection Threshold Limits
/// </summary>
public interface IDetectionThresholdLimitsAccess
{
	/// <summary>
	/// Gets the Area limit threshold.
	/// </summary>
	/// <value>The Area limit threshold.</value>
	double AreaThresholdLimit { get; }

	/// <summary>
	/// Gets the height limit threshold.
	/// </summary>
	/// <value>The height limit threshold.</value>
	double HeightThresholdLimit { get; }
}
