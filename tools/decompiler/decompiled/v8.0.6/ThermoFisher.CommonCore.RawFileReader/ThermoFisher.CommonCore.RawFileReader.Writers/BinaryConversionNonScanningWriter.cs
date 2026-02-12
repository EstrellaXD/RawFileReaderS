using System;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Class to convert analog or UV data objects into binary (for streaming).
/// </summary>
public class BinaryConversionNonScanningWriter : IBaseDeviceWriter, IDisposable, IFileError
{
	/// <summary>
	/// Gets or sets a tool which can format "status" log entries as bytes.
	/// </summary>
	private IGenericDataPack StatusLogDescriptors { get; set; }

	private bool StatusLogHeaderReceived { get; set; }

	/// <summary>
	/// Gets the object which is receiving the converted data
	/// </summary>
	public IConvertedBaseDataReceiver Receiver { get; set; }

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
	/// Returns a warning message (recoverable error). Always empty.
	/// </summary>
	public string WarningMessage => string.Empty;

	/// <summary>
	/// Gets a value indicating whether "prepare for run" actions have been done.
	/// This includes: sending Instrument Info, Comments and Expected runtime
	/// Other data writing methods are not valid without this call being made on the interface.
	/// </summary>
	public bool IsPreparedForRun
	{
		get
		{
			if (StatusLogHeaderReceived && CommentsReceived && ExpectedRunTimeReceived)
			{
				return InstrumentInfoReceived;
			}
			return false;
		}
	}

	private bool CommentsReceived { get; set; }

	private bool ExpectedRunTimeReceived { get; set; }

	private bool InstrumentInfoReceived { get; set; }

	/// <summary>
	/// Dispose of this object (no action for this class)
	/// </summary>
	public void Dispose()
	{
		Receiver.Dispose();
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
	public bool WriteInstComments(string comment1, string comment2)
	{
		byte[] bytesWithLength = MsDataPacker.GetBytesWithLength(comment1);
		byte[] bytesWithLength2 = MsDataPacker.GetBytesWithLength(comment2);
		CommentsReceived = true;
		byte[] array = new byte[bytesWithLength.Length + bytesWithLength2.Length];
		Array.Copy(bytesWithLength, array, bytesWithLength.Length);
		Array.Copy(bytesWithLength2, 0, array, bytesWithLength.Length, bytesWithLength2.Length);
		return Receiver.ReceiveComments(array);
	}

	/// <inheritdoc />
	public bool WriteInstExpectedRunTime(double runTime)
	{
		ExpectedRunTimeReceived = true;
		return Receiver.ReceiveExpectedRunTime(runTime);
	}

	/// <inheritdoc />
	public bool WriteInstrumentInfo(IInstrumentDataAccess instrumentId)
	{
		byte[] packedInstrumentData = instrumentId.Pack();
		InstrumentInfoReceived = true;
		return Receiver.ReceiveInstrumentData(packedInstrumentData);
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
		return Receiver.ReceiveStatusLog(retentionTime, StatusLogDescriptors.ConvertDataEntryToByteArray(data));
	}

	/// <inheritdoc />
	public bool WriteStatusLogHeader(IHeaderItem[] headerItems)
	{
		Tuple<byte[], DataDescriptors> tuple = MsDataPacker.PackHeaderItem(headerItems);
		byte[] item = tuple.Item1;
		StatusLogDescriptors = tuple.Item2;
		StatusLogHeaderReceived = true;
		return Receiver.ReceiveStatusLogHeader(item);
	}

	/// <summary>
	/// Handle the error condition that data is being logged
	/// without first calling PrepareForRun
	/// </summary>
	/// <returns>always returns false (error condition)</returns>
	private bool NotPreparedForRun()
	{
		ErrorMessage = "Cannot log data before sending expected run time, instrument info and comments";
		return false;
	}
}
