using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Write mass spec data using binary encoded values which can be placed in the arw file with minimal processing
/// </summary>
public interface IMassSpecDeviceBinaryWriter : IMassSpecDeviceWriter, IDisposable, IFileError, IBinaryBaseDataWriter
{
	/// <summary>
	/// This method should be called (when creating an acquisition file) during the "Prepare for run" state.<para />
	/// It may not be called multiple times for one device. It may not be called after any of the data logging calls have been made.<para />
	/// It will perform the following operations:<para />
	/// 1. Write instrument information<para />
	/// 2. Write run header information<para />
	/// 3. Write status log header <para />
	/// 4. Write trailer extra header <para />
	/// 5. Write tune data header <para />     
	/// 6. Write run header information - expected run time, comments, mass resolution and precision.<para />
	/// 7. Write method scan events.
	/// </summary>
	/// <param name="packedInstrumentData">The instrument ID.</param>
	/// <param name="packedHeaders">The generic data headers, packed into byte arrays.</param>
	/// <param name="packedRunHeaderInfo">The run header information, packed into a byte array.</param>
	/// <param name="packedMsScanEvents">Method scan events, packed into a byte array.</param>
	/// <returns>True if all the values are written to disk successfully, false otherwise.</returns>
	bool PrepareForRun(byte[] packedInstrumentData, IPackedMassSpecHeaders packedHeaders, byte[] packedRunHeaderInfo, byte[] packedMsScanEvents);

	/// <summary>
	/// Writes Data for 1 scan. 
	/// </summary>
	/// <param name="instrumentData">Data ready to be written to a raw file, with most values in byte array format.
	/// in order to write the binary data</param>
	/// <returns>true on success</returns>
	bool WriteInstData(IBinaryMsInstrumentData instrumentData);

	/// <summary>
	/// Writes the scan index and event
	/// </summary>
	/// <param name="unpacked"></param>
	/// <param name="packedEvent"></param>
	/// <returns></returns>
	bool WriteInstScanIndex(IScanStatisticsAccess unpacked, byte[] packedEvent);
}
