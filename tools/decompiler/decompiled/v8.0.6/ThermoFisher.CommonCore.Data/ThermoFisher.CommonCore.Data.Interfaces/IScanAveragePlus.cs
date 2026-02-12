using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to provide algorithms to average and subtract scans.
/// </summary>
public interface IScanAveragePlus : IScanCache, IScanAverage
{
	/// <summary>
	/// Gets or sets the scan reader.
	/// </summary>
	IScanCreator ScanReader { get; set; }

	/// <summary>
	/// Gets the average scan between the given times.
	/// </summary>
	/// <param name="startTime">
	/// start time
	/// </param>
	/// <param name="endTime">
	/// end time
	/// </param>
	/// <param name="filter">
	/// filter string
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <returns>
	/// the averaged scan.
	/// </returns>
	Scan AverageScansInTimeRange(double startTime, double endTime, string filter, MassOptions options = null);

	/// <summary>
	/// Gets the average scan between the given times.
	/// </summary>
	/// <param name="startTime">
	/// start time
	/// </param>
	/// <param name="endTime">
	/// end time
	/// </param>
	/// <param name="filter">
	/// filter string
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <returns>
	/// the averaged scan.
	/// </returns>
	Scan AverageScansInTimeRange(double startTime, double endTime, IScanFilter filter, MassOptions options = null);

	/// <summary>
	/// Gets the average scan between the given times.
	/// </summary>
	/// <param name="startScan">
	/// start scan
	/// </param>
	/// <param name="endScan">
	/// end scan
	/// </param>
	/// <param name="filter">
	/// filter string
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <returns>
	/// the averaged scan.
	/// </returns>
	Scan AverageScansInScanRange(int startScan, int endScan, string filter, MassOptions options = null);

	/// <summary>
	/// Gets the average scan between the given times.
	/// </summary>
	/// <param name="startScan">
	/// start scan
	/// </param>
	/// <param name="endScan">
	/// end scan
	/// </param>
	/// <param name="filter">
	/// filter string
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <returns>
	/// the averaged scan.
	/// </returns>
	Scan AverageScansInScanRange(int startScan, int endScan, IScanFilter filter, MassOptions options = null);

	/// <summary>
	/// Calculates the average spectra based upon the list supplied.
	/// The application should filter the data before making this code, to ensure that
	/// the scans are of equivalent format. The result, when the list contains scans of 
	/// different formats (such as linear trap MS centroid data added to orbitrap MS/MS profile data) is undefined.
	/// If the first scan in the list contains "FT Profile",
	/// then the FT data profile is averaged for each
	/// scan in the list. The combined profile is then centroided.
	/// If the first scan is profile data, but not orbitrap data:
	/// All scans are summed, starting from the final scan in this list, moving back to the first scan in
	/// the list, and the average is then computed.
	/// For simple centroid data formats: The scan stats "TIC" value is used to find the "most abundant scan".
	/// This scan is then used as the "first scan of the average".
	/// Scans are then added to this average, taking scans alternatively before and after
	/// the apex, merging data within tolerance.
	/// </summary>
	/// <param name="scanStatsList">
	/// list of ScanStatistics
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <returns>
	/// The average of the listed scans.
	/// </returns>
	Scan AverageScans(List<ScanStatistics> scanStatsList, MassOptions options = null);

	/// <summary>
	/// Calculates the average spectra based upon the list supplied.
	/// The application should filter the data before making this code, to ensure that
	/// the scans are of equivalent format. The result, when the list contains scans of 
	/// different formats (such as linear trap MS centroid data added to orbitrap MS/MS profile data) is undefined.
	/// If the first scan in the list contains "FT Profile",
	/// then the FT data profile is averaged for each
	/// scan in the list. The combined profile is then centroided.
	/// If the first scan is profile data, but not orbitrap data:
	/// All scans are summed, starting from the final scan in this list, moving back to the first scan in
	/// the list, and the average is then computed.
	/// For simple centroid data formats: The scan stats "TIC" value is used to find the "most abundant scan".
	/// This scan is then used as the "first scan of the average".
	/// Scans are then added to this average, taking scans alternatively before and after
	/// the apex, merging data within tolerance.
	/// </summary>
	/// <param name="scans">
	/// list of scans to average
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="alwaysMergeSegments">
	/// Merge data from scans
	/// which were not scanned over a similar range.
	/// Only applicable when scans only have a single segment.
	/// By default: Scans are considered incompatible if:
	/// The span of the scanned mass range differs by 10%
	/// The start or end of the scanned mass range differs by 10%
	/// If this is set as "true" then any mass ranges will be merged.
	/// Default: false
	/// </param>
	/// <returns>
	/// The average of the listed scans.
	/// </returns>
	Scan AverageScans(List<int> scans, MassOptions options = null, bool alwaysMergeSegments = false);

	/// <summary>
	/// Subtracts the background scan from the foreground scan
	/// </summary>
	/// <param name="foreground">Foreground data (Left of "scan-scan" operation</param>
	/// <param name="background">Background data (right of"scan-scan" operation)</param>
	/// <param name="options"> (optional) mass tolerance settings. If not supplied, values set in "foreground" are used</param>
	/// <returns>The result of foreground-background</returns>
	Scan SubtractScans(Scan foreground, Scan background, MassOptions options = null);
}
