namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Access to basic data about a replicate
/// As defined in Xcalibur PMD
/// </summary>
public interface IReplicateDataAccess
{
	/// <summary>
	/// Gets the amount of target compound in calibration or QC standard.
	/// </summary>
	double Amount { get; }

	/// <summary>
	/// Gets the Ratio of target peak height to ISTD peak height in result file.
	/// </summary>
	double HeightRatio { get; }

	/// <summary>
	/// Gets the Ratio of target peak area to ISTD peak area in result file.
	/// </summary>
	double AreaRatio { get; }

	/// <summary>
	/// Gets a value indicating whether to exclude this data point from calibration curve.
	/// </summary>
	bool ExcludeFromCalibration { get; }

	/// <summary>
	/// Gets the raw file name for the replicate
	/// </summary>
	string File { get; }
}
