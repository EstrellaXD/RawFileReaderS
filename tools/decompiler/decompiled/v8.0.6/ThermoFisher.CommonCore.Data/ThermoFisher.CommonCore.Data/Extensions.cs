using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Extension methods.
/// Includes extensions to several types and interfaces defined in this DLL,
/// plus some extensions to common .Net types. 
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Determines whether [the specified source] [is null or empty] .
	/// </summary>
	/// <typeparam name="T">Type parameter
	/// </typeparam>
	/// <param name="source">
	/// The source.
	/// </param>
	/// <returns>
	/// <see langword="true" /> if [the specified source] [is null or empty]; otherwise, <see langword="false" />.
	/// </returns>
	public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			return true;
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		return !enumerator.MoveNext();
	}

	/// <summary>
	/// Test if a value is within range
	/// </summary>
	/// <param name="range">
	/// The range.
	/// </param>
	/// <param name="value">
	/// The value.
	/// </param>
	/// <returns>
	/// True if the value is within or equal to the range limits (closed range).
	/// </returns>
	public static bool Contains(this IRangeAccess range, double value)
	{
		if (value >= range.Low)
		{
			return value <= range.High;
		}
		return false;
	}

	/// <summary>
	/// find the first data value which is equal to or after start
	/// (for example, first value within a tolerance range).
	/// Similar to Array.Binary search, but faster
	/// as does not need to call comparer functions
	/// </summary>
	/// <param name="data">
	/// Array of increasing data (for example masses in a scan)
	/// </param>
	/// <param name="start">
	/// Position to look for (for example start of X axis range)
	/// </param>
	/// <returns>
	/// The first value after start, or -1 if there are no points after start
	/// </returns>
	public static int FastBinarySearch(this double[] data, double start)
	{
		if (data == null)
		{
			return -1;
		}
		int num = data.Length;
		if (num == 0)
		{
			return -1;
		}
		if (data[0] >= start)
		{
			return 0;
		}
		int num2 = 0;
		int num3 = num - 1;
		if (data[num3] < start)
		{
			return -1;
		}
		while (num3 > num2 + 1)
		{
			int num4 = (num3 + num2) / 2;
			if (data[num4] < start)
			{
				num2 = num4;
			}
			else
			{
				num3 = num4;
			}
		}
		return num3;
	}

	/// <summary>
	/// Create the ion chromatogram value from a scan data
	/// which is the sum of the intensities of all masses between given limits.
	/// These limits are typically calculated (elsewhere) based on a known mass +/- tolerance.
	/// </summary>
	/// <param name="scan">
	/// The scan data.
	/// </param>
	/// <param name="lowMassLimit">
	/// The low mass limit.
	/// </param>
	/// <param name="highMassLimit">
	/// The high mass limit.
	/// </param>
	/// <returns>
	/// The summed intensities.
	/// </returns>
	public static double IntensitySum(this ISimpleScanAccess scan, double lowMassLimit, double highMassLimit)
	{
		double[] intensities = scan.Intensities;
		double num = 0.0;
		if (intensities != null)
		{
			double[] masses = scan.Masses;
			int num2 = masses.Length;
			int num3 = masses.FastBinarySearch(lowMassLimit);
			if (num3 < 0)
			{
				return 0.0;
			}
			while (num3 < num2 && masses[num3] <= highMassLimit)
			{
				num += intensities[num3++];
			}
		}
		return num;
	}

	/// <summary>
	/// Create the ion chromatogram value from a centroid stream,
	/// which is the largest of the positive intensities of all masses between given limits.
	/// These limits are typically calculated (elsewhere) based on a known mass +/- tolerance.
	/// If there are no peaks in range, or all data in range has negative intensities, zero is returned.
	/// </summary>
	/// <param name="centroids">
	/// The centroids.
	/// </param>
	/// <param name="lowMassLimit">
	/// The low mass limit.
	/// </param>
	/// <param name="highMassLimit">
	/// The high mass limit.
	/// </param>
	/// <returns>
	/// The largest positive peak intensity within limits, or zero if there are no positive values within limits.
	/// </returns>
	public static double LargestIntensity(this ISimpleScanAccess centroids, double lowMassLimit, double highMassLimit)
	{
		double[] intensities = centroids.Intensities;
		double num = 0.0;
		if (intensities != null)
		{
			double[] masses = centroids.Masses;
			int num2 = masses.Length;
			int num3 = masses.FastBinarySearch(lowMassLimit);
			if (num3 < 0)
			{
				return 0.0;
			}
			while (num3 < num2 && masses[num3] <= highMassLimit)
			{
				double num4 = intensities[num3++];
				if (num4 > num)
				{
					num = num4;
				}
			}
		}
		return num;
	}

	/// <summary>
	/// Create the ion chromatogram value from a scan,
	/// which is mass of the largest of the positive intensities of all masses between given limits.
	/// These limits are typically calculated (elsewhere) based on a known mass +/- tolerance.
	/// If there are no peaks in range, or all data in range has negative intensities, zero is returned.
	/// </summary>
	/// <param name="centroids">
	/// The centroids.
	/// </param>
	/// <param name="lowMassLimit">
	/// The low mass limit.
	/// </param>
	/// <param name="highMassLimit">
	/// The high mass limit.
	/// </param>
	/// <returns>
	/// The mass of the peak with the largest positive peak intensity within limits,
	/// or zero if there are no positive values within limits.
	/// </returns>
	public static double MassAtLargestIntensity(this ISimpleScanAccess centroids, double lowMassLimit, double highMassLimit)
	{
		double[] intensities = centroids.Intensities;
		double num = 0.0;
		double result = 0.0;
		if (intensities != null)
		{
			double[] masses = centroids.Masses;
			int num2 = masses.Length;
			int num3 = masses.FastBinarySearch(lowMassLimit);
			if (num3 < 0)
			{
				return 0.0;
			}
			while (num3 < num2 && masses[num3] <= highMassLimit)
			{
				double num4 = intensities[num3++];
				if (num4 > num)
				{
					num = num4;
					result = masses[num3 - 1];
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Create the ion chromatogram value from a scan,
	/// which is mass of the largest of the positive intensities of all masses between given limits.
	/// These limits are typically calculated (elsewhere) based on a known mass +/- tolerance.
	/// If there are no peaks in range, or all data in range has negative intensities, zero is returned.
	/// </summary>
	/// <param name="centroids">
	/// The centroids.
	/// </param>
	/// <param name="lowMassLimit">
	/// The low mass limit.
	/// </param>
	/// <param name="highMassLimit">
	/// The high mass limit.
	/// </param>
	/// <returns>
	/// The mass of the peak with the largest positive peak intensity within limits,
	/// or zero if there are no positive values within limits.
	/// </returns>
	public static Tuple<double, double> MassAndIntensityAtLargestIntensity(this ISimpleScanAccess centroids, double lowMassLimit, double highMassLimit)
	{
		double[] intensities = centroids.Intensities;
		double num = 0.0;
		double item = 0.0;
		if (intensities != null)
		{
			double[] masses = centroids.Masses;
			int num2 = masses.Length;
			int num3 = masses.FastBinarySearch(lowMassLimit);
			if (num3 < 0)
			{
				return new Tuple<double, double>(0.0, 0.0);
			}
			int num4 = masses.FastBinarySearch(highMassLimit);
			int num5 = num2;
			if (num4 >= 0 && num4 < masses.Length)
			{
				if (masses[num4] == highMassLimit)
				{
					num4++;
				}
				num5 = Math.Min(num2, num4);
			}
			int num6 = -1;
			while (num3 < num5)
			{
				if (intensities[num3++] > num)
				{
					num6 = num3 - 1;
					num = intensities[num6];
				}
			}
			if (num6 >= 0)
			{
				item = masses[num6];
			}
		}
		return new Tuple<double, double>(item, num);
	}

	/// <summary>
	/// Create the ion chromatogram value from a scan,
	/// which is mass of the largest of the positive intensities of all masses between given limits.
	/// These limits are typically calculated (elsewhere) based on a known mass +/- tolerance.
	/// If there are no peaks in range, or all data in range has negative intensities, zero is returned.
	/// </summary>
	/// <param name="centroids">
	/// The centroids.
	/// </param>
	/// <returns>
	/// The mass of the peak with the largest positive peak intensity within limits,
	/// or zero if there are no positive values within limits.
	/// </returns>
	public static double MassAtLargestIntensity(this ISimpleScanAccess centroids)
	{
		double[] intensities = centroids.Intensities;
		double num = 0.0;
		double result = 0.0;
		if (intensities != null)
		{
			double[] masses = centroids.Masses;
			int num2 = masses.Length;
			int num3 = 0;
			while (num3 < num2)
			{
				double num4 = intensities[num3++];
				if (num4 > num)
				{
					num = num4;
					result = masses[num3 - 1];
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Create the ion chromatogram value from a scan,
	/// which is mass of the largest of the positive intensities of all masses between given limits.
	/// These limits are typically calculated (elsewhere) based on a known mass +/- tolerance.
	/// If there are no peaks in range, or all data in range has negative intensities, zero is returned.
	/// </summary>
	/// <param name="centroids">
	/// The centroids.
	/// </param>
	/// <returns>
	/// The mass of the peak with the largest positive peak intensity within limits,
	/// or zero if there are no positive values within limits.
	/// </returns>
	public static Tuple<double, double> MassAndIntensityAtLargestIntensity(this ISimpleScanAccess centroids)
	{
		double[] intensities = centroids.Intensities;
		double num = 0.0;
		double item = 0.0;
		if (intensities != null)
		{
			double[] masses = centroids.Masses;
			int num2 = masses.Length;
			int num3 = 0;
			while (num3 < num2)
			{
				double num4 = intensities[num3++];
				if (num4 > num)
				{
					num = num4;
					item = masses[num3 - 1];
				}
			}
		}
		return new Tuple<double, double>(item, num);
	}

	/// <summary>
	/// Perform binary search on IList
	/// </summary>
	/// <param name="list">
	/// The list.
	/// </param>
	/// <param name="value">
	/// The value.
	/// </param>
	/// <param name="comparer">
	/// The comparer.
	/// </param>
	/// <typeparam name="T">
	/// Type of items in list
	/// </typeparam>
	/// <returns>
	/// The index of the found item, or compliment of the next item after the supplied value.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">
	/// Thrown if arguments are null
	/// </exception>
	public static int BinarySearch<T>(this IList<T> list, T value, IComparer<T> comparer)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		int num = 0;
		int num2 = list.Count - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num) / 2;
			int num4 = comparer.Compare(value, list[num3]);
			if (num4 < 0)
			{
				num2 = num3 - 1;
				continue;
			}
			if (num4 > 0)
			{
				num = num3 + 1;
				continue;
			}
			return num3;
		}
		return ~num;
	}

	/// <summary>
	/// Test if a raw file has MS data.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <returns>
	/// True if the file has MS data
	/// </returns>
	public static bool HasMsData(this IRawData data)
	{
		return data.GetInstrumentCountOfType(Device.MS) >= 1;
	}

	/// <summary>
	/// Test if a raw file has MS data, and select the MS detector, if available.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <returns>
	/// True if the MS data has been selected.
	/// False if the file does not have MS data.
	/// </returns>
	public static bool SelectMsData(this IRawData data)
	{
		if (data.HasMsData())
		{
			data.SelectInstrument(Device.MS, 1);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Get a filtered scan enumerator, to obtain the collection of scans matching given filter rules.
	/// Gets scans within the given time range.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <param name="startTime">
	/// The start Time.
	/// </param>
	/// <param name="endTime">
	/// The end Time.
	/// </param>
	/// <returns>
	/// The collection of scans
	/// </returns>
	public static IEnumerable<int> GetFilteredScanEnumeratorOverTime(this IRawDataPlus data, string filter, double startTime, double endTime)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		IScanFilter filterFromString = data.GetFilterFromString(filter);
		if (filterFromString == null)
		{
			throw new ArgumentException("Invalid filter format: " + filter, "filter");
		}
		return data.GetFilteredScanEnumeratorOverTime(filterFromString, startTime, endTime);
	}

	/// <summary>
	/// Calculate the filters for this raw file for scans within the time range supplied.
	/// </summary>
	///             <param name="data">the raw data</param>
	/// <param name="startTime">start of time window</param>
	/// <param name="endTime">end of time window</param>
	/// <returns>
	/// Auto generated list of unique filters
	/// </returns>  
	public static ReadOnlyCollection<IScanFilter> GetFiltersForTimeRange(this IRawDataPlus data, double startTime, double endTime)
	{
		if (ScansIncludedInTimeRange(data, startTime, endTime, out var first, out var last))
		{
			return data.GetFiltersForScanRange(first, last);
		}
		return new ReadOnlyCollection<IScanFilter>(new IScanFilter[0]);
	}

	/// <summary>
	/// Calculate the filters for this raw file for scans within the time range supplied, with accurate precursor matching.
	/// </summary>
	///             <param name="data">the raw data</param>
	/// <param name="startTime">start of time window</param>
	/// <param name="endTime">end of time window</param>
	/// <param name="mode">Optional: Determine how precursor tolerance is handled.
	/// Default: Use value provided by instrument in run header</param>
	/// <param name="decimalPlaces">Optional: When a specified tolerance is specified, then a number of matched decimal places 
	/// can be specified (default 2)</param>
	/// <returns>
	/// Auto generated list of unique filters
	/// </returns>  
	public static ReadOnlyCollection<IScanFilter> GetAccurateFiltersForTimeRange(this IRawDataPlus data, double startTime, double endTime, FilterPrecisionMode mode, int decimalPlaces)
	{
		if (ScansIncludedInTimeRange(data, startTime, endTime, out var first, out var last))
		{
			return data.GetAccurateFiltersForScanRange(first, last, mode, decimalPlaces);
		}
		return new ReadOnlyCollection<IScanFilter>(new IScanFilter[0]);
	}

	/// <summary>
	/// Calculate the compound names for this raw file for scans within the time range supplied.
	/// </summary>
	///             <param name="data">the raw data</param>
	/// <param name="startTime">start of time window</param>
	/// <param name="endTime">end of time window</param>
	/// <returns>
	/// Array of compound names within the supplied time range
	/// </returns>  
	public static string[] GetCompoundNamesForTimeRange(this IRawDataPlus data, double startTime, double endTime)
	{
		if (ScansIncludedInTimeRange(data, startTime, endTime, out var first, out var last))
		{
			return data.GetCompoundNamesForScanRange(first, last);
		}
		return new string[0];
	}

	/// <summary>
	/// Get the scans included in the time range
	/// </summary>
	/// <param name="data">raw file</param>
	/// <param name="startTime">start time of range</param>
	/// <param name="endTime">end time of range</param>
	/// <param name="first">first scan in range</param>
	/// <param name="last">last scan in range</param>
	/// <returns>True if scans numbers are returned</returns>
	private static bool ScansIncludedInTimeRange(IRawDataPlus data, double startTime, double endTime, out int first, out int last)
	{
		first = data.ScanNumberFromRetentionTime(startTime);
		if (first == -1)
		{
			last = -1;
			return false;
		}
		IRunHeader runHeaderEx = data.RunHeaderEx;
		if (data.RetentionTimeFromScanNumber(first) < startTime && first < runHeaderEx.LastSpectrum)
		{
			first++;
		}
		last = data.ScanNumberFromRetentionTime(endTime);
		if (data.RetentionTimeFromScanNumber(last) > endTime && last > runHeaderEx.FirstSpectrum)
		{
			last--;
		}
		return last >= first;
	}

	/// <summary>
	/// Get a filtered scan enumerator, to obtain the collection of scans matching given filter rules.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The collection of scans
	/// </returns>
	public static IEnumerable<int> GetFilteredScanEnumerator(this IDetectorReaderPlus data, string filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		IScanFilter filterFromString = data.GetFilterFromString(filter);
		if (filterFromString == null)
		{
			throw new ArgumentException("Invalid filter format: " + filter, "filter");
		}
		return data.GetFilteredScanEnumerator(filterFromString);
	}

	/// <summary>
	/// Get a scan enumerator, to obtain the collection of scans matching a given compound.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="compound">
	/// The filter.
	/// </param>
	/// <returns>
	/// The collection of scans
	/// </returns>
	public static IEnumerable<int> GetCompoundScanEnumerator(this IRawDataPlus data, string compound)
	{
		if (compound == null)
		{
			throw new ArgumentNullException("compound");
		}
		IRunHeader header = data.RunHeaderEx;
		for (int scanNumber = header.FirstSpectrum; scanNumber <= header.LastSpectrum; scanNumber++)
		{
			if (ScanIsOfCompound(data, scanNumber, compound))
			{
				yield return scanNumber;
			}
		}
	}

	/// <summary>
	/// Test is a scan is of a given compound
	/// </summary>
	/// <param name="rawData">raw data reader</param>
	/// <param name="scanNumber">scan number</param>
	/// <param name="compound">compound name</param>
	/// <returns>true if this is a scan of the compound</returns>
	private static bool ScanIsOfCompound(IDetectorReaderPlus rawData, int scanNumber, string compound)
	{
		return rawData.GetScanEventForScanNumber(scanNumber).Name == compound;
	}

	/// <summary>
	/// Get a scan enumerator, to obtain the collection of scans matching a given compound, within a given time range
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="compound">
	/// The compound name.
	/// </param>
	/// <param name="startTime">
	/// The start Time.
	/// </param>
	/// <param name="endTime">
	/// The end Time.
	/// </param>/// <returns>
	/// The collection of scans within the supplied time range, which have this compound name
	/// </returns>
	public static IEnumerable<int> GetCompoundScanEnumeratorOverTime(this IRawDataPlus data, string compound, double startTime, double endTime)
	{
		if (compound == null)
		{
			throw new ArgumentNullException("compound");
		}
		Tuple<int, int> scans = data.ScanRangeWithinTimeRange(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime));
		for (int scanNumber = scans.Item1; scanNumber <= scans.Item2; scanNumber++)
		{
			if (ScanIsOfCompound(data, scanNumber, compound))
			{
				yield return scanNumber;
			}
		}
	}

	/// <summary>
	/// Test if a scan passes a filter.
	/// This extension is provided for improved efficiency
	/// where the same filter string needs to be used to test multiple scans,
	/// without repeating the parsing.
	/// Parsing can be done using: GetFilterFromString(string filter), 
	/// with the IRawDataPlus interface.
	/// Also consider using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
	/// when processing all scans in a file.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="scan">the scan number</param>
	/// <param name="filter">the filter to test</param>
	/// <returns>
	/// True if this scan passes the filter
	/// </returns>
	public static bool TestScan(this IRawDataPlus data, int scan, IScanFilter filter)
	{
		return data.TestScan(scan, data.BuildFilterHelper(filter));
	}

	/// <summary>
	/// Test if a scan passes a filter.
	/// This extension is provided for improved efficiency
	/// where the same filter string needs to be used to test multiple scans,
	/// without repeating the parsing. Consider using one of the overloads of BuildFilterHelper()
	/// Parsing can be done using: GetFilterFromString(string filter), 
	/// with the IRawDataPlus interface.
	/// Also consider using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
	/// when processing all scans in a file.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="scan">the scan number</param>
	/// <param name="filterHelper">the filter to test</param>
	/// <returns>
	/// True if this scan passes the filter
	/// </returns>
	public static bool TestScan(this IDetectorReaderPlus data, int scan, ScanFilterHelper filterHelper)
	{
		if (filterHelper == null)
		{
			throw new ArgumentNullException("filterHelper");
		}
		return ScanEventHelper.ScanEventHelperFactory(data.GetScanEventForScanNumber(scan)).TestScanAgainstFilter(filterHelper);
	}

	/// <summary>
	/// Test if a scan passes a filter.
	/// This extension is provided for improved efficiency
	/// where the same filter string needs to be used to test multiple scans,
	/// without repeating the parsing. Consider using one of the overloads of BuildFilterHelper()
	/// Parsing can be done using: GetFilterFromString(string filter), 
	/// with the IRawDataPlus interface.
	/// Also consider using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
	/// when processing all scans in a file.
	/// </summary>
	/// <param name="detectorReader">
	/// The detector reader data.
	/// </param>
	/// <param name="scan">the scan number</param>
	/// <param name="filterHelper">the filter to test</param>
	/// <returns>
	/// True if this scan passes the filter
	/// </returns>
	public static bool TestScan(this IDetectorReader detectorReader, int scan, ScanFilterHelper filterHelper)
	{
		if (filterHelper == null)
		{
			throw new ArgumentNullException("filterHelper");
		}
		return ScanEventHelper.ScanEventHelperFactory(detectorReader.GetScanEventForScanNumber(scan)).TestScanAgainstFilter(filterHelper);
	}

	/// <summary>
	/// Constructs an object which has an analysis of the selections being made by
	/// a scan filter. Improves efficiency when validating many scans against a filter.
	/// Filter precision is automatically set, based on the file.
	/// </summary>
	/// <param name="rawData">
	/// The raw data.
	/// </param>
	/// <param name="filter">the filter to analyze</param>
	/// <returns>
	/// Helper, to efficiently validate scans.
	/// </returns>
	public static ScanFilterHelper BuildFilterHelper(this IDetectorReaderPlus rawData, string filter)
	{
		IScanFilter filterFromString = rawData.GetFilterFromString(filter);
		if (filterFromString == null)
		{
			throw new ArgumentException("Invalid filter format: " + filter, "filter");
		}
		return rawData.BuildFilterHelper(filterFromString);
	}

	/// <summary>
	/// Constructs an object which has an analysis of the selections being made by
	/// a scan filter. Improves efficiency when validating many scans against a filter.
	/// Filter precision is automatically set, based on the file.
	/// </summary>
	/// <param name="detectorReader">
	/// The detector reader.
	/// </param>
	/// <param name="filter">the filter to analyze</param>
	/// <returns>
	/// Helper, to efficiently validate scans.
	/// </returns>
	public static ScanFilterHelper BuildFilterHelper(this IDetectorReader detectorReader, string filter)
	{
		IScanFilter filterFromString = detectorReader.GetFilterFromString(filter);
		if (filterFromString == null)
		{
			throw new ArgumentException("Invalid filter format: " + filter, "filter");
		}
		return detectorReader.BuildFilterHelper(filterFromString);
	}

	/// <summary>
	/// Constructs an object which has an analysis of the selections being made by
	/// a scan filter. Improves efficiency when validating many scans against a filter.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="filter">the filter to analyze</param>
	/// <param name="precision">Precision of filter</param>
	/// <returns>
	/// Helper, to efficiently validate scans.
	/// </returns>
	public static ScanFilterHelper BuildFilterHelper(this IDetectorReaderPlus data, string filter, int precision)
	{
		IScanFilter filterFromString = data.GetFilterFromString(filter);
		if (filterFromString == null)
		{
			throw new ArgumentException("Invalid filter format: " + filter, "filter");
		}
		return data.BuildFilterHelper(filterFromString, precision);
	}

	/// <summary>
	/// Constructs an object which has an analysis of the selections being made by
	/// a scan filter. Improves efficiency when validating many scans against a filter.
	/// Filter precision is automatically set from the raw file.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="filter">the filter to analyze</param>
	/// <returns>
	/// Helper, to efficiently validate scans.
	/// </returns>
	public static ScanFilterHelper BuildFilterHelper(this IDetectorReaderPlus data, IScanFilter filter)
	{
		int filterMassPrecision = data.RunHeaderEx.FilterMassPrecision;
		return data.BuildFilterHelper(filter, filterMassPrecision);
	}

	/// <summary>
	/// Constructs an object which has an analysis of the selections being made by
	/// a scan filter. Improves efficiency when validating many scans against a filter.
	/// Filter precision is automatically set from the raw file.
	/// </summary>
	/// <param name="detectorReader">
	/// The detector reader data.
	/// </param>
	/// <param name="filter">the filter to analyze</param>
	/// <returns>
	/// Helper, to efficiently validate scans.
	/// </returns>
	public static ScanFilterHelper BuildFilterHelper(this IDetectorReader detectorReader, IScanFilter filter)
	{
		int filterMassPrecision = detectorReader.RunHeaderEx.FilterMassPrecision;
		return detectorReader.BuildFilterHelper(filter, filterMassPrecision);
	}

	/// <summary>
	/// Constructs an object which has an analysis of the selections being made by
	/// a scan filter. Improves efficiency when validating many scans against a filter.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="filter">the filter to analyze</param>
	/// <param name="precision">Filter Precision</param>
	/// <returns>
	/// Helper, to efficiently validate scans.
	/// </returns>
	public static ScanFilterHelper BuildFilterHelper(this IDetectorReaderPlus data, IScanFilter filter, int precision)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		bool accuratePrecursors = data.GetInstrumentData().IsTsqQuantumFile();
		return new ScanFilterHelper(filter, accuratePrecursors, precision);
	}

	/// <summary>
	/// Constructs an object which has an analysis of the selections being made by
	/// a scan filter. Improves efficiency when validating many scans against a filter.
	/// </summary>
	/// <param name="detectorReader">
	/// The detector reader data.
	/// </param>
	/// <param name="filter">the filter to analyze</param>
	/// <param name="precision">Filter Precision</param>
	/// <returns>
	/// Helper, to efficiently validate scans.
	/// </returns>
	public static ScanFilterHelper BuildFilterHelper(this IDetectorReader detectorReader, IScanFilter filter, int precision)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		bool accuratePrecursors = detectorReader.GetInstrumentData().IsTsqQuantumFile();
		return new ScanFilterHelper(filter, accuratePrecursors, precision);
	}

	/// <summary>
	/// Format ionization mode, based on the scan filter string format.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="mode">
	/// The mode to format.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	public static string FormatIonizationMode(this IRawDataPlus data, IonizationModeType mode)
	{
		if (data.SelectMsData())
		{
			ScanEventBuilder scanEvent = new ScanEventBuilder
			{
				IonizationMode = mode
			};
			return data.CreateFilterFromScanEvent(scanEvent).ToString();
		}
		return string.Empty;
	}

	/// <summary>
	/// Format activation type, based on the scan filter string format.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="mode">
	/// The mode to format.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	public static string FormatActivationType(this IRawDataPlus data, ActivationType mode)
	{
		if (data.SelectMsData())
		{
			ScanEventBuilder scanEventBuilder = new ScanEventBuilder
			{
				MSOrder = MSOrderType.Par
			};
			MsReaction item = new MsReaction
			{
				ActivationType = mode,
				MultipleActivation = true
			};
			MsStage stage = new MsStage(new List<IReaction> { item });
			scanEventBuilder.AddMsStage(stage);
			string text = data.CreateFilterFromScanEvent(scanEventBuilder).ToString();
			int num = text.IndexOf('@');
			if (num >= 0 && num < text.Length)
			{
				return text.Substring(num + 1);
			}
		}
		return string.Empty;
	}

	/// <summary>
	/// Format mass analyzer, based on the scan filter string format.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <param name="analyzer">
	/// The analyzer to format.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	public static string FormatMassAnalyzer(this IRawDataPlus data, MassAnalyzerType analyzer)
	{
		if (data.SelectMsData())
		{
			ScanEventBuilder scanEvent = new ScanEventBuilder
			{
				MassAnalyzer = analyzer
			};
			return data.CreateFilterFromScanEvent(scanEvent).ToString();
		}
		return string.Empty;
	}

	/// <summary>
	/// Get the mass tolerance and precision values from a raw file
	/// </summary>
	/// <param name="rawData">Raw file</param>
	/// <returns>The default tolerance and filter precision</returns>
	public static MassOptions DefaultMassOptions(this IDetectorReaderPlus rawData)
	{
		IRunHeader runHeaderEx = rawData.RunHeaderEx;
		return new MassOptions
		{
			Precision = runHeaderEx.FilterMassPrecision,
			Tolerance = runHeaderEx.MassResolution,
			ToleranceUnits = runHeaderEx.ToleranceUnit
		};
	}

	/// <summary>
	/// Enumerate over a range of scans
	/// </summary>
	/// <param name="data">File to read from</param>
	/// <param name="firstScan">start of scan range</param>
	/// <param name="lastScan">final scan in scan range</param>
	/// <returns>An enumerator for a collection of scans</returns>
	public static IEnumerable<Scan> GetScans(this IRawDataPlus data, int firstScan, int lastScan)
	{
		for (int i = firstScan; i <= lastScan; i++)
		{
			yield return Scan.FromFile(data, i);
		}
	}

	/// <summary>
	/// Enumerate over a set of scans
	/// </summary>
	/// <param name="data">File to read from</param>
	/// <param name="scanNumber">Numbers for the scans</param>
	/// <returns>An enumerator for a collection of scans</returns>
	public static IEnumerable<Scan> GetScans(this IRawDataPlus data, List<int> scanNumber)
	{
		if (scanNumber == null)
		{
			throw new ArgumentNullException("scanNumber");
		}
		return scanNumber.Select((int i) => Scan.FromFile(data, i));
	}

	/// <summary>
	/// Gets the average of scans between the given times, which match a filter string.
	/// </summary>
	/// <param name="data">File to read from</param>
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
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageScansInTimeRange(this IRawDataPlus data, double startTime, double endTime, string filter, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		return CreateScanAverager(data, averageOptions).AverageScansInTimeRange(startTime, endTime, filter, options);
	}

	/// <summary>
	/// Gets the average of scans between the given times, which match a compound name.
	/// </summary>
	/// <param name="rawData">raw data to read from</param>
	/// <param name="startTime">
	/// start time
	/// </param>
	/// <param name="endTime">
	/// end time
	/// </param>
	/// <param name="compound">
	/// compound name
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageCompoundScansInTimeRange(this IDetectorReaderPlus rawData, double startTime, double endTime, string compound, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		Tuple<int, int> tuple = rawData.ScanRangeFromTimeRange(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime));
		return rawData.AverageCompoundScansInScanRange(tuple.Item1, tuple.Item2, compound, options, averageOptions);
	}

	/// <summary>
	/// Gets the average of scans between the given scan numbers, which match a compound name.
	/// </summary>
	/// <param name="data">File to read from</param>
	/// <param name="startScan">
	/// start scan number
	/// </param>
	/// <param name="endScan">
	/// end scan number
	/// </param>
	/// <param name="compound">
	/// compound name
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageCompoundScansInScanRange(this IRawDataPlus data, int startScan, int endScan, string compound, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		List<int> list = new List<int>(20);
		for (int i = startScan; i <= endScan; i++)
		{
			if (ScanIsOfCompound(data, i, compound))
			{
				list.Add(i);
			}
		}
		return CreateScanAverager(data, averageOptions).AverageScans(list, options, alwaysMergeSegments: true);
	}

	/// <summary>
	/// Gets the average of scans between the given scan numbers, which match a compound name.
	/// </summary>
	/// <param name="rawData">The data.</param>
	/// <param name="startScan">The start scan.</param>
	/// <param name="endScan">The end scan.</param>
	/// <param name="compound">The compound.</param>
	/// <param name="options">The options.</param>
	/// <param name="averageOptions">The average options.</param>
	/// <returns>
	///   Scan
	/// </returns>
	public static Scan AverageCompoundScansInScanRange(this IDetectorReaderPlus rawData, int startScan, int endScan, string compound, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		List<int> list = new List<int>(20);
		for (int i = startScan; i <= endScan; i++)
		{
			if (ScanIsOfCompound(rawData, i, compound))
			{
				list.Add(i);
			}
		}
		return CreateScanAverager(rawData, averageOptions).AverageScans(list, options, alwaysMergeSegments: true);
	}

	/// <summary>
	/// Gets the average of scans between the given scan numbers, which match a filter string.
	/// </summary>
	/// <param name="rawData">
	/// File to read from
	/// </param>
	/// <param name="startScan">
	/// start scan
	/// </param>
	/// <param name="endScan">
	/// end scan
	/// </param>
	/// <param name="filter">
	/// filter string
	/// </param>
	/// <param name="options">
	/// mass tolerance settings. If not supplied, these are default from the raw file
	/// </param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageScansInScanRange(this IDetectorReaderPlus rawData, int startScan, int endScan, string filter, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		return CreateScanAverager(rawData, averageOptions).AverageScansInScanRange(startScan, endScan, filter, options);
	}

	/// <summary>
	/// Create a scan averager.
	/// </summary>
	/// <param name="rawData">
	/// The data.
	/// </param>
	/// <param name="averageOptions">
	/// The average options.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IScanAveragePlus" />.
	/// </returns>
	private static IScanAveragePlus CreateScanAverager(IDetectorReaderPlus rawData, FtAverageOptions averageOptions)
	{
		IScanAveragePlus scanAverager = ScanAveragerFactory.GetScanAverager(rawData);
		if (averageOptions != null)
		{
			scanAverager.FtOptions = averageOptions;
		}
		return scanAverager;
	}

	/// <summary>
	/// Gets the average of scans between the given times, which match the supplied filter rules.
	/// </summary>
	/// <param name="data">File to read from</param>
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
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageScansInTimeRange(this IRawDataPlus data, double startTime, double endTime, IScanFilter filter, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		return CreateScanAverager(data, averageOptions).AverageScansInTimeRange(startTime, endTime, filter, options);
	}

	/// <summary>
	/// Gets the average of scans between the given scan numbers, which match the supplied filter rules.
	/// </summary>
	/// <param name="data">File to read from</param>
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
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// the averaged scan. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageScansInScanRange(this IRawDataPlus data, int startScan, int endScan, IScanFilter filter, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		return CreateScanAverager(data, averageOptions).AverageScansInScanRange(startScan, endScan, filter, options);
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
	/// <param name="rawData">File to read from</param>
	/// <param name="scans">
	/// list of scans to average
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <param name="alwaysMergeSegments">Merge segments, even if mass ranges are not similar.
	/// Only applies to data with 1 mass segment</param>
	/// <returns>
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageScans(this IDetectorReaderPlus rawData, List<int> scans, MassOptions options = null, FtAverageOptions averageOptions = null, bool alwaysMergeSegments = false)
	{
		return CreateScanAverager(rawData, averageOptions).AverageScans(scans, options, alwaysMergeSegments);
	}

	/// <summary>
	/// Calculates the averaged and subtracted spectra based upon the lists supplied.
	/// The application should filter the data before making this code, to ensure that
	/// the scans in both lists are of equivalent format. The result, when the lists contains scans of 
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
	/// <param name="data">File to read from</param>
	/// <param name="foregroundScans">
	/// foreground scans: list of scans to average
	/// </param>
	/// <param name="backgroundScans">background scans: list of scans which are averaged, and then subtracted from the averaged foreground scans</param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageAndSubtractScans(this IRawDataPlus data, List<int> foregroundScans, List<int> backgroundScans, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		if (foregroundScans == null)
		{
			throw new ArgumentNullException("foregroundScans");
		}
		if (backgroundScans == null)
		{
			throw new ArgumentNullException("backgroundScans");
		}
		IScanAveragePlus scanAveragePlus = CreateScanAverager(data, averageOptions);
		Scan foreground = scanAveragePlus.AverageScans(foregroundScans, options);
		Scan background = scanAveragePlus.AverageScans(backgroundScans, options);
		return scanAveragePlus.SubtractScans(foreground, background);
	}

	/// <summary>
	/// Calculates the averaged and subtracted spectra based upon the lists supplied.
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
	/// <param name="data">File to read from</param>
	/// <param name="foregroundTimeRange">
	/// time range to average.
	/// </param>
	/// <param name="backgroundTimeRanges">
	/// time ranges to subtract. The sets of scans matching the filter in each of these time ranges
	/// are joined to define the background.
	/// </param>
	/// <param name="filter">
	/// Filter to apply for averaging
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageAndSubtractScans(this IRawDataPlus data, IRangeAccess foregroundTimeRange, IList<IRangeAccess> backgroundTimeRanges, string filter, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		return data.AverageAndSubtractScans(foregroundTimeRange, backgroundTimeRanges, data.GetFilterFromString(filter), options, averageOptions);
	}

	/// <summary>
	/// Calculates the averaged and subtracted spectra based upon the lists supplied.
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
	/// <param name="data">File to read from</param>
	/// <param name="foregroundTimeRange">
	/// time range to average.
	/// </param>
	/// <param name="backgroundTimeRanges">
	/// time ranges to subtract. The sets of scans matching the filter in each of these time ranges
	/// are joined to define the background.
	/// </param>
	/// <param name="filter">
	/// Filter to apply for averaging
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageAndSubtractScans(this IRawDataPlus data, IRangeAccess foregroundTimeRange, IList<IRangeAccess> backgroundTimeRanges, IScanFilter filter, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		if (backgroundTimeRanges == null)
		{
			throw new ArgumentNullException("backgroundTimeRanges");
		}
		IScanAveragePlus scanAveragePlus = CreateScanAverager(data, averageOptions);
		Scan foreground = scanAveragePlus.AverageScansInTimeRange(foregroundTimeRange.Low, foregroundTimeRange.High, filter, options);
		List<int> list = new List<int>();
		foreach (IRangeAccess backgroundTimeRange in backgroundTimeRanges)
		{
			list.AddRange(data.GetFilteredScansListByTimeRange(filter, backgroundTimeRange));
		}
		Scan background = scanAveragePlus.AverageScans(list, options);
		return scanAveragePlus.SubtractScans(foreground, background);
	}

	/// <summary>
	/// Calculates the averaged and subtracted spectra based upon the lists supplied,
	/// which include the named compound.
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
	/// <param name="rawData">File to read from</param>
	/// <param name="foregroundTimeRange">
	/// time range to average.
	/// </param>
	/// <param name="backgroundTimeRanges">
	/// time ranges to subtract. The sets of scans matching the filter in each of these time ranges
	/// are joined to define the background.
	/// </param>
	/// <param name="compound">
	/// Compound to select scans for averaging
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageAndSubtractScansForCompound(this IDetectorReaderPlus rawData, IRangeAccess foregroundTimeRange, IList<IRangeAccess> backgroundTimeRanges, string compound, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		if (backgroundTimeRanges == null)
		{
			throw new ArgumentNullException("backgroundTimeRanges");
		}
		IScanAveragePlus scanAveragePlus = CreateScanAverager(rawData, averageOptions);
		List<int> compoundScanListByTimeRange = rawData.GetCompoundScanListByTimeRange(compound, foregroundTimeRange);
		Scan scan = scanAveragePlus.AverageScans(compoundScanListByTimeRange, options, alwaysMergeSegments: true);
		List<int> list = new List<int>();
		foreach (IRangeAccess backgroundTimeRange in backgroundTimeRanges)
		{
			list.AddRange(rawData.GetCompoundScanListByTimeRange(compound, backgroundTimeRange));
		}
		Scan background = scanAveragePlus.AverageScans(list, options, alwaysMergeSegments: true);
		scan.AlwaysMergeSegments = true;
		return scanAveragePlus.SubtractScans(scan, background);
	}

	/// <summary>
	/// Get the list of scans which contain a given compound, in the range
	/// "nearest scan time to range.Low" to "nearest scan time to range.High"
	/// </summary>
	/// <param name="data">Raw file</param>
	/// <param name="compound">Compound name to match against scan event</param>
	/// <param name="range">Retention Time range</param>
	/// <returns></returns>
	public static List<int> GetCompoundScanListByTimeRange(this IDetectorReaderPlus data, string compound, IRangeAccess range)
	{
		Tuple<int, int> tuple = data.ScanRangeFromTimeRange(range);
		return data.GetCompoundScansListByScanRange(compound, tuple.Item1, tuple.Item2);
	}

	/// <summary>Gets the filtered scans list by scan range.</summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="startScanNumber">The start scan number.</param>
	/// <param name="endScanNumber">The end scan number.</param>
	/// <returns>
	///   List of filtered scan numbers
	/// </returns>
	/// <exception cref="T:System.ArgumentException">Scan number out of range - startScanNumber
	/// or
	/// Scan number out of range - endScanNumber</exception>
	public static List<int> GetFilteredScansListByScanRange(this IDetectorReaderPlus detectorReader, string filter, int startScanNumber, int endScanNumber)
	{
		IRunHeader header = detectorReader.RunHeaderEx;
		if (!ValidScan(startScanNumber))
		{
			throw new ArgumentException("Scan number out of range", "startScanNumber");
		}
		if (!ValidScan(endScanNumber))
		{
			throw new ArgumentException("Scan number out of range", "endScanNumber");
		}
		ScanFilterHelper filterHelper = detectorReader.BuildFilterHelper(filter);
		return FilteredScansListByScanRange(detectorReader, startScanNumber, endScanNumber, filterHelper);
		bool ValidScan(int scan)
		{
			if (scan >= header.FirstSpectrum)
			{
				return scan <= header.LastSpectrum;
			}
			return false;
		}
	}

	/// <summary>
	/// Creates a list of all scans in the given range, which have a component name
	/// </summary>
	/// <param name="rawData">The raw file.</param>
	/// <param name="compound">The compound.</param>
	/// <param name="startScanNumber">The start scan number.</param>
	/// <param name="endScanNumber">The end scan number.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetCompoundScansListByScanRange(this IDetectorReaderPlus rawData, string compound, int startScanNumber, int endScanNumber)
	{
		if (compound == null)
		{
			throw new ArgumentNullException("compound");
		}
		return CompoundScansListByScanRange(rawData, startScanNumber, endScanNumber, compound);
	}

	/// <summary>
	/// Creates a list of all scans in the given range, which pass the supplied filter rules.
	/// </summary>
	/// <param name="rawFile">The raw file.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="startScanNumber">The start scan number.</param>
	/// <param name="endScanNumber">The end scan number.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListByScanRange(this IRawDataPlus rawFile, IScanFilter filter, int startScanNumber, int endScanNumber)
	{
		ScanFilterHelper filterHelper = rawFile.BuildFilterHelper(filter);
		return FilteredScansListByScanRange(rawFile, startScanNumber, endScanNumber, filterHelper);
	}

	/// <summary>
	/// Creates a list of all scans in the given range, which pass the supplied filter rules.
	/// </summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="startScanNumber">The start scan number.</param>
	/// <param name="endScanNumber">The end scan number.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListByScanRange(this IDetectorReader detectorReader, IScanFilter filter, int startScanNumber, int endScanNumber)
	{
		ScanFilterHelper filterHelper = detectorReader.BuildFilterHelper(filter);
		return FilteredScansListByScanRange(detectorReader, startScanNumber, endScanNumber, filterHelper);
	}

	/// <summary>
	/// Creates a list of all scans in the given time range, which pass the supplied filter rules.
	/// </summary>
	/// <param name="rawFile">The raw file.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListByTimeRange(this IRawDataPlus rawFile, IScanFilter filter, double startTime, double endTime)
	{
		return rawFile.GetFilteredScansListByTimeRange(filter, ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime));
	}

	/// <summary>
	/// Creates a list of all scans in the given time range, which pass the supplied filter rules.
	/// </summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	public static List<int> GetFilteredScansListByTimeRange(this IDetectorReader detectorReader, IScanFilter filter, double startTime, double endTime)
	{
		return detectorReader.GetFilteredScansListByTimeRange(filter, ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime));
	}

	/// <summary>
	/// Creates a list of all scans in the given time range, which pass the supplied filter rules.
	/// This version returns scans from the "nearest scan to start time" to "nearest scan to end time".
	/// </summary>
	/// <param name="rawFile">The raw file.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="timeRange">The time range.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListByTimeRange(this IRawDataPlus rawFile, IScanFilter filter, IRangeAccess timeRange)
	{
		if (timeRange == null)
		{
			throw new ArgumentNullException("timeRange");
		}
		Tuple<int, int> tuple = rawFile.ScanRangeFromTimeRange(timeRange);
		return rawFile.GetFilteredScansListByScanRange(filter, tuple.Item1, tuple.Item2);
	}

	/// <summary>
	/// Creates a list of all scans in the given time range, which pass the supplied filter rules.
	/// This version returns scans from the "nearest scan to start time" to "nearest scan to end time".
	/// </summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="timeRange">The time range.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListByTimeRange(this IDetectorReader detectorReader, IScanFilter filter, IRangeAccess timeRange)
	{
		if (timeRange == null)
		{
			throw new ArgumentNullException("timeRange");
		}
		if (timeRange.Low > timeRange.High)
		{
			throw new ArgumentException("Invalid time range", "timeRange");
		}
		Tuple<int, int> tuple = detectorReader.ScanRangeFromTimeRange(timeRange);
		return detectorReader.GetFilteredScansListByScanRange(filter, tuple.Item1, tuple.Item2);
	}

	/// <summary>
	/// Creates a list of all scans in the given time range, which pass the supplied filter rules.
	/// This version returns scans from the "first scan &gt;= start time" to "last scan Less or equal to end time".
	/// </summary>
	/// <param name="rawFile">The raw file.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="timeRange">The time range.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListWithinTimeRange(this IRawDataPlus rawFile, IScanFilter filter, ThermoFisher.CommonCore.Data.Business.Range timeRange)
	{
		if (timeRange == null)
		{
			throw new ArgumentNullException("timeRange");
		}
		Tuple<int, int> tuple = rawFile.ScanRangeWithinTimeRange(timeRange);
		return rawFile.GetFilteredScansListByScanRange(filter, tuple.Item1, tuple.Item2);
	}

	/// <summary>
	/// Creates a list of all scans in the given time range, which pass a filter
	/// </summary>
	/// <param name="rawFile">The raw file.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListByTimeRange(this IRawDataPlus rawFile, string filter, double startTime, double endTime)
	{
		Tuple<int, int> tuple = rawFile.ScanRangeFromTimeRange(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime));
		return rawFile.GetFilteredScansListByScanRange(filter, tuple.Item1, tuple.Item2);
	}

	/// <summary>
	/// Creates a list of all scans in the given time range, which pass a filter
	/// </summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="filter">The filter.</param>
	/// <param name="startTime">The start time.</param>
	/// <param name="endTime">The end time.</param>
	/// <returns>List of filtered scan numbers</returns>
	public static List<int> GetFilteredScansListByTimeRange(this IDetectorReader detectorReader, string filter, double startTime, double endTime)
	{
		Tuple<int, int> tuple = detectorReader.ScanRangeFromTimeRange(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime));
		return detectorReader.GetFilteredScansListByScanRange(filter, tuple.Item1, tuple.Item2);
	}

	/// <summary>
	/// Convert a time range to a the nearest scan range
	/// </summary>
	/// <param name="rawData">
	/// The raw data reader.
	/// </param>
	/// <param name="timeRange">
	/// The time range.
	/// </param>
	/// <returns>
	/// The scan range of the nearest scans to the start and end times
	/// </returns>
	public static Tuple<int, int> ScanRangeFromTimeRange(this IDetectorReaderPlus rawData, IRangeAccess timeRange)
	{
		return new Tuple<int, int>(rawData.ScanNumberFromRetentionTime(timeRange.Low), rawData.ScanNumberFromRetentionTime(timeRange.High));
	}

	/// <summary>
	/// Convert a time range to a the nearest scan range
	/// </summary>
	/// <param name="detectorReader">
	/// The detector reader.
	/// </param>
	/// <param name="timeRange">
	/// The time range.
	/// </param>
	/// <returns>
	/// The scan range of the nearest scans to the start and end times
	/// </returns>
	public static Tuple<int, int> ScanRangeFromTimeRange(this IDetectorReader detectorReader, IRangeAccess timeRange)
	{
		return new Tuple<int, int>(detectorReader.ScanNumberFromRetentionTime(timeRange.Low), detectorReader.ScanNumberFromRetentionTime(timeRange.High));
	}

	/// <summary>
	/// Convert a time range to am included scan range,
	/// include scans which are within or equal to the time limits supplied.
	/// </summary>
	/// <param name="rawFile">
	/// The raw File.
	/// </param>
	/// <param name="timeRange">
	/// The time range.
	/// </param>
	/// <returns>
	/// The scan range of the lowest and highest scans in the time range supplied,
	/// including scans at the boundary times.
	/// </returns>
	public static Tuple<int, int> ScanRangeWithinTimeRange(this IRawDataPlus rawFile, ThermoFisher.CommonCore.Data.Business.Range timeRange)
	{
		int num = rawFile.ScanNumberFromRetentionTime(timeRange.Low);
		int num2 = rawFile.ScanNumberFromRetentionTime(timeRange.High);
		IRunHeader runHeaderEx = rawFile.RunHeaderEx;
		if (num >= runHeaderEx.FirstSpectrum && num <= runHeaderEx.LastSpectrum)
		{
			if (rawFile.RetentionTimeFromScanNumber(num) < timeRange.Low)
			{
				if (num >= runHeaderEx.LastSpectrum)
				{
					return new Tuple<int, int>(0, 0);
				}
				num++;
			}
			if (num2 >= runHeaderEx.FirstSpectrum && num2 <= runHeaderEx.LastSpectrum)
			{
				if (rawFile.RetentionTimeFromScanNumber(num2) > timeRange.High)
				{
					if (num2 <= runHeaderEx.FirstSpectrum)
					{
						return new Tuple<int, int>(0, 0);
					}
					num2--;
				}
				if (num > num2)
				{
					return new Tuple<int, int>(0, 0);
				}
				return new Tuple<int, int>(num, num2);
			}
			return new Tuple<int, int>(0, 0);
		}
		return new Tuple<int, int>(0, 0);
	}

	/// <summary>
	/// Creates a list of all scans in the given scan range, which pass a filter
	/// </summary>
	/// <param name="sampleRawFile">
	/// The sample raw file.
	/// </param>
	/// <param name="startScanNum">
	/// The start scan number.
	/// </param>
	/// <param name="endScanNum">
	/// The end scan number.
	/// </param>
	/// <param name="filterHelper">
	/// The (analyzed) filter.
	/// </param>
	/// <returns>
	/// The list of matching scans.
	/// </returns>
	private static List<int> FilteredScansListByScanRange(IRawDataPlus sampleRawFile, int startScanNum, int endScanNum, ScanFilterHelper filterHelper)
	{
		List<int> list = new List<int>();
		for (int i = startScanNum; i <= endScanNum; i++)
		{
			if (sampleRawFile.TestScan(i, filterHelper))
			{
				list.Add(i);
			}
		}
		return list;
	}

	/// <summary>Filters the scans list by scan range.</summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="startScanNum">The start scan number.</param>
	/// <param name="endScanNum">The end scan number.</param>
	/// <param name="filterHelper">The filter helper.</param>
	/// <returns>
	///   List of filtered scans
	/// </returns>
	private static List<int> FilteredScansListByScanRange(IDetectorReaderPlus detectorReader, int startScanNum, int endScanNum, ScanFilterHelper filterHelper)
	{
		List<int> list = new List<int>();
		for (int i = startScanNum; i <= endScanNum; i++)
		{
			if (detectorReader.TestScan(i, filterHelper))
			{
				list.Add(i);
			}
		}
		return list;
	}

	/// <summary>
	/// Creates a list of all scans in the given scan range, which pass a filter
	/// </summary>
	/// <param name="rawData">
	/// The sample raw file.
	/// </param>
	/// <param name="startScanNum">
	/// The start scan number.
	/// </param>
	/// <param name="endScanNum">
	/// The end scan number.
	/// </param>
	/// <param name="compound">
	/// The (analyzed) filter.
	/// </param>
	/// <returns>
	/// The list of matching scans.
	/// </returns>
	private static List<int> CompoundScansListByScanRange(IDetectorReaderPlus rawData, int startScanNum, int endScanNum, string compound)
	{
		List<int> list = new List<int>();
		for (int i = startScanNum; i <= endScanNum; i++)
		{
			if (ScanIsOfCompound(rawData, i, compound))
			{
				list.Add(i);
			}
		}
		return list;
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
	/// <param name="data">File to read from</param>
	/// <param name="scans">
	/// list of ScanStatistics
	/// </param>
	/// <param name="options">mass tolerance settings. If not supplied, these are default from the raw file</param>
	/// <param name="averageOptions">
	/// The average Options (for FT format data).
	/// </param>
	/// <returns>
	/// The average of the listed scans. Use Scan.ScansCombined to find how many scans were averaged.
	/// </returns>
	public static Scan AverageScans(this IRawDataPlus data, List<ScanStatistics> scans, MassOptions options = null, FtAverageOptions averageOptions = null)
	{
		if (scans == null)
		{
			throw new ArgumentNullException("scans");
		}
		return CreateScanAverager(data, averageOptions).AverageScans(scans, options);
	}

	/// <summary>
	/// Subtracts the background scan from the foreground scan
	/// </summary>
	/// <param name="data">File to read from</param>
	/// <param name="foreground">Foreground data (Left of "scan-scan" operation</param>
	/// <param name="background">Background data (right of"scan-scan" operation)</param>
	/// <returns>The result of foreground-background</returns>
	public static Scan SubtractScans(this IRawDataPlus data, Scan foreground, Scan background)
	{
		if (foreground == null)
		{
			throw new ArgumentNullException("foreground");
		}
		if (background == null)
		{
			throw new ArgumentNullException("background");
		}
		return ScanAveragerFactory.GetScanAverager(data).SubtractScans(foreground, background);
	}

	/// <summary>
	/// Test if this file has variable trailers.
	/// Calling code must select and MS data stream first.
	/// If this is true, then the number of items returned in each scan for "trailer extra" can vary.
	/// </summary>
	/// <param name="data">
	/// The raw data.
	/// </param>
	/// <returns>
	/// True, if the trailer extra records are variable sized.
	/// </returns>
	public static bool HasVariableTrailers(this IRawDataPlus data)
	{
		return data.GetTrailerExtraHeaderInformation().HasVariableRecords();
	}

	/// <summary>
	/// Test if a set of headers defines "variable sized records".
	/// </summary>
	/// <param name="headers">
	/// The headers.
	/// </param>
	/// <returns>
	/// True, if the records controlled by these headers are variable sized.
	/// </returns>
	public static bool HasVariableRecords(this HeaderItem[] headers)
	{
		if (headers.Length != 0)
		{
			return headers[0].IsVariableHeader(headers.Length);
		}
		return false;
	}

	/// <summary>
	/// Get trailer extra data for scan with validation.
	/// Gets the "trailer extra" custom scan data in object form.
	/// <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetTrailerExtraValues(System.Int32)" />
	/// The application should select MS data before calling this.
	/// For example, If the string "Ion Time" is found at index 3 in the 
	/// headers with format "double", and an application needs to read this double value
	/// from scan 19, then the application can first test if item[3] is valid in scan 19, by
	/// inspecting the returned boolean array (result.Item1[3]).
	/// If this array element is "true" then the "Ion Time" (double) value in the returned object array
	/// at index 3 may be used. If result.Item1[3] is false, then this value is not available.
	/// So: If producing a chart of this value, the retention time of this scan should be
	/// omitted from the data series. (Do not record "0" for a missing value).
	/// Argument exceptions may be returned by the underlying file, if (for example) the scan number
	/// is out of range.
	/// </summary>
	/// <param name="file">
	/// The raw file.
	/// </param>
	/// <param name="scan">
	/// The scan number.
	/// </param>
	/// <param name="headers">
	/// The headers as read by IRawDataPlus.GetTrailerExtraHeaderInformation.
	/// When processing multiple scans, it is more efficient for an application
	/// to request this data once, and supply it to each call.
	/// Instruments may have fixed record sizes. Having read the headers, an application
	/// may call this <c>bool hasVariableRecords = headers.HasVariableRecords();</c>
	/// and if this returns "false" then there is no need to call this extension,
	/// as all values will be valid in all scans.
	/// </param>
	/// <returns>
	/// A tuple, whose first element is an array of valid flags, and second is the exact data
	/// returned from GetTrailerExtraValues, for the supplied scan. 
	/// </returns>
	public static Tuple<bool[], object[]> GetTrailerExtraDataForScanWithValidation(this IRawDataPlus file, int scan, HeaderItem[] headers = null)
	{
		if (headers == null)
		{
			headers = file.GetTrailerExtraHeaderInformation();
		}
		bool num = headers.HasVariableRecords();
		object[] trailerExtraValues = file.GetTrailerExtraValues(scan);
		int num2 = trailerExtraValues.Length;
		bool[] array = new bool[num2];
		if (num && trailerExtraValues[0] is string text)
		{
			if (text.Length >= num2 - 1)
			{
				array[0] = false;
				for (int i = 1; i < num2; i++)
				{
					array[i] = text[i - 1] == '\t';
				}
			}
			return new Tuple<bool[], object[]>(array, trailerExtraValues);
		}
		for (int j = 0; j < num2; j++)
		{
			array[j] = true;
		}
		return new Tuple<bool[], object[]>(array, trailerExtraValues);
	}

	/// <summary>
	/// Returns RunHeader as an object.
	/// This extension is for backwards compatibility, as an older format of IRawData return this.
	/// Code should use IRunHeaderAccess IRawData.RunHeader directly where possible
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	public static RunHeader RunHeader(this IRawData data)
	{
		return new RunHeader(data.RunHeader);
	}

	/// <summary>
	/// Configure filter matching criteria.
	/// This can reduce the number of returned results from "GetFilters" by allowing wider matching limits on target mass ranges
	/// and by attempting to find a scan types where there are precursor masses both above and below that
	/// mass which could be considered identical.
	/// </summary>
	/// <param name="mode">The mode (enum) to configure</param>
	/// <param name="useSpecified">use a specified value for precision</param>
	/// <param name="FromInstrument">base precision on values from the instrument</param>
	/// <param name="extendDataDependent">When scans are data dependent, extended the matching of target mass limits</param>
	/// <param name="extendBy">specifies how to extend target mass range limits</param>
	/// <param name="findMidPoint">When a group of MS2 scans is found within tolerance, look for the mid point of the range </param>
	public static FilterPrecisionMode Configure(this FilterPrecisionMode mode, bool useSpecified, bool FromInstrument, bool extendDataDependent, ExtendToleranceBy extendBy, bool findMidPoint)
	{
		mode = FilterPrecisionMode.Auto;
		if (useSpecified)
		{
			mode |= FilterPrecisionMode.Specified;
		}
		if (FromInstrument)
		{
			mode |= FilterPrecisionMode.Instrument;
		}
		if (extendDataDependent)
		{
			mode |= FilterPrecisionMode.ExtendedDataDependentMatch;
		}
		mode = (FilterPrecisionMode)((int)mode | ((int)extendBy << 4));
		if (findMidPoint)
		{
			mode |= FilterPrecisionMode.FindPrecursorMidPoint;
		}
		return mode;
	}
}
