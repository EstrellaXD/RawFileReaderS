using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// This inteface extends the functions of "auto filter".
/// methods are  called to construct a list of filters based on internal data
/// of the class which implements this. Inital data is, for example,
/// the results of "auto filter" on a raw file.
/// The list can be searched for "items matching a given filter rule".
/// </summary>
public interface IEnhancedAutoFilter
{
	/// <summary>
	/// Gets the results of the enhanced auto filter.
	/// these results are initailly empty, and et extended
	/// as "Add" methods are called.
	/// </summary>
	List<IFilterWithString> FilterList { get; }

	/// <summary>
	/// Gets or sets a value indicating whether compound names shoud be included.
	/// This only has an effect if the "Name" property is set for at least one filter.
	/// Results which have names are shown in sorted (alpha) order.
	/// Any other filters, which do not have a name, are then added after the 
	/// named items, in the original "auto filter" order.
	/// If a name appaers for more than one filter, then an entry is created
	/// which only contains a compound name, such that a chromatogram
	/// can be created based on all data identified for that compound.
	/// Note that this "name only" list is excluded when a "unique filter list" is
	/// requested with a specific subset filter.
	/// </summary>
	bool IncludeCompoundNames { get; set; }

	/// <summary>
	/// Gets or sets a separator which appears between a compound name and a filter.
	/// </summary>
	string CompoundNameSeparator { get; set; }

	/// <summary>
	/// Searches for the activation types used.
	/// Adds "Activation type" to the list, for MS/MS data (MS2 or above).
	/// This can find CID, HCD, ETD, UVPD, EID
	/// </summary>
	void AddActivationTypeFilters();

	/// <summary>
	/// Adds the "empty filter" to the list.
	/// </summary>
	void AddEmptyFilter();

	/// <summary>
	/// Adds "ms order filters" to the list
	/// this includes ms, ms2 etc up to ms5
	/// if any ms/ms is found msn is also added.
	/// </summary>
	/// <param name="addMsn">When true (default): if any ms/ms is found msn is also added.</param>
	void AddMsOrderFilters(bool addMsn = true);

	/// <summary>
	/// Add all unique filter groups (auto filter).
	/// </summary>
	/// <param name="mustContain">If this is not empty: The filters must all contan this sub-filter.
	/// For example "d" for "only return data dependent"</param>
	void AddUniqueFilters(string mustContain);

	/// <summary>
	/// Merge filters which differ only by CV.
	/// Because there is no speciic CV value, the merged filters will not show
	/// the code CV. 
	/// </summary>
	void MergeCvValues();
}
