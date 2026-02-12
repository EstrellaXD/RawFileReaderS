namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// How relative response is measured
/// </summary>
public enum ResponseRatio
{
	/// <summary>
	/// Use the ratio of height of peaks to find responses
	/// </summary>
	Height,
	/// <summary>
	/// Use the ratio of peak areas to find responses
	/// </summary>
	Area
}
