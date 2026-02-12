using System;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Class to convert MS data objects into binary (for streaming).
/// </summary>
public class BinaryConversionMsWriter : IMassSpecDeviceWriter, IDisposable, IFileError
{
	private MsDataPacker.MassSpecGenericHeadersData _logHeaders;

	/// <summary>
	/// Gets a value indicating whether "prepare for run" action has been done.
	/// Other data writing methods are not valid without this call being made on the interface.
	/// </summary>
	public bool IsPreparedForRun { get; private set; }

	/// <summary>
	/// Gets or sets a value indicating whether there is an error.
	/// </summary>
	public bool HasError { get; set; }

	/// <summary>
	/// Gets a flag indicating whether there is a warning.
	/// Always false for this implementation.
	/// </summary>
	public bool HasWarning => false;

	/// <summary>
	/// Gets or sets an error code
	/// </summary>
	public int ErrorCode => 0;

	/// <summary>
	/// Gets or sets an error message
	/// </summary>
	public string ErrorMessage { get; set; }

	/// <summary>
	/// Gets the object which is receiving the converted data
	/// </summary>
	public IConvertedMsDataReceiver Receiver { get; }

	/// <summary>
	/// Returns a warning message (recoverable error). Always empty.
	/// </summary>
	public string WarningMessage => string.Empty;

	/// <summary>
	/// Dispose of this object (no action for this class)
	/// </summary>
	public void Dispose()
	{
		Receiver.Dispose();
	}

	/// <summary>
	/// Constructs a new instance of the binary conversion writer.
	/// This does not immediately write data to disk.
	/// The data is converted ito byte arrays, and it storage is then
	/// delegated to the receiver.
	/// </summary>
	/// <param name="receiver">Injected object to receive the converted data</param>
	public BinaryConversionMsWriter(IConvertedMsDataReceiver receiver)
	{
		ErrorMessage = string.Empty;
		Receiver = receiver;
	}

	/// <inheritdoc />
	public bool PrepareForRun(IInstrumentDataAccess instrumentId, IMassSpecGenericHeaders headers, IMassSpecRunHeaderInfo runHeaderInfo, IScanEvents methodScanEvents)
	{
		try
		{
			byte[] packedInstrumentData = instrumentId.Pack();
			MsDataPacker.MassSpecGenericHeadersData massSpecGenericHeadersData = headers.Pack();
			byte[] packedRunHeaderInfo = runHeaderInfo.Pack();
			byte[] packedMsScanEvents = methodScanEvents.Pack();
			Receiver.ReceivePrepareForRun(packedInstrumentData, massSpecGenericHeadersData.Pack(), packedRunHeaderInfo, packedMsScanEvents);
			IsPreparedForRun = true;
			_logHeaders = massSpecGenericHeadersData;
		}
		catch (ArgumentException ex)
		{
			ErrorMessage = ex.Message;
			HasError = true;
			return false;
		}
		return true;
	}

	/// <summary>
	/// Handle the error condition that data is being logged
	/// without first calling PrepareForRun
	/// </summary>
	/// <returns>always returns false (error condition)</returns>
	private bool NotPreparedForRun()
	{
		ErrorMessage = "Cannot log data before prepare for run";
		return false;
	}

	/// <inheritdoc />
	public bool WriteErrorLog(float retentionTime, string errorLog)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveErrorLog(retentionTime, MsDataPacker.GetBytesWithLength(errorLog));
	}

	/// <inheritdoc />
	public bool WriteInstData(IMsInstrumentData instData)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveInstData(instData.Pack());
	}

	/// <inheritdoc />
	public bool WriteInstData(byte[] dataBlock, SpectrumPacketType packetType)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveInstData(dataBlock, (int)packetType);
	}

	/// <inheritdoc />
	public bool WriteInstScanIndex(IScanStatisticsAccess scanIndex, IScanEvent trailerScanEvent = null)
	{
		if (IsPreparedForRun)
		{
			byte[] packedEvent = ((trailerScanEvent != null) ? trailerScanEvent.Pack() : Array.Empty<byte>());
			return Receiver.ReceiveInstScanIndex(scanIndex.Pack(), packedEvent);
		}
		return NotPreparedForRun();
	}

	/// <inheritdoc />
	public bool WriteStatusLog(float retentionTime, byte[] data)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveStatusLog(retentionTime, data);
	}

	/// <inheritdoc />
	public bool WriteStatusLog(float retentionTime, object[] data)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveStatusLog(retentionTime, _logHeaders.StatusLogDescriptors.ConvertDataEntryToByteArray(data));
	}

	/// <inheritdoc />
	public bool WriteTrailerExtraData(object[] data)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveTrailerExtra(_logHeaders.TrailerExtraDescriptors.ConvertDataEntryToByteArray(data));
	}

	/// <inheritdoc />
	public bool WriteTrailerExtraData(byte[] data)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveTrailerExtra(data);
	}

	/// <inheritdoc />
	public bool WriteTuneData(object[] data)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveTuneData(_logHeaders.TuneDescriptors.ConvertDataEntryToByteArray(data));
	}

	/// <inheritdoc />
	public bool WriteTuneData(byte[] data)
	{
		if (!IsPreparedForRun)
		{
			return NotPreparedForRun();
		}
		return Receiver.ReceiveTuneData(data);
	}
}
