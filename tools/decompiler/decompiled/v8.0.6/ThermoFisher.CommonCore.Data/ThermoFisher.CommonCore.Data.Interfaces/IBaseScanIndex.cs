namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines the base format for a instrument data index.
/// </summary>
public interface IBaseScanIndex
{
	/// <summary>
	///     Gets the start time.
	/// </summary>
	double StartTime { get; }

	/// <summary>
	///     Gets the tic.
	/// </summary>
	double TIC { get; }
}
