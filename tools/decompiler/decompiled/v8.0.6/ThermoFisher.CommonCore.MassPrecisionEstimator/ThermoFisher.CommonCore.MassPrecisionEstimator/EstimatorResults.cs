namespace ThermoFisher.CommonCore.MassPrecisionEstimator;

/// <summary>  
/// Class to hold mass precision estimator results for individual mass/intensity points 
/// in a scan. 
/// </summary>
public class EstimatorResults
{
	/// <summary>
	/// Gets or sets the intensity value
	/// </summary>
	public double Intensity { get; set; }

	/// <summary>
	/// Gets or sets the mass value
	/// </summary>
	public double Mass { get; set; }

	/// <summary>
	/// Gets or sets the mass accuracy in MMU value
	/// </summary>
	public double MassAccuracyInMmu { get; set; }

	/// <summary>
	/// Gets or sets the mass accuracy in PPM value
	/// </summary>
	public double MassAccuracyInPpm { get; set; }

	/// <summary>
	/// Gets or sets the resolution value
	/// </summary>
	public double Resolution { get; set; }
}
