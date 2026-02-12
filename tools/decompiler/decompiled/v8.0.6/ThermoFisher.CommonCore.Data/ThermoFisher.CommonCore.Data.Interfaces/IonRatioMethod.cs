namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines how ion ratio tests are performed
/// (as found in Xcalibur PMD files)
/// </summary>
public enum IonRatioMethod
{
	/// <summary>
	/// Use a weighted average test
	/// </summary>
	WeightedAverage,
	/// <summary>
	/// Use a simple average test
	/// </summary>
	NormalAverage,
	/// <summary>
	/// Use a standard
	/// </summary>
	UseStandard,
	/// <summary>
	/// Manual test settings
	/// </summary>
	Manual
}
