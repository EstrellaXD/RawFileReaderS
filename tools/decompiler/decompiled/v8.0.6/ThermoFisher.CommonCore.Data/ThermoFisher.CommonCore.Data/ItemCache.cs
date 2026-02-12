using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Make a cache of the "n" most recently used copies of an object.
/// For example: If different parts of an application are likely to read the same data from a file
/// multiple times, this can be used to reduce the file operations.
/// </summary>
/// <typeparam name="T">Type of object in the cache
/// </typeparam>
public class ItemCache<T> where T : class, IDeepCloneable<T>
{
	/// <summary>
	/// Obtain a record from a data source, based on an index
	/// Example: Scan from raw file, based on scan number.
	/// </summary>
	/// <param name="index">
	/// Index of object
	/// </param>
	/// <returns>
	/// Object read from data source
	/// </returns>
	public delegate T ItemReader(int index);

	/// <summary>
	/// The queue item.
	/// </summary>
	private class QueueItem
	{
		/// <summary>
		/// Gets or sets Index.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Gets or sets Value.
		/// </summary>
		public T Value { get; set; }
	}

	private readonly int _cacheLimit;

	private readonly Queue<QueueItem> _itemCache;

	private readonly ItemReader _reader;

	/// <summary>
	/// Gets the number of cached items
	/// </summary>
	public int Cached => _itemCache.Count;

	/// <summary>
	/// Gets the maximum number of cached items
	/// </summary>
	public int Capacity => _cacheLimit;

	/// <summary>
	/// Gets or sets a value indicating whether all items returned from the cache are Deep Cloned.
	/// If false: The cache only keeps references
	/// </summary>
	public bool CloneItems { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ItemCache`1" /> class. 
	/// Initialize a cache
	/// </summary>
	/// <param name="capacity">
	/// Maximum number of items in the cache. Oldest items are discarded when the cache is full
	/// </param>
	/// <param name="reader">
	/// Delegate to read a new item, when not in the cache.
	/// </param>
	public ItemCache(int capacity, ItemReader reader)
	{
		_cacheLimit = capacity;
		_reader = reader;
		_itemCache = new Queue<QueueItem>(capacity);
	}

	/// <summary>
	/// Get an item from the cache.
	/// If the item is not available in the cache, it is requested from the item reader (supplied to the constructor).
	/// When new items are read: This automatically caches new items, discarding oldest items.
	/// </summary>
	/// <param name="index">
	/// Record number
	/// </param>
	/// <returns>
	/// item from the cache. May be "null" if this item is not in the cache, and the reader has no value for this index
	/// </returns>
	public T GetItem(int index)
	{
		if (_itemCache.Count > 0)
		{
			foreach (QueueItem item in _itemCache)
			{
				if (item.Index == index)
				{
					T value = item.Value;
					return CloneItems ? value.DeepClone() : value;
				}
			}
		}
		T val = _reader(index);
		if (val != null)
		{
			if (_itemCache.Count == _cacheLimit)
			{
				_itemCache.Dequeue();
			}
			T value2 = (CloneItems ? val.DeepClone() : val);
			_itemCache.Enqueue(new QueueItem
			{
				Value = value2,
				Index = index
			});
		}
		return val;
	}

	/// <summary>
	/// empty the cache
	/// </summary>
	public void Clear()
	{
		_itemCache.Clear();
	}
}
