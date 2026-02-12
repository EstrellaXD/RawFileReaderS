namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Specifies units for measuring mass tolerance.
/// <para>Tolerance is used to determine if a results should be kept,
/// in formula search. If the exact mass of a formula is not within tolerance
/// of a measured mass from an instrument, then the formula is not considered a valid result.</para>
/// </summary>
public enum ToleranceMode
{
	/// <summary>
	/// No tolerance mode
	/// </summary>
	None,
	/// <summary>
	/// Atomic mass units (or Daltons)
	/// </summary>
	Amu,
	/// <summary>
	/// Milli Mass Units (1/1000 Dalton)
	/// </summary>
	Mmu,
	/// <summary>
	/// Parts Per Million
	/// </summary>
	Ppm
}
