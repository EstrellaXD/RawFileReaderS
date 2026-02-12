namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The shape of the peaks to simulate, when making a profile out of an isotope pattern.
/// </summary>
public enum PeakShape
{
	/// <summary>
	/// Create Profile with a Gaussian peak shape.
	/// (Legacy mode, as used in common core 2.0)
	/// </summary>
	Gaussian,
	/// <summary>
	/// Create profile with a cosine peak shape
	/// </summary>
	Cosine,
	/// <summary>
	/// Create profile with a triangular peak shape
	/// </summary>
	Triangular,
	/// <summary>
	/// Create profile with a Lorentzian peaks shape
	/// </summary>
	Lorentzian,
	/// <summary>
	/// Use: Updated Gaussian table
	/// This table is designed to allow slightly higher 
	/// precision in simulation and higher performance
	/// </summary>
	GaussianNew
}
