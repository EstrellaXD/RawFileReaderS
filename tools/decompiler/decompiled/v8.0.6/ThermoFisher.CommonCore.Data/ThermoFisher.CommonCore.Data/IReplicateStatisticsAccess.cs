namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Statistics calculated from a replicate table
/// </summary>
public interface IReplicateStatisticsAccess
{
	/// <summary>
	/// Gets the calculated % RSD for a replicate table
	/// </summary>
	double PercentRSD { get; }

	/// <summary>
	/// Gets the calculated % CV for a replicate table
	/// </summary>
	double PercentCV { get; }

	/// <summary>
	/// Gets the average retention time of all peaks added to the replicate table
	/// </summary>
	double AverageRetentionTime { get; }

	/// <summary>
	/// Gets the average response factor (or average response for ISTD)
	/// </summary>
	double AverageRf { get; }

	/// <summary>
	/// Gets a value indicating whether the calibration or QC level was "found"
	/// </summary>
	bool Valid { get; }
}
