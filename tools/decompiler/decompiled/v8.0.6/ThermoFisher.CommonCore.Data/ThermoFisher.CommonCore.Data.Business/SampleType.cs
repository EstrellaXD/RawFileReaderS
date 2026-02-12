namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Enumeration of sample types
/// </summary>
public enum SampleType
{
	/// <summary>
	/// Unknown sample
	/// </summary>
	Unknown,
	/// <summary>
	/// Blank sample
	/// </summary>
	Blank,
	/// <summary>
	/// QC sample
	/// </summary>
	QC,
	/// <summary>
	/// Standard Clear (None) sample
	/// </summary>
	StdClear,
	/// <summary>
	/// Standard Update (None) sample
	/// </summary>
	StdUpdate,
	/// <summary>
	/// Standard Bracket (Open) sample
	/// </summary>
	StdBracket,
	/// <summary>
	/// Standard Bracket Start (multiple brackets) sample
	/// </summary>
	StdBracketStart,
	/// <summary>
	/// Standard Bracket End (multiple brackets) sample
	/// </summary>
	StdBracketEnd,
	/// <summary>
	/// Program sample
	/// </summary>
	Program,
	/// <summary>
	/// A sample which only contains solvent
	/// </summary>
	SolventBlank,
	/// <summary>
	/// Blank which includes internal standard only.
	/// </summary>
	MatrixBlank,
	/// <summary>
	/// Matrix sample with known amounts of surrogates
	/// </summary>
	MatrixSpike,
	/// <summary>
	/// Matrix sample with known amounts of target.
	/// </summary>
	MatrixSpikeDuplicate
}
