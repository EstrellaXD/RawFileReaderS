using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A collection of scans, designed to slide forwards through a raw file.
/// </summary>
internal class SimpleScanWindow : IDisposable
{
	private readonly Func<ISimpleScanHeader, SimpleScanWithHeader> _reader;

	private readonly IList<ISimpleScanHeader> _availableScans;

	private readonly Func<ISimpleScanHeader[], SimpleScanWithHeader[]> _parallelReader;

	private readonly List<ScanAndIndex> _currentScans = new List<ScanAndIndex>();

	private ScanAndIndex[] _lastScanSet;

	private int _plannedLow;

	private int _plannedHigh = -1;

	private int _workPlanIndex;

	private List<(int, int)> _workPlan = new List<(int, int)>();

	private Task<SimpleScanWithHeader[]> _backgroundScans;

	public int Lookahead { get; set; } = 100;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SimpleScanWindow" /> class.
	/// </summary>
	/// <param name="spectrumReader">
	/// The spectrum reader.
	/// </param>
	/// <param name="parallelScanReader">Optional: Reader for batch of scans.
	/// If not supplied, spectrumReader is used.</param>
	/// <param name="availableScans">Table of scan number and RT of scans in raw file</param>
	public SimpleScanWindow(Func<ISimpleScanHeader, SimpleScanWithHeader> spectrumReader, Func<ISimpleScanHeader[], SimpleScanWithHeader[]> parallelScanReader, IList<ISimpleScanHeader> availableScans)
	{
		_reader = spectrumReader;
		_parallelReader = parallelScanReader;
		_availableScans = availableScans;
	}

	/// <summary>
	/// return data within scan range.
	/// </summary>
	/// <param name="startIndex">
	/// The start scan.
	/// </param>
	/// <param name="endIndex">
	/// The end scan.
	/// </param>
	/// <returns>
	/// A ref copy of the scans in range.
	/// </returns>
	public ScanAndIndex[] ReturnScansWithin(int startIndex, int endIndex)
	{
		int scanNumber = _availableScans[startIndex].ScanNumber;
		int scanNumber2 = _availableScans[endIndex].ScanNumber;
		if (_currentScans.Count == 0)
		{
			LoadAllScansInRange(startIndex, endIndex);
			return _lastScanSet = _currentScans.ToArray();
		}
		if (scanNumber == _currentScans[0].Header.ScanNumber && scanNumber2 == _currentScans[_currentScans.Count - 1].Header.ScanNumber)
		{
			return _lastScanSet;
		}
		if (scanNumber < _currentScans[0].Header.ScanNumber)
		{
			return Array.Empty<ScanAndIndex>();
		}
		int num = -1;
		int count = _currentScans.Count;
		for (int i = 0; i < count; i++)
		{
			if (_currentScans[i].Header.ScanNumber >= scanNumber)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			if (num > 0)
			{
				_currentScans.RemoveRange(0, num);
			}
		}
		else
		{
			_currentScans.Clear();
		}
		if (_currentScans.Count == 0)
		{
			LoadAllScansInRange(startIndex, endIndex);
			return _lastScanSet = _currentScans.ToArray();
		}
		int index = _currentScans[_currentScans.Count - 1].Index;
		if (index <= endIndex)
		{
			if (index < endIndex)
			{
				int num2 = index + 1;
				int num3 = num2 + Lookahead;
				if (num3 > endIndex)
				{
					if (num3 >= _availableScans.Count)
					{
						num3 = _availableScans.Count - 1;
					}
					LoadAllScansInRange(num2, num3);
				}
				else
				{
					LoadAllScansInRange(num2, endIndex);
				}
				int num4 = endIndex - startIndex + 1;
				_lastScanSet = new ScanAndIndex[num4];
				_currentScans.CopyTo(0, _lastScanSet, 0, num4);
				return _lastScanSet;
			}
			return _lastScanSet = _currentScans.ToArray();
		}
		int num5 = index - endIndex;
		int num6 = _currentScans.Count - num5;
		_lastScanSet = new ScanAndIndex[num6];
		_currentScans.CopyTo(0, _lastScanSet, 0, num6);
		return _lastScanSet;
	}

	private Task<SimpleScanWithHeader[]> FetchScansBackground(int startIndex, int endIndex)
	{
		return Task.Run(() => FetchScans(startIndex, endIndex));
	}

	private void AppendScans(int startIndex, int endIndex, SimpleScanWithHeader[] allScans)
	{
		for (int i = startIndex; i <= endIndex; i++)
		{
			_currentScans.Add(new ScanAndIndex(allScans[i - startIndex], i));
		}
	}

	private SimpleScanWithHeader[] FetchScans(int startIndex, int endIndex)
	{
		if (_parallelReader != null && endIndex > startIndex + 6)
		{
			return FetchParallelScans();
		}
		return FetchSimpleScans();
		SimpleScanWithHeader[] FetchParallelScans()
		{
			ISimpleScanHeader[] array = new ISimpleScanHeader[endIndex - startIndex + 1];
			for (int i = startIndex; i <= endIndex; i++)
			{
				array[i - startIndex] = _availableScans[i];
			}
			return _parallelReader(array);
		}
		SimpleScanWithHeader[] FetchSimpleScans()
		{
			SimpleScanWithHeader[] array = new SimpleScanWithHeader[endIndex - startIndex + 1];
			for (int i = startIndex; i <= endIndex; i++)
			{
				array[i - startIndex] = _reader(_availableScans[i]);
			}
			return array;
		}
	}

	/// <summary>
	/// Load all the scans in a range.
	/// </summary>
	/// <param name="startIndex">
	/// The start scan index.
	/// </param>
	/// <param name="endIndex">
	/// The end scan index.
	/// </param>
	private void LoadAllScansInRange(int startIndex, int endIndex)
	{
		List<(int, int)> workPlan = _workPlan;
		if (workPlan != null && workPlan.Count > 0)
		{
			if (_workPlanIndex == 0)
			{
				(int, int) tuple = _workPlan[_workPlanIndex];
				if (tuple.Item1 != startIndex || tuple.Item2 != endIndex)
				{
					throw new InvalidOperationException("Mismatch in work plan");
				}
				AppendScans(startIndex, endIndex, FetchScans(startIndex, endIndex));
			}
			else
			{
				if (_backgroundScans == null)
				{
					throw new InvalidOperationException("Mismatch in work plan");
				}
				_backgroundScans.Wait();
				SimpleScanWithHeader[] result = _backgroundScans.Result;
				_backgroundScans.Dispose();
				_backgroundScans = null;
				if (result.Length != endIndex - startIndex + 1)
				{
					throw new InvalidOperationException("Mismatch in work plan");
				}
				AppendScans(startIndex, endIndex, result);
				_backgroundScans = null;
			}
			_workPlanIndex++;
			if (_workPlanIndex < _workPlan.Count)
			{
				(int, int) tuple2 = _workPlan[_workPlanIndex];
				_backgroundScans = FetchScansBackground(tuple2.Item1, tuple2.Item2);
			}
		}
		else
		{
			AppendScans(startIndex, endIndex, FetchScans(startIndex, endIndex));
		}
	}

	private int PlannedScans()
	{
		return _plannedHigh - _plannedLow + 1;
	}

	/// <summary>
	/// Determine order of data reading, by adding "read plans" based on
	/// a requested range of scans, knowing what other scans would remain buffered
	/// Logic must be exactly the same as "ApplyScansWithin"
	/// </summary>
	/// <param name="startIndex">First required scan</param>
	/// <param name="endIndex">Last required scan</param>
	internal void PlanScansWithin(int startIndex, int endIndex)
	{
		if (PlannedScans() == 0)
		{
			_plannedLow = startIndex;
			_plannedHigh = endIndex;
			_workPlan.Add((startIndex, endIndex));
		}
		if ((startIndex == _plannedLow && endIndex == _plannedHigh) || startIndex < _plannedLow)
		{
			return;
		}
		int num = -1;
		if (startIndex <= _plannedHigh)
		{
			num = startIndex;
		}
		if (num >= 0)
		{
			_plannedLow = num;
		}
		else
		{
			_plannedLow = 0;
			_plannedHigh = -1;
		}
		if (PlannedScans() <= 0)
		{
			_plannedLow = startIndex;
			_plannedHigh = endIndex;
			_workPlan.Add((startIndex, endIndex));
		}
		int plannedHigh = _plannedHigh;
		if (plannedHigh > endIndex || plannedHigh >= endIndex)
		{
			return;
		}
		int num2 = plannedHigh + 1;
		int num3 = num2 + Lookahead;
		if (num3 > endIndex)
		{
			if (num3 >= _availableScans.Count)
			{
				num3 = _availableScans.Count - 1;
			}
			_plannedHigh = num3;
			_workPlan.Add((num2, num3));
		}
		else
		{
			_plannedHigh = endIndex;
			_workPlan.Add((num2, endIndex));
		}
	}

	public void Dispose()
	{
		if (_backgroundScans != null)
		{
			_backgroundScans.Wait();
			_backgroundScans.Dispose();
			_backgroundScans = null;
		}
	}
}
