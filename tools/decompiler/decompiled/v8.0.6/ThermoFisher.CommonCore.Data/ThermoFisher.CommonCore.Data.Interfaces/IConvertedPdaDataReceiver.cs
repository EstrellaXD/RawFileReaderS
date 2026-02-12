using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines an interface which is used to recieve PDA data, which is packed in a binary form.
/// This could be used, for example, to transmit this data though a messaging system, with no knowledge of specific MS data types.
/// An implementation (linked to a decoder) could save this data to a file or database.
/// External code would need to pack and unpack this binary data.
/// </summary>
public interface IConvertedPdaDataReceiver : IConvertedBaseDataReceiver, IDisposable
{
	/// <summary>
	/// receives a PDA scan at a time point (header followed by data)
	/// </summary>
	/// <param name="data">PDA header followed by data</param>
	/// <returns>true on success</returns>
	bool ReceivePdaScan(byte[] data);
}
