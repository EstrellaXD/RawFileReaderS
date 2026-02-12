namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// NISY search similarity mode.
/// Refer to NIST documentation for details.
/// </summary>
public enum SimilarityMode
{
	/// <summary>
	/// Simple similarity mode
	/// </summary>
	Simple,
	/// <summary>
	/// Hybrid similarity mode
	/// </summary>
	Hybrid,
	/// <summary>
	/// Neutral Loss similarity mode
	/// </summary>
	NeutralLoss
}
