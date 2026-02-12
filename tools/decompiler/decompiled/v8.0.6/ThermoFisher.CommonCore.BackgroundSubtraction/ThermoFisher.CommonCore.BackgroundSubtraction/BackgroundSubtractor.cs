using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// Computes and returns the average subtracted scan
/// </summary>
public class BackgroundSubtractor
{
	private readonly List<ScanStatistics> _scanList = new List<ScanStatistics>();

	private IDetectorReaderBase _rawDataReader;

	private string _filter = string.Empty;

	private IScanAverage _averager;

	/// <summary>
	/// Gets Averager.
	/// </summary>
	private IScanAverage Averager
	{
		get
		{
			if (_averager == null)
			{
				_averager = ((_rawDataReader is IDetectorReader rawDataReader) ? ScanAverager.FromDetector(rawDataReader) : ScanAverager.FromFile(_rawDataReader as IRawData));
			}
			return _averager;
		}
	}

	/// <summary>
	/// Calculate the foreground average scan.
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <returns>
	/// The averaged scan
	/// </returns>
	private Scan GetForeGroundAvgScan(double startTime, double endTime)
	{
		return Averager.GetAverageScanInTimeRange(startTime, endTime, _filter);
	}

	/// <summary>
	/// Calculate a background average scan.
	/// </summary>
	/// <param name="backSettings">
	/// The background subtraction settings.
	/// </param>
	/// <returns>
	/// The averaged background
	/// </returns>
	private Scan GetBackGroundAverageScan(BackgroundSubtractionSettings backSettings)
	{
		Scan result = null;
		_scanList.Clear();
		if (backSettings != null && (backSettings.SelectedRange1 || backSettings.SelectedRange2))
		{
			IDetectorReaderPlus plus = _rawDataReader as IDetectorReaderPlus;
			if (backSettings.SelectedRange1)
			{
				int start = _rawDataReader.ScanNumberFromRetentionTime(backSettings.Range1StartTime);
				int end = _rawDataReader.ScanNumberFromRetentionTime(backSettings.Range1EndTime);
				GetScanStatistics(plus, start, end, _filter);
			}
			if (backSettings.SelectedRange2)
			{
				int start = _rawDataReader.ScanNumberFromRetentionTime(backSettings.Range2StartTime);
				int end = _rawDataReader.ScanNumberFromRetentionTime(backSettings.Range2EndTime);
				GetScanStatistics(plus, start, end, _filter);
			}
			if (_scanList.Count > 0)
			{
				result = Averager.AverageSpectra(_scanList);
			}
		}
		return result;
	}

	/// <summary>
	/// Get scan statistics.
	/// </summary>
	/// <param name="plus">
	/// The plus.
	/// </param>
	/// <param name="start">
	/// The start scan.
	/// </param>
	/// <param name="end">
	/// The end scan.
	/// </param>
	/// <param name="filter">
	/// The scan filter.
	/// </param>
	private void GetScanStatistics(IDetectorReaderPlus plus, int start, int end, string filter)
	{
		if (plus != null)
		{
			ScanFilterHelper filterHelper;
			try
			{
				filterHelper = plus.BuildFilterHelper(filter);
			}
			catch (ArgumentException)
			{
				return;
			}
			for (int i = start; i <= end; i++)
			{
				if (plus.TestScan(i, filterHelper))
				{
					_scanList.Add(plus.GetScanStatsForScanNumber(i));
				}
			}
		}
		else if (plus is IDetectorReader detectorReader)
		{
			InitializeScanList(detectorReader, start, end, filter);
		}
		else
		{
			InitializeScanList(plus as IRawData, start, end, filter);
		}
	}

	/// <summary>Initializes the scan list.</summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="start">The start.</param>
	/// <param name="end">The end.</param>
	/// <param name="filter">The filter.</param>
	/// <exception cref="T:System.ArgumentNullException">detectorReader</exception>
	private void InitializeScanList(IDetectorReader detectorReader, int start, int end, string filter)
	{
		for (int i = start; i <= end; i++)
		{
			UpdateScanList(filter, Scan.FromDetector(detectorReader, i));
		}
	}

	/// <summary>Initializes the scan list.</summary>
	/// <param name="rawData">The raw data.</param>
	/// <param name="start">The start.</param>
	/// <param name="end">The end.</param>
	/// <param name="filter">The filter.</param>
	/// <exception cref="T:System.ArgumentNullException">rawData</exception>
	private void InitializeScanList(IRawData rawData, int start, int end, string filter)
	{
		if (rawData == null)
		{
			throw new ArgumentNullException("rawData");
		}
		for (int i = start; i <= end; i++)
		{
			UpdateScanList(filter, Scan.FromFile(rawData, i));
		}
	}

	/// <summary>Updates the scan list.</summary>
	/// <param name="filter">The filter.</param>
	/// <param name="scan">The scan.</param>
	private void UpdateScanList(string filter, Scan scan)
	{
		if (ScanDefinition.FromString(filter).MatchToScanType(scan.ScanType))
		{
			_scanList.Add(scan.ScanStatistics);
		}
	}

	/// <summary>
	/// Computes the average scan and then subtracts the background average scan and 
	/// return the average subtracted scan.
	/// </summary>
	/// <param name="rawFile">
	/// Raw file to read scans based on time.
	/// </param>
	/// <param name="filter">
	/// scan filter
	/// </param>
	/// <param name="backSettings">
	/// Background Information.
	/// </param>
	/// <param name="startTime">
	/// Foreground StartTime.
	/// </param>
	/// <param name="endTime">
	/// Foreground EndTime.
	/// </param>
	/// <returns>
	/// Scan object containing the subtracted scan.
	/// </returns>
	public Scan GetBackgroundSubtraction(IRawData rawFile, string filter, BackgroundSubtractionSettings backSettings, double startTime, double endTime)
	{
		Scan scan = null;
		_rawDataReader = rawFile;
		_filter = filter;
		if (backSettings != null && _rawDataReader != null && !string.IsNullOrEmpty(filter) && rawFile.SelectMsData())
		{
			scan = GetForeGroundAvgScan(startTime, endTime);
			if (scan != null)
			{
				Scan backGroundAverageScan = GetBackGroundAverageScan(backSettings);
				SpectrumAverager subtractionPointer = new SpectrumAverager();
				scan.SubtractionPointer = subtractionPointer;
				scan -= backGroundAverageScan;
			}
		}
		return scan;
	}

	/// <summary>
	/// Computes the average scan and then subtracts the background average scan and 
	/// return the average subtracted scan.
	/// </summary>
	/// <param name="detectorReader">
	/// Raw file to read scans based on time.
	/// </param>
	/// <param name="filter">
	/// scan filter
	/// </param>
	/// <param name="backSettings">
	/// Background Information.
	/// </param>
	/// <param name="startTime">
	/// Foreground StartTime.
	/// </param>
	/// <param name="endTime">
	/// Foreground EndTime.
	/// </param>
	/// <returns>
	/// Scan object containing the subtracted scan.
	/// </returns>
	public Scan GetBackgroundSubtraction(IDetectorReader detectorReader, string filter, BackgroundSubtractionSettings backSettings, double startTime, double endTime)
	{
		Scan scan = null;
		_rawDataReader = detectorReader;
		_filter = filter;
		if (backSettings != null && _rawDataReader != null && !string.IsNullOrEmpty(filter) && detectorReader.ConfiguredDetector.DeviceType == Device.MS)
		{
			scan = GetForeGroundAvgScan(startTime, endTime);
			if (scan != null)
			{
				Scan backGroundAverageScan = GetBackGroundAverageScan(backSettings);
				SpectrumAverager subtractionPointer = new SpectrumAverager();
				scan.SubtractionPointer = subtractionPointer;
				scan -= backGroundAverageScan;
			}
		}
		return scan;
	}

	/// <summary>
	/// Computes the average scan and then subtracts the background average scan and 
	/// return the average subtracted scan.
	/// </summary>
	/// <param name="rawFile">
	/// Raw file to read scans based on time.
	/// </param>
	/// <param name="filter">
	/// scan filter
	/// </param>
	/// <param name="backSettings">
	/// Background Information.
	/// </param>
	/// <param name="foreAverageScan">
	/// Scan which data is subtracted from
	/// </param>
	/// <returns>
	/// The subtracted scan.
	/// </returns>
	public Scan GetBackgroundSubtraction(IRawData rawFile, string filter, BackgroundSubtractionSettings backSettings, Scan foreAverageScan)
	{
		Scan scan = null;
		if (rawFile != null && backSettings != null)
		{
			_rawDataReader = rawFile;
			if (!string.IsNullOrEmpty(filter))
			{
				_filter = filter;
				if (rawFile.SelectMsData() && foreAverageScan != null)
				{
					scan = foreAverageScan;
					Scan backGroundAverageScan = GetBackGroundAverageScan(backSettings);
					SpectrumAverager subtractionPointer = new SpectrumAverager();
					scan.SubtractionPointer = subtractionPointer;
					scan -= backGroundAverageScan;
				}
				else
				{
					scan = GetBackGroundAverageScan(backSettings);
				}
			}
		}
		return scan;
	}

	/// <summary>
	/// Computes the average scan and then subtracts the background average scan and 
	/// return the average subtracted scan.
	/// </summary>
	/// <param name="detectorReader">
	/// Detector reader to read scans based on time.
	/// </param>
	/// <param name="filter">
	/// scan filter
	/// </param>
	/// <param name="backSettings">
	/// Background Information.
	/// </param>
	/// <param name="foreAverageScan">
	/// Scan which data is subtracted from
	/// </param>
	/// <returns>
	/// The subtracted scan.
	/// </returns>
	public Scan GetBackgroundSubtraction(IDetectorReader detectorReader, string filter, BackgroundSubtractionSettings backSettings, Scan foreAverageScan)
	{
		Scan scan = null;
		if (detectorReader != null && backSettings != null)
		{
			_rawDataReader = detectorReader;
			if (!string.IsNullOrEmpty(filter))
			{
				_filter = filter;
				if (detectorReader.ConfiguredDetector.DeviceType == Device.MS && foreAverageScan != null)
				{
					scan = foreAverageScan;
					Scan backGroundAverageScan = GetBackGroundAverageScan(backSettings);
					SpectrumAverager subtractionPointer = new SpectrumAverager();
					scan.SubtractionPointer = subtractionPointer;
					scan -= backGroundAverageScan;
				}
				else
				{
					scan = GetBackGroundAverageScan(backSettings);
				}
			}
		}
		return scan;
	}
}
