namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Caching feature which can be supported by objects implementing IRawData
/// </summary>
public interface IRawCache
{
	/// <summary>
	/// Request the object to keep a cache of the listed item.
	/// Setting the caching to "zero" disables further caching.
	/// </summary>
	/// <param name="item">
	/// Item to cache
	/// </param>
	/// <param name="limit">
	/// Limit of number of items to cache
	/// </param>
	/// <param name="useCloning">
	/// (optional, default false) if set True, all values returned from the cache are unique  (cloned) references. 
	/// By default, the cache just keeps references to the objects 
	/// </param>
	void SetCaching(RawCacheItem item, int limit, bool useCloning = false);

	/// <summary>
	/// Clear items in the cache
	/// </summary>
	/// <param name="item">
	/// item type to clear
	/// </param>
	void ClearCache(RawCacheItem item);

	/// <summary>
	/// Count the number currently in the cache
	/// </summary>
	/// <param name="item">
	/// Item type to count
	/// </param>
	/// <returns>
	/// The number of items in this cache
	/// </returns>
	int Cached(RawCacheItem item);
}
