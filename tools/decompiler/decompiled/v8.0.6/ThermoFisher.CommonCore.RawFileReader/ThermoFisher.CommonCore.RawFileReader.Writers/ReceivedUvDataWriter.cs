using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This class takes UV Data, which is intended to be transmitted over a protocol or cached and 
/// writes it to a raw file.
/// </summary>
public class ReceivedUvDataWriter : IConvertedUvDataReceiver, IConvertedBaseDataReceiver, IDisposable
{
	private readonly IUvDeviceBinaryWriter _writer;

	/// <summary>
	/// construct an object to transfer UV data to a raw file, starting with packed (byte array) data.
	/// </summary>
	/// <param name="fileName">The raw file which will receive the data</param>
	/// <param name="device">Type of devive to create (can be UV or Other)</param>
	/// <param name="domain">Determines the format of this channel (such as Xcalibur or chromeleon)</param>
	public ReceivedUvDataWriter(string fileName, Device device, RawDataDomain domain)
	{
		_writer = DeviceWriterAdapter.CreateUvDeviceBinaryWriter(fileName, device, domain);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_writer.Dispose();
	}

	/// <inheritdoc />
	public bool ReceiveUvScan(byte[] data)
	{
		UvScanIndex uvScanIndex = new UvScanIndex();
		uvScanIndex.StartTime = BitConverter.ToDouble(data);
		uvScanIndex.TIC = BitConverter.ToDouble(data, 8);
		uvScanIndex.NumberOfChannels = BitConverter.ToInt32(data, 16);
		uvScanIndex.Frequency = BitConverter.ToDouble(data, 20);
		uvScanIndex.IsUniformTime = data[28] != 0;
		int num = data.Length - 29;
		double[] array = new double[num / 8];
		Buffer.BlockCopy(data, 29, array, 0, num);
		return _writer.WriteInstData(array, uvScanIndex);
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
