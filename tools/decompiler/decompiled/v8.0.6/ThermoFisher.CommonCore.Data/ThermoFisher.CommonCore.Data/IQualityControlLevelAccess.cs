namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// interface to read QC level settings
/// </summary>
public interface IQualityControlLevelAccess : ICalibrationLevelAccess
{
	/// <summary>
	/// Gets the QC test <c>standard: 100 * (yobserved-ypredicted)/ypreditced</c>
	/// </summary>
	double TestPercent { get; }
}
