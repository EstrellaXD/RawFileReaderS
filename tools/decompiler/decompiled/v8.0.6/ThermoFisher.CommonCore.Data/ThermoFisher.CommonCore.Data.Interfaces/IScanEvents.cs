namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ScanEvents interface.
/// </summary>
public interface IScanEvents
{
	/// <summary>
	/// Gets the number segments.
	/// </summary>
	int Segments { get; }

	/// <summary>
	/// Gets the total number of scan events, in all segments
	/// </summary>
	int ScanEvents { get; }

	/// <summary>
	/// Gets the number of events in a specific segment (0 based)
	/// </summary>
	/// <param name="segment">The segment number</param>
	/// <returns>The number of events in this segment</returns>
	int GetEventCount(int segment);

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
	IScanEvent GetEvent(int segment, int eventNumber);

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
	IScanEvent GetEvent(int eventNumber);
}
