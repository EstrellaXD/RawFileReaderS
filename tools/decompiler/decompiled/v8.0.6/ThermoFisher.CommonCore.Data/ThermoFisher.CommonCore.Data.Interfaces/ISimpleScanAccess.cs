namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The SimpleScanAccess interface.
/// Defines the most basic data of a scan (mass and intensity)
/// </summary>
public interface ISimpleScanAccess
{
	/// <summary>
	/// Gets the list of masses of each centroid
	/// </summary>
	double[] Masses { get; }

	/// <summary>
	/// Gets the list of Intensities for each centroid
	/// </summary>
	double[] Intensities { get; }
}
