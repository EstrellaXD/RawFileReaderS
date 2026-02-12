using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped scan events.
/// Converts internal data to public interface IScanEvents
/// </summary>
internal class WrappedScanEvents : IScanEvents
{
	private readonly List<ScanEvent> _allScanEvents;

	private readonly int _allScanEventsCount;

	private readonly int _allSegmentsCount;

	private int[] _numScanEventsPerSegment;

	/// <summary>
	/// Gets the total number of scan events, in all segments
	/// </summary>
	public int ScanEvents => _allScanEventsCount;

	/// <summary>
	/// Gets the number segments.
	/// </summary>
	public int Segments => _allSegmentsCount;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedScanEvents" /> class.
	/// </summary>
	/// <param name="msDevice">The MS device.
	/// Since this is internal: Caller will validate
	/// that this is not null.</param>
	public WrappedScanEvents(MassSpecDevice msDevice)
	{
		_allSegmentsCount = msDevice.NumScanEventSegments;
		_allScanEventsCount = msDevice.NumScanEvents;
		_allScanEvents = GetAllScanEvents(msDevice);
	}

	/// <summary>
	/// Get an event, using indexed event number (zero based).
	/// This gets events from all segments in order,
	/// use "ScanEvents" to get the total count of events.
	/// </summary>
	/// <param name="eventNumber">The event Number.</param>
	/// <returns>
	/// The event.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when eventNumber is out of range</exception>
	public IScanEvent GetEvent(int eventNumber)
	{
		if (eventNumber >= _allScanEventsCount || eventNumber < 0)
		{
			throw new ArgumentOutOfRangeException("eventNumber", "Event number must be positive and less than " + _allScanEventsCount);
		}
		return _allScanEvents[eventNumber];
	}

	/// <summary>
	/// Get an event, indexed by the segment and event numbers (zero based).
	/// </summary>
	/// <param name="segmentIndex">The segment index.</param>
	/// <param name="eventNumber">The event number.</param>
	/// <returns>
	/// The event.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">When event number out of range</exception>
	public IScanEvent GetEvent(int segmentIndex, int eventNumber)
	{
		if (segmentIndex < 0 || segmentIndex > Segments)
		{
			throw new ArgumentOutOfRangeException("segmentIndex");
		}
		int num = _numScanEventsPerSegment[segmentIndex];
		if (num == 0)
		{
			return null;
		}
		if (eventNumber < 0 || eventNumber >= num)
		{
			throw new ArgumentOutOfRangeException($"Invalid event number is out of range {eventNumber} > {num}");
		}
		int num2 = 0;
		for (int i = 0; i < segmentIndex; i++)
		{
			int num3 = _numScanEventsPerSegment[i];
			num2 += num3;
		}
		return _allScanEvents[num2 + eventNumber];
	}

	/// <summary>
	/// Gets the number of events in a specific segment (0 based)
	/// </summary>
	/// <param name="segmentIndex">The segment number.</param>
	/// <returns>The number of events in this segment</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">segment Number</exception>
	public int GetEventCount(int segmentIndex)
	{
		if (segmentIndex < 0 || segmentIndex > Segments)
		{
			throw new ArgumentOutOfRangeException("segmentIndex");
		}
		return _numScanEventsPerSegment[segmentIndex];
	}

	/// <summary>
	/// Gets all scan events.
	/// </summary>
	/// <param name="msDevice">
	/// The mass spec Device.
	/// </param>
	/// <returns>
	/// All the scan events
	/// </returns>
	private List<ScanEvent> GetAllScanEvents(MassSpecDevice msDevice)
	{
		List<ScanEvent> list = new List<ScanEvent>();
		int numScanEventSegments = msDevice.NumScanEventSegments;
		_numScanEventsPerSegment = new int[numScanEventSegments];
		for (int i = 0; i < numScanEventSegments; i++)
		{
			List<ScanEvent> scanEvents = msDevice.GetScanEvents(i);
			if (scanEvents == null || !scanEvents.Any())
			{
				_numScanEventsPerSegment[i] = 0;
				continue;
			}
			int count = scanEvents.Count;
			_numScanEventsPerSegment[i] = count;
			for (int j = 0; j < count; j++)
			{
				list.Add(scanEvents[j]);
			}
		}
		return list;
	}
}
