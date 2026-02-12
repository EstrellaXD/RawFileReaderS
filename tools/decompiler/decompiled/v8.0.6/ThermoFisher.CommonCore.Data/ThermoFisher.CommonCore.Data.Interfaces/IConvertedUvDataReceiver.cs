using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines an interface which is used to recieve non scanning (UV) data, which is packed in a binary form.
/// This could be used, for example, to transmit this data though a messaging system, with no knowledge of specific MS data types.
/// An implementation (linked to a decoder) could save this data to a file or database.
/// External code would need to pack and unpack this binary data.
/// </summary>
public interface IConvertedUvDataReceiver : IConvertedBaseDataReceiver, IDisposable
{
	/// <summary>
	/// receives a UV set of data channels at a time point (header followed by data)
	/// </summary>
	/// <param name="data">UV header followed by data channels</param>
	/// <returns>true on success</returns>
	bool ReceiveUvScan(byte[] data);
}
