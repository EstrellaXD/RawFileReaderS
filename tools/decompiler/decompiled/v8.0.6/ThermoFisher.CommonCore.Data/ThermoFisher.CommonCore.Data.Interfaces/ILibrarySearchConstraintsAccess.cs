using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read NIST library search constraints
/// </summary>
public interface ILibrarySearchConstraintsAccess
{
	/// <summary>
	/// Gets a value indicating whether molecular weight constraint is enabled
	/// </summary>
	bool MolecularWeightEnabled { get; }

	/// <summary>
	/// Gets the minimum molecular weight
	/// </summary>
	int MinMolecularWeight { get; }

	/// <summary>
	/// Gets the maximum molecular weight
	/// </summary>
	int MaximumMolecularWeight { get; }

	/// <summary>
	/// Gets a value indicating whether name fragment constraint is enabled
	/// </summary>
	bool NameFragmentEnabled { get; }

	/// <summary>
	/// Gets the name fragment constraint
	/// </summary>
	string NameFragment { get; }

	/// <summary>
	/// Gets a value indicating whether DB constraint is enabled
	/// </summary>
	bool DbEnabled { get; }

	/// <summary>
	///  Gets a value indicating whether Fine constraint is enabled
	/// </summary>
	bool FineEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether EPA (Environmental Protection Agency) constraint is applied
	/// </summary>
	bool EpaEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether NIH (National Institute of Health) constraint is applied
	/// </summary>
	bool NihEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether TSCA (Toxic Substances Control Act) constraint is applied
	/// </summary>
	bool TscaEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether USP (United States Pharmacopoeia) constraint is applied
	/// </summary>
	bool UspEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether EINECS (European Inventory of Existing Commercial Chemical Substances) constraint is applied
	/// </summary>
	bool EinecsEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether RTECS (Registry of Toxic Effects of Chemical Substances) constraint is applied
	/// </summary>
	bool RtecsEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether HODOC (Handbook of Data on Organic Compounds) constraint is applied
	/// </summary>
	bool HodocEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether IR constraint is applied
	/// </summary>
	bool IrEnabled { get; }

	/// <summary>
	/// Gets a value indicating whether Elements constraint is applied
	/// </summary>
	bool ElementsEnabled { get; }

	/// <summary>
	/// Gets the Element constraint
	/// </summary>
	string Element { get; }

	/// <summary>
	/// Gets the element constraint method (used when ElementsEnabled)
	/// </summary>
	ElementsInCompound ElementsMethod { get; }

	/// <summary>
	/// Gets a value indicating whether Ion Constraints are enabled
	/// </summary>
	bool IonConstraintsEnabled { get; }

	/// <summary>
	/// Gets the method of Ion Constraints (used when IonConstraintsEnabled)
	/// </summary>
	IonConstraints IonConstraintMethod { get; }

	/// <summary>
	/// Gets the Ion Constraints (see NIST documentation for details)
	/// </summary>
	ReadOnlyCollection<IIonConstraintAccess> IonConstraints { get; }

	/// <summary>
	/// Gets the individual element constraints (limits on specific elements)
	/// </summary>
	ReadOnlyCollection<IIndividualConstraintAccess> IndivdualConstraints { get; }
}
