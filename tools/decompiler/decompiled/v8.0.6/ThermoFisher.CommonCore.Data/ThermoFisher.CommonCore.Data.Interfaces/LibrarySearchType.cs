namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines how NIST search is done
/// Refer to NIST documentation for details.
/// </summary>
public enum LibrarySearchType
{
	/// <summary>
	/// Perform NIST identity search
	/// </summary>
	Identity,
	/// <summary>
	/// Perform NIST similarity search
	/// </summary>
	Similarity
}
