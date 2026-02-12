namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// This internal class is used to capture a point in the centroid scan.
/// It is specific to the algorithm (internal)
/// as it includes values such as "index" used for certain sorting features.
/// Also:
/// Noise and baseline values are not included in this data structure.
/// </summary>
internal class CentroidStreamPoint
{
	/// <summary>
	/// Gets or sets the mass to charge ratio
	/// </summary>
	internal double Position { get; set; }

	/// <summary>
	/// Gets or sets the intensity
	/// </summary>
	internal double Intensity { get; set; }

	/// <summary>
	/// Gets or sets the resolution
	/// </summary>
	internal double Resolution { get; set; }

	/// <summary>
	/// Gets or sets the charge
	/// </summary>
	internal int Charge { get; set; }

	/// <summary>
	/// Gets or sets the index into the mass ordered list
	/// </summary>
	internal int Index { get; set; }
}
