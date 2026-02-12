using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// Implements the IScanAveragePlus interface against a raw file
/// </summary>
public class ScanAveragerPlus : ScanAverager, IScanAveragePlus, IScanCache, IScanAverage
{
	/// <summary>
	/// Gets or sets the scan reader.
	/// </summary>
	public IScanCreator ScanReader
	{
		get
		{
			return base.ScanCreator;
		}
		set
		{
			base.ScanCreator = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.ScanAveragerPlus" /> class.
	/// </summary>
	/// <param name="rawDataReaderPlus">
	/// The raw data reader plus.
	/// </param>
	private ScanAveragerPlus(IDetectorReaderPlus rawDataReaderPlus)
		: base(rawDataReaderPlus)
	{
	}

	/// <summary>
	/// Factory Method to return the IScanAverage interface.
	/// </summary>
	/// <param name="rawData">
	/// Access to the raw data, to read the scans.
	/// </param>
	/// <returns>
	/// An interface to average scans.
	/// </returns>
	public static IScanAveragePlus FromFile(IDetectorReaderPlus rawData)
	{
		return new ScanAveragerPlus(rawData)
		{
			CacheLimit = 500
		};
	}

	/// <summary>
	/// Factory Method to return the IScanAverage interface.
	/// </summary>
	/// <param name="detectorReader">
	/// Access to the raw data, to read the scans.
	/// </param>
	/// <returns>
	/// An interface to average scans.
	/// </returns>
	public static IScanAveragePlus FromDetector(IDetectorReader detectorReader)
	{
		return new ScanAveragerPlus(detectorReader)
		{
			CacheLimit = 500
		};
	}

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
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public Scan AverageScansInTimeRange(double startTime, double endTime, string filter, MassOptions options = null)
	{
		Tuple<int, int> tuple = ScanRangeFromTimeRange(new Tuple<double, double>(startTime, endTime));
		return AverageScansInScanRange(tuple.Item1, tuple.Item2, filter, options);
	}

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
	/// filter rules
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public Scan AverageScansInTimeRange(double startTime, double endTime, IScanFilter filter, MassOptions options = null)
	{
		Tuple<int, int> tuple = ScanRangeFromTimeRange(new Tuple<double, double>(startTime, endTime));
		return AverageScansInScanRange(tuple.Item1, tuple.Item2, filter, options);
	}

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
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public Scan AverageScansInScanRange(int startScan, int endScan, string filter, MassOptions options = null)
	{
		if (options == null)
		{
			options = base.RawDataReaderPlus.DefaultMassOptions();
		}
		ScanFilterHelper filterHelper = base.RawDataReaderPlus.BuildFilterHelper(filter, options.Precision);
		MakeScanStatsListPlus(startScan, endScan, filterHelper);
		return AverageScans(userTolerance: true, options.Tolerance, ScanAverager.ConvertToleranceMode(options.ToleranceUnits));
	}

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
	/// filter rules
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public Scan AverageScansInScanRange(int startScan, int endScan, IScanFilter filter, MassOptions options = null)
	{
		if (options == null)
		{
			options = base.RawDataReaderPlus.DefaultMassOptions();
		}
		ScanFilterHelper filterHelper = base.RawDataReaderPlus.BuildFilterHelper(filter, options.Precision);
		MakeScanStatsListPlus(startScan, endScan, filterHelper);
		return AverageScans(userTolerance: true, options.Tolerance, ScanAverager.ConvertToleranceMode(options.ToleranceUnits));
	}

	/// <summary>
	/// Make scan stats list plus.
	/// </summary>
	/// <param name="startScan">
	/// The start scan.
	/// </param>
	/// <param name="endScan">
	/// The end scan.
	/// </param>
	/// <param name="filterHelper">
	/// The filter helper.
	/// </param>
	private void MakeScanStatsListPlus(int startScan, int endScan, ScanFilterHelper filterHelper)
	{
		List<ScanStatistics> list = new List<ScanStatistics>();
		IRunHeader runHeaderEx = base.RawDataReaderPlus.RunHeaderEx;
		for (int i = startScan; i <= endScan; i++)
		{
			if (i >= runHeaderEx.FirstSpectrum && i <= runHeaderEx.LastSpectrum && base.RawDataReaderPlus.TestScan(i, filterHelper))
			{
				list.Add(base.RawDataReaderPlus.GetScanStatsForScanNumber(i));
			}
		}
		ScanStatsList = list;
	}

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
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public Scan AverageScans(List<ScanStatistics> scanStatsList, MassOptions options = null)
	{
		if (options == null)
		{
			options = base.RawDataReaderPlus.DefaultMassOptions();
		}
		ScanStatsList = scanStatsList;
		return AverageScans(userTolerance: true, options.Tolerance, ScanAverager.ConvertToleranceMode(options.ToleranceUnits));
	}

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
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public Scan AverageScans(List<int> scans, MassOptions options = null, bool alwaysMergeSegments = false)
	{
		if (options == null)
		{
			options = base.RawDataReaderPlus.DefaultMassOptions();
		}
		List<ScanStatistics> list = new List<ScanStatistics>();
		foreach (int scan in scans)
		{
			list.Add(base.RawDataReaderPlus.GetScanStatsForScanNumber(scan));
		}
		ScanStatsList = list;
		return AverageScans(userTolerance: true, options.Tolerance, ScanAverager.ConvertToleranceMode(options.ToleranceUnits), alwaysMergeSegments);
	}

	/// <summary>
	/// Subtracts the background scan from the foreground scan
	/// </summary>
	/// <param name="foreground">Foreground data (Left of "scan-scan" operation</param>
	/// <param name="background">Background data (right of"scan-scan" operation)</param>
	/// <param name="options">(optional) mass tolerance options. If this is null,
	/// tolerance which is already configured in "foreground" is used</param>
	/// <returns>The result of foreground-background</returns>
	public Scan SubtractScans(Scan foreground, Scan background, MassOptions options = null)
	{
		SpectrumAverager subtractionPointer = new SpectrumAverager
		{
			FtOptions = base.FtOptions
		};
		foreground.SubtractionPointer = subtractionPointer;
		if (options != null)
		{
			foreground.ToleranceUnit = ScanAverager.ConvertToleranceMode(options.ToleranceUnits);
			foreground.MassResolution = options.Tolerance;
			foreground.IsUserTolerance = true;
		}
		return foreground - background;
	}
}
