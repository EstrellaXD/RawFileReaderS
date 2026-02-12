using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to provide algorithms to average scans.
/// </summary>
public interface IScanAverage
{
	/// <summary>
	/// Gets or sets Options For FT/Orbitrap data.
	/// </summary>
	FtAverageOptions FtOptions { get; set; }

	/// <summary>
	/// Gets the average scan between the given times.
	/// Mass tolerance is taken from default values in the raw file
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
	/// <returns>
	/// the averaged scan.
	/// </returns>
	Scan GetAverageScanInTimeRange(double startTime, double endTime, string filter);

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
	/// <param name="tolerance">
	/// mass tolerance
	/// </param>
	/// <param name="toleranceMode">
	/// unit of tolerance
	/// </param>
	/// <returns>
	/// returns the averaged scan.
	/// </returns>
	Scan GetAverageScanInTimeRange(double startTime, double endTime, string filter, double tolerance, ToleranceMode toleranceMode);

	/// <summary>
	/// Gets the average scan between the given scan numbers.
	/// Mass tolerance is taken from default values in the raw file
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
	/// <returns>
	/// returns the averaged scan.
	/// </returns>
	Scan GetAverageScanInScanRange(int startScan, int endScan, string filter);

	/// <summary>
	/// Gets the average scan between the given scan numbers.
	/// This finds the list all scans over the scan range, which pass the supplied filter.
	/// The scans are then averaged, as described by <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IScanAverage.AverageSpectra(System.Collections.Generic.List{ThermoFisher.CommonCore.Data.Business.ScanStatistics})" />
	/// </summary>
	/// <param name="startScan">
	/// start scan of the range to average
	/// </param>
	/// <param name="endScan">
	/// end scan of the range to average
	/// </param>
	/// <param name="filter">
	/// Filter string. Only scans passing this filter are averaged.
	/// </param>
	/// <param name="tolerance">
	/// mass tolerance, used to merge close peaks.
	/// </param>
	/// <param name="toleranceMode">
	/// unit of tolerance
	/// </param>
	/// <returns>
	/// returns the averaged scan.
	/// </returns>
	Scan GetAverageScanInScanRange(int startScan, int endScan, string filter, double tolerance, ToleranceMode toleranceMode);

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
	/// <returns>
	/// The average of the listed scans.
	/// </returns>
	Scan AverageSpectra(List<ScanStatistics> scanStatsList);
}
