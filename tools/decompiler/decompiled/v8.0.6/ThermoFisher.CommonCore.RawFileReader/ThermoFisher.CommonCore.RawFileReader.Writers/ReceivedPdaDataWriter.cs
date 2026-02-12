using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This class takes PDA Data, which is intended to be transmitted over a protocol or cached and 
/// writes it to a raw file.
/// </summary>
public class ReceivedPdaDataWriter : IConvertedPdaDataReceiver, IConvertedBaseDataReceiver, IDisposable
{
	private readonly IPdaDeviceBinaryWriter _writer;

	/// <summary>
	/// construct an object to transfer PDA data to a raw file, starting with packed (byte array) data.
	/// </summary>
	/// <param name="fileName">The raw file which will receive the data</param>
	///   /// <param name="domain">Determines if this channel came from Xcalibur or Chromeleon</param>
	public ReceivedPdaDataWriter(string fileName, RawDataDomain domain = RawDataDomain.MassSpectrometry)
	{
		_writer = DeviceWriterAdapter.CreatePdaDeviceBinaryWriter(fileName, domain);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_writer.Dispose();
	}

	/// <inheritdoc />
	public bool ReceivePdaScan(byte[] data)
	{
		PdaScanIndex pdaScanIndex = new PdaScanIndex();
		pdaScanIndex.StartTime = BitConverter.ToDouble(data);
		pdaScanIndex.TIC = BitConverter.ToDouble(data, 8);
		pdaScanIndex.LongWavelength = BitConverter.ToDouble(data, 16);
		pdaScanIndex.ShortWavelength = BitConverter.ToDouble(data, 24);
		pdaScanIndex.WavelengthStep = BitConverter.ToDouble(data, 32);
		pdaScanIndex.AUScale = BitConverter.ToDouble(data, 40);
		int num = data.Length - 48;
		double[] array = new double[num / 8];
		Buffer.BlockCopy(data, 48, array, 0, num);
		return _writer.WriteInstData(array, pdaScanIndex);
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
