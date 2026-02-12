namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Determines how spectra are subtracted
/// </summary>
public enum SpectrumSubtractMethod
{
	/// <summary>
	/// Subtract at a peak
	/// </summary>
	AtPeak,
	/// <summary>
	/// Subtract: not at a peak
	/// </summary>
	NotAtPeak
}
