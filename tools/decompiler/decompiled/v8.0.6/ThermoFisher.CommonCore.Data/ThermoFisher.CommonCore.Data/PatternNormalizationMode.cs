namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// For the Spectral Distance calculation, the theoretical and measured isotope
/// patterns must be normalized. There are three different normalization modes available.
/// </summary>
public enum PatternNormalizationMode
{
	/// <summary>
	/// Base peak normalization means that base
	/// peak intensities are assumed to be identical.
	/// </summary>
	NormModeBasePeak,
	/// <summary>
	/// If the mode is LINEAR, both theoretical and measured
	/// spectrum are normalized such that the sum of their
	/// intensity differences is minimized.
	/// </summary>
	NormModeLinear,
	/// <summary>
	/// If the mode is QUADRATIC, both theoretical and measured
	/// spectrum are normalized such that the sum of their
	/// squared intensity differences is minimized.
	/// </summary>
	NormModeQuadratic
}
