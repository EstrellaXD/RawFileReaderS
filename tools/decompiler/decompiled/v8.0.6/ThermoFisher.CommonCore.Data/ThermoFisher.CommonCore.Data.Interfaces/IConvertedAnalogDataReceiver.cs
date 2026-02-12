using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines an interface which is used to receive non scanning (Analog) data, which is packed in a binary form.
/// Devices of type Analog, MsAnalog and Other are supported.
/// This could be used, for example, to transmit this data though a messaging system, with no knowledge of specific MS data types.
/// An implementation (linked to a decoder) could save this data to a file or database.
/// External code would need to pack and unpack this binary data.
/// </summary>
public interface IConvertedAnalogDataReceiver : IConvertedBaseDataReceiver, IDisposable
{
	/// <summary>
	/// receives an analog set of data channels at a time point (header followed by data)
	/// </summary>
	/// <param name="data">Analog header followed by data channels</param>
	/// <returns>true on success</returns>
	bool ReceiveAnalogScan(byte[] data);
}
