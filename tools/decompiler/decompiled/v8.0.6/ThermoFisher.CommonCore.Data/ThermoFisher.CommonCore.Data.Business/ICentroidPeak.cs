namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The centroid peak interface.
/// </summary>
public interface ICentroidPeak
{
	/// <summary>
	/// Gets or sets mass.
	/// </summary>
	double Mass { get; set; }

	/// <summary>
	/// Gets or sets intensity.
	/// </summary>
	double Intensity { get; set; }

	/// <summary>
	/// Gets or sets resolution.
	/// </summary>
	double Resolution { get; set; }

	/// <summary>
	/// Gets or sets base Line.
	/// </summary>
	double Baseline { get; set; }

	/// <summary>
	/// Gets or sets noise.
	/// </summary>
	double Noise { get; set; }

	/// <summary>
	/// Gets or sets charge.
	/// </summary>
	double Charge { get; set; }

	/// <summary>
	/// Gets or sets Peak Options Flag.
	/// </summary>
	PeakOptions Flag { get; set; }

	/// <summary>
	/// Gets or sets the signal to noise.
	/// </summary>
	double SignalToNoise { get; set; }
}
