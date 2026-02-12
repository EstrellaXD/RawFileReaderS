using System;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// Creates a tool to load raw data, based on various data reading methods
/// </summary>
internal static class RawFileLoaderFactory
{
	/// <summary>
	/// Create a tool to load raw data.
	/// </summary>
	/// <param name="dataName">Name of data</param>
	/// <param name="preferRandomAccess">true if random access is preferred (some files may get memory mapped).</param>
	/// <param name="manager">tool for random acess data reading</param>
	/// <returns></returns>
	public static IRawFileLoader CreateLoader(string dataName, bool preferRandomAccess = false, IViewCollectionManager manager = null)
	{
		if (preferRandomAccess)
		{
			try
			{
				if (manager == null)
				{
					throw new ArgumentNullException("manager");
				}
				return new RandomAccessRawFileLoader(dataName, manager);
			}
			catch (NotSupportedException)
			{
				return new MemoryMappingRawFileLoader(dataName);
			}
		}
		return new MemoryMappingRawFileLoader(dataName);
	}
}
