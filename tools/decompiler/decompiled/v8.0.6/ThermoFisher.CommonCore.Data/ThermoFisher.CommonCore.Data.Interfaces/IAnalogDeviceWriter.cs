using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to write Analog devices data.<para />
/// Note: The following functions should be called before acquisition begins:<para />
/// 1. Write Instrument Info<para />
/// 2. Write Instrument Expected Run Time<para />
/// 3. Write Status Log Header <para />
/// If caller is not intended to use the status log data, pass a null argument or zero length array.<para />
/// ex. WriteStatusLogHeader(null) or WriteStatusLogHeader(new IHeaderItem[0])
/// </summary>    
public interface IAnalogDeviceWriter : IBaseDeviceWriter, IDisposable, IFileError
{
	/// <summary>
	/// Writes the Analog instrument data and index into the disk. This is the
	/// simplest format of data we write to a raw file.
	/// </summary>
	/// <param name="instData">The Analog instrument data.</param>
	/// <param name="instDataIndex">Index of the Analog instrument scan (scan header).</param>
	/// <returns>True if scan data and index are written to disk successfully, False otherwise</returns>
	bool WriteInstData(double[] instData, IAnalogScanIndex instDataIndex);
}
