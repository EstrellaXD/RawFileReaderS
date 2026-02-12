using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines information which can select a scan.
/// For example: Determine if a scan should be included in a chromatogram.
/// </summary>
public interface IScanSelect
{
	/// <summary>
	/// Gets or sets a value indicating whether the "scan filter" will be used as a selection mechanism.
	/// </summary>
	bool UseFilter { get; set; }

	/// <summary>
	/// Gets or sets the scan filter.
	/// If UseFilter is false, or this is null, it is not used as a selection mechanism.
	/// </summary>
	IScanFilter ScanFilter { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// This is the component or compound name list to filter against.
	/// If this is null or empty, it is not used as a selection mechanism.
	/// </summary>
	IList<string> Names { get; set; }
}
