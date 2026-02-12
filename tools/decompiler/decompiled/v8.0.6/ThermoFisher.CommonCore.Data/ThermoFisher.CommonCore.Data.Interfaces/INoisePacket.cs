namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines noise from FT profile
/// </summary>
public interface INoisePacket
{
	/// <summary>
	/// Gets or sets the mass
	/// </summary>
	float Mass { get; set; }

	/// <summary>
	/// Gets or sets the noise
	/// </summary>
	float Noise { get; set; }

	/// <summary>
	/// Gets or sets the baseline
	/// </summary>
	float Baseline { get; set; }
}
