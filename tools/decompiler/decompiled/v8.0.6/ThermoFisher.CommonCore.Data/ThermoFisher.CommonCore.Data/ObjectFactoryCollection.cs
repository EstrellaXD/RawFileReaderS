using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Represents the strongly typed collection of <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" /> that can be
/// Individually accessed by index.
/// This collection can then use the contained items to open files, which can read the
/// required data, and return to the caller {T}. <see cref="M:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1.FileFactory(System.String)" />
/// </summary>
/// <typeparam name="T">Interface to data read from a file by the factory
/// </typeparam>
public class ObjectFactoryCollection<T> : IList<ObjectFactory<T>>, ICollection<ObjectFactory<T>>, IEnumerable<ObjectFactory<T>>, IEnumerable where T : class
{
	private readonly List<ObjectFactory<T>> _factoryCollection = new List<ObjectFactory<T>>();

	/// <summary>
	/// Gets or sets the default file extension.
	/// When a file name is sent to the <see cref="M:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1.FileFactory(System.String)" /> method with no extension,
	/// or an empty extension is sent to to <see cref="M:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1.GetObjectCreatorFromExtension(System.String)" />,
	/// this extension is assumed.
	/// </summary>
	public string DefaultExtension { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to aggressively attempt to open the file,
	/// using any known format. It is only applicable if the supplied file
	/// has no extension supplied. It is false by default.
	/// For files with no extension, the adapter for the default extension
	/// is tried first, and then the file is attempted to be
	/// opened with all other adapters. 
	/// If no adapters can open the file, the exception from
	/// the default adapter is re-thrown.
	/// </summary>
	public bool TryAllAdapters { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether a "dynamic default extension" is used.
	/// This property is only used when <see cref="P:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1.TryAllAdapters" /> is set, 
	/// and when files names sent to <see cref="M:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1.FileFactory(System.String)" /> have no extension.
	/// If DynamicDefault is true, When successfully opening a file,
	/// the <see cref="P:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1.DefaultExtension" /> is adjusted to the extension of the
	/// adapter found. This is intended to increase efficiency when processing a
	/// sequence of files, as the adapter search would be faster after the first file.
	/// By default this is false, and the default is not adjusted.
	/// </summary>
	public bool DynamicDefault { get; set; }

	/// <summary>
	/// indexer property for the list
	/// </summary>
	/// <param name="index">index location</param>
	/// <returns>item at given location/index</returns>
	public ObjectFactory<T> this[int index]
	{
		get
		{
			return _factoryCollection[index];
		}
		set
		{
			_factoryCollection[index] = value;
		}
	}

	/// <summary>
	/// Gets the total number of items in the list
	/// </summary>
	public int Count => _factoryCollection.Count;

	/// <summary>
	/// Gets a value indicating whether the list is readonly
	/// </summary>
	public bool IsReadOnly => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1" /> class. 
	/// </summary>
	public ObjectFactoryCollection()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1" /> class. 
	/// Initializes/loads the assembly and types by iterating through the collection.
	/// </summary>
	/// <param name="objectFactories">
	/// collection of <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" />
	/// </param>
	public ObjectFactoryCollection(IEnumerable<ObjectFactory<T>> objectFactories)
	{
		Initialize(objectFactories);
	}

	/// <summary>
	/// Initialize the collection.
	/// </summary>
	/// <param name="objectFactories">
	/// The collection of object factories.
	/// </param>
	protected void Initialize(IEnumerable<ObjectFactory<T>> objectFactories)
	{
		foreach (ObjectFactory<T> objectFactory in objectFactories)
		{
			if (!Contains(objectFactory) && objectFactory.Initialize(throwExceptions: false))
			{
				_factoryCollection.Add(objectFactory);
			}
		}
	}

	/// <summary>
	/// To Find the index of a given item
	/// </summary>
	/// <param name="item">
	/// item to find
	/// </param>
	/// <returns>
	/// index of the item
	/// </returns>
	public int IndexOf(ObjectFactory<T> item)
	{
		return _factoryCollection.IndexOf(item);
	}

	/// <summary>
	/// Insert an item at a given index
	/// </summary>
	/// <param name="index">
	/// index at which to insert
	/// </param>
	/// <param name="item">
	/// item to insert
	/// </param>
	public void Insert(int index, ObjectFactory<T> item)
	{
		_factoryCollection.Insert(index, item);
	}

	/// <summary>
	/// remove an item from a given location
	/// </summary>
	/// <param name="index">
	/// index from where to remove
	/// </param>
	public void RemoveAt(int index)
	{
		_factoryCollection.RemoveAt(index);
	}

	/// <summary>
	/// adds an item to the list
	/// </summary>
	/// <param name="item">
	/// item to add
	/// </param>
	public void Add(ObjectFactory<T> item)
	{
		_factoryCollection.Add(item);
	}

	/// <summary>
	/// adds a range of items
	/// </summary>
	/// <param name="itemArray">
	/// items as an array
	/// </param>
	public void AddRange(ObjectFactory<T>[] itemArray)
	{
		_factoryCollection.AddRange(itemArray);
	}

	/// <summary>
	/// clears the underlying list
	/// </summary>
	public void Clear()
	{
		_factoryCollection.Clear();
	}

	/// <summary>
	/// checks for the existence of an item in the list
	/// </summary>
	/// <param name="item">
	/// item to check
	/// </param>
	/// <returns>
	/// true if found, else false
	/// </returns>
	public bool Contains(ObjectFactory<T> item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		return _factoryCollection.Any((ObjectFactory<T> rdc) => string.Equals(rdc.FileExtension, item.FileExtension, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// copies a part of the list to the given array
	/// </summary>
	/// <param name="array">
	/// array into which items will be copied
	/// </param>
	/// <param name="arrayIndex">
	/// index from where the copy would begin
	/// </param>
	public void CopyTo(ObjectFactory<T>[] array, int arrayIndex)
	{
		_factoryCollection.CopyTo(array, arrayIndex);
	}

	/// <summary>
	/// removes an item from the list
	/// </summary>
	/// <param name="item">
	/// item to remove
	/// </param>
	/// <returns>
	/// true if removed, else false
	/// </returns>
	public bool Remove(ObjectFactory<T> item)
	{
		if (_factoryCollection.Contains(item))
		{
			_factoryCollection.Remove(item);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Get a strongly types enumerator
	/// </summary>
	/// <returns>
	/// A strongly types enumerator
	/// </returns>
	public IEnumerator<ObjectFactory<T>> GetEnumerator()
	{
		return _factoryCollection.GetEnumerator();
	}

	/// <summary>
	/// Get a default enumerator of the list
	/// </summary>
	/// <returns>
	/// a default enumerator of the list
	/// </returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _factoryCollection.GetEnumerator();
	}

	/// <summary>
	/// Gets the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" /> for the given extension.
	/// This finds an object for opening files with the given extension (such as <c>".pmd"</c>)
	/// </summary>
	/// <param name="extension">
	/// valid extension
	/// </param>
	/// <returns>
	/// return a <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" /> which can open files of the given extension,
	///  or null if there is no available creator for the extension
	/// </returns>
	public ObjectFactory<T> GetObjectCreatorFromExtension(string extension)
	{
		if (string.IsNullOrEmpty(extension))
		{
			extension = DefaultExtension;
		}
		return _factoryCollection.FirstOrDefault((ObjectFactory<T> rdc) => string.Equals(rdc.FileExtension, extension, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	/// Implementation of delegate which can open any
	/// kind of file, based on the extension. 
	/// </summary>
	/// <param name="fileName">
	/// The name of the file
	/// </param>
	/// <returns>
	/// An interface to the object data
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// <c>Exception</c>.
	/// </exception>
	public T FileFactory(string fileName)
	{
		string extension = Path.GetExtension(fileName);
		if (string.IsNullOrEmpty(extension) && TryAllAdapters)
		{
			ObjectFactory<T> objectCreatorFromExtension = GetObjectCreatorFromExtension(DefaultExtension);
			Exception ex = null;
			if (objectCreatorFromExtension != null)
			{
				try
				{
					T val = objectCreatorFromExtension.OpenFile(fileName);
					if (val != null)
					{
						return val;
					}
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
			}
			T val2 = TryAllOtherAdapters(fileName, objectCreatorFromExtension);
			if (val2 != null)
			{
				return val2;
			}
			if (ex != null)
			{
				throw ex;
			}
			return null;
		}
		ObjectFactory<T> objectCreatorFromExtension2 = GetObjectCreatorFromExtension(extension);
		if (objectCreatorFromExtension2 == null)
		{
			return null;
		}
		return objectCreatorFromExtension2.OpenFile(fileName);
	}

	/// <summary>
	/// The try all other adapters.
	/// </summary>
	/// <param name="fileName">
	/// The file name.
	/// </param>
	/// <param name="defaultCreator">
	/// The default creator.
	/// </param>
	/// <returns>
	/// An interface to the object data
	/// </returns>
	private T TryAllOtherAdapters(string fileName, ObjectFactory<T> defaultCreator)
	{
		foreach (ObjectFactory<T> item in _factoryCollection)
		{
			if (item == defaultCreator)
			{
				continue;
			}
			try
			{
				T val = item.OpenFile(fileName);
				if (val != null)
				{
					if (DynamicDefault)
					{
						DefaultExtension = item.FileExtension;
					}
					return val;
				}
			}
			catch
			{
			}
		}
		return null;
	}
}
