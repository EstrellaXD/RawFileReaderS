using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines an interface which is used to recieve MS data, which is packed in a binary form.
/// This could be used, for example, to transmit this data though a messaging system, with no knowledge of specific MS data types.
/// An implementation (linked to a decoder) could save this data to a file or database.
/// External code would need to pack and unpack this binary data.
/// </summary>
public interface IConvertedMsDataReceiver : IDisposable
{
	/// <summary>
	/// Receives the "prerpare for run" data. This preparation data must be sent before any other data.
	/// Implementations may indicate an errror (such as throw an exception) if any method to log data is called
	/// without this being called first.
	/// </summary>
	/// <param name="packedInstrumentData">Data about the instrument</param>
	/// <param name="packedHeaders">Headers for all logs</param>
	/// <param name="packedRunHeaderInfo">Header for the data stream</param>
	/// <param name="packedMsScanEvents">List of kmown scan types</param>
	/// <returns>True on success</returns>
	bool ReceivePrepareForRun(byte[] packedInstrumentData, byte[] packedHeaders, byte[] packedRunHeaderInfo, byte[] packedMsScanEvents);

	/// <summary>
	/// Receives byte encoded data as sent by an instrument using "WriteInstData" overload with IMsInstrumentData 
	/// </summary>
	/// <param name="packedMsInstrumentData">Instrument data in binary format</param>
	/// <returns>True on success</returns>
	bool ReceiveInstData(byte[] packedMsInstrumentData);

	/// <summary>
	/// Receives byte encoded data as sent by an instrument using "WriteInstData" overlaod with byte[] data
	/// </summary>
	/// <param name="dataBlock">A mass spectromter scan</param>
	/// <param name="packetType">The data type, which determines which decoder is needed for this scan.</param>
	/// <returns>True on success</returns>
	bool ReceiveInstData(byte[] dataBlock, int packetType);

	/// <summary>
	/// Receive byte encoded data from WriteStatusLog
	/// </summary>
	/// <param name="retentionTime">Time (minutes) since injection.</param>
	/// <param name="data">byte encoded data</param>
	/// <returns>True on success</returns>
	bool ReceiveStatusLog(float retentionTime, byte[] data);

	/// <summary>
	/// Receive byte encoded data from WriteTrailerExtraData
	/// </summary>
	/// <param name="data">byte encoded data</param>
	/// <returns>True on success</returns>
	bool ReceiveTrailerExtra(byte[] data);

	/// <summary>
	/// Receive byte encoded data from WriteTuneData
	/// </summary>
	/// <param name="buffer">byte encoded data</param>
	/// <returns>True on success</returns>
	bool ReceiveTuneData(byte[] buffer);

	/// <summary>
	/// Receive byte encoded data from WriteErrorLog
	/// </summary>
	/// <param name="retentionTime">Time (minutes) since injection.</param>
	/// <param name="data">byte encoded data</param>
	/// <returns>true on success</returns>
	bool ReceiveErrorLog(float retentionTime, byte[] data);

	/// <summary>
	/// Receive the scan index (for a MS instrument)
	/// </summary>
	/// <param name="packedIndex">binary packed scan index</param>
	/// <param name="packedEvent">binary packed scan event for this scan</param>
	/// <returns>true on success</returns>
	bool ReceiveInstScanIndex(byte[] packedIndex, byte[] packedEvent);
}
