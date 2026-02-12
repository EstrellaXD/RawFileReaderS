using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Defines a class which can convert UV data from an instrument to a binary, and forward to a receiver.
/// </summary>
public class BinaryConversionUvWriter : BinaryConversionNonScanningWriter, IUvDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError
{
	private IConvertedUvDataReceiver UvDataReceiver { get; }

	/// <summary>
	/// Construct a UV writer with a specific data receiver
	/// </summary>
	/// <param name="receiver">Object to receive the binary converted data</param>
	public BinaryConversionUvWriter(IConvertedUvDataReceiver receiver)
	{
		base.Receiver = receiver;
		UvDataReceiver = receiver;
	}

	/// <inheritdoc />
	public bool WriteInstData(double[] instData, IUvScanIndex instDataIndex)
	{
		byte[] bytes = BitConverter.GetBytes(instDataIndex.StartTime);
		byte[] bytes2 = BitConverter.GetBytes(instDataIndex.TIC);
		byte[] bytes3 = BitConverter.GetBytes(instDataIndex.NumberOfChannels);
		byte[] bytes4 = BitConverter.GetBytes(instDataIndex.Frequency);
		int num = instData.Length * 8;
		byte[] array = new byte[29 + num];
		Array.Copy(bytes, array, bytes.Length);
		Array.Copy(bytes2, 0, array, 8, bytes2.Length);
		Array.Copy(bytes3, 0, array, 16, bytes3.Length);
		Array.Copy(bytes4, 0, array, 20, bytes4.Length);
		array[28] = (instDataIndex.IsUniformTime ? ((byte)1) : ((byte)0));
		Buffer.BlockCopy(instData, 0, array, 29, num);
		return UvDataReceiver.ReceiveUvScan(array);
	}
}
