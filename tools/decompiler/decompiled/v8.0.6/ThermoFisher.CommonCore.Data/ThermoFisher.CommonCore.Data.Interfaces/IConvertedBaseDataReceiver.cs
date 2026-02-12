using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a base interface which is used to recieve non scanning (UV, Analog) data, which is packed in a binary form.
/// This could be used, for example, to transmit this data though a messaging system, with no knowledge of specific MS data types.
/// An implementation (linked to a decoder) could save this data to a file or database.
/// External code would need to pack and unpack this binary data.
/// A derived interface is needed for each specicic detector detector type
/// </summary>
public interface IConvertedBaseDataReceiver : IDisposable
{
	/// <summary>
	/// Receive byte encoded data from WriteStatusLog
	/// </summary>
	/// <param name="retentionTime">Time (minutes) since injection.</param>
	/// <param name="data">byte encoded data</param>
	/// <returns>True on success</returns>
	bool ReceiveStatusLog(float retentionTime, byte[] data);

	/// <summary>
	/// Receive byte encoded data from WriteErrorLog
	/// </summary>
	/// <param name="retentionTime">Time (minutes) since injection.</param>
	/// <param name="data">byte encoded data</param>
	/// <returns>true on success</returns>
	bool ReceiveErrorLog(float retentionTime, byte[] data);

	/// <summary>
	/// Receives a definition of the instrument
	/// </summary>
	/// <param name="packedInstrumentData"></param>
	/// <returns></returns>
	bool ReceiveInstrumentData(byte[] packedInstrumentData);

	/// <summary>
	/// Receives the expected run time
	/// </summary>
	/// <param name="runTime">the run time</param>
	/// <returns>true on success</returns>
	bool ReceiveExpectedRunTime(double runTime);

	/// <summary>
	/// receives the run comments
	/// </summary>
	/// <param name="comments">2 comments, each encoded as length (32 bit) follwed by 16 bit chars</param>
	/// <returns>true on success</returns>
	bool ReceiveComments(byte[] comments);

	/// <summary>
	/// Receives a status log header
	/// </summary>
	/// <param name="statusLogHeader">The status log header</param>
	/// <returns>true on success</returns>
	bool ReceiveStatusLogHeader(byte[] statusLogHeader);
}
