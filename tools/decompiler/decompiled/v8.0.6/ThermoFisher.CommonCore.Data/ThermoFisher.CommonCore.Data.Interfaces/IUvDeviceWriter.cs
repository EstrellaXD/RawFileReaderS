using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to write UV devices data.<para />
/// Note: The following functions should be called before acquisition begins:<para />
/// 1. Write Instrument Info<para />
/// 2. Write Instrument Expected Run Time<para />
/// 3. Write Status Log Header <para />
/// If caller is not intended to use the status log data, pass a null argument or zero length array.<para />
/// ex. WriteStatusLogHeader(null) or WriteStatusLogHeader(new IHeaderItem[0])
/// </summary>
public interface IUvDeviceWriter : IBaseDeviceWriter, IDisposable, IFileError
{
	/// <summary>
	/// Writes both the UV type of the instrument data and the index into the disk. 
	/// This is the simplest format of data we write to a raw file since it doesn't have any
	/// profile indexing and there is no segmentation of the data.      
	/// </summary>
	/// <param name="instData">The UV type of instrument scan data.</param>
	/// <param name="instDataIndex">The UV type of the scan index.</param>
	/// <returns>True if scan data and index are written to disk successfully, False otherwise</returns>
	bool WriteInstData(double[] instData, IUvScanIndex instDataIndex);
}
