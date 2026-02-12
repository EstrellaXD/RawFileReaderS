namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Caching of scans, which may be provided by an averager
/// </summary>
public interface IScanCache
{
	/// <summary>
	/// Gets or sets the maximum number of scans kept in a cache.
	/// Setting ScansCached &gt;0 will enable caching of recently read scans.
	/// This is useful if averaging multiple overlapping ranges of scans.
	/// </summary>
	int ScansCached { get; set; }
}
