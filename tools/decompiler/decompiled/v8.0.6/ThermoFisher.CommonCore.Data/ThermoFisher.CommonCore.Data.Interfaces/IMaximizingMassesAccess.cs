namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines settings for the maximizing masses algorithm
/// </summary>
public interface IMaximizingMassesAccess
{
	/// <summary>
	/// Gets the number of masses required to maximize
	/// </summary>
	int MassRequired { get; }

	/// <summary>
	/// Gets the percentage of masses which must maximize
	/// </summary>
	double PercentMassesFound { get; }

	/// <summary>
	/// Gets the box filter width for Mass-Maximizing detection
	/// </summary>
	int FilterWidth { get; }

	/// <summary>
	/// Gets the number of scans in the max-masses window
	/// </summary>
	int WindowSize { get; }

	/// <summary>
	/// Gets the minimum peak separation (time) for Mass-Maximizing detection
	/// </summary>
	double MinimumPeakSeparation { get; }

	/// <summary>
	/// Gets the number of scans averaged for a background
	/// </summary>
	int BackgroundWidth { get; }
}
