using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This class includes static methods which turn C# objects from the MS writer interface
/// into byte arrays, such that they a be transported with any protocols that can transport binary data.
/// The data packing is intended to match the binary format of a raw file.
/// </summary>
public static class MsDataPacker
{
	/// <summary>
	/// Binary (byte array) data from the generic headers of a mass spectrometer.
	/// </summary>
	public class MassSpecGenericHeadersData : IPackedMassSpecHeaders
	{
		/// <summary>
		/// Packed trailer extra headers
		/// </summary>
		public byte[] TrailerExtraHeader { get; set; }

		/// <summary>
		///  Packed status log headers
		/// </summary>
		public byte[] StatusLogHeader { get; set; }

		/// <summary>
		///  Packed tune headers
		/// </summary>
		public byte[] TuneHeader { get; set; }

		/// <summary>
		/// Gets or sets a tool which can format "trailer" log entries as bytes.
		/// </summary>
		public IGenericDataPack TrailerExtraDescriptors { get; set; }

		/// <summary>
		/// Gets or sets a tool which can format "status" log entries as bytes.
		/// </summary>
		public IGenericDataPack StatusLogDescriptors { get; set; }

		/// <summary>
		/// Gets or sets a tool which can format "tune" log entries as bytes.
		/// </summary>
		public IGenericDataPack TuneDescriptors { get; set; }
	}

	/// <summary>
	/// Headers for all logs as byte arrays
	/// </summary>
	private class SplitHeaders : IPackedMassSpecHeaders
	{
		public byte[] TrailerExtraHeader { get; set; }

		public byte[] StatusLogHeader { get; set; }

		public byte[] TuneHeader { get; set; }
	}

	/// <summary>
	/// Data for a scan, converted into byte arrays
	/// </summary>
	public class PackedMsInstrumentData : IPackedMsInstrumentData
	{
		/// <summary>
		/// Gets or sets the converted scan event
		/// </summary>
		public byte[] PackedScanEvent { get; set; }

		/// <summary>
		/// Gets or sets the converted scan stats
		/// </summary>
		public byte[] PackedScanStats { get; internal set; }

		/// <summary>
		/// Gets or sets the converted scan data
		/// </summary>
		public byte[] ScanData { get; internal set; }

		/// <inheritdoc />
		public int ProfilePaketCount { get; internal set; }

		/// <inheritdoc />
		public byte[] ProfileArray { get; internal set; }

		/// <summary>
		/// Convert this object to a byte array
		/// </summary>
		/// <returns></returns>
		public byte[] ToByteArray()
		{
			byte[] array = new byte[PackedScanEvent.Length + PackedScanStats.Length + ScanData.Length + ProfileArray.Length + 20];
			Array.Copy(BitConverter.GetBytes(ProfilePaketCount), array, 4);
			int offset = 4;
			InsertArray(PackedScanEvent, array, ref offset);
			InsertArray(PackedScanStats, array, ref offset);
			InsertArray(ScanData, array, ref offset);
			InsertArray(ProfileArray, array, ref offset);
			return array;
		}

		/// <summary>
		/// Create Packed Data from a byte array
		/// </summary>
		/// <param name="data"></param>
		public PackedMsInstrumentData(byte[] data)
		{
			ProfilePaketCount = BitConverter.ToInt32(data);
			int index = 4;
			PackedScanEvent = GetByteArrayFromBlob(data, ref index);
			PackedScanStats = GetByteArrayFromBlob(data, ref index);
			ScanData = GetByteArrayFromBlob(data, ref index);
			ProfileArray = GetByteArrayFromBlob(data, ref index);
		}

		/// <summary>
		/// default constructor
		/// </summary>
		public PackedMsInstrumentData()
		{
		}
	}

	/// <summary>
	/// Packs IInstrumentDataAccess into a byte array
	/// </summary>
	/// <param name="instrumentData">Data to be packed</param>
	/// <returns>the packed data, or an empty array if there is an issue packing the data</returns>
	public static byte[] Pack(this IInstrumentDataAccess instrumentData)
	{
		List<byte> list = new List<byte>(100);
		AppendBool(list, instrumentData.IsValid);
		AppendInt(list, (int)instrumentData.Units);
		string[] channelLabels = instrumentData.ChannelLabels;
		AppendInt(list, channelLabels.Length);
		string[] array = channelLabels;
		foreach (string label in array)
		{
			list.AppendString(label);
		}
		list.AppendString(instrumentData.Name);
		list.AppendString(instrumentData.Model);
		list.AppendString(instrumentData.SerialNumber);
		list.AppendString(instrumentData.SoftwareVersion);
		list.AppendString(instrumentData.HardwareVersion);
		list.AppendString(instrumentData.Flags);
		list.AppendString(instrumentData.AxisLabelX);
		list.AppendString(instrumentData.AxisLabelY);
		return list.ToArray();
	}

	/// <summary>
	/// Unpacks a byte array into IInstrumentDataAccess 
	/// </summary>
	/// <param name="bytes">Data to be unpacked</param>
	/// <returns>the unpacked data</returns>
	public static IInstrumentDataAccess UnpackInstrumentData(this byte[] bytes)
	{
		InstrumentData instrumentData = new InstrumentData();
		int index = 0;
		instrumentData.IsValid = GetBool(ref index, bytes);
		instrumentData.Units = (DataUnits)GetInt(ref index, bytes);
		int num = GetInt(ref index, bytes);
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = GetString(ref index, bytes);
		}
		instrumentData.ChannelLabels = array;
		instrumentData.Name = GetString(ref index, bytes);
		instrumentData.Model = GetString(ref index, bytes);
		instrumentData.SerialNumber = GetString(ref index, bytes);
		instrumentData.SoftwareVersion = GetString(ref index, bytes);
		instrumentData.HardwareVersion = GetString(ref index, bytes);
		instrumentData.Flags = GetString(ref index, bytes);
		instrumentData.AxisLabelX = GetString(ref index, bytes);
		instrumentData.AxisLabelY = GetString(ref index, bytes);
		return instrumentData;
	}

	private static string GetString(ref int index, byte[] bytes)
	{
		int num = BitConverter.ToInt32(bytes, index);
		index += 4;
		char[] array = new char[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = BitConverter.ToChar(bytes, index);
			index += 2;
		}
		return new string(array);
	}

	private static void AppendString(this List<byte> result, string label)
	{
		result.AddRange(BitConverter.GetBytes(label.Length));
		foreach (char value in label)
		{
			result.AddRange(BitConverter.GetBytes(value));
		}
	}

	private static int GetInt(ref int index, byte[] bytes)
	{
		char result = BitConverter.ToChar(bytes, index);
		index += 4;
		return result;
	}

	private static void AppendInt(List<byte> result, int i)
	{
		result.AddRange(BitConverter.GetBytes(i));
	}

	private static bool GetBool(ref int index, byte[] bytes)
	{
		return bytes[index++] > 0;
	}

	private static void AppendBool(List<byte> result, bool b)
	{
		result.Add(b ? ((byte)1) : ((byte)0));
	}

	/// <summary>
	/// Convert all mass spec generic headers into byte arrays
	/// </summary>
	/// <param name="massSpecGenericHeaders">Headers to pack</param>
	/// <returns>The headers packed into byte arrays</returns>
	public static MassSpecGenericHeadersData Pack(this IMassSpecGenericHeaders massSpecGenericHeaders)
	{
		MassSpecGenericHeadersData massSpecGenericHeadersData = new MassSpecGenericHeadersData();
		try
		{
			Tuple<byte[], DataDescriptors> tuple = PackHeaderItem(massSpecGenericHeaders.TrailerExtraHeader);
			massSpecGenericHeadersData.TrailerExtraHeader = tuple.Item1;
			massSpecGenericHeadersData.TrailerExtraDescriptors = tuple.Item2;
		}
		catch
		{
			throw new ArgumentException("invalid TrailerExtraHeader", "massSpecGenericHeaders");
		}
		try
		{
			Tuple<byte[], DataDescriptors> tuple2 = PackHeaderItem(massSpecGenericHeaders.StatusLogHeader);
			massSpecGenericHeadersData.StatusLogHeader = tuple2.Item1;
			massSpecGenericHeadersData.StatusLogDescriptors = tuple2.Item2;
		}
		catch
		{
			throw new ArgumentException("invalid StatusLogHeader", "massSpecGenericHeaders");
		}
		try
		{
			Tuple<byte[], DataDescriptors> tuple3 = PackHeaderItem(massSpecGenericHeaders.TuneHeader);
			massSpecGenericHeadersData.TuneHeader = tuple3.Item1;
			massSpecGenericHeadersData.TuneDescriptors = tuple3.Item2;
			return massSpecGenericHeadersData;
		}
		catch
		{
			throw new ArgumentException("invalid TuneHeader", "massSpecGenericHeaders");
		}
	}

	/// <summary>
	/// pack headers into a single byte array
	/// </summary>
	/// <param name="headers"></param>
	/// <returns>byte array with all headers</returns>
	public static byte[] Pack(this IPackedMassSpecHeaders headers)
	{
		byte[] trailerExtraHeader = headers.TrailerExtraHeader;
		byte[] statusLogHeader = headers.StatusLogHeader;
		byte[] tuneHeader = headers.TuneHeader;
		byte[] array = new byte[12 + trailerExtraHeader.Length + statusLogHeader.Length + tuneHeader.Length];
		int offset = 0;
		InsertArray(trailerExtraHeader, array, ref offset);
		InsertArray(statusLogHeader, array, ref offset);
		InsertArray(tuneHeader, array, ref offset);
		return array;
	}

	/// <summary>
	/// Insery an array within another, prceeded by the array length
	/// </summary>
	/// <param name="data">data to inseret</param>
	/// <param name="target">array to copy into</param>
	/// <param name="offset">offset of tqrget array, updated by inserted data</param>
	private static void InsertArray(byte[] data, byte[] target, ref int offset)
	{
		int num = data.Length;
		Array.Copy(BitConverter.GetBytes(num), 0, target, offset, 4);
		Array.Copy(data, 0, target, offset + 4, num);
		offset += 4 + num;
	}

	/// <summary>
	/// Separates out Trailer, Status and Tune headers from a byte array.
	/// </summary>
	/// <param name="packedHeaders"></param>
	/// <returns>Inteface to retrieve separate log headers</returns>
	public static IPackedMassSpecHeaders HeadersFromBytes(byte[] packedHeaders)
	{
		int index = 0;
		return new SplitHeaders
		{
			TrailerExtraHeader = GetByteArrayFromBlob(packedHeaders, ref index),
			StatusLogHeader = GetByteArrayFromBlob(packedHeaders, ref index),
			TuneHeader = GetByteArrayFromBlob(packedHeaders, ref index)
		};
	}

	private static byte[] GetByteArrayFromBlob(byte[] blob, ref int index)
	{
		int num = BitConverter.ToInt32(blob, index);
		byte[] array = new byte[num];
		Array.Copy(blob, index + 4, array, 0, num);
		index = index + 4 + num;
		return array;
	}

	/// <summary>
	/// Unpack all mass spec generic headers from byte arrays
	/// </summary>
	/// <param name="packedMassSpecGenericHeaders">Headers to unpack</param>
	/// <returns>The headers packed into byte arrays</returns>
	public static IMassSpecGenericHeaders Unpack(this IPackedMassSpecHeaders packedMassSpecGenericHeaders)
	{
		return new MassSpecGenericHeaders
		{
			TrailerExtraHeader = UnpackHeaderItem(packedMassSpecGenericHeaders.TrailerExtraHeader),
			StatusLogHeader = UnpackHeaderItem(packedMassSpecGenericHeaders.StatusLogHeader),
			TuneHeader = UnpackHeaderItem(packedMassSpecGenericHeaders.TuneHeader)
		};
	}

	/// <summary>
	/// Unpack bytes into a Generic header
	/// </summary>
	/// <param name="headers">Bytes for the header</param>
	/// <returns>The table of headers for this generic format</returns>
	public static IHeaderItem[] UnpackHeaderItem(byte[] headers)
	{
		int num = BitConverter.ToInt32(headers, 0);
		int index = 4;
		IHeaderItem[] array = new IHeaderItem[num];
		for (int i = 0; i < num; i++)
		{
			HeaderItem headerItem = new HeaderItem();
			headerItem.DataType = (GenericDataTypes)BitConverter.ToInt32(headers, index);
			index += 4;
			int num2 = BitConverter.ToInt32(headers, index);
			index += 4;
			headerItem.StringLengthOrPrecision = num2 & 0xFFFF;
			headerItem.IsScientificNotation = (num2 & 0xFFFF0000u) != 0;
			headerItem.Label = UnpackString(headers, ref index);
			array[i] = headerItem;
		}
		return array;
	}

	/// <summary>
	/// Pack a set of header items into a byte array
	/// </summary>
	/// <param name="headers">The items to be packed</param>
	/// <returns>A byte array for the headers</returns>
	internal static Tuple<byte[], DataDescriptors> PackHeaderItem(IHeaderItem[] headers)
	{
		using MemoryStream memoryStream = new MemoryStream();
		int numHeaderItems = ((headers != null) ? headers.Length : 0);
		headers.ConvertHeaderItemsToGenericHeader(out var dataDescriptors);
		dataDescriptors.SaveGenericHeaderToMemoryStream(memoryStream, numHeaderItems);
		return new Tuple<byte[], DataDescriptors>(memoryStream.ToArray(), dataDescriptors);
	}

	/// <summary>
	/// Pack a set of header items into a byte array
	/// </summary>
	/// <param name="headers">The items to be packed</param>
	/// <returns>A byte array for the headers</returns>
	public static byte[] PackHeaderItems(IHeaderItem[] headers)
	{
		return PackHeaderItem(headers).Item1;
	}

	/// <summary>
	/// Convert a mass spec run header to a byte array
	/// </summary>
	/// <param name="massSpecRunHeader"></param>
	/// <returns>An array representing the header</returns>
	public static byte[] Pack(this IMassSpecRunHeaderInfo massSpecRunHeader)
	{
		byte[] bytes = BitConverter.GetBytes(massSpecRunHeader.ExpectedRunTime);
		byte[] bytes2 = BitConverter.GetBytes(massSpecRunHeader.MassResolution);
		byte[] bytes3 = BitConverter.GetBytes(massSpecRunHeader.Precision);
		byte[] bytesWithLength = GetBytesWithLength(massSpecRunHeader.Comment1);
		byte[] bytesWithLength2 = GetBytesWithLength(massSpecRunHeader.Comment2);
		int num = bytesWithLength.Length + bytesWithLength2.Length;
		int num2 = bytes.Length + bytes2.Length + bytes3.Length + num;
		int num3 = 0;
		byte[] array = new byte[num2];
		Array.Copy(bytes, array, bytes.Length);
		num3 += bytes.Length;
		Array.Copy(bytes2, 0, array, num3, bytes2.Length);
		num3 += bytes2.Length;
		Array.Copy(bytes3, 0, array, num3, bytes3.Length);
		num3 += bytes3.Length;
		Array.Copy(bytesWithLength, 0, array, num3, bytesWithLength.Length);
		num3 += bytesWithLength.Length;
		Array.Copy(bytesWithLength2, 0, array, num3, bytesWithLength2.Length);
		return array;
	}

	/// <summary>
	/// Convert a byte array to a mass spec run header
	/// </summary>
	/// <param name="data"></param>
	/// <returns>An array representing the header</returns>
	public static IMassSpecRunHeaderInfo UnpackRunHeader(byte[] data)
	{
		int index = 20;
		string comment = UnpackString(data, ref index);
		string comment2 = UnpackString(data, ref index);
		return new MassSpecRunHeaderInfo
		{
			ExpectedRunTime = BitConverter.ToDouble(data, 0),
			MassResolution = BitConverter.ToDouble(data, 8),
			Precision = BitConverter.ToInt32(data, 16),
			Comment1 = comment,
			Comment2 = comment2
		};
	}

	/// <summary>
	/// Unpacks a string, which is preceded by it's length
	/// </summary>
	/// <param name="data">message</param>
	/// <param name="index">the start index</param>
	/// <returns></returns>
	public static string UnpackString(byte[] data, ref int index)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int num = BitConverter.ToInt32(data, index);
		index += 4;
		if (num == 0)
		{
			return string.Empty;
		}
		byte[] array = new byte[num];
		Buffer.BlockCopy(data, index, array, 0, num);
		index += num;
		return GetString(array);
	}

	/// <summary>
	/// Convert the scans events to a byte array
	/// </summary>
	/// <param name="scanEvents">the events</param>
	/// <returns>data packed as a byte array</returns>
	public static byte[] Pack(this IScanEvents scanEvents)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.Unicode);
		DeviceErrors devErrors = new DeviceErrors();
		MassSpecDeviceWriter.SaveScanEvents(scanEvents, writer, devErrors);
		return memoryStream.ToArray();
	}

	/// <summary>
	/// Convert the scans statistics to a byte array
	/// </summary>
	/// <param name="scanStatistics">the stats</param>
	/// <returns>data packed as a byte array</returns>
	public static byte[] Pack(this IScanStatisticsAccess scanStatistics)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(scanStatistics.ScanEventNumber);
		binaryWriter.Write(scanStatistics.ScanNumber);
		binaryWriter.Write(scanStatistics.PacketType);
		binaryWriter.Write(scanStatistics.StartTime);
		binaryWriter.Write(scanStatistics.TIC);
		binaryWriter.Write(scanStatistics.BasePeakIntensity);
		binaryWriter.Write(scanStatistics.BasePeakMass);
		binaryWriter.Write(scanStatistics.HighMass);
		binaryWriter.Write(scanStatistics.LowMass);
		binaryWriter.Write(scanStatistics.CycleNumber);
		binaryWriter.Write(scanStatistics.LongWavelength);
		binaryWriter.Write(scanStatistics.ShortWavelength);
		binaryWriter.Write(scanStatistics.AbsorbanceUnitScale);
		binaryWriter.Write(scanStatistics.NumberOfChannels);
		binaryWriter.Write(scanStatistics.Frequency);
		binaryWriter.Write(scanStatistics.IsUniformTime);
		binaryWriter.Write(scanStatistics.IsCentroidScan);
		binaryWriter.Write(scanStatistics.PacketCount);
		binaryWriter.Write(scanStatistics.WavelengthStep);
		binaryWriter.Write(scanStatistics.SegmentNumber);
		binaryWriter.Flush();
		return memoryStream.ToArray();
	}

	/// <summary>
	/// Convert the scans statistics to a byte array
	/// </summary>
	/// <param name="packedScanStatistics">the stats</param>
	/// <returns>data packed as a byte array</returns>
	public static IScanStatisticsAccess UnpackScanStatistics(byte[] packedScanStatistics)
	{
		return new ScanStatistics
		{
			ScanEventNumber = BitConverter.ToInt32(packedScanStatistics, 0),
			ScanNumber = BitConverter.ToInt32(packedScanStatistics, 4),
			PacketType = BitConverter.ToInt32(packedScanStatistics, 8),
			StartTime = BitConverter.ToDouble(packedScanStatistics, 12),
			TIC = BitConverter.ToDouble(packedScanStatistics, 20),
			BasePeakIntensity = BitConverter.ToDouble(packedScanStatistics, 28),
			BasePeakMass = BitConverter.ToDouble(packedScanStatistics, 36),
			HighMass = BitConverter.ToDouble(packedScanStatistics, 44),
			LowMass = BitConverter.ToDouble(packedScanStatistics, 52),
			CycleNumber = BitConverter.ToInt32(packedScanStatistics, 60),
			LongWavelength = BitConverter.ToDouble(packedScanStatistics, 64),
			ShortWavelength = BitConverter.ToDouble(packedScanStatistics, 72),
			AbsorbanceUnitScale = BitConverter.ToDouble(packedScanStatistics, 80),
			NumberOfChannels = BitConverter.ToInt32(packedScanStatistics, 88),
			Frequency = BitConverter.ToDouble(packedScanStatistics, 92),
			IsUniformTime = BitConverter.ToBoolean(packedScanStatistics, 100),
			IsCentroidScan = BitConverter.ToBoolean(packedScanStatistics, 101),
			PacketCount = BitConverter.ToInt32(packedScanStatistics, 102),
			WavelengthStep = BitConverter.ToDouble(packedScanStatistics, 106),
			SegmentNumber = BitConverter.ToInt32(packedScanStatistics, 114)
		};
	}

	/// <summary>
	/// Convert a scan event into a byte array
	/// </summary>
	/// <param name="scanEvent">the event to convert</param>
	/// <returns>the data as a byte array</returns>
	public static byte[] Pack(this IScanEvent scanEvent)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.Unicode);
		DeviceErrors errors = new DeviceErrors();
		if (scanEvent.SaveScanEvent(writer, errors))
		{
			return memoryStream.ToArray();
		}
		throw new ArgumentException("invalid data", "scanEvent");
	}

	/// <summary>
	/// Converts a string tp a byte array, with the string length also encoded
	/// </summary>
	/// <param name="str">String to encode</param>
	/// <returns>Byte array of the length prefixed string</returns>
	public static byte[] GetBytesWithLength(string str)
	{
		byte[] array = new byte[str.Length * 2 + 4];
		Buffer.BlockCopy(BitConverter.GetBytes(str.Length * 2), 0, array, 0, 4);
		Buffer.BlockCopy(str.ToCharArray(), 0, array, 4, array.Length - 4);
		return array;
	}

	/// <summary>
	/// Convert a byte array to a string
	/// </summary>
	/// <param name="bytes">bytes of string</param>
	/// <returns>the converted string</returns>
	private static string GetString(byte[] bytes)
	{
		char[] array = new char[bytes.Length / 2];
		Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
		return new string(array);
	}

	/// <summary>
	/// Pack data from an MS scan to byte arrays.
	/// </summary>
	/// <param name="instData">Data to be packed</param>
	/// <returns>Packed data</returns>
	public static byte[] Pack(this IMsInstrumentData instData)
	{
		PackedMsInstrumentData packedMsInstrumentData = new PackedMsInstrumentData();
		packedMsInstrumentData.PackedScanEvent = instData.EventData.Pack();
		packedMsInstrumentData.PackedScanStats = instData.StatisticsData.Pack();
		packedMsInstrumentData.ScanData = PeakData.ConvertRawScanIntoPackets(instData, out var profileDataPacket);
		if (profileDataPacket != null && profileDataPacket.Length != 0)
		{
			packedMsInstrumentData.ProfilePaketCount = profileDataPacket.Length;
			int num = Marshal.SizeOf(typeof(ProfileDataPacket64));
			byte[] array = new byte[profileDataPacket.Length * num];
			int num2 = 0;
			ProfileDataPacket64[] array2 = profileDataPacket;
			for (int i = 0; i < array2.Length; i++)
			{
				Array.Copy(WriterHelper.StructToByteArray(array2[i], num), 0, array, num2, num);
				num2 += num;
			}
			packedMsInstrumentData.ProfileArray = array;
		}
		else
		{
			packedMsInstrumentData.ProfileArray = Array.Empty<byte>();
		}
		return packedMsInstrumentData.ToByteArray();
	}

	/// <summary>
	/// Unpack data from an MS scan from byte arrays.
	/// </summary>
	/// <param name="instData">Data to be packed</param>
	/// <returns>Packed data</returns>
	public static IBinaryMsInstrumentData Unpack(this IPackedMsInstrumentData instData)
	{
		return new UnpackedMsInstrumentData
		{
			PackedScanEvent = instData.PackedScanEvent,
			StatisticsData = UnpackScanStatistics(instData.PackedScanStats),
			ScanData = instData.ScanData,
			ProfileIndexCount = instData.ProfilePaketCount,
			ProfileData = instData.ProfileArray
		};
	}

	/// <summary>Packs the ms analog scan.</summary>
	/// <param name="instDataIndex">Index of the inst data.</param>
	/// <param name="instData">The instrument data.</param>
	/// <returns>Packed analog scan data </returns>
	public static byte[] PackMsAnalogScan(this IAnalogScanIndex instDataIndex, double[] instData)
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
		return array;
	}

	/// <summary>
	/// Gets the generic data pack from header bytes.
	///
	/// Example:
	/// convert status data entry to byte array
	/// IGenericDataPack.ConvertDataEntryToByteArray(data)
	/// </summary>
	/// <param name="headers">The headers.</param>
	/// <returns>
	///   The generic data pack object
	/// </returns>
	public static IGenericDataPack GetGenericDataPackFromHeaderBytes(byte[] headers)
	{
		UnpackHeaderItem(headers).ConvertHeaderItemsToGenericHeader(out var dataDescriptors);
		return dataDescriptors;
	}
}
