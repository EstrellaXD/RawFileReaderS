namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ChromatogramRequest interface.
/// Defines how chromatogram data is created from a set of scans
/// </summary>
public interface IChromatogramRequest
{
	/// <summary>
	/// Gets a value indicating whether this point type needs "scan data".
	/// This value may not change, but the current compiler does not permit a "read-only" tag.
	/// If all request for one file return false, the code can save time by never reading scan data.
	/// In any request that returns "false" from this, the ValueForScan should not access the "Data" property of the supplied scan.
	/// </summary>
	bool RequiresScanData { get; }

	/// <summary>
	/// Gets the retention time range.
	/// Only scans within this range are included.
	/// </summary>
	IRangeAccess RetentionTimeRange { get; }

	/// <summary>
	/// Gets the scan selector, which determines if a scan is in the chromatogram, or not
	/// </summary>
	IScanSelect ScanSelector { get; }

	/// <summary>
	/// Gets the value for scan.
	/// This function returns the chromatogram value for a scan.
	/// For example: An XIC from the scan data.
	/// Can use values from the scan data or index.
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <returns>
	/// The chromatogram value of this scan.
	/// </returns>
	double ValueForScan(ISimpleScanWithHeader scan);
}
