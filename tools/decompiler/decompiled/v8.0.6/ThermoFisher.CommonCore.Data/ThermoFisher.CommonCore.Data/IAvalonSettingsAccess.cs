using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to avalon setting
/// </summary>
public interface IAvalonSettingsAccess
{
	/// <summary>
	/// Gets the list of time events
	/// </summary>
	ReadOnlyCollection<IntegratorEvent> Events { get; }

	/// <summary>
	/// Count the number of timed (as opposed to initial) events
	/// </summary>
	/// <returns>
	/// The number of timed events
	/// </returns>
	int CountTimedEvents();

	/// <summary>
	/// Step past any initial events, and find the first time event
	/// </summary>
	/// <returns>
	/// The node for the first timed event
	/// </returns>
	int FirstTimedEvent();

	/// <summary>
	/// Find the first event matching a specific event code
	/// </summary>
	/// <param name="code">
	/// The code to search for
	/// </param>
	/// <param name="eventNumber">
	/// (returned) the number of the event in the list
	/// </param>
	/// <returns>
	/// The node containing the first event matching the supplied event code
	/// </returns>
	IntegratorEvent FindEventValue(EventCode code, out int eventNumber);

	/// <summary>
	/// Find the first event matching a specific event code and kind
	/// </summary>
	/// <param name="kind">
	/// Initial or timed version
	/// </param>
	/// <param name="code">
	/// The code to search for
	/// </param>
	/// <param name="eventNumber">
	/// (returned) the number of the event in the list
	/// </param>
	/// <returns>
	/// The node containing the first event matching the supplied event code
	/// </returns>
	IntegratorEvent FindEventValue(EventKind kind, EventCode code, out int eventNumber);
}
