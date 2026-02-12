using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// Get scan data from a file, based on a known list of scans.
/// Using multiple threads read scans in parallel, up to the cache limit.
/// If multiple passes over data are needed,
/// this can avoid getting the same scan more than once from the raw file, 
/// if the cache is large enough.
/// </summary>
public class ScanFromThreadedFileCreator : IScanCreator
{
	private readonly IRawDataPlus _commonFile;

	private readonly bool _includeReferencePeaks;

	private readonly IRawFileThreadManager _rawData;

	private readonly InstrumentSelection _required;

	private List<int> _scanNumbers;

	private SegmentedScan[] _segmentedScanCache;

	/// <summary>
	/// Gets or sets the cache limit.
	/// </summary>
	private int CacheLimit { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.ScanFromThreadedFileCreator" /> class.
	/// </summary>
	/// <param name="rawFile">
	///     The raw file.
	/// </param>
	/// <param name="requiredInstrument">Instrument whole scans are to be read</param>
	/// <param name="includeReferencePeaks">set if reference and exception peaks must be read. Default false</param>
	public ScanFromThreadedFileCreator(IRawFileThreadManager rawFile, InstrumentSelection requiredInstrument, bool includeReferencePeaks = false)
	{
		_rawData = rawFile;
		_commonFile = rawFile.CreateThreadAccessor();
		_commonFile.SelectInstrument(requiredInstrument.DeviceType, requiredInstrument.InstrumentIndex);
		_commonFile.IncludeReferenceAndExceptionData = includeReferencePeaks;
		_required = requiredInstrument;
		_includeReferencePeaks = includeReferencePeaks;
		CacheLimit = 20;
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
			return _rawData.CreateThreadAccessor().GetCentroidStream(_scanNumbers[index], includeReferenceAndExceptionPeaks: false);
		}
		throw new ArgumentOutOfRangeException("index");
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
		if (CacheLimit <= 0)
		{
			return _commonFile.GetSegmentedScanFromScanNumber(scanNumber, null);
		}
		SegmentedScan segmentedScan = _segmentedScanCache[index];
		if (segmentedScan != null)
		{
			return segmentedScan;
		}
		Array.Clear(_segmentedScanCache, 0, _segmentedScanCache.Length);
		int endCacheIndexExclusive = Math.Min(index + CacheLimit, count);
		CacheScans(_scanNumbers, index, endCacheIndexExclusive);
		return _segmentedScanCache[index];
	}

	/// <inheritdoc />
	public void Initialize(List<int> scanNumbers, int cacheLimit)
	{
		_scanNumbers = scanNumbers;
		CacheLimit = cacheLimit;
		int count = _scanNumbers.Count;
		_segmentedScanCache = new SegmentedScan[count];
		int num = Math.Min(count, cacheLimit);
		if (num > 0)
		{
			int startCacheIndex = 0;
			int endCacheIndexExclusive = num;
			CacheScans(scanNumbers, startCacheIndex, endCacheIndexExclusive);
		}
	}

	/// <summary>
	/// Add some scans to the cache.
	/// </summary>
	/// <param name="scanNumbers">
	/// The scan numbers.
	/// </param>
	/// <param name="startCacheIndex">
	/// The start cache index. Cache from this index in the scan numbers.
	/// </param>
	/// <param name="endCacheIndexExclusive">
	/// The end cache index exclusive. Cache up to but not including this index in scan numbers.
	/// </param>
	private void CacheScans(List<int> scanNumbers, int startCacheIndex, int endCacheIndexExclusive)
	{
		int num = endCacheIndexExclusive - startCacheIndex;
		int num2 = num;
		int num3 = startCacheIndex;
		while (num2 > 0)
		{
			int num4 = ((num2 > 300) ? 300 : num2);
			int num5 = num3 + num4;
			int rangeSize = Math.Max(10, num / 10);
			Parallel.ForEach(Partitioner.Create(num3, num5, rangeSize), delegate(Tuple<int, int> range)
			{
				for (int i = range.Item1; i < range.Item2; i++)
				{
					IRawDataExtended rawDataExtended = _rawData.CreateThreadAccessor();
					rawDataExtended.SelectInstrument(_required.DeviceType, _required.InstrumentIndex);
					rawDataExtended.IncludeReferenceAndExceptionData = _includeReferencePeaks;
					_segmentedScanCache[i] = rawDataExtended.GetSegmentedScanFromScanNumber(scanNumbers[i]);
				}
			});
			num2 -= num4;
			num3 = num5;
		}
	}
}
