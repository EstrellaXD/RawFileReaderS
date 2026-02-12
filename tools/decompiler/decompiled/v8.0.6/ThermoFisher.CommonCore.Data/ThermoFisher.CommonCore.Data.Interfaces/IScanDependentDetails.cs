namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ScanDependentDetails interface.
/// </summary>
public interface IScanDependentDetails
{
	/// <summary>
	/// Gets the index of the scan.
	/// </summary>
	/// <value>
	/// The index of the scan.
	/// </value>
	int ScanIndex { get; }

	/// <summary>
	/// Gets the filter string.
	/// </summary>
	/// <value>
	/// The filter string.
	/// </value>
	string FilterString { get; }

	/// <summary>
	/// Gets the precursor array.
	/// </summary>
	/// <value>
	/// The precursor mass array.
	/// </value>
	double[] PrecursorMassArray { get; }

	/// <summary>
	/// Gets the isolation width array.
	/// </summary>
	/// <value>
	/// The isolation width array.
	/// </value>
	double[] IsolationWidthArray { get; }
}
