using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to get various statistics from raw data.
/// Intended to gather data for bar charts.
/// </summary>
public class Analytics
{
	private enum SuppliedInterface
	{
		RawPlusMode,
		DetectorPlusMode
	}

	/// <summary>
	/// Class to keep trace of numeric value (bar number) for each text string found in a log
	/// </summary>
	private class NameData
	{
		/// <summary>
		/// Name found in the log
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Assigned bar number
		/// </summary>
		public int Value { get; set; }
	}

	private const string ElapsedKey = "Elapsed Scan Time (sec):";

	private readonly IRawDataPlus _rawData;

	private SuppliedInterface _mode;

	private IDetectorReaderPlus _reader;

	private IRawDataExtensions _rawDataExtensions;

	private Lazy<double[]> AllScanTimes => new Lazy<double[]>(CalculateAllScanTimes);

	/// <summary>
	/// Gets or sets the scan filter string (sets "ScanFilter").
	/// </summary>
	public string Filter { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the compound names, for filtering chromatograms.
	/// </summary>
	public string[] CompoundNames { get; set; }

	/// <summary>
	/// Gets or sets the maximum number of unique strings which are shown as a bar category,
	/// for "text string" diagnostic items.
	/// Any additional strings are displayed as a bar labeled "Other".
	/// Valid range: 3-1000
	/// Default: 10
	/// </summary>
	public int MaxTextCategories { get; set; } = 10;

	/// <summary>
	/// Create an analyzer for raw data.
	/// This can be created from several interfaces.
	/// An earlier version expected IRawDataPlus, which implements IDetectorReaderPlus.
	/// This version requires IDetectorReaderPlus, then examines which known derivation
	/// of this has been provided.
	/// If the provided object implements IRawDataPlus, then the code operates as before (RawPlusMode)
	/// </summary>
	/// <param name="file">the (open) raw file</param>
	public Analytics(IDetectorReaderPlus file)
	{
		_reader = file;
		_mode = SuppliedInterface.DetectorPlusMode;
		if (file is IRawDataPlus rawData)
		{
			_rawData = rawData;
			_mode = SuppliedInterface.RawPlusMode;
		}
		if (file is IRawDataExtensions rawDataExtensions)
		{
			_rawDataExtensions = rawDataExtensions;
		}
	}

	/// <summary>
	/// Calculate the band size (bin size) needed to divide all data values into a given number of bands.
	/// The bands would usually represent "bars on a histogram".
	/// </summary>
	/// <param name="data">Data to analyze</param>
	/// <param name="bands">Number of bands needed</param>
	/// <returns>The width of each band</returns>
	private static double BandSize(double[] data, int bands)
	{
		if (data.Length == 0)
		{
			return 1.0;
		}
		double num = data.Min();
		double num2 = data.Max();
		if (num >= num2)
		{
			return -1.0;
		}
		return (num2 - num) / (double)bands;
	}

	/// <summary>
	/// Gets the common average (mean) of a set of values, defined as 0 for "null or empty" data sets
	/// </summary>
	/// <param name="data">The data to analyze</param>
	/// <returns>the average</returns>
	public static double Average(double[] data)
	{
		if (data == null || data.Length == 0)
		{
			return 0.0;
		}
		return data.Average();
	}

	/// <summary>
	/// Gets the modal band of a set of data, by grouping into a fixed number of bands.
	/// </summary>
	/// <param name="data">data to analyze</param>
	/// <param name="bands">The number of bands (groups) used to split the data</param>
	/// <returns>the center of the modal band (from 0 to bands-1). 0 for null or empty data</returns>
	public static double Mode(double[] data, int bands)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (bands <= 0)
		{
			throw new ArgumentException("Bands must be > 0", "bands");
		}
		if (data.Length == 0)
		{
			return 0.0;
		}
		BandedData bandedData = GetBandedData(data, bands);
		double[] bandData = bandedData.BandData;
		double[] bandCenters = bandedData.BandCenters;
		if (bandData == null || bandCenters == null || bandData.Length == 0)
		{
			return 0.0;
		}
		double num = bandData[0];
		int num2 = 0;
		for (int i = 1; i < bandData.Length; i++)
		{
			if (bandData[i] > num)
			{
				num = bandData[i];
				num2 = i;
			}
		}
		return bandCenters[num2];
	}

	/// <summary>
	/// Find the average delta between values in an array.
	/// </summary>
	/// <param name="data">the data</param>
	/// <returns>The average of the delta (d[x+1]-d[x])</returns>
	private double AverageDeltaTime(IList<double> data)
	{
		if (data == null || data.Count < 2)
		{
			return 0.0;
		}
		int num = data.Count - 1;
		double num2 = 0.0;
		for (int i = 0; i < num; i++)
		{
			num2 += data[i + 1] - data[i];
		}
		return num2 / (double)num;
	}

	/// <summary>
	/// Find the deltas between values in an array.
	/// </summary>
	/// <param name="data">the data</param>
	/// <returns>The average of the delta (d[x+1]-d[x])</returns>
	private double[] DeltaTimes(IList<double> data)
	{
		if (data == null || data.Count < 2)
		{
			return new double[0];
		}
		int num = data.Count - 1;
		double[] array = new double[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = data[i + 1] - data[i];
		}
		return array;
	}

	private bool EnsureMsDetector()
	{
		if (_mode == SuppliedInterface.RawPlusMode)
		{
			if (_rawData != null)
			{
				return _rawData.SelectMsData();
			}
			return false;
		}
		return _reader.ConfiguredDetector.DeviceType == Device.MS;
	}

	/// <summary>
	/// Find the average delta between start times of scans matching a filter (minutes).
	/// </summary>
	/// <returns>The average of the delta (d[x+1]-d[x]) in the start times</returns>
	public double AverageDeltaTime()
	{
		if (EnsureMsDetector())
		{
			ChromatogramSignal signalForFilter = GetSignalForFilter();
			return AverageDeltaTime(signalForFilter.Times);
		}
		return 0.0;
	}

	/// <summary>
	/// Find the delta between start times of scans matching a filter (minutes).
	/// </summary>
	/// <returns>The the deltas (d[x+1]-d[x]) in the start times</returns>
	public double[] DeltaTimes()
	{
		if (EnsureMsDetector())
		{
			ChromatogramSignal signalForFilter = GetSignalForFilter();
			return DeltaTimes(signalForFilter.Times);
		}
		return new double[0];
	}

	/// <summary>
	/// Gets the scan number and start time of scans matching a filter
	/// </summary>
	/// <returns>All scans which match this filter and their times.</returns>
	public Tuple<int, double>[] FilteredScanTimes()
	{
		if (EnsureMsDetector())
		{
			ChromatogramSignal signalForFilter = GetSignalForFilter();
			int length = signalForFilter.Length;
			Tuple<int, double>[] array = new Tuple<int, double>[length];
			IList<int> scans = signalForFilter.Scans;
			IList<double> times = signalForFilter.Times;
			for (int i = 0; i < length; i++)
			{
				array[i] = new Tuple<int, double>(scans[i], times[i]);
			}
			return array;
		}
		return new Tuple<int, double>[0];
	}

	/// <summary>
	/// Get a chromatogram from a filter
	/// </summary>
	/// <returns>The chromatogram</returns>
	private ChromatogramSignal GetSignalForFilter()
	{
		ChromatogramTraceSettings chromatogramTraceSettings = new ChromatogramTraceSettings(TraceType.TIC)
		{
			Filter = Filter
		};
		if (CompoundNames != null && CompoundNames.Length >= 1)
		{
			chromatogramTraceSettings.CompoundNames = CompoundNames;
		}
		return ChromatogramSignal.FromChromatogramData(_reader.GetChromatogramDataEx(new IChromatogramSettingsEx[1] { chromatogramTraceSettings }, -1, -1))[0];
	}

	/// <summary>
	/// Find the RT of all scans
	/// </summary>
	/// <returns>the RT of every scan in a raw file</returns>
	private double[] CalculateAllScanTimes()
	{
		if (EnsureMsDetector())
		{
			return ChromatogramSignal.FromChromatogramData(_reader.GetChromatogramDataEx(new IChromatogramSettingsEx[1]
			{
				new ChromatogramTraceSettings(TraceType.TIC)
			}, -1, -1))[0].SignalTimes;
		}
		return new double[0];
	}

	/// <summary>
	/// Find scan durations,in seconds.
	/// </summary>
	/// <returns>the set of durations, in seconds. returns "new double[0] if there is no MS data</returns>
	public double[] ScanDurations()
	{
		if (EnsureMsDetector())
		{
			HeaderItem[] trailerExtraHeaderInformation = _reader.GetTrailerExtraHeaderInformation();
			int num = 0;
			bool flag = false;
			int indexOfKey = 0;
			HeaderItem[] array = trailerExtraHeaderInformation;
			foreach (HeaderItem headerItem in array)
			{
				if (headerItem.Label == "Elapsed Scan Time (sec):" && headerItem.IsNumeric)
				{
					flag = true;
					indexOfKey = num;
				}
				num++;
			}
			if (flag)
			{
				return ScanDurationsWithKey(Filter, indexOfKey);
			}
			return ScanDurationsNoKey();
		}
		return new double[0];
	}

	/// <summary>
	/// Find scan durations, for files which do not have "ElapsedKey"
	/// </summary>
	/// <returns>the set of durations</returns>
	private double[] ScanDurationsNoKey()
	{
		double[] value = AllScanTimes.Value;
		int num = value.Length;
		Tuple<int, double>[] array = FilteredScanTimes();
		int num2 = array.Length;
		double num3 = 1.0;
		if (num2 > 0)
		{
			double[] array2 = new double[num2];
			for (int i = 0; i < num2; i++)
			{
				int item = array[i].Item1;
				if (item > 0 && item < num)
				{
					num3 = value[item] - value[item - 1];
				}
				array2[i] = num3 * 60.0;
			}
			return array2;
		}
		return new double[0];
	}

	/// <summary>
	/// Find scan durations, for files which do have "ElapsedKey"
	/// </summary>
	/// <param name="filter">scan filter</param>
	/// <param name="indexOfKey">index into trailer extra for time key</param>
	/// <returns>the set of durations in seconds</returns>
	private double[] ScanDurationsWithKey(string filter, int indexOfKey)
	{
		IEnumerable<int> filteredScanEnumerator = _reader.GetFilteredScanEnumerator(filter);
		return GetDurationsFromScans(indexOfKey, filteredScanEnumerator);
	}

	private double[] GetDurationsFromScans(int indexOfKey, IEnumerable<int> scanSet)
	{
		List<double> list = new List<double>();
		foreach (int item2 in scanSet)
		{
			object trailerExtraValue = _reader.GetTrailerExtraValue(item2, indexOfKey);
			if (trailerExtraValue is float num)
			{
				list.Add(num);
			}
			else if (trailerExtraValue is double item)
			{
				list.Add(item);
			}
			else
			{
				list.Add(0.0);
			}
		}
		return list.ToArray();
	}

	/// <summary>
	/// Determine if this is a data type which needs banding.
	/// Small types (such as byte) can be mapped to a small array of results.
	/// For example: "unsigned byte" has only 256 states, so:
	/// we can collect data about each state.
	/// But: Float has infinite states, so must be banded.
	/// Note that "int" sometimes can handle all states, depending on the
	/// range of values, so is not considered "large" here
	/// </summary>
	/// <param name="dataType"></param>
	/// <returns>true if this is a large numeric type</returns>
	public static bool IsLargeNumeric(GenericDataTypes dataType)
	{
		if ((uint)(dataType - 9) <= 2u)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Find a trend for the given trailer item
	/// </summary>
	/// <param name="indexOfKey">index into trailer extra for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for fasle/off/no</returns>
	public double[] TrailerValueTrendFromKey(int indexOfKey, DataFilter dataMode)
	{
		string filter = Filter;
		HeaderItem[] trailerExtraHeaderInformation = _reader.GetTrailerExtraHeaderInformation();
		if (indexOfKey < 0 || indexOfKey >= trailerExtraHeaderInformation.Length)
		{
			throw new ArgumentOutOfRangeException("indexOfKey");
		}
		HeaderItem recordFormat = trailerExtraHeaderInformation[indexOfKey];
		IEnumerable<int> filteredScanEnumerator = _reader.GetFilteredScanEnumerator(filter);
		List<double> list = new List<double>();
		Dictionary<string, NameData> foundNames = new Dictionary<string, NameData>();
		int maxTextCategories = ValidateMaxTextcategories();
		foreach (int item in filteredScanEnumerator)
		{
			double trailerNumericValueFromScan = GetTrailerNumericValueFromScan(indexOfKey, recordFormat, item, foundNames, maxTextCategories);
			if (double.IsNaN(trailerNumericValueFromScan))
			{
				continue;
			}
			switch (dataMode)
			{
			case DataFilter.PositiveAndZero:
				if (trailerNumericValueFromScan < 0.0)
				{
					continue;
				}
				break;
			case DataFilter.PositiveOnly:
				if (trailerNumericValueFromScan <= 0.0)
				{
					continue;
				}
				break;
			}
			list.Add(trailerNumericValueFromScan);
		}
		return list.ToArray();
	}

	/// <summary>
	/// Find a trend for the given status item. Calling code should select a device with a status log first.
	/// </summary>
	/// <param name="indexOfKey">index into status log for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for false/off/no</returns>
	public double[] StatusValueTrendFromKey(int indexOfKey, DataFilter dataMode)
	{
		int startRecord = 0;
		return StatusValueTrendFromKey(indexOfKey, dataMode, ref startRecord);
	}

	/// <summary>
	/// Find a trend for the given status item. Calling code should select a device with a status log first.
	/// </summary>
	/// <param name="indexOfKey">index into status log for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <param name="startRecord">First record in the log to inspect. This is intended for incremental log reading (real time data).
	/// The value is updated to the next needed record number, such that repeated calls get new information as it arrives.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for false/off/no.
	/// If there are no further records after "startRecord" or all records are "NaN" the result will be an empty array.</returns>
	public double[] StatusValueTrendFromKey(int indexOfKey, DataFilter dataMode, ref int startRecord)
	{
		HeaderItem[] statusLogHeaderInformation = _reader.GetStatusLogHeaderInformation();
		if (indexOfKey < 0 || indexOfKey >= statusLogHeaderInformation.Length)
		{
			throw new ArgumentOutOfRangeException("indexOfKey");
		}
		HeaderItem recordFormat = statusLogHeaderInformation[indexOfKey];
		int num = ((_rawDataExtensions == null) ? _reader.RunHeaderEx.StatusLogCount : _reader.GetStatusLogEntriesCount());
		List<double> list = new List<double>();
		Dictionary<string, NameData> foundNames = new Dictionary<string, NameData>();
		int maxTextCategories = ValidateMaxTextcategories();
		for (int i = startRecord; i < num; i++)
		{
			double time;
			double num2 = ((_rawDataExtensions == null) ? GetStatusNumericValueFromLog(indexOfKey, recordFormat, i, out time, foundNames, maxTextCategories) : GetStatusNumericValueFromSortedLog(indexOfKey, recordFormat, i, out time, foundNames, maxTextCategories));
			if (double.IsNaN(num2))
			{
				continue;
			}
			switch (dataMode)
			{
			case DataFilter.PositiveAndZero:
				if (num2 < 0.0)
				{
					continue;
				}
				break;
			case DataFilter.PositiveOnly:
				if (num2 <= 0.0)
				{
					continue;
				}
				break;
			}
			list.Add(num2);
		}
		startRecord = num;
		return list.ToArray();
	}

	/// <summary>
	/// Find a trend for the given trailer item
	/// </summary>
	/// <param name="indexOfKey">index into trailer extra for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for fasle/off/no</returns>
	public ChromatogramSignal TrailerValueSignalFromKey(int indexOfKey, DataFilter dataMode)
	{
		string filter = Filter;
		HeaderItem[] trailerExtraHeaderInformation = _reader.GetTrailerExtraHeaderInformation();
		if (indexOfKey < 0 || indexOfKey >= trailerExtraHeaderInformation.Length)
		{
			throw new ArgumentOutOfRangeException("indexOfKey");
		}
		HeaderItem recordFormat = trailerExtraHeaderInformation[indexOfKey];
		IEnumerable<int> filteredScanEnumerator = _reader.GetFilteredScanEnumerator(filter);
		List<double> list = new List<double>();
		List<double> list2 = new List<double>();
		Dictionary<string, NameData> foundNames = new Dictionary<string, NameData>();
		int maxTextCategories = ValidateMaxTextcategories();
		foreach (int item in filteredScanEnumerator)
		{
			double trailerNumericValueFromScan = GetTrailerNumericValueFromScan(indexOfKey, recordFormat, item, foundNames, maxTextCategories);
			if (double.IsNaN(trailerNumericValueFromScan))
			{
				continue;
			}
			switch (dataMode)
			{
			case DataFilter.PositiveAndZero:
				if (trailerNumericValueFromScan < 0.0)
				{
					continue;
				}
				break;
			case DataFilter.PositiveOnly:
				if (trailerNumericValueFromScan <= 0.0)
				{
					continue;
				}
				break;
			}
			list.Add(trailerNumericValueFromScan);
			list2.Add(_reader.RetentionTimeFromScanNumber(item));
		}
		return ChromatogramSignal.FromTimeAndIntensity(list2.ToArray(), list.ToArray());
	}

	/// <summary>
	/// Find a trend for the given status item. Calling code should select a device with a status log first.
	/// </summary>
	/// <param name="indexOfKey">index into trailer extra for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for false/off/no.
	/// If there are no further records after "startRecord" or all records are "NaN" the result will be an empty array.</returns>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for fasle/off/no</returns>
	public ChromatogramSignal StatusValueSignalFromKey(int indexOfKey, DataFilter dataMode)
	{
		int startRecord = 0;
		return StatusValueSignalFromKey(indexOfKey, dataMode, ref startRecord);
	}

	/// <summary>
	/// Find a trend for the given status item. Calling code should select a device with a status log first.
	/// </summary>
	/// <param name="indexOfKey">index into trailer extra for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <param name="startRecord">First record in the log to inspect. This is intended for incremental log reading (real time data).
	/// The value is updated to the next needed record number, such that repeated calls get new information as it arrives.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for false/off/no.
	/// If there are no further records after "startRecord" or all records are "NaN" the result will be an empty array.</returns>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for fasle/off/no</returns>
	public ChromatogramSignal StatusValueSignalFromKey(int indexOfKey, DataFilter dataMode, ref int startRecord)
	{
		HeaderItem[] statusLogHeaderInformation = _reader.GetStatusLogHeaderInformation();
		if (indexOfKey < 0 || indexOfKey >= statusLogHeaderInformation.Length)
		{
			throw new ArgumentOutOfRangeException("indexOfKey");
		}
		HeaderItem recordFormat = statusLogHeaderInformation[indexOfKey];
		List<double> list = new List<double>();
		List<double> list2 = new List<double>();
		int num = ((_rawDataExtensions == null) ? _reader.RunHeaderEx.StatusLogCount : _reader.GetStatusLogEntriesCount());
		Dictionary<string, NameData> foundNames = new Dictionary<string, NameData>();
		int maxTextCategories = ValidateMaxTextcategories();
		double num2 = 0.0;
		double num3 = 0.0;
		int i = startRecord;
		bool flag = i == 0;
		int num4 = 0;
		for (; i < num; i++)
		{
			double time;
			double num5 = ((_rawDataExtensions == null) ? GetStatusNumericValueFromLog(indexOfKey, recordFormat, i, out time, foundNames, maxTextCategories) : GetStatusNumericValueFromSortedLog(indexOfKey, recordFormat, i, out time, foundNames, maxTextCategories));
			if (double.IsNaN(num5))
			{
				continue;
			}
			switch (dataMode)
			{
			case DataFilter.PositiveAndZero:
				if (num5 < 0.0)
				{
					continue;
				}
				break;
			case DataFilter.PositiveOnly:
				if (num5 <= 0.0)
				{
					continue;
				}
				break;
			}
			list.Add(num5);
			double num6 = time;
			if (i > 0)
			{
				if (num6 < num3)
				{
					list.Clear();
					list2.Clear();
					num3 = num6;
					num2 = num6;
					list.Add(num5);
					list2.Add(time);
					continue;
				}
				if (time <= num2)
				{
					time = num2 + 1E-06;
					num4++;
				}
				else if (flag)
				{
					flag = false;
					if (num4 > 0 && num4 == i - 1)
					{
						double num7 = time / (double)i;
						for (int j = 0; j < i; j++)
						{
							list2[j] = num7 * (double)j;
						}
					}
				}
			}
			list2.Add(time);
			num2 = time;
			num3 = num6;
		}
		startRecord = i;
		return ChromatogramSignal.FromTimeAndIntensity(list2.ToArray(), list.ToArray());
	}

	/// <summary>
	/// Get a status numeric value from a status log
	/// </summary>
	/// <param name="indexOfKey">Index of the item in status log header</param>
	/// <param name="recordFormat">status log format for this item</param>
	/// <param name="log">stats log record index</param>
	/// <param name="time">Time of this log entry</param>
	/// <param name="foundNames">list of names for "string" items</param>
	/// <param name="maxTextCategories">The max number of unique strings accepted</param>
	/// <returns>The numeric value for this log entry</returns>
	private double GetStatusNumericValueFromLog(int indexOfKey, HeaderItem recordFormat, int log, out double time, Dictionary<string, NameData> foundNames, int maxTextCategories)
	{
		IStatusLogEntry statusLogEntry = _reader.GetStatusLogEntry(log);
		object val = statusLogEntry.Values[indexOfKey];
		time = statusLogEntry.Time;
		return DecodeAllGenericNumerics(recordFormat, val, foundNames, maxTextCategories);
	}

	/// <summary>
	/// Get a status numeric value from a sorted status log
	/// </summary>
	/// <param name="indexOfKey">Index of the item in status log header</param>
	/// <param name="recordFormat">status log format for this item</param>
	/// <param name="log">stats log record index</param>
	/// <param name="time">Time of this log entry</param>
	/// <param name="foundNames">list of names for "string" items</param>
	/// <param name="maxTextCategories">The max number of unique strings accepted</param>
	/// <returns>The numeric value for this log entry</returns>
	private double GetStatusNumericValueFromSortedLog(int indexOfKey, HeaderItem recordFormat, int log, out double time, Dictionary<string, NameData> foundNames, int maxTextCategories)
	{
		IStatusLogEntry sortedStatusLogEntry = _rawDataExtensions.GetSortedStatusLogEntry(log);
		object val = sortedStatusLogEntry.Values[indexOfKey];
		time = sortedStatusLogEntry.Time;
		return DecodeAllGenericNumerics(recordFormat, val, foundNames, maxTextCategories);
	}

	/// <summary>
	/// Return a numeric value from any generic data type.
	/// Numeric types (double,int etc.) return the encoded number.
	/// String types "count unique string states" and return a state number.
	/// </summary>
	/// <param name="recordFormat">generic record format</param>
	/// <param name="val">data to decode</param>
	/// <param name="foundNames">For string data: unique strings found so far</param>
	/// <param name="maxTextCategories">Max unique strings to accept</param>
	/// <returns>the value from this log entry</returns>
	private static double DecodeAllGenericNumerics(HeaderItem recordFormat, object val, Dictionary<string, NameData> foundNames, int maxTextCategories)
	{
		switch (recordFormat.DataType)
		{
		case GenericDataTypes.NULL:
			return 0.0;
		case GenericDataTypes.CHAR:
			return (sbyte)val;
		case GenericDataTypes.TRUEFALSE:
			return ((bool)val) ? 1 : 0;
		case GenericDataTypes.YESNO:
			return ((bool)val) ? 1 : 0;
		case GenericDataTypes.ONOFF:
			return ((bool)val) ? 1 : 0;
		case GenericDataTypes.UCHAR:
			return (int)(byte)val;
		case GenericDataTypes.SHORT:
			return (short)val;
		case GenericDataTypes.USHORT:
			return (int)(ushort)val;
		case GenericDataTypes.LONG:
			return (int)val;
		case GenericDataTypes.ULONG:
			return (uint)val;
		case GenericDataTypes.FLOAT:
			return (float)val;
		case GenericDataTypes.DOUBLE:
			return (double)val;
		case GenericDataTypes.CHAR_STRING:
		case GenericDataTypes.WCHAR_STRING:
			return ValueForText(val, foundNames, maxTextCategories);
		default:
			return 0.0;
		}
	}

	/// <summary>
	/// Return a numeric value from any countable generic data type.
	/// Numeric types (short, byte etc.) return the encoded number.
	/// String types "count unique string states" and return a state number.
	/// </summary>
	/// <param name="val">data to decode</param>
	/// <param name="foundNames">For string data: unique strings found so far</param>
	/// <param name="maxTextCategories">Max unique strings to accept</param>
	/// <returns>the value from this log entry</returns>
	private static int ValueForText(object val, Dictionary<string, NameData> foundNames, int maxTextCategories)
	{
		if (val is string text)
		{
			if (foundNames.TryGetValue(text, out var value))
			{
				return value.Value;
			}
			if (foundNames.Count >= maxTextCategories)
			{
				return -1;
			}
			switch (text)
			{
			case "Load":
				return ValueToInsert(1, text);
			case "Inject":
				return ValueToInsert(0, text);
			case "Running":
				return ValueToInsert(1, text);
			case "Connected":
				return ValueToInsert(1, text);
			case "Ready":
				return ValueToInsert(1, text);
			case "Ok":
				return ValueToInsert(1, text);
			case "On":
				return ValueToInsert(1, text);
			case "Off":
				return ValueToInsert(0, text);
			case "Error":
				return ValueToInsert(2, text);
			default:
			{
				int num = -1;
				foreach (KeyValuePair<string, NameData> foundName in foundNames)
				{
					int value2 = foundName.Value.Value;
					if (value2 > num)
					{
						num = value2;
					}
				}
				int num2 = num + 1;
				foundNames.Add(text, new NameData
				{
					Name = text,
					Value = num2
				});
				return num2;
			}
			}
		}
		return 0;
		int ValueToInsert(int lookFor, string s)
		{
			bool flag = false;
			int num3 = -1;
			foreach (KeyValuePair<string, NameData> foundName2 in foundNames)
			{
				int value3 = foundName2.Value.Value;
				if (value3 == lookFor)
				{
					flag = true;
				}
				if (value3 > num3)
				{
					num3 = value3;
				}
			}
			int num4 = (flag ? (num3 + 1) : lookFor);
			foundNames.Add(s, new NameData
			{
				Name = s,
				Value = num4
			});
			return num4;
		}
	}

	private double GetTrailerNumericValueFromScan(int indexOfKey, HeaderItem recordFormat, int scan, Dictionary<string, NameData> foundNames, int maxTextCategories)
	{
		object trailerExtraValue = _reader.GetTrailerExtraValue(scan, indexOfKey);
		return DecodeAllGenericNumerics(recordFormat, trailerExtraValue, foundNames, maxTextCategories);
	}

	/// <summary>
	/// Silently ensure MaxTextCategories is in bounds, and fix if not
	/// </summary>
	/// <returns>the fixed max</returns>
	private int ValidateMaxTextcategories()
	{
		if (MaxTextCategories <= 3)
		{
			return 3;
		}
		if (MaxTextCategories >= 1000)
		{
			return 1000;
		}
		return MaxTextCategories;
	}

	/// <summary>
	/// Get names for know "enum" formats.
	/// </summary>
	/// <param name="recordFormat">format of this field</param>
	/// <returns>string version of values</returns>
	private string[] GetSeriesNamesFromFormat(HeaderItem recordFormat)
	{
		return recordFormat.DataType switch
		{
			GenericDataTypes.TRUEFALSE => new string[2] { "False", "True" }, 
			GenericDataTypes.YESNO => new string[2] { "No", "Yes" }, 
			GenericDataTypes.ONOFF => new string[2] { "Off", "On" }, 
			_ => new string[0], 
		};
	}

	/// <summary>
	/// Find a trend for the given status log item. Calling code should select a device with a status log first.
	/// </summary>
	/// <param name="indexOfKey">index into trailer extra for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for fasle/off/no</returns>
	public Tuple<int[], string[]> SmallStatusValueTrendFromKey(int indexOfKey, DataFilter dataMode)
	{
		HeaderItem[] statusLogHeaderInformation = _reader.GetStatusLogHeaderInformation();
		return GetSmallStatusLogValueTrendFromKey(indexOfKey, dataMode, statusLogHeaderInformation);
	}

	/// <summary>
	/// Find a trend for the given trailer item
	/// </summary>
	/// <param name="indexOfKey">index into trailer extra for required item</param>
	/// <param name="dataMode">Determines how positive, zero and negative data are handled.</param>
	/// <returns>the set of values. Items which are (True, false) or (On,Off)or (yes,no) return 1 for true/on/yes and 0 for fasle/off/no</returns>
	public Tuple<int[], string[]> SmallTrailerValueTrendFromKey(int indexOfKey, DataFilter dataMode)
	{
		HeaderItem[] trailerExtraHeaderInformation = _reader.GetTrailerExtraHeaderInformation();
		return GetSmallLogValueTrendFromKey(indexOfKey, dataMode, trailerExtraHeaderInformation);
	}

	private Tuple<int[], string[]> GetSmallLogValueTrendFromKey(int indexOfKey, DataFilter dataMode, HeaderItem[] header)
	{
		string filter = Filter;
		if (indexOfKey < 0 || indexOfKey >= header.Length)
		{
			throw new ArgumentOutOfRangeException("indexOfKey");
		}
		int maxTextCategories = ValidateMaxTextcategories();
		HeaderItem headerItem = header[indexOfKey];
		IEnumerable<int> filteredScanEnumerator = _reader.GetFilteredScanEnumerator(filter);
		List<int> list = new List<int>();
		string[] seriesNamesFromFormat = GetSeriesNamesFromFormat(headerItem);
		GenericDataTypes dataType = headerItem.DataType;
		int num;
		switch (dataType)
		{
		case GenericDataTypes.NULL:
			return new Tuple<int[], string[]>(new int[0], new string[0]);
		default:
			num = ((dataType == GenericDataTypes.WCHAR_STRING) ? 1 : 0);
			break;
		case GenericDataTypes.CHAR_STRING:
			num = 1;
			break;
		}
		bool flag = (byte)num != 0;
		if (seriesNamesFromFormat.Length != 0 || flag)
		{
			dataMode = DataFilter.AllData;
		}
		Dictionary<string, NameData> foundNames = new Dictionary<string, NameData>();
		foreach (int item in filteredScanEnumerator)
		{
			object trailerExtraValue = _reader.GetTrailerExtraValue(item, indexOfKey);
			int num2 = DecodeSmallValue(dataType, trailerExtraValue, foundNames, maxTextCategories);
			switch (dataMode)
			{
			case DataFilter.PositiveAndZero:
				if (num2 < 0)
				{
					continue;
				}
				break;
			case DataFilter.PositiveOnly:
				if (num2 <= 0)
				{
					continue;
				}
				break;
			}
			list.Add(num2);
		}
		seriesNamesFromFormat = CollateNames(foundNames, list, seriesNamesFromFormat);
		return new Tuple<int[], string[]>(list.ToArray(), seriesNamesFromFormat);
	}

	/// <summary>
	/// Decode only small (countable) values from logs.
	/// </summary>
	/// <param name="recordFormat">Format of this field</param>
	/// <param name="val">Value to decode</param>
	/// <param name="foundNames">Categories found so far</param>
	/// <param name="maxTextCategories">maximum number of categories</param>
	/// <returns>The log value</returns>
	private static int DecodeSmallValue(GenericDataTypes recordFormat, object val, Dictionary<string, NameData> foundNames, int maxTextCategories)
	{
		int result = 0;
		switch (recordFormat)
		{
		case GenericDataTypes.CHAR:
			result = (sbyte)val;
			break;
		case GenericDataTypes.TRUEFALSE:
			result = (((bool)val) ? 1 : 0);
			break;
		case GenericDataTypes.YESNO:
			result = (((bool)val) ? 1 : 0);
			break;
		case GenericDataTypes.ONOFF:
			result = (((bool)val) ? 1 : 0);
			break;
		case GenericDataTypes.UCHAR:
			result = (byte)val;
			break;
		case GenericDataTypes.SHORT:
			result = (short)val;
			break;
		case GenericDataTypes.USHORT:
			result = (ushort)val;
			break;
		case GenericDataTypes.LONG:
			result = (int)val;
			break;
		case GenericDataTypes.CHAR_STRING:
		case GenericDataTypes.WCHAR_STRING:
			result = ValueForText(val, foundNames, maxTextCategories);
			break;
		}
		return result;
	}

	/// <summary>
	/// Get a small (countable state) status log value trend, based on 
	/// index into the status log fields.
	/// </summary>
	/// <param name="indexOfKey">Index of field to read</param>
	/// <param name="dataMode">Filtering rule</param>
	/// <param name="header">Format of the log</param>
	/// <returns>The log value</returns>
	private Tuple<int[], string[]> GetSmallStatusLogValueTrendFromKey(int indexOfKey, DataFilter dataMode, HeaderItem[] header)
	{
		if (indexOfKey < 0 || indexOfKey >= header.Length)
		{
			throw new ArgumentOutOfRangeException("indexOfKey");
		}
		int maxTextCategories = ValidateMaxTextcategories();
		HeaderItem headerItem = header[indexOfKey];
		List<int> list = new List<int>();
		string[] seriesNamesFromFormat = GetSeriesNamesFromFormat(headerItem);
		GenericDataTypes dataType = headerItem.DataType;
		int num;
		switch (dataType)
		{
		case GenericDataTypes.NULL:
			return new Tuple<int[], string[]>(new int[0], new string[0]);
		default:
			num = ((dataType == GenericDataTypes.WCHAR_STRING) ? 1 : 0);
			break;
		case GenericDataTypes.CHAR_STRING:
			num = 1;
			break;
		}
		bool flag = (byte)num != 0;
		if (seriesNamesFromFormat.Length != 0 || flag)
		{
			dataMode = DataFilter.AllData;
		}
		int num2 = ((_rawDataExtensions == null) ? _reader.RunHeaderEx.StatusLogCount : _reader.GetStatusLogEntriesCount());
		Dictionary<string, NameData> foundNames = new Dictionary<string, NameData>();
		for (int i = 0; i < num2; i++)
		{
			IStatusLogEntry statusLogEntry = ((_rawDataExtensions == null) ? _reader.GetStatusLogEntry(i) : _rawDataExtensions.GetSortedStatusLogEntry(i));
			object val = statusLogEntry.Values[indexOfKey];
			int num3 = DecodeSmallValue(dataType, val, foundNames, maxTextCategories);
			switch (dataMode)
			{
			case DataFilter.PositiveAndZero:
				if (num3 < 0)
				{
					continue;
				}
				break;
			case DataFilter.PositiveOnly:
				if (num3 <= 0)
				{
					continue;
				}
				break;
			}
			list.Add(num3);
		}
		seriesNamesFromFormat = CollateNames(foundNames, list, seriesNamesFromFormat);
		return new Tuple<int[], string[]>(list.ToArray(), seriesNamesFromFormat);
	}

	/// <summary>
	/// Ensure that all trend items have a valid value.
	/// Assign values with "no saved name" as a category "other", valued 1 above the known names.
	/// </summary>
	/// <param name="foundNames">The set of names found (categories) in the log</param>
	/// <param name="trend">The data for the trend plot</param>
	/// <param name="names">The names for the numbered categories.</param>
	/// <returns></returns>
	private static string[] CollateNames(Dictionary<string, NameData> foundNames, List<int> trend, string[] names)
	{
		if (foundNames.Count > 0)
		{
			int num = -100;
			foreach (KeyValuePair<string, NameData> foundName in foundNames)
			{
				NameData value = foundName.Value;
				if (value.Value > num)
				{
					num = value.Value;
				}
			}
			int num2 = num + 1;
			for (int i = 0; i < trend.Count; i++)
			{
				if (trend[i] < 0)
				{
					trend[i] = num2;
				}
			}
			names = new string[num2 + 1];
			names[num2] = "Other";
			foreach (KeyValuePair<string, NameData> foundName2 in foundNames)
			{
				NameData value2 = foundName2.Value;
				names[value2.Value] = value2.Name;
			}
		}
		return names;
	}

	/// <summary>
	/// Find the durations of scans, and bin them into a number of "millisecond" bands, ready for a histogram plot.
	/// </summary>
	/// <param name="bands">number of bands</param>
	/// <returns>analysis of scan times (milliseconds)</returns>
	public BandedData GetScanDurationBands(int bands)
	{
		if (bands <= 0)
		{
			throw new ArgumentException("Bands must be > 0", "bands");
		}
		if (!EnsureMsDetector())
		{
			return new BandedData();
		}
		double[] array = ScanDurations();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] *= 1000.0;
		}
		return GetBandedData(array, bands);
	}

	/// <summary>
	/// Get banded data for a bar chart.
	/// </summary>
	/// <param name="data">data to map into bands</param>
	/// <param name="bands">requested number of bands</param>
	/// <returns>banded data for a bar chart</returns>
	public static BandedData GetBandedData(double[] data, int bands)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (bands <= 0)
		{
			throw new ArgumentException("Bands must be > 0", "bands");
		}
		if (data.Length == 0)
		{
			return new BandedData();
		}
		double num = BandSize(data, bands);
		bool banded = true;
		double[] array;
		double num2;
		if (num < 0.0)
		{
			array = new double[1];
			num = 1.0;
			num2 = data[0] - 0.5;
			bands = 1;
			banded = false;
		}
		else
		{
			array = new double[bands];
			num2 = data.Min();
		}
		for (int i = 0; i < data.Length; i++)
		{
			int num3 = (int)((data[i] - num2) / num);
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (num3 >= array.Length)
			{
				num3 = array.Length - 1;
			}
			array[num3] += 1.0;
		}
		double[] array2 = new double[bands];
		for (int j = 0; j < bands; j++)
		{
			array2[j] = num2 + num / 2.0 + (double)j * num;
		}
		return new BandedData
		{
			BandCenters = array2,
			Bands = array2.Length,
			BandWidth = num,
			BandData = array,
			Banded = banded
		};
	}

	/// <summary>
	/// Get banded data for a chart (histogram). If the data has a small enough range of values,
	/// then all values have their own bar.
	/// Each bar represents "the count of values within a given band".
	/// </summary>
	/// <param name="data">Data to band</param>
	/// <param name="defaultBands">If there are too many unique data values (&gt; maxBands) then
	/// the data becomes banded (grouped) into this many bands</param>
	/// <param name="maxBands">The maximum number of unique categories before that data must be banded</param>
	/// <returns>The banded data</returns>
	public static BandedData GetBandedData(int[] data, int defaultBands, int maxBands = 1000)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			return new BandedData();
		}
		int num = data.Min();
		int num2 = data.Max() - num + 1;
		if (num2 <= maxBands)
		{
			double[] array = new double[num2];
			for (int i = 0; i < data.Length; i++)
			{
				int num3 = data[i] - num;
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num3 >= array.Length)
				{
					num3 = array.Length - 1;
				}
				array[num3] += 1.0;
			}
			double[] array2 = new double[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = num + j;
			}
			return new BandedData
			{
				BandCenters = array2,
				Bands = array2.Length,
				BandWidth = 1.0,
				BandData = array,
				Banded = false
			};
		}
		if (defaultBands <= 0)
		{
			throw new ArgumentException("Number of bands must be > 0", "defaultBands");
		}
		double[] array3 = new double[data.Length];
		for (int k = 0; k < data.Length; k++)
		{
			array3[k] = data[k];
		}
		return GetBandedData(array3, defaultBands);
	}
}
