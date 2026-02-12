using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Private class to pull the Base peak mass value from a scan header (MS only)
/// </summary>
internal class InstrumentBasePeakMassChromatogramPoint : IChromatogramRequest
{
	/// <summary>
	/// Gets a value indicating whether this point type needs "scan data".
	/// Always "false" for this type, as it uses only the header.
	/// </summary>
	public bool RequiresScanData => false;

	/// <summary>
	/// Gets or sets the retention time range of this chromatogram
	/// </summary>
	public IRangeAccess RetentionTimeRange { get; set; }

	/// <summary>
	/// Gets or sets the mechanism to select scans for inclusion in the chromatogram
	/// </summary>
	public IScanSelect ScanSelector { get; set; }

	/// <summary>
	/// method to calculate the chromatogram data value from a scan
	/// </summary>
	/// <param name="scan">The scan to analyze</param>
	/// <returns>The base peak of the scan</returns>
	public double ValueForScan(ISimpleScanWithHeader scan)
	{
		if (!(scan.Header is ScanIndex scanIndex))
		{
			return 0.0;
		}
		return scanIndex.BasePeakMass;
	}
}
