namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The FilteredScanIterator interface.
/// </summary>
public interface IFilteredScanIterator
{
	/// <summary>
	/// Gets the string form of the filter which was used to construct this iterator
	/// </summary>
	string Filter { get; }

	/// <summary>
	/// Gets the previous scan number, which matches the filter.
	/// Returns 0 if there is no open file.
	/// If there are no additional scans matching the filter, returns -1.
	/// </summary>
	int PreviousScan { get; }

	/// <summary>
	/// Gets the next scan number, which matches the filter.
	/// Returns 0 if there is no open file.
	/// If there are no additional scans matching the filter, returns -1.
	/// </summary>
	int NextScan { get; }

	/// <summary>
	/// Sets the iterator's position.
	/// This scan number does not have to match the given filter.
	/// This can be used to find next or previous matching scan, from a given scan.
	/// Assuming the first scan is 1, a value of 0 will reset the iterator to
	/// start of file.
	/// A value of "Last scan number +1" can be used to reset to
	/// iterate backwards from the end of the file.
	/// </summary>
	int SpectrumPosition { set; }

	/// <summary>
	/// Gets a value indicating whether there are possible previous scans before the current scan.
	/// This does not guarantee that another matching scan exists. It simply tests that the current iterator position
	/// is not the first scan in the file.
	/// </summary>
	bool MayHavePrevious { get; }

	/// <summary>
	/// Gets a value indicating whether there are possible next scans after the current scan.
	/// This does not guarantee that another matching scan exists. It simply tests that the current iterator position
	/// is not the last scan in the file.
	/// </summary>
	bool MayHaveNext { get; }
}
