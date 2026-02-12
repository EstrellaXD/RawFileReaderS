namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Specifies a method of calculation centroids from a profile.
/// </summary>
public enum CentroidAlgorithm
{
	/// <summary>
	///  used in LCQ, TSQ, Quantum
	/// </summary>
	TSQ,
	/// <summary>
	/// Austin tweaking of TSQ algorithm
	/// </summary>
	GCQ,
	/// <summary>
	/// used in MAT95 and DFS products
	/// </summary>
	MAT,
	/// <summary>
	/// used in Orbitrap and FT analyzers
	/// </summary>
	FTORBITRAP
}
