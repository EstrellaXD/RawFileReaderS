using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A simple mass and intensity scan.
/// Represents a scan as two simple array objects
/// This format only creates two objects,
/// and uses much less memory that "array of class with mass and intensity".
/// </summary>
public class SimpleScan : ISimpleScanAccess
{
	/// <summary>
	/// Gets or sets the masses.
	/// </summary>
	public double[] Masses { get; set; }

	/// <summary>
	/// Gets or sets the intensities.
	/// </summary>
	public double[] Intensities { get; set; }
}
