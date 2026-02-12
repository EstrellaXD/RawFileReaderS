namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines how spectrum enhancement is done
/// </summary>
public enum SpectrumEnhanceMode
{
	/// <summary>
	/// Refine the spectrum
	/// </summary>
	Refine,
	/// <summary>
	/// Combine spectra
	/// </summary>
	Combine,
	/// <summary>
	/// Threshold the data
	/// </summary>
	Threshold
}
