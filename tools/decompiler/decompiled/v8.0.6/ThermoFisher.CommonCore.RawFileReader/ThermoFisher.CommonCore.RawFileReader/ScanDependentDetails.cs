using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Local version of ScanDependentsDetails
/// </summary>
internal class ScanDependentDetails : IScanDependentDetails
{
	private string _filterString;

	/// <summary>
	/// Gets or sets the index of the scan.
	/// </summary>
	/// <value>
	/// The index of the scan.
	/// </value>
	public int ScanIndex { get; set; }

	/// <summary>
	/// Gets the filter string.
	/// </summary>
	/// <value>
	/// The filter string.
	/// </value>
	public string FilterString => _filterString ?? (_filterString = new FilterScanEvent(FilterData.CreateEditor()).ToString());

	/// <summary>
	/// Gets or sets the precursor array.
	/// </summary>
	/// <value>
	/// The precursor mass array.
	/// </value>]
	public double[] PrecursorMassArray { get; set; }

	/// <summary>
	/// Gets or sets the isolation width array.
	/// </summary>
	/// <value>
	/// The isolation width array.
	/// </value>
	public double[] IsolationWidthArray { get; set; }

	/// <summary>
	/// Gets or sets the filter data.
	/// </summary>
	internal ScanEvent FilterData { get; set; }
}
