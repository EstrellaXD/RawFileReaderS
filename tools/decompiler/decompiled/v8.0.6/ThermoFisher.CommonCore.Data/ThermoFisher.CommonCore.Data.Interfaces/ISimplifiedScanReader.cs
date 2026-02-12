namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to return scan data, or centroid scan data as just mass and intensity values
/// </summary>
public interface ISimplifiedScanReader
{
	/// <summary>
	/// This method is similar to GetSegmentedScanFromScanNumber in the IRawData interface.
	/// The method returns only the mass and intensity values from
	/// the scan data for a scan. 
	/// Values for flags etc. are not returned, saving data space and improving efficiency.
	/// This method never returns "reference and exception peak" data.
	/// The method is designed for improved performance in custom XIC generators.
	/// </summary>
	/// <param name="scanNumber">The scan whose mass intensity data are needed</param>
	/// <returns>Mass and intensity values from the scan.</returns>
	ISimpleScanAccess GetSimplifiedScan(int scanNumber);

	/// <summary>
	/// This method is similar to GetCentroidStream in the IRawData interface.
	/// The method returns only the mass and intensity values from
	/// the "centroid stream" data for a scan. This is also known as "Label Stream"
	/// Values for flags etc. are not returned, saving data space and improving efficiency.
	/// The method is designed for improved performance in custom XIC generators.
	/// </summary>
	/// <param name="scanNumber">The scan whose mass intensity data are needed</param>
	/// <returns>Mass and intensity values from the scan "centroid data".</returns>
	ISimpleScanAccess GetSimplifiedCentroids(int scanNumber);

	/// <summary>
	/// This method permits events to be read as a block for a range of scans,
	/// which may reduce overheads involved in requesting one by one.
	/// Events define how scans were acquired.
	/// Potentially, in some data models, the same "event" may apply to several scans
	/// so it is permissible for the same reference to appear multiple times.
	/// </summary>
	/// <param name="firstScanNumber">The first scan whose event is needed</param>
	/// <param name="lastScanNumber">The last scan</param>
	/// <returns>An array of scan events</returns>
	IScanEvent[] GetScanEvents(int firstScanNumber, int lastScanNumber);
}
