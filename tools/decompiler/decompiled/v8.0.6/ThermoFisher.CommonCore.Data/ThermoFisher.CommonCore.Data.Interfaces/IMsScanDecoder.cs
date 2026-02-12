namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface which can decode data for a single scan from a data source (such as a byte array).
/// This interface is a preview and may be updated and extended significantly.
/// </summary>
public interface IMsScanDecoder
{
	/// <summary>
	/// Decodes data from a scan which has "centroid" data in an advanced scan format.
	/// This will only be available for scans with specific advanced packet types.
	/// </summary>
	/// <param name="includeReferenceAndExceptionData">set if calibration ref peaks need to be returned</param>
	/// <returns>Mass and intensity data for the scan</returns>
	ISimpleScanAccess DecodeSimplifiedCentroids(bool includeReferenceAndExceptionData);

	/// <summary>
	/// Decodes data from a scan. If this scan contains both centroids and profile (such as FT format data),
	/// then the profile data will be returned.
	/// </summary>
	/// <param name="includeReferenceAndExceptionData">set if calibration ref peaks need to be returned</param>
	/// <param name="calibrators">mass calibration table,
	/// needed for certain scan modes, where other units such as "time" or "frequency" are encoded</param>
	/// <returns>Mass and intensity data for the scan</returns>
	ISimpleScanAccess DecodeSimplifiedScan(bool includeReferenceAndExceptionData, double[] calibrators);

	/// <summary>
	/// This method get the noise, baselines and frequencies data.
	/// This will typically used by the application when exporting mass spec data to a raw file.<para />
	/// The advanced packet data is for LT/FT formats only.<para />
	/// </summary>
	/// <param name="includeReferenceAndExceptionData">Set if centroid data should include the reference and exception peaks</param>
	/// <param name="calibrators">Mass calibration tables needed to decode profiles</param>
	/// <returns>Returns the IAdvancedPacketData object.</returns>
	/// <exception cref="T:System.Exception">Thrown if encountered an error while retrieving LT/FT's data, i.e. noise data and frequencies.</exception>
	IAdvancedPacketData DecodeAdvancedPacketData(bool includeReferenceAndExceptionData, double[] calibrators);

	/// <summary>
	/// Get the centroids saved with a profile scan.
	/// This is only valid for data types which support
	/// multiple sets of data per scan (such as <c>Orbitrap</c> data).
	/// This method does not "Centroid profile data".
	/// </summary>
	/// <param name="includeReferenceAndExceptionPeaks">
	/// determines if peaks flagged as ref should be returned
	/// </param>
	/// <returns>
	/// access to centroid stream from the data to be decoded"/&gt;.
	/// </returns>
	ICentroidStreamAccess DecodeCentroidStream(bool includeReferenceAndExceptionPeaks);

	/// <summary>
	/// Get a segmented scan. This is the primary scan from the raw file.
	/// FT instrument files (such as Calcium) will have a second format of the scan (a centroid stream)
	/// </summary>
	/// <returns>The segmented scan.</returns>
	ISegmentedScanAccess DecodeSegmentedScan(bool includeReferenceAndExceptionData, double[] coefficients);

	/// <summary>
	/// Gets the extended scan data for a scan
	/// </summary>
	/// <returns>The extended data</returns>
	IExtendedScanData DecodeExtendedScanData();
}
