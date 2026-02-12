using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Defines a class which can convert Analog data from an instrument to a binary, and forward to a receiver.
/// </summary>
public class BinaryConversionAnalogWriter : BinaryConversionNonScanningWriter, IAnalogDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError
{
	private IConvertedAnalogDataReceiver AnalogDataReceiver { get; }

	/// <summary>
	/// Construct an Analog writer with a specific data receiver
	/// </summary>
	/// <param name="receiver">Object to receive the binary converted data</param>
	public BinaryConversionAnalogWriter(IConvertedAnalogDataReceiver receiver)
	{
		base.Receiver = receiver;
		AnalogDataReceiver = receiver;
	}

	/// <inheritdoc />
	public bool WriteInstData(double[] instData, IAnalogScanIndex instDataIndex)
	{
		byte[] bytes = BitConverter.GetBytes(instDataIndex.StartTime);
		byte[] bytes2 = BitConverter.GetBytes(instDataIndex.TIC);
		byte[] bytes3 = BitConverter.GetBytes(instDataIndex.NumberOfChannels);
		int num = instData.Length * 8;
		byte[] array = new byte[20 + num];
		Array.Copy(bytes, array, bytes.Length);
		Array.Copy(bytes2, 0, array, 8, bytes2.Length);
		Array.Copy(bytes3, 0, array, 16, bytes3.Length);
		Buffer.BlockCopy(instData, 0, array, 20, num);
		return AnalogDataReceiver.ReceiveAnalogScan(array);
	}
}
