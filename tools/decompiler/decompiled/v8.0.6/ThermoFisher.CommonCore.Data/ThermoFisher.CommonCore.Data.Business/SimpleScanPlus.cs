using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Adds a charge, Noise and resolution table to a simple scan
/// </summary>
public class SimpleScanPlus : SimpleScan, ISimpleScanPlus, ISimpleScanAccess
{
	/// <summary>
	/// Gets or sets the charges.
	/// </summary>
	public int[] Charge { get; set; }

	/// <summary>
	/// Gets or sets the resolutions.
	/// </summary>
	public float[] Resolution { get; set; }

	/// <summary>
	/// Gets or sets the Noise values.
	/// </summary>
	public float[] Noise { get; set; }
}
