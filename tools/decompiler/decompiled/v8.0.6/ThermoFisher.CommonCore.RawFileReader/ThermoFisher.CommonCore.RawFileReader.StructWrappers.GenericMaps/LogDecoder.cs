using System.Text;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// Decodes values in logs (status logs, tune method logs, etc.)
/// </summary>
internal class LogDecoder : ILogDecoder
{
	private readonly bool _useBufferManager;

	private readonly RecordBufferManager _bufferManager;

	private readonly int _logEntrySize;

	private IMemoryReader Reader { get; set; }

	/// <summary>
	/// Number of bytes that can be read (from the stream start offset)
	/// </summary>
	public long Available { get; }

	public DataDescriptors Descriptors { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LogDecoder" /> class.
	/// </summary>
	/// <param name="reader">
	/// Where to read the logs.
	/// </param>
	/// <param name="descriptors">Format for all log entries</param>
	public LogDecoder(IMemoryReader reader, DataDescriptors descriptors)
	{
		Reader = reader;
		Available = reader.Length - reader.InitialOffset;
		Descriptors = descriptors;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LogDecoder" /> class.
	/// </summary>
	/// <param name="bufferManager">Manages buffers of ranges of records</param>
	/// <param name="logEntrySize">size of one log record</param>
	/// <param name="descriptors">Format for all log entrie</param>
	public LogDecoder(RecordBufferManager bufferManager, int logEntrySize, DataDescriptors descriptors)
	{
		_useBufferManager = true;
		_bufferManager = bufferManager;
		_logEntrySize = logEntrySize;
		Descriptors = descriptors;
		Available = bufferManager.Available;
	}

	/// <summary>
	/// Get the value of a field, with minimal decoding
	/// </summary>
	/// <param name="dataOffset">offset into map</param>
	/// <param name="dataDescriptor">definition of type</param>
	/// <returns>The value as an object</returns>
	public object GetValue(ref long dataOffset, DataDescriptor dataDescriptor)
	{
		IMemoryReader memoryReader = Reader;
		if (_useBufferManager)
		{
			long num = dataOffset / _logEntrySize;
			memoryReader = _bufferManager.FindReader((int)num);
		}
		switch (dataDescriptor.DataType)
		{
		case DataTypes.Char:
			return (sbyte)memoryReader.ReadByteExt(ref dataOffset);
		case DataTypes.TrueFalse:
		case DataTypes.YesNo:
		case DataTypes.OnOff:
			return memoryReader.ReadByteExt(ref dataOffset) != 0;
		case DataTypes.UnsignedChar:
			return memoryReader.ReadByteExt(ref dataOffset);
		case DataTypes.Short:
			return memoryReader.ReadShortExt(ref dataOffset);
		case DataTypes.UnsignedShort:
			return memoryReader.ReadUnsignedShortExt(ref dataOffset);
		case DataTypes.Long:
			return memoryReader.ReadIntExt(ref dataOffset);
		case DataTypes.UnsignedLong:
			return memoryReader.ReadUnsignedIntExt(ref dataOffset);
		case DataTypes.Float:
			return memoryReader.ReadFloatExt(ref dataOffset);
		case DataTypes.Double:
			return memoryReader.ReadDoubleExt(ref dataOffset);
		case DataTypes.CharString:
			return DecodeCharString(memoryReader, ref dataOffset, dataDescriptor);
		case DataTypes.WideCharString:
			return DecodeWideCharString(memoryReader, ref dataOffset, dataDescriptor);
		default:
			return null;
		}
	}

	/// <summary>
	/// Decipher a value.
	/// </summary>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="dataDescriptor">
	/// The data descriptor.
	/// </param>
	/// <returns>
	/// The value, as an object
	/// </returns>
	public object DecipherValue(ref long dataOffset, DataDescriptor dataDescriptor)
	{
		IMemoryReader memoryReader = Reader;
		if (_useBufferManager)
		{
			long num = dataOffset / _logEntrySize;
			memoryReader = _bufferManager.FindReader((int)num);
		}
		switch (dataDescriptor.DataType)
		{
		case DataTypes.Char:
			return memoryReader.ReadByteExt(ref dataOffset);
		case DataTypes.TrueFalse:
			if (memoryReader.ReadByteExt(ref dataOffset) != 0)
			{
				return "True";
			}
			return "False";
		case DataTypes.YesNo:
			if (memoryReader.ReadByteExt(ref dataOffset) != 0)
			{
				return "Yes";
			}
			return "No";
		case DataTypes.OnOff:
			if (memoryReader.ReadByteExt(ref dataOffset) != 0)
			{
				return "On";
			}
			return "Off";
		case DataTypes.UnsignedChar:
			return memoryReader.ReadByteExt(ref dataOffset);
		case DataTypes.Short:
			return memoryReader.ReadShortExt(ref dataOffset);
		case DataTypes.UnsignedShort:
			return memoryReader.ReadUnsignedShortExt(ref dataOffset);
		case DataTypes.Long:
			return memoryReader.ReadIntExt(ref dataOffset);
		case DataTypes.UnsignedLong:
			return memoryReader.ReadUnsignedIntExt(ref dataOffset);
		case DataTypes.Float:
			return memoryReader.ReadFloatExt(ref dataOffset);
		case DataTypes.Double:
			return memoryReader.ReadDoubleExt(ref dataOffset);
		case DataTypes.CharString:
			return DecodeCharString(memoryReader, ref dataOffset, dataDescriptor);
		case DataTypes.WideCharString:
			return DecodeWideCharString(memoryReader, ref dataOffset, dataDescriptor);
		default:
			return null;
		}
	}

	/// <summary>
	/// Decode char string.
	/// </summary>
	/// <param name="reader">Reader to get the string from</param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="dataDescriptor">
	/// The data descriptor.
	/// </param>
	/// <returns>
	/// The decoded string
	/// </returns>
	private string DecodeCharString(IMemoryReader reader, ref long dataOffset, DataDescriptor dataDescriptor)
	{
		byte[] array = reader.ReadBytesExt(ref dataOffset, (int)dataDescriptor.ItemSize);
		int i;
		for (i = 0; i < array.Length && array[i] != 0; i++)
		{
		}
		if (i != 0)
		{
			return Encoding.ASCII.GetString(array, 0, i);
		}
		return string.Empty;
	}

	/// <summary>
	/// Decode a wide char string.
	/// </summary>
	/// <param name="reader">Reader to get the string from</param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="dataDescriptor">
	/// The data descriptor.
	/// </param>
	/// <returns>
	/// The decoded string.
	/// </returns>
	private string DecodeWideCharString(IMemoryReader reader, ref long dataOffset, DataDescriptor dataDescriptor)
	{
		long numOfBytesRead = 0L;
		string result = reader.ReadWideChars(dataOffset, ref numOfBytesRead, dataDescriptor.LengthOrPrecision);
		dataOffset += numOfBytesRead;
		return result;
	}
}
