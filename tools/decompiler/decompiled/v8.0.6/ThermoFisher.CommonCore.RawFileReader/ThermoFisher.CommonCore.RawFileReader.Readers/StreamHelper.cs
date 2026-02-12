using System;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// Common naming conventions for streams within raw files
/// </summary>
public static class StreamHelper
{
	/// <summary>
	/// Constructs the stream identifier.
	/// </summary>
	/// <param name="id">The identifier.</param>
	/// <param name="rawDataName">The raw data name.</param>
	/// <returns>Memory mapped file ID</returns>
	public static string ConstructStreamId(Guid id, string rawDataName)
	{
		return id.ToString("N") + "__" + rawDataName;
	}
}
