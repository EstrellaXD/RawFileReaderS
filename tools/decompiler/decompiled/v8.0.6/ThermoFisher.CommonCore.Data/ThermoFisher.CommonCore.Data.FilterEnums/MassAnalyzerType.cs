namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Specifies type of mass analyzer in scans.
/// </summary>
public enum MassAnalyzerType
{
	/// <summary>
	/// Ion trap
	/// </summary>
	MassAnalyzerITMS,
	/// <summary>
	/// Triple quad
	/// </summary>
	MassAnalyzerTQMS,
	/// <summary>
	/// Single quad
	/// </summary>
	MassAnalyzerSQMS,
	/// <summary>
	/// Time of flight
	/// </summary>
	MassAnalyzerTOFMS,
	/// <summary>
	/// Fourier Transform
	/// </summary>
	MassAnalyzerFTMS,
	/// <summary>
	/// Magnetic sector
	/// </summary>
	MassAnalyzerSector,
	/// <summary>
	/// Match any type
	/// </summary>
	Any,
	/// <summary>
	/// Asymmetric Track Lossless (ASTRAL)
	/// AS         T 
	/// </summary>
	MassAnalyzerASTMS
}
