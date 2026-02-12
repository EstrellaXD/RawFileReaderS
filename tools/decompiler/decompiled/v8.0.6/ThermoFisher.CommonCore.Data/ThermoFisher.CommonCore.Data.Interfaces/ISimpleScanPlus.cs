namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Extends The SimpleScanAccess interface to add charges and resolutions
/// </summary>
public interface ISimpleScanPlus : ISimpleScanAccess
{
	/// <summary>
	/// Gets the list of charges of each centroid
	/// </summary>
	int[] Charge { get; }

	/// <summary>
	/// Gets or sets the resolutions.
	/// </summary>
	float[] Resolution { get; set; }

	/// <summary>
	/// Gets or sets the Noise values.
	/// </summary>
	float[] Noise { get; }
}
