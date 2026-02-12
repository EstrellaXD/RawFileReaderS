using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// ItemCollection is a custom collection of generic (user defined) types.
/// It allows for triggering events when items are added or removed.  It also provides
/// a means to have read-only collection properties during XML serialization.
/// </summary>
/// <typeparam name="TGenericType">The type of the generic type.</typeparam>
[Serializable]
[CollectionDataContract]
public class ItemCollection<TGenericType> : Collection<TGenericType>, ICommonCoreDataObject, ICloneable
{
	private bool _collectionChanged;

	/// <summary>
	/// Gets a value indicating whether this instance has changed.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance has changed; otherwise, <c>false</c>.
	/// </value>
	[XmlIgnore]
	public bool HasCollectionChanged
	{
		get
		{
			return _collectionChanged;
		}
		private set
		{
			_collectionChanged = value;
		}
	}

	/// <summary>
	/// Occurs when [item added].
	/// </summary>
	public event EventHandler<EventArgs> ItemAdded;

	/// <summary>
	/// Occurs when [item removed]. The sender parameter is a pointer to the object being changed.
	/// </summary>
	public event EventHandler<EventArgs> ItemRemoved;

	/// <summary>
	/// Occurs when [items cleared]. The sender parameter is a pointer to the object being changed.
	/// </summary>
	public event EventHandler<EventArgs> ItemsCleared;

	/// <summary>
	/// Occurs when a property value changes.
	/// </summary>
	public event PropertyChangedEventHandler PropertyChanged;

	/// <summary>
	/// Clears the has changed status of the item collection
	/// </summary>
	public void ClearHasChangedStatus()
	{
		HasCollectionChanged = false;
	}

	/// <summary>
	/// Sort the collection
	/// </summary>
	/// <param name="comparer">comparer, to order the sort</param>
	public void Sort(IComparer<TGenericType> comparer)
	{
		TGenericType[] array = this.ToArray();
		Array.Sort(array, comparer);
		Clear();
		AddRange(array);
	}

	/// <summary>
	/// Sort the collection
	/// </summary>
	/// <param name="comparison">comparison method, to order the sort</param>
	public void Sort(Comparison<TGenericType> comparison)
	{
		TGenericType[] array = this.ToArray();
		Array.Sort(array, comparison);
		Clear();
		AddRange(array);
	}

	/// <summary>
	/// Add a range of items to the collection
	/// </summary>
	/// <param name="items">Items to add</param>
	public void AddRange(IEnumerable<TGenericType> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		foreach (TGenericType item in items)
		{
			Add(item);
		}
	}

	/// <summary>
	/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1" /> at the specified index and fires an <see cref="E:ThermoFisher.CommonCore.Data.ItemCollection`1.ItemAdded" /> event and a <see cref="E:ThermoFisher.CommonCore.Data.ItemCollection`1.PropertyChanged" /> event.
	/// </summary>
	/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
	/// <param name="item">The object to insert. The value can be null for reference types.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	protected override void InsertItem(int index, TGenericType item)
	{
		base.InsertItem(index, item);
		_collectionChanged = true;
		if (item is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged += PropertySignalChangedHandler;
		}
		this.ItemAdded?.Invoke(item, EventArgs.Empty);
		SignalChanged("ItemCollection");
	}

	/// <summary>
	/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1" /> and fires an <see cref="E:ThermoFisher.CommonCore.Data.ItemCollection`1.ItemRemoved" /> event and a property changed event.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
	protected override void RemoveItem(int index)
	{
		TGenericType val = base.Items[index];
		base.RemoveItem(index);
		_collectionChanged = true;
		if (val is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged -= PropertySignalChangedHandler;
		}
		this.ItemRemoved?.Invoke(val, EventArgs.Empty);
		SignalChanged("ItemCollection");
	}

	/// <summary>
	/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1" /> and fires a <see cref="E:ThermoFisher.CommonCore.Data.ItemCollection`1.ItemsCleared" /> event and a property changed event.
	/// </summary>
	protected override void ClearItems()
	{
		using (IEnumerator<TGenericType> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is INotifyPropertyChanged notifyPropertyChanged)
				{
					notifyPropertyChanged.PropertyChanged -= PropertySignalChangedHandler;
				}
			}
		}
		base.ClearItems();
		_collectionChanged = true;
		this.ItemsCleared?.Invoke(this, EventArgs.Empty);
		SignalChanged("ItemCollection");
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	public object Clone()
	{
		ItemCollection<TGenericType> itemCollection = new ItemCollection<TGenericType>();
		itemCollection.AddRange(this);
		return itemCollection;
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
	/// <returns>
	/// true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
	/// </returns>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="obj" /> parameter is null.</exception>
	public override bool Equals(object obj)
	{
		if (!(obj is ItemCollection<TGenericType> itemCollection))
		{
			return false;
		}
		if (base.Count != itemCollection.Count)
		{
			return false;
		}
		for (int i = 0; i < base.Count; i++)
		{
			TGenericType val = base[i];
			TGenericType val2 = itemCollection[i];
			if (!val.Equals(val2))
			{
				return false;
			}
		}
		return true;
	}

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
	/// Provides a custom deep equality operation when checking for equality.
	/// </summary>
	/// <param name="valueToCompare">The value to compare.</param>
	/// <returns>true if the objects are equal</returns>
	public bool DeepEquals(object valueToCompare)
	{
		return Equals(valueToCompare);
	}

	/// <summary>
	/// Performs the default settings for the data object.  This can be overridden in each data object that implements the interface to perform
	/// initialization settings. Method is empty (does nothing) in ItemCollection
	/// </summary>
	public virtual void PerformDefaultSettings()
	{
	}

	/// <summary>
	/// Signals the changed.
	/// </summary>
	/// <param name="propertyName">Name of the property that has changed</param>
	protected virtual void SignalChanged(string propertyName)
	{
		SignalChanged(this, propertyName);
	}

	/// <summary>
	/// Signals the changed.
	/// </summary>
	/// <param name="sender">The sender.</param>
	/// <param name="propertyName">Name of the property.</param>
	protected virtual void SignalChanged(object sender, string propertyName)
	{
		if (sender == this)
		{
			_collectionChanged = true;
		}
		this.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// Internal handler for property changed
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
