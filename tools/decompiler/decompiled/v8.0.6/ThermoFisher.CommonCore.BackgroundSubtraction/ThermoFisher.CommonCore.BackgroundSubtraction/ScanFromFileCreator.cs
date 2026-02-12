using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// Get scan data from a raw data, based on a know list of scans.
/// Can avoid getting the same can multiple times, using a cache.
/// </summary>
internal class ScanFromFileCreator : IScanCreator
{
	private readonly IDetectorReaderBase _rawDataReader;

	private List<int> _scanNumbers;

	private SegmentedScan[] _segmentedScanCache;

	/// <summary>
	/// Gets or sets the cache limit.
	/// </summary>
	private int CacheLimit { get; set; }

	/// <inheritdoc />
	public void Initialize(List<int> scanNumbers, int cacheLimit)
	{
		_scanNumbers = scanNumbers;
		CacheLimit = cacheLimit;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.ScanFromFileCreator" /> class.
	/// </summary>
	/// <param name="rawDataReader">
	/// The raw file.
	/// </param>
	public ScanFromFileCreator(IDetectorReaderBase rawDataReader)
	{
		_rawDataReader = rawDataReader;
		CacheLimit = 20;
	}

	/// <summary>
	/// create segmented scan.
	/// </summary>
	/// <param name="index">
	/// The index into the supplied table of scans (on construction)
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" />.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">when index is outside of the scan numbers array
	/// </exception>
	public SegmentedScan CreateSegmentedScan(int index)
	{
		int count = _scanNumbers.Count;
		if (index >= count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int scanNumber = _scanNumbers[index];
		if (count > CacheLimit)
		{
			return _rawDataReader.GetSegmentedScanFromScanNumber(scanNumber, null);
		}
		if (_segmentedScanCache == null)
		{
			_segmentedScanCache = new SegmentedScan[count];
		}
		return _segmentedScanCache[index] ?? (_segmentedScanCache[index] = _rawDataReader.GetSegmentedScanFromScanNumber(scanNumber));
	}

	/// <summary>
	/// Create a centroid stream.
	/// </summary>
	/// <param name="index">
	/// The index into the supplied table of scans (on construction)
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.CentroidStream" />.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">when index is outside of the scan numbers array
	/// </exception>
	public CentroidStream CreateCentroidStream(int index)
	{
		if (index < _scanNumbers.Count)
		{
			return _rawDataReader.GetCentroidStream(_scanNumbers[index], includeReferenceAndExceptionPeaks: false);
		}
		throw new ArgumentOutOfRangeException("index");
	}
}
