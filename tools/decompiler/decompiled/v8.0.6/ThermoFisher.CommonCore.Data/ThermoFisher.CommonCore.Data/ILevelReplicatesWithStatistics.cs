namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// This interface represents replicate information after preforming calibration calculations
/// and determining statistics. These statistics are may be used to annotate calibration curves.
/// </summary>
public interface ILevelReplicatesWithStatistics : ILevelReplicates
{
	/// <summary>
	/// Gets or sets the calculated % RSD for a replicate table
	/// </summary>
	double PercentRSD { get; set; }

	/// <summary>
	/// Gets or sets the calculated % RSD for a replicate table
	/// </summary>
	double PercentCV { get; set; }

	/// <summary>
	/// Gets the average value of the response factor
	/// </summary>
	double AverageRf { get; }
}
