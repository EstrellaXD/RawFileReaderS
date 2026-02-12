using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Event list, includes both initial events and timed events.
/// <para>Initial events are similar to properties of the peak detector.
/// They are always at the start of the list, and are not time triggered.</para>
/// <para>Timed events modify the initial values for a limited duration,
/// or control special features, such a detecting the next peak as "negative".</para>
/// The additional functions are intended to assist in editing the list.
/// <para>The initial events are at the start of the list</para>
/// <para>The timed events are in order</para>
/// </summary>
[Serializable]
[CollectionDataContract]
public class EventCollection : ItemCollection<IntegratorEvent>, IAvalonSettingsAccess, ICloneable
{
	/// <summary>
	/// Gets the List of time events
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<IntegratorEvent> Events => new ReadOnlyCollection<IntegratorEvent>(this);

	/// <summary>
	/// Count the number of timed (as opposed to initial) events
	/// </summary>
	/// <returns>The number of timed events</returns>
	public int CountTimedEvents()
	{
		return this.Count((IntegratorEvent integratorEvent) => integratorEvent.Kind == EventKind.Timed);
	}

	/// <summary>
	/// Step past any initial events, and find the first time event
	/// </summary>
	/// <returns>The node for the first timed event</returns>
	public int FirstTimedEvent()
	{
		int i;
		for (i = 0; i < base.Count && base[i].Kind == EventKind.Initial; i++)
		{
		}
		return i;
	}

	/// <summary>
	/// Find the first event matching a specific event code
	/// </summary>
	/// <param name="code">The code to search for</param>
	/// <param name="eventNumber">(returned) the number of the event in the list</param>
	/// <returns>The node containing the first event matching the supplied event code</returns>
	public IntegratorEvent FindEventValue(EventCode code, out int eventNumber)
	{
		int num = 1;
		int i = 0;
		eventNumber = 0;
		for (; i < base.Count; i++)
		{
			if (base[i].Opcode == code)
			{
				eventNumber = num;
				return base[i];
			}
			num++;
		}
		return null;
	}

	/// <summary>
	/// Find the first event matching a specific event code and kind
	/// </summary>
	/// <param name="kind">Initial or timed version</param>
	/// <param name="code">The code to search for</param>
	/// <param name="eventNumber">(returned) the number of the event in the list</param>
	/// <returns>The node containing the first event matching the supplied event code</returns>
	public IntegratorEvent FindEventValue(EventKind kind, EventCode code, out int eventNumber)
	{
		int num = 1;
		int i = 0;
		eventNumber = 0;
		for (; i < base.Count; i++)
		{
			if (base[i].Opcode == code && base[i].Kind == kind)
			{
				eventNumber = num;
				return base[i];
			}
			num++;
		}
		return null;
	}

	/// <summary>
	/// Creates the initial default events for the collection
	/// </summary>
	public void ResetToDefaults()
	{
		Clear();
		IntegratorEvent integratorEvent = new IntegratorEvent();
		integratorEvent.SetInitialValue(EventCode.StartThreshold, 10000);
		Add(integratorEvent);
		integratorEvent = new IntegratorEvent();
		integratorEvent.SetInitialValue(EventCode.EndThreshold, 10000);
		Add(integratorEvent);
		integratorEvent = new IntegratorEvent();
		integratorEvent.SetInitialValue(EventCode.AreaThreshold, 10000);
		Add(integratorEvent);
		integratorEvent = new IntegratorEvent();
		integratorEvent.SetInitialValue(EventCode.PPResolution, 1);
		Add(integratorEvent);
		integratorEvent = new IntegratorEvent();
		integratorEvent.SetInitialValue(EventCode.BunchFactor, 1);
		Add(integratorEvent);
		integratorEvent = new IntegratorEvent();
		integratorEvent.SetInitialValue(EventCode.NegativePeaks, 0);
		Add(integratorEvent);
		integratorEvent = new IntegratorEvent();
		integratorEvent.SetInitialValue(EventCode.Tension, 1);
		Add(integratorEvent);
	}

	/// <summary>
	/// Make a copy of the current event collection
	/// </summary>
	/// <returns>A copy of the event collection</returns>
	public new object Clone()
	{
		EventCollection eventCollection = new EventCollection();
		eventCollection.AddRange(this);
		return eventCollection;
	}
}
