using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This class takes MS data, which is intended to be transmitted over a protocol or cached and 
/// writes it to a raw file.
/// </summary>
public class ReceivedMsDataWriter : IConvertedMsDataReceiver, IDisposable
{
	private readonly IMassSpecDeviceBinaryWriter _writer;

	/// <summary>
	/// construct an object to transfer MS data to a raw file, starting with packed (byte array) data.
	/// </summary>
	/// <param name="fileName">The raw file which will receive the data</param>
	public ReceivedMsDataWriter(string fileName)
	{
		_writer = DeviceWriterAdapter.CreateMassSpecDeviceBinaryWriter(fileName);
	}

	/// <inheritdoc />
	public bool ReceiveErrorLog(float retentionTime, byte[] packedLogMessage)
	{
		int index = 0;
		return _writer.WriteErrorLog(retentionTime, MsDataPacker.UnpackString(packedLogMessage, ref index));
	}

	/// <inheritdoc />
	public bool ReceiveInstScanIndex(byte[] packedIndex, byte[] packedEvent)
	{
		IScanStatisticsAccess unpacked = MsDataPacker.UnpackScanStatistics(packedIndex);
		return _writer.WriteInstScanIndex(unpacked, packedEvent);
	}

	/// <inheritdoc />
	public bool ReceiveInstData(byte[] data)
	{
		IBinaryMsInstrumentData instrumentData = new MsDataPacker.PackedMsInstrumentData(data).Unpack();
		return _writer.WriteInstData(instrumentData);
	}

	/// <inheritdoc />
	public bool ReceiveInstData(byte[] dataBlock, int packetType)
	{
		return _writer.WriteInstData(dataBlock, (SpectrumPacketType)packetType);
	}

	/// <inheritdoc />
	public bool ReceivePrepareForRun(byte[] packedInstrumentData, byte[] packedHeaders, byte[] packedRunHeaderInfo, byte[] packedMsScanEvents)
	{
		IPackedMassSpecHeaders packedHeaders2 = MsDataPacker.HeadersFromBytes(packedHeaders);
		return _writer.PrepareForRun(packedInstrumentData, packedHeaders2, packedRunHeaderInfo, packedMsScanEvents);
	}

	/// <inheritdoc />
	public bool ReceiveStatusLog(float retentionTime, byte[] data)
	{
		return _writer.WriteStatusLog(retentionTime, data);
	}

	/// <inheritdoc />
	public bool ReceiveTrailerExtra(byte[] data)
	{
		return _writer.WriteTrailerExtraData(data);
	}

	/// <inheritdoc />
	public bool ReceiveTuneData(byte[] buffer)
	{
		return _writer.WriteTuneData(buffer);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_writer.Dispose();
	}
}
