using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class for initial and timed events.
/// Includes the event, any attributes and the event time.
/// </summary>
[Serializable]
public class IntegratorEvent : TrackedDataObject, ICloneable
{
	private EventCode _opcode;

	private EventKind _kind;

	private double _time;

	private double _val1;

	private double _val2;

	/// <summary>
	/// Gets or sets the event which is occurring (for example "start cluster")
	/// </summary>
	public EventCode Opcode
	{
		get
		{
			return _opcode;
		}
		set
		{
			if (base.HasPropertyChange)
			{
				SetProperty(ref _opcode, "Opcode", value);
			}
			else
			{
				_opcode = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets a value which determines if this is an initial value, or a timed event.
	/// </summary>
	public EventKind Kind
	{
		get
		{
			return _kind;
		}
		set
		{
			if (base.HasPropertyChange)
			{
				SetProperty(ref _kind, "Kind", value);
			}
			else
			{
				_kind = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets the event time.
	/// If this is a timed event, this is the time the event occurs, in minutes.
	/// </summary>
	public double Time
	{
		get
		{
			return _time;
		}
		set
		{
			if (base.HasPropertyChange)
			{
				SetProperty(ref _time, "Time", value);
			}
			else
			{
				_time = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets the first data value associated with the event
	/// </summary>
	public double Value1
	{
		get
		{
			return _val1;
		}
		set
		{
			if (base.HasPropertyChange)
			{
				SetProperty(ref _val1, "Value1", value);
			}
			else
			{
				_val1 = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets the second data value associated with the event
	/// </summary>
	public double Value2
	{
		get
		{
			return _val2;
		}
		set
		{
			if (base.HasPropertyChange)
			{
				SetProperty(ref _val2, "Value2", value);
			}
			else
			{
				_val2 = value;
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IntegratorEvent" /> class. 
	/// Create an empty event
	/// </summary>
	public IntegratorEvent()
	{
		_opcode = EventCode.NoCode;
		_kind = EventKind.Initial;
	}

	/// <summary>
	/// Set the initial value of a timed event.
	/// This sets an event at time=0.
	/// </summary>
	/// <param name="eventCode">
	/// The event being programmed
	/// </param>
	/// <param name="value">
	/// initial value of the event
	/// </param>
	public void SetInitialValue(EventCode eventCode, int value)
	{
		Opcode = eventCode;
		Kind = EventKind.Initial;
		Time = 0.0;
		Value1 = value;
		Value2 = 0.0;
	}

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>
	/// An exact copy of the current collection.
	/// </returns>
	public virtual object Clone()
	{
		return MemberwiseClone();
	}
}
