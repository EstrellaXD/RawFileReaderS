using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The cached scan provider.
/// </summary>
internal class CachedScanProvider : IScanReader
{
	private readonly Queue<Scan> _scanCache;

	private readonly int _cacheLimit;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.CachedScanProvider" /> class.
	/// </summary>
	/// <param name="capacity">
	/// The capacity.
	/// </param>
	public CachedScanProvider(int capacity)
	{
		_cacheLimit = capacity;
		_scanCache = new Queue<Scan>(capacity);
	}

	/// <summary>
	/// Get scan from a scan number.
	/// </summary>
	/// <param name="rawDataReader">
	/// The raw data.
	/// </param>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The scan at that scan number, or null if the scan number is out of range.
	/// </returns>
	public Scan GetScanFromScanNumber(IDetectorReaderBase rawDataReader, int scanNumber)
	{
		if (_scanCache.Count > 0)
		{
			foreach (Scan item in _scanCache)
			{
				if (item.ScanStatistics.ScanNumber == scanNumber)
				{
					return item.DeepClone();
				}
			}
		}
		Scan scan = ((rawDataReader is IDetectorReader detectorReader) ? Scan.FromDetector(detectorReader, scanNumber) : Scan.FromFile(rawDataReader as IRawData, scanNumber));
		if (scan != null)
		{
			if (_scanCache.Count == _cacheLimit)
			{
				_scanCache.Dequeue();
			}
			_scanCache.Enqueue(scan.DeepClone());
		}
		return scan;
	}

	/// <summary>
	/// The get scan at time.
	/// </summary>
	/// <param name="rawFile">
	/// The raw file.
	/// </param>
	/// <param name="time">
	/// The time.
	/// </param>
	/// <returns>
	/// The scan nearest the given time.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">Thrown on null raw file
	/// </exception>
	public Scan GetScanAtTime(IDetectorReaderBase rawFile, double time)
	{
		if (rawFile == null)
		{
			throw new ArgumentNullException("rawFile");
		}
		return GetScanFromScanNumber(rawFile, rawFile.ScanNumberFromRetentionTime(time));
	}
}
