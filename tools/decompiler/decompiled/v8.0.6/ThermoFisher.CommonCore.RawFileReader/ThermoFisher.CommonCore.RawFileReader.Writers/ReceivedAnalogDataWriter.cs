using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This class takes Analog Data, which is intended to be transmitted over a protocol or cached and 
/// writes it to a raw file.
/// </summary>
public class ReceivedAnalogDataWriter : IConvertedAnalogDataReceiver, IConvertedBaseDataReceiver, IDisposable
{
	private readonly IAnalogDeviceBinaryWriter _writer;

	/// <summary>
	/// construct an object to transfer Analog data to a raw file, starting with packed (byte array) data.
	/// </summary>
	/// <param name="fileName">The raw file which will receive the data</param>
	/// <param name="device">Type of devive to create (can be Analog or Other)</param>
	public ReceivedAnalogDataWriter(string fileName, Device device)
	{
		_writer = DeviceWriterAdapter.CreateAnalogDeviceBinaryWriter(fileName, device);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_writer.Dispose();
	}

	/// <inheritdoc />
	public bool ReceiveAnalogScan(byte[] data)
	{
		AnalogScanIndex analogScanIndex = new AnalogScanIndex();
		analogScanIndex.StartTime = BitConverter.ToDouble(data);
		analogScanIndex.TIC = BitConverter.ToDouble(data, 8);
		analogScanIndex.NumberOfChannels = BitConverter.ToInt32(data, 16);
		int num = data.Length - 20;
		double[] array = new double[num / 8];
		Buffer.BlockCopy(data, 20, array, 0, num);
		return _writer.WriteInstData(array, analogScanIndex);
	}

	/// <inheritdoc />
	public bool ReceiveComments(byte[] comments)
	{
		int index = 0;
		string comment = MsDataPacker.UnpackString(comments, ref index);
		string comment2 = MsDataPacker.UnpackString(comments, ref index);
		return _writer.WriteInstComments(comment, comment2);
	}

	/// <inheritdoc />
	public bool ReceiveErrorLog(float retentionTime, byte[] data)
	{
		int index = 0;
		return _writer.WriteErrorLog(retentionTime, MsDataPacker.UnpackString(data, ref index));
	}

	/// <inheritdoc />
	public bool ReceiveExpectedRunTime(double runTime)
	{
		return _writer.WriteInstExpectedRunTime(runTime);
	}

	/// <inheritdoc />
	public bool ReceiveInstrumentData(byte[] packedInstrumentData)
	{
		return _writer.WriteInstrumentInfo(packedInstrumentData.UnpackInstrumentData());
	}

	/// <inheritdoc />
	public bool ReceiveStatusLog(float retentionTime, byte[] data)
	{
		return _writer.WriteStatusLog(retentionTime, data);
	}

	/// <inheritdoc />
	public bool ReceiveStatusLogHeader(byte[] statusLogHeaderBytes)
	{
		return _writer.WriteStatusLogHeader(MsDataPacker.UnpackHeaderItem(statusLogHeaderBytes));
	}
}
