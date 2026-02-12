namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Method information for an Ion Ratio Test
/// </summary>
public interface IIonRatioConfirmationTestAccess
{
	/// <summary>
	/// Gets the Mass to be tested
	/// </summary>
	double MZ { get; }

	/// <summary>
	/// Gets the Expected ratio 
	/// The ratio of the qualifier ion response to the quan ion response. 
	/// Range: 0 - 200%
	/// </summary>
	double TargetRatio { get; }

	/// <summary>
	/// Gets the Window to determine how accurate the match must be
	/// The ratio must be +/- this percentage.
	/// </summary>
	double WindowPercent { get; }
}
