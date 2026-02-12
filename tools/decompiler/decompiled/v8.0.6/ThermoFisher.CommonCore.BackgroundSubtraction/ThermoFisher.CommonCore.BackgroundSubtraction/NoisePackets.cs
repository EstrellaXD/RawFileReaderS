namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// The noise packets.
/// </summary>
internal class NoisePackets
{
	/// <summary>
	/// Gets or sets the mass.
	/// </summary>
	internal double[] Mass { get; set; }

	/// <summary>
	/// Gets or sets the noise.
	/// </summary>
	internal double[] Noise { get; set; }

	/// <summary>
	/// Gets or sets the base line.
	/// </summary>
	internal double[] BaseLine { get; set; }
}
