namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines noise and baseline at a given mass
/// (Part of support for reading orbitrap data)
/// </summary>
public class NoiseAndBaseline
{
	/// <summary>
	/// Gets or sets the mass.
	/// </summary>
	public float Mass { get; set; }

	/// <summary>
	/// Gets or sets the noise.
	/// </summary>
	public float Noise { get; set; }

	/// <summary>
	/// Gets or sets the baseline.
	/// </summary>
	public float Baseline { get; set; }
}
