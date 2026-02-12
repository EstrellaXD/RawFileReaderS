using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A request for a Precursor mass chromatogram
/// </summary>
public class PrecursorChromatogramRequest : IChromatogramRequest
{
	/// <summary>
	/// Gets a value indicating whether this point type needs "scan data".
	/// Always "false" for this type, as it uses only the event.
	/// </summary>
	public bool RequiresScanData => false;

	/// <inheritdoc />
	public IRangeAccess RetentionTimeRange { get; set; }

	/// <inheritdoc />
	public IScanSelect ScanSelector { get; set; }

	/// <summary>
	/// Gets or sets index of the precursor mass
	/// </summary>
	public int Index { get; set; }

	/// <summary>
	/// Find the data for one scan.
	/// </summary>
	/// <param name="scanWithHeader">
	/// The scan, including header and scan event.
	/// </param>
	/// <returns>
	/// The chromatogram point value for this scan.
	/// </returns>
	public double ValueForScan(ISimpleScanWithHeader scanWithHeader)
	{
		IScanEvent scanEvent = scanWithHeader.Event;
		if (scanEvent.MSOrder >= MSOrderType.Ms2 && scanEvent.MassCount > Index)
		{
			return scanEvent.GetMass(Index);
		}
		return 0.0;
	}
}
