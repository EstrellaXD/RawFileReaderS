namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ScanDependents interface.
/// Result of call to "GetScanDependents"
/// Provides a set of scan numbers which were created form a particular master scan.
/// </summary>
public interface IScanDependents
{
	/// <summary>
	/// Gets or sets the type of the raw file instrument.
	/// </summary>
	/// <value>
	/// The type of the raw file instrument.
	/// </value>
	RawFileClassification RawFileInstrumentType { get; set; }

	/// <summary>
	/// Gets or sets the scan dependent detail array.
	/// </summary>
	/// <value>
	/// The scan dependent detail array.
	/// </value>
	IScanDependentDetails[] ScanDependentDetailArray { get; set; }
}
