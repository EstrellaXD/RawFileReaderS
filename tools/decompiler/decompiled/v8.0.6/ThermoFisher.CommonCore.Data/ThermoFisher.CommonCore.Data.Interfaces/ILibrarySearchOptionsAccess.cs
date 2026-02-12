using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to access NIST library search options
/// </summary>
public interface ILibrarySearchOptionsAccess
{
	/// <summary>
	/// Gets the similarity setting for NIST search
	/// </summary>
	SimilarityMode SimilarityMode { get; }

	/// <summary>
	/// Gets the identity mode for NIST search
	/// </summary>
	IdentityMode IdentityMode { get; }

	/// <summary>
	/// Gets the type of NIST search
	/// </summary>
	LibrarySearchType LibrarySearchType { get; }

	/// <summary>
	/// Gets the molecular weight
	/// </summary>
	int MolecularWeight { get; }

	/// <summary>
	/// Gets a value indicating whether search with Molecular Weight is enabled
	/// </summary>
	bool SearchMolecularWeightEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether reverse search is enabled
	/// </summary>
	bool ReverseSearch { get; }

	/// <summary>
	/// Gets a value indicating whether to append to the user library
	/// </summary>
	bool AppendUserLibrary { get; }

	/// <summary>
	/// Gets the search molecular weight
	/// </summary>
	int SearchMolecularWeight { get; }

	/// <summary>
	/// Gets the maximum number of reported search hits
	/// </summary>
	double MaxHits { get; }

	/// <summary>
	/// Gets the match factor
	/// </summary>
	int MatchFactor { get; }

	/// <summary>
	/// Gets the reverse match factor
	/// </summary>
	int ReverseMatchFactor { get; }

	/// <summary>
	/// Gets the Probability Percent (match limit)
	/// </summary>
	int ProbabilityPercent { get; }

	/// <summary>
	/// Gets the name of the user library (for append operation)
	/// </summary>
	string UserLibrary { get; }

	/// <summary>
	/// Gets the list of libraries to search
	/// </summary>
	ReadOnlyCollection<string> SearchList { get; }

	/// <summary>
	/// Gets a value indicating whether mass defect should be applied
	/// </summary>
	bool ApplyMassDefect { get; }

	/// <summary>
	/// Gets the mass defect for the low mass
	/// </summary>
	int DefectAtMass1 { get; }

	/// <summary>
	/// Gets the mass defect for the High mass
	/// </summary>
	int DefectAtMass2 { get; }

	/// <summary>
	/// Gets the mass at which "DefectAtMass1" applies
	/// </summary>
	double Mass1 { get; }

	/// <summary>
	/// Gets the mass at which "DefectAtMass2" applies
	/// </summary>
	double Mass2 { get; }
}
