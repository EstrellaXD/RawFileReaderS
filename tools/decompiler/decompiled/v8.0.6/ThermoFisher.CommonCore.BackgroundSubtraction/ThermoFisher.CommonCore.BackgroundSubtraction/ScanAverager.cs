using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// This class owns and manages a Scan that is produced by averaging a list of scans.
/// </summary>
public class ScanAverager : IScanAverage, IScanCache
{
	private readonly IDetectorReaderBase _rawDataReader;

	internal List<ScanStatistics> ScanStatsList = new List<ScanStatistics>();

	private int _scansCached;

	private IScanReader _scanReader = Scan.CreateScanReader(0);

	/// <summary>
	/// Gets the raw data plus.
	/// </summary>
	internal IDetectorReaderPlus RawDataReaderPlus { get; }

	/// <summary>
	/// Gets or sets the scan reader.
	/// </summary>
	internal IScanCreator ScanCreator { get; set; }

	/// <summary>
	/// Gets or sets the limit to the number of scans which may be kept in a cache.
	/// This is valuable when many scan averages are requested from a raw file, with overlapping scan ranges.
	/// </summary>
	internal int CacheLimit { get; set; }

	/// <summary>
	/// Gets or sets options For FT or Orbitrap data.
	/// </summary>
	public FtAverageOptions FtOptions { get; set; }

	/// <summary>
	/// Gets RawFileToleranceMode.
	/// </summary>
	private ToleranceMode RawFileToleranceMode => ConvertToleranceMode(RawDataReaderPlus.RunHeaderEx.ToleranceUnit);

	/// <summary>
	/// Gets or sets the number of scans which may be cached.
	/// Setting ScansCached &gt;0 will enable caching of recently read scans.
	/// This is useful if averaging multiple overlapping ranges of scans.
	/// </summary>
	public int ScansCached
	{
		get
		{
			return _scansCached;
		}
		set
		{
			_scansCached = value;
			CacheLimit = value;
			_scanReader = Scan.CreateScanReader(value);
		}
	}

	/// <summary>
	/// Factory Method to return the IScanAverage interface.
	/// </summary>
	/// <param name="data">
	/// Access to the raw data, to read the scans.
	/// </param>
	/// <param name="cacheLimit">
	/// Permits the FT averaging algorithm to cache a number of scans. Default 20 
	/// </param>
	/// <returns>
	/// An interface to average scans.
	/// </returns>
	public static IScanAverage FromFile(IRawData data, int cacheLimit = 20)
	{
		return new ScanAverager(data)
		{
			CacheLimit = cacheLimit
		};
	}

	/// <summary>
	/// Factory Method to return the IScanAverage interface.
	/// </summary>
	/// <param name="rawDataReader">
	/// Access to the raw data, to read the scans.
	/// </param>
	/// <param name="cacheLimit">
	/// Permits the FT averaging algorithm to cache a number of scans. Default 20 
	/// </param>
	/// <returns>
	/// An interface to average scans.
	/// </returns>
	public static IScanAverage FromDetector(IDetectorReader rawDataReader, int cacheLimit = 20)
	{
		return new ScanAverager(rawDataReader)
		{
			CacheLimit = cacheLimit
		};
	}

	/// <summary>
	/// Gets the average scan between the given times.
	/// Mass tolerance is taken from default values in the raw file
	/// If "PermitRedirection" and the supplied raw file reader supports IScanAverage,
	/// then the averaging is performed by the file reading tool.
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
	/// returns the averaged scan.
	/// </returns>
	public Scan GetAverageScanInTimeRange(double startTime, double endTime, string filter)
	{
		return GetAverageScanInTimeRange(startTime, endTime, filter, RawDataReaderPlus.RunHeaderEx.MassResolution, RawFileToleranceMode);
	}

	/// <summary>
	/// Gets the average scan between the given times.
	/// If "PermitRedirection" and the supplied raw file reader supports IScanAverage,
	/// then the averaging is performed by the file reading tool.
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
	public Scan GetAverageScanInTimeRange(double startTime, double endTime, string filter, double tolerance, ToleranceMode toleranceMode)
	{
		return AverageScanInScanRange(filter, tolerance, toleranceMode, ScanRangeFromTimeRange(new Tuple<double, double>(startTime, endTime)));
	}

	/// <summary>
	/// Convert a time range to a scan range
	/// </summary>
	/// <param name="timeRange">
	/// The time range.
	/// </param>
	/// <returns>
	/// The scan range.
	/// </returns>
	protected Tuple<int, int> ScanRangeFromTimeRange(Tuple<double, double> timeRange)
	{
		return new Tuple<int, int>(_rawDataReader.ScanNumberFromRetentionTime(timeRange.Item1), _rawDataReader.ScanNumberFromRetentionTime(timeRange.Item2));
	}

	/// <summary>
	/// Average scan in a given scan range.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <param name="tolerance">
	/// The tolerance.
	/// </param>
	/// <param name="toleranceMode">
	/// The tolerance mode.
	/// </param>
	/// <param name="scans">
	/// The start scan (Item1) and end scan (Item2).
	/// </param>
	/// <returns>
	/// The average of the scans in range which match the filter.
	/// </returns>
	private Scan AverageScanInScanRange(string filter, double tolerance, ToleranceMode toleranceMode, Tuple<int, int> scans)
	{
		ScanStatsList = GetScanStatistics(scans.Item1, scans.Item2, filter);
		return AverageScans(userTolerance: true, tolerance, toleranceMode);
	}

	/// <summary>
	/// Gets the average scan between the given times.
	/// Mass tolerance is taken from default values in the raw file
	/// If "PermitRedirection" and the supplied raw file reader supports IScanAverage,
	/// then the averaging is performed by the file reading tool.
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
	public Scan GetAverageScanInScanRange(int startScan, int endScan, string filter)
	{
		return GetAverageScanInScanRange(startScan, endScan, filter, RawDataReaderPlus.RunHeaderEx.MassResolution, RawFileToleranceMode);
	}

	/// <summary>
	/// Gets the average scan between the given times.
	/// If "PermitRedirection" and the supplied raw file reader supports IScanAverage,
	/// then the averaging is performed by the file reading tool.
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
	/// <param name="tolerance">
	/// mass tolerance
	/// </param>
	/// <param name="toleranceMode">
	/// unit of tolerance
	/// </param>
	/// <returns>
	/// returns the averaged scan.
	/// </returns>
	public Scan GetAverageScanInScanRange(int startScan, int endScan, string filter, double tolerance, ToleranceMode toleranceMode)
	{
		return AverageScanInScanRange(filter, tolerance, toleranceMode, new Tuple<int, int>(startScan, endScan));
	}

	/// <summary>
	/// Calculates the average spectra based upon the list supplied.
	/// If "PermitRedirection" and the supplied raw file reader supports IScanAverage,
	/// then the averaging is performed by the file reading tool.
	/// </summary>
	/// <param name="scanStatsList">
	/// list of ScanStatistics
	/// </param>
	/// <returns>
	/// The average of all scans in the list
	/// </returns>
	public Scan AverageSpectra(List<ScanStatistics> scanStatsList)
	{
		ScanStatsList = scanStatsList;
		return AverageScans(userTolerance: true, RawDataReaderPlus.RunHeaderEx.MassResolution, RawFileToleranceMode);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.ScanAverager" /> class.
	/// </summary>
	/// <param name="data">
	/// The data.
	/// </param>
	protected ScanAverager(IDetectorReaderBase data)
	{
		_rawDataReader = data;
		RawDataReaderPlus = data as IDetectorReaderPlus;
		FtOptions = new FtAverageOptions();
	}

	/// <summary>
	/// Calculates the average scan given the list of scans.
	/// </summary>
	/// <param name="userTolerance">
	/// To use User Tolerance or not
	/// </param>
	/// <param name="tolerance">
	/// Tolerance value
	/// </param>
	/// <param name="unitType">
	/// Types of Tolerance units (MMU,PPM...)
	/// </param>
	/// <param name="alwaysMergeSegments">Always merge segments, regardless of mass range (where there is only one segment)</param>
	/// <returns>
	/// The averaged scans.
	/// </returns>
	protected Scan AverageScans(bool userTolerance, double tolerance, ToleranceMode unitType, bool alwaysMergeSegments = false)
	{
		int count = ScanStatsList.Count;
		if (count == 0)
		{
			return null;
		}
		Scan scanFromScanNumber = _scanReader.GetScanFromScanNumber(_rawDataReader, ScanStatsList[0].ScanNumber);
		bool flag = CommonData.IsProfileScan(scanFromScanNumber.ScanStatistics.PacketType);
		bool dataDependent = ScanDefinition.FromString(scanFromScanNumber.ScanType).DataDependent;
		if (CommonData.LOWord(scanFromScanNumber.ScanStatistics.PacketType) == 21)
		{
			return AverageFtProfiles(scanFromScanNumber, ScanCreator);
		}
		int scanCount;
		Scan scan;
		if (flag && !dataDependent)
		{
			Scan scanFromScanNumber2 = _scanReader.GetScanFromScanNumber(_rawDataReader, ScanStatsList[count - 1].ScanNumber);
			scanFromScanNumber2.ScanAdder = new SpectrumAverager();
			scan = scanFromScanNumber2;
			scanCount = 1;
			scan.ToleranceUnit = unitType;
			scan.MassResolution = tolerance;
			scan.IsUserTolerance = userTolerance;
			scan = AddAllScansToAverage(count, scan, ref scanCount);
		}
		else
		{
			int indexOfScanWithHighestTic = GetIndexOfScanWithHighestTic(count);
			Scan scanFromScanNumber3 = _scanReader.GetScanFromScanNumber(_rawDataReader, ScanStatsList[indexOfScanWithHighestTic].ScanNumber);
			scanFromScanNumber3.ScanAdder = new SpectrumAverager();
			scanFromScanNumber3.AlwaysMergeSegments = alwaysMergeSegments;
			scan = scanFromScanNumber3;
			scanCount = 1;
			scan.ToleranceUnit = unitType;
			scan.MassResolution = tolerance;
			scan.IsUserTolerance = userTolerance;
			long num = Math.Max(indexOfScanWithHighestTic, count - indexOfScanWithHighestTic);
			for (int i = 1; i <= num; i++)
			{
				int num2 = indexOfScanWithHighestTic - i;
				if (num2 >= 0)
				{
					scan = AddScanToAverage(scan, ref scanCount, num2);
				}
				int num3 = indexOfScanWithHighestTic + i;
				if (num3 < count)
				{
					scan = AddScanToAverage(scan, ref scanCount, num3);
				}
			}
		}
		if (scanCount > 1)
		{
			scan /= (double)scanCount;
		}
		scan.ScansCombined = scanCount;
		return scan;
	}

	/// <summary>
	/// Average ft profiles.
	/// </summary>
	/// <param name="firstScan">
	///     The first scan.
	/// </param>
	/// <param name="ftmsScanCreator">tool to read and cache scans</param>
	/// <returns>
	/// The average of the listed scans.
	/// </returns>
	private Scan AverageFtProfiles(Scan firstScan, IScanCreator ftmsScanCreator)
	{
		return new SpectrumAverager
		{
			FtOptions = FtOptions
		}.GetAverage(_rawDataReader, ScanStatsList, firstScan, CacheLimit, ftmsScanCreator);
	}

	/// <summary>
	/// Add all scans to the average.
	/// </summary>
	/// <param name="scanInList">
	/// The scan in list.
	/// </param>
	/// <param name="averageScan">
	/// The average scan so far.
	/// </param>
	/// <param name="scanCount">
	/// The scan count.
	/// </param>
	/// <returns>
	/// The averaged scan.
	/// </returns>
	private Scan AddAllScansToAverage(int scanInList, Scan averageScan, ref int scanCount)
	{
		for (int num = scanInList - 2; num >= 0; num--)
		{
			Scan scanFromScanNumber = _scanReader.GetScanFromScanNumber(_rawDataReader, ScanStatsList[num].ScanNumber);
			averageScan += scanFromScanNumber;
			scanCount++;
		}
		return averageScan;
	}

	/// <summary>
	/// Add a scan to the average, provided it can be merged (is compatible) with the
	/// average so far.
	/// </summary>
	/// <param name="averageScan">
	/// Average so far
	/// </param>
	/// <param name="scanCount">
	/// Count of scans successfully added to the average
	/// </param>
	/// <param name="indexOfScanToAdd">
	/// Index into table of scans
	/// </param>
	/// <returns>
	/// The updated scan
	/// </returns>
	private Scan AddScanToAverage(Scan averageScan, ref int scanCount, int indexOfScanToAdd)
	{
		bool identicalFlag = false;
		Scan scanFromScanNumber = _scanReader.GetScanFromScanNumber(_rawDataReader, ScanStatsList[indexOfScanToAdd].ScanNumber);
		if (Scan.CanMergeScan(ref identicalFlag, averageScan, scanFromScanNumber))
		{
			averageScan += scanFromScanNumber;
			scanCount++;
		}
		return averageScan;
	}

	/// <summary>
	/// Find the scan number with the highest TIC value
	/// </summary>
	/// <param name="scansInList">
	/// number of scans to search
	/// </param>
	/// <returns>
	/// index into ScanStatsList with largest TIC
	/// </returns>
	private int GetIndexOfScanWithHighestTic(int scansInList)
	{
		int result = 0;
		double num = 0.0;
		for (int i = 0; i < scansInList; i++)
		{
			double tIC = ScanStatsList[i].TIC;
			if (tIC > num)
			{
				num = tIC;
				result = i;
			}
		}
		return result;
	}

	/// <summary>
	/// convert tolerance mode.
	/// </summary>
	/// <param name="unit">
	/// The unit.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.ToleranceMode" />.
	/// </returns>
	internal static ToleranceMode ConvertToleranceMode(ToleranceUnits unit)
	{
		return unit switch
		{
			ToleranceUnits.mmu => ToleranceMode.Mmu, 
			ToleranceUnits.ppm => ToleranceMode.Ppm, 
			ToleranceUnits.amu => ToleranceMode.Amu, 
			_ => ToleranceMode.None, 
		};
	}

	/// <summary>
	/// Get the scan statistics.
	/// </summary>
	/// <param name="start">
	/// The start scan.
	/// </param>
	/// <param name="end">
	/// The end scan.
	/// </param>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The list of scan statistics for scans matching the filter.
	/// </returns>
	private List<ScanStatistics> GetScanStatistics(int start, int end, string filter)
	{
		List<ScanStatistics> list = new List<ScanStatistics>();
		if (RawDataReaderPlus != null)
		{
			ScanFilterHelper filterHelper = RawDataReaderPlus.BuildFilterHelper(filter);
			for (int i = start; i <= end; i++)
			{
				if (RawDataReaderPlus.TestScan(i, filterHelper))
				{
					list.Add(_rawDataReader.GetScanStatsForScanNumber(i));
				}
			}
		}
		else
		{
			ScanDefinition scanDefinition = ScanDefinition.FromString(filter);
			MassOptions precursorMassTolerance = new MassOptions(0.0);
			for (int j = start; j <= end; j++)
			{
				string scanType = _rawDataReader.GetScanType(j);
				if (scanDefinition.Match(ScanDefinition.FromString(scanType), precursorMassTolerance))
				{
					list.Add(_rawDataReader.GetScanStatsForScanNumber(j));
				}
			}
		}
		return list;
	}
}
