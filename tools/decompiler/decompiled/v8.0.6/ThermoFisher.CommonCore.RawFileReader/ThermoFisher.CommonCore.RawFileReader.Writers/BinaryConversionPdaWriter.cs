using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Defines a class which can convert PDA data from an instrument to a binary, and forward to a receiver.
/// </summary>
public class BinaryConversionPdaWriter : BinaryConversionNonScanningWriter, IPdaDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError
{
	private IConvertedPdaDataReceiver PdaDataReceiver { get; }

	/// <summary>
	/// Constrcut a PDA writer with a specific data reciever
	/// </summary>
	/// <param name="receiver">Object to receive the binary convered data</param>
	public BinaryConversionPdaWriter(IConvertedPdaDataReceiver receiver)
	{
		base.Receiver = receiver;
		PdaDataReceiver = receiver;
	}

	/// <inheritdoc />
	public bool WriteInstData(double[] instData, IPdaScanIndex instDataIndex)
	{
		byte[] bytes = BitConverter.GetBytes(instDataIndex.StartTime);
		byte[] bytes2 = BitConverter.GetBytes(instDataIndex.TIC);
		byte[] bytes3 = BitConverter.GetBytes(instDataIndex.LongWavelength);
		byte[] bytes4 = BitConverter.GetBytes(instDataIndex.ShortWavelength);
		byte[] bytes5 = BitConverter.GetBytes(instDataIndex.WavelengthStep);
		byte[] bytes6 = BitConverter.GetBytes(instDataIndex.AUScale);
		int num = instData.Length * 8;
		byte[] array = new byte[48 + num];
		Array.Copy(bytes, array, bytes.Length);
		Array.Copy(bytes2, 0, array, 8, bytes2.Length);
		Array.Copy(bytes3, 0, array, 16, bytes3.Length);
		Array.Copy(bytes4, 0, array, 24, bytes4.Length);
		Array.Copy(bytes5, 0, array, 32, bytes5.Length);
		Array.Copy(bytes6, 0, array, 40, bytes6.Length);
		Buffer.BlockCopy(instData, 0, array, 48, num);
		return PdaDataReceiver.ReceivePdaScan(array);
	}
}
