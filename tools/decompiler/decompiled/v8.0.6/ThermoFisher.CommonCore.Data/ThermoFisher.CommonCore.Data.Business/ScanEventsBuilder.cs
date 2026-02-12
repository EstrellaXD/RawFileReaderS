using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class constructs a table of "segments and events".
/// Because this implements IScanEvents this class
/// can then be used to record the "method scan events" into a raw file.
/// The information can be organized as a set of segments,
/// with a set of events per segment.
/// The segments were designed to represent "time slices of a chromatogram"
/// though no time bounds are required by this object.
/// Many instruments just create a flat table (1 segment with all the events).
/// </summary>
public class ScanEventsBuilder : IScanEvents
{
	private readonly List<List<IScanEvent>> _segments = new List<List<IScanEvent>>();

	/// <summary>
	/// Gets the number segments.
	/// </summary>
	public int Segments => _segments.Count;

	/// <summary>
	/// Gets the total number of scan events, in all segments
	/// </summary>
	public int ScanEvents => _segments.Sum((List<IScanEvent> seg) => seg.Count);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventsBuilder" /> class.
	/// </summary>
	public ScanEventsBuilder()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventsBuilder" /> class.
	/// Copy from another sets of events
	/// </summary>
	/// <param name="other">
	/// The other events (to copy).
	/// </param>
	public ScanEventsBuilder(IScanEvents other)
	{
		int segments = other.Segments;
		for (int i = 0; i < segments; i++)
		{
			int eventCount = other.GetEventCount(i);
			IScanEvent[] array = new IScanEvent[eventCount];
			for (int j = 0; j < eventCount; j++)
			{
				array[j] = other.GetEvent(i, j);
			}
			AddSegment(array);
		}
	}

	/// <summary>
	/// Add a segment.
	/// </summary>
	/// <param name="events">
	/// The events for this segment.
	/// </param>
	public void AddSegment(IEnumerable<IScanEvent> events)
	{
		_segments.Add(new List<IScanEvent>(events));
	}

	/// <summary>
	/// Gets the number of events in a specific segment (0 based)
	/// </summary>
	/// <param name="segment">The segment number</param>
	/// <returns>The number of events in this segment</returns>
	public int GetEventCount(int segment)
	{
		return _segments[segment].Count;
	}

	/// <summary>
	/// Get an event, indexed by the segment and event numbers (zero based).
	/// </summary>
	/// <param name="segment">
	/// The segment.
	/// </param>
	/// <param name="eventNumber">
	/// The event number.
	/// </param>
	/// <returns>
	/// The event.
	/// </returns>
	public IScanEvent GetEvent(int segment, int eventNumber)
	{
		return _segments[segment][eventNumber];
	}

	/// <summary>
	/// Get an event, using indexed event number (zero based).
	/// This gets events from all segments in order,
	/// use "ScanEvents" to get the total count of events.
	/// </summary>
	/// <param name="eventNumber">
	/// The event Number.
	/// </param>
	/// <returns>
	/// The event.
	/// </returns>
	public IScanEvent GetEvent(int eventNumber)
	{
		if (eventNumber < 0)
		{
			throw new ArgumentOutOfRangeException("eventNumber");
		}
		int num = 0;
		foreach (List<IScanEvent> segment in _segments)
		{
			if (num + segment.Count > eventNumber)
			{
				return segment[eventNumber - num];
			}
			num += segment.Count;
		}
		throw new ArgumentOutOfRangeException("eventNumber");
	}
}
