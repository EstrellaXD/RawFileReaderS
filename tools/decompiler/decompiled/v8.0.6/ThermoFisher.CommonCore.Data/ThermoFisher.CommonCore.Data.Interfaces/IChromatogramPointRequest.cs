namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines how data for a given mass range is included
/// in a chromatogram.
/// </summary>
public interface IChromatogramPointRequest
{
	/// <summary>
	/// Gets or sets a value indicating whether all data
	/// in the scan is used, or just a mass range.
	/// </summary>
	bool AllData { get; set; }

	/// <summary>
	/// Gets or sets the scale.
	/// This can be 1 to "add data in a mass range" or
	/// -1 to "subtract data a mass range",
	/// or any other value to apply scaling to a range.
	/// </summary>
	double Scale { get; set; }

	/// <summary>
	/// Gets or sets the mass range.
	/// If an application has a center mass +/ tolerance,
	/// then it a setter in a derived object could be used to convert to
	/// a range of mass. 
	/// </summary>
	IRangeAccess MassRange { get; set; }

	/// <summary>
	/// Gets or sets the rule for how a chromatogram point is created from a mass range.
	/// </summary>
	ChromatogramPointMode PointMode { get; set; }

	/// <summary>
	/// Find the data for one scan.
	/// </summary>
	/// <param name="scanWithHeader">
	/// The scan, including header and scan event.
	/// </param>
	/// <returns>
	/// The chromatogram point value for this scan.
	/// </returns>
	double DataForPoint(ISimpleScanWithHeader scanWithHeader);
}
