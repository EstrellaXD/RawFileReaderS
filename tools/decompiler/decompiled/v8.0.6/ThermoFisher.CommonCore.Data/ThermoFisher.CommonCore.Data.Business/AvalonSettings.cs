using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Parameters for the Avalon integrator
/// </summary>
[Serializable]
[CollectionDataContract]
public class AvalonSettings : EventCollection
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.AvalonSettings" /> class. 
	/// Default constructor
	/// </summary>
	public AvalonSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.AvalonSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public AvalonSettings(IAvalonSettingsAccess access)
	{
		if (access == null)
		{
			throw new ArgumentNullException("access");
		}
		Clear();
		foreach (IntegratorEvent @event in access.Events)
		{
			Add(@event);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.AvalonSettings" /> class. 
	/// </summary>
	/// <param name="events">
	/// Integrator events to copy
	/// </param>
	/// <exception cref="T:System.ArgumentNullException">
	/// </exception>
	public AvalonSettings(ReadOnlyCollection<IntegratorEvent> events)
	{
		if (events == null)
		{
			throw new ArgumentNullException("events");
		}
		Clear();
		foreach (IntegratorEvent @event in events)
		{
			Add(@event);
		}
	}
}
