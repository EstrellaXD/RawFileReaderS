using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The scan dependents. Provides information about the relationship between
/// data dependant scans and the scan, whose data triggered the dependant scan.
/// </summary>
internal class ScanDependents : IScanDependents
{
	/// <summary>
	/// Gets or sets the type of the raw file instrument.
	/// </summary>
	/// <value>
	/// The type of the raw file instrument.
	/// </value>
	public RawFileClassification RawFileInstrumentType { get; set; }

	/// <summary>
	/// Gets or sets the scan dependent detail array.
	/// </summary>
	/// <value>
	/// The scan dependent detail array.
	/// </value>
	public IScanDependentDetails[] ScanDependentDetailArray { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.ScanDependents" /> class.
	/// </summary>
	public ScanDependents()
	{
		RawFileInstrumentType = RawFileClassification.Indeterminate;
	}
}
