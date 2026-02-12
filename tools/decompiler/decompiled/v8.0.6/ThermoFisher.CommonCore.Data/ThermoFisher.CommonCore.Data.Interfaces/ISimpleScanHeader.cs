namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The SimpleScanHeader interface.
/// Defines minimal data about a scan (its number and retention time)
/// </summary>
public interface ISimpleScanHeader
{
	/// <summary>
	/// Gets the retention time.
	/// </summary>
	double RetentionTime { get; }

	/// <summary>
	/// Gets the scan number.
	/// </summary>
	int ScanNumber { get; }
}
