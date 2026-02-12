using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// TrackedDataObject object is an abstract class that provides property
/// change event processing without redundancy.
/// If the business object you are designing
/// requires property change notifications,
/// implement this abstract class and call the SetProperty inside your setters./&gt;
/// </summary>
[Serializable]
[DataContract]
public abstract class TrackedDataObject : CommonCoreDataObject, INotifyPropertyChanged
{
	/// <summary>
	/// Gets a value indicating whether this object has a property change event
	/// </summary>
	protected bool HasPropertyChange => this.PropertyChanged != null;

	/// <summary>
	/// Occurs when a property value changes.
	/// </summary>
	[CoreDataElement(Ignore = true)]
	[field: NonSerialized]
	public event PropertyChangedEventHandler PropertyChanged;

	/// <summary>
	/// Serves as a hash function for a particular type.
	/// </summary>
	/// <returns>
	/// A hash code for the current <see cref="T:System.Object" />.
	/// </returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>
	/// Compares this object with another.
	/// Traverse the set of member variables to compare against the object that was passed in.
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
	/// <returns>
	/// true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
	/// </returns>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="obj" /> parameter is null.</exception>
	public override bool Equals(object obj)
	{
		return PerformEquals(obj, deep: false);
	}

	/// <summary>
	/// Sets the property and hooks up event processing automatically.
	/// </summary>
	/// <typeparam name="T">The type of object the underlying data class is.</typeparam>
	/// <param name="privateMember">The private member.</param>
	/// <param name="propertyName">Name of the property that will appear in the <see cref="E:ThermoFisher.CommonCore.Data.TrackedDataObject.PropertyChanged" /> event.</param>
	/// <param name="value">The value the property is being set to.</param>
	/// <returns>true if the property value has changed</returns>
	protected bool SetProperty<T>(ref T privateMember, string propertyName, object value)
	{
		bool flag = false;
		if ((object)privateMember != value)
		{
			flag = privateMember == null || !privateMember.Equals(value);
			if (privateMember is INotifyPropertyChanged notifyPropertyChanged)
			{
				notifyPropertyChanged.PropertyChanged -= PropertySignalChangedHandler;
			}
			privateMember = (T)value;
			if (privateMember is INotifyPropertyChanged notifyPropertyChanged2)
			{
				notifyPropertyChanged2.PropertyChanged += PropertySignalChangedHandler;
			}
			if (flag)
			{
				SignalChanged(propertyName);
			}
		}
		return flag;
	}

	/// <summary>
	/// Signals when a property value is changed.  This is merely a central function that is called every time the property is changed so as to 
	/// eliminate redundant code.
	/// </summary>
	/// <param name="propertyName">Name of the property being changed.</param>
	protected virtual void SignalChanged(string propertyName)
	{
		SignalChanged(this, propertyName);
	}

	/// <summary>
	/// Signals when a property value is changed.  This is merely a central function that is called every time the property is changed so as to 
	/// eliminate redundant code.
	/// </summary>
	/// <param name="sender">The sender, i.e., the instance of the object being changed.</param>
	/// <param name="propertyName">Name of the property being changed.</param>
	protected virtual void SignalChanged(object sender, string propertyName)
	{
		this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// The property signal changed handler.
	/// </summary>
	/// <param name="sender">
	/// The sender.
	/// </param>
	/// <param name="args">
	/// The args.
	/// </param>
	private void PropertySignalChangedHandler(object sender, PropertyChangedEventArgs args)
	{
		SignalChanged(sender, args.PropertyName);
	}
}
