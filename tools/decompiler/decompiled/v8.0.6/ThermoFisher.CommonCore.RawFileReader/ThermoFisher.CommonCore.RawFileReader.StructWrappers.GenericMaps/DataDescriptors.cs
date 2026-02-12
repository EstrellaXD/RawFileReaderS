using System;
using System.Collections.Generic;
using System.Text;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// The collection of data descriptors.
/// </summary>
internal class DataDescriptors : List<DataDescriptor>, IRawObjectBase, IGenericDataPack
{
	/// <summary>
	/// Gets the total size of the data blob being described.
	/// </summary>
	internal uint TotalDataSize { get; private set; }

	/// <summary>
	/// Gets the offset from the start of a data block to a specific field
	/// permitting individual fields to be decoded.
	/// </summary>
	internal uint[] FieldOffset { get; private set; }

	/// <summary>
	/// Gets the "all valid" table.
	/// Fields may have optional validity flags (per record)
	/// When that feature is not used, all fields are valid.
	/// This internal array is initialized to "all values valid'
	/// so it does not have to be recreated each scan
	/// </summary>
	internal bool[] AllValid { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.DataDescriptors" /> class.
	/// </summary>
	/// <param name="capacity">
	/// The capacity.
	/// </param>
	public DataDescriptors(int capacity)
	{
		base.Capacity = capacity;
		TotalDataSize = 0u;
		FieldOffset = new uint[capacity];
		AllValid = new bool[capacity];
	}

	/// <summary>
	/// Loads from the specified viewer.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>The number of bytes reader</returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		int numberOfItems = viewer.ReadIntExt(ref startPos);
		FieldOffset = new uint[numberOfItems];
		AllValid = new bool[numberOfItems];
		if (numberOfItems <= 0)
		{
			return startPos - dataOffset;
		}
		startPos = (viewer.PreferLargeReads ? Utilities.LoadDataFromInternalMemoryArrayReader(GetDataDescriptors, viewer, startPos, numberOfItems * 256) : GetDataDescriptors(viewer, startPos));
		return startPos - dataOffset;
		long GetDataDescriptors(IMemoryReader reader, long offset)
		{
			for (int i = 0; i < numberOfItems; i++)
			{
				AllValid[i] = true;
				FieldOffset[i] = TotalDataSize;
				DataDescriptor dataDescriptor = reader.LoadRawFileObjectExt<DataDescriptor>(fileRevision, ref offset);
				Add(dataDescriptor);
				TotalDataSize += dataDescriptor.ItemSize;
			}
			return offset;
		}
	}

	/// <summary>
	/// add an item.
	/// </summary>
	/// <param name="item">
	/// The item.
	/// </param>
	public void AddItem(DataDescriptor item)
	{
		FieldOffset[base.Count] = TotalDataSize;
		AllValid[base.Count] = true;
		Add(item);
		TotalDataSize += item.ItemSize;
	}

	/// <summary>
	/// Calculate the buffer size. (The size of a record).
	/// </summary>
	/// <returns>
	/// The size of one record.
	/// </returns>
	public int CalcBufferSize()
	{
		uint num = 0u;
		for (int i = 0; i < base.Count; i++)
		{
			num += base[i].ItemSize;
		}
		return (int)num;
	}

	/// <summary>
	/// convert data entry to byte array.
	/// </summary>
	/// <param name="data">The log entries. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of log entries do not match the header definitions</exception>
	public byte[] ConvertDataEntryToByteArray(object[] data)
	{
		ConvertDataEntryToByteArray(data, out var buffer);
		return buffer;
	}

	/// <summary>
	///  convert data entry to byte array.
	/// </summary>
	/// <param name="data">The log entries. </param>
	/// <param name="buffer">The buffer for storing the log entries in byte array.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The number of log entries do not match the header definitions</exception>
	public void ConvertDataEntryToByteArray(object[] data, out byte[] buffer)
	{
		int num = ((data != null) ? data.Length : 0);
		if (num != base.Count)
		{
			throw new ArgumentOutOfRangeException("data", "The number of log entries do not match the header definitions");
		}
		if (data == null || base.Count == 0)
		{
			buffer = Array.Empty<byte>();
			return;
		}
		buffer = new byte[TotalDataSize];
		for (int i = 0; i < num; i++)
		{
			DataTypes dataType = base[i].DataType;
			int num2 = -1;
			double num3 = -1.0;
			if (dataType == DataTypes.Short || dataType == DataTypes.UnsignedShort || dataType == DataTypes.Long || dataType == DataTypes.UnsignedLong || dataType == DataTypes.UnsignedChar || dataType == DataTypes.Char)
			{
				try
				{
					num2 = Convert.ToInt32(data[i]);
				}
				catch
				{
				}
			}
			if (dataType == DataTypes.Float || dataType == DataTypes.Double)
			{
				try
				{
					num3 = Convert.ToDouble(data[i]);
				}
				catch
				{
				}
			}
			switch (dataType)
			{
			case DataTypes.Char:
				buffer[FieldOffset[i]] = Convert.ToByte(data[i]);
				break;
			case DataTypes.TrueFalse:
			case DataTypes.YesNo:
			case DataTypes.OnOff:
				buffer[FieldOffset[i]] = Convert.ToByte(data[i]);
				break;
			case DataTypes.UnsignedChar:
				buffer[FieldOffset[i]] = Convert.ToByte(num2 & 0xFF);
				break;
			case DataTypes.Short:
				Buffer.BlockCopy(BitConverter.GetBytes((short)num2), 0, buffer, (int)FieldOffset[i], 2);
				break;
			case DataTypes.UnsignedShort:
				Buffer.BlockCopy(BitConverter.GetBytes((ushort)num2), 0, buffer, (int)FieldOffset[i], 2);
				break;
			case DataTypes.Long:
				Buffer.BlockCopy(BitConverter.GetBytes(num2), 0, buffer, (int)FieldOffset[i], 4);
				break;
			case DataTypes.UnsignedLong:
				Buffer.BlockCopy(BitConverter.GetBytes((uint)num2), 0, buffer, (int)FieldOffset[i], 4);
				break;
			case DataTypes.Float:
				Buffer.BlockCopy(BitConverter.GetBytes((float)num3), 0, buffer, (int)FieldOffset[i], 4);
				break;
			case DataTypes.Double:
				Buffer.BlockCopy(BitConverter.GetBytes(num3), 0, buffer, (int)FieldOffset[i], 8);
				break;
			case DataTypes.CharString:
			{
				string text = (string)data[i];
				int count = (int)Math.Min(text.Length, base[i].ItemSize);
				Buffer.BlockCopy(Encoding.ASCII.GetBytes(text), 0, buffer, (int)FieldOffset[i], count);
				break;
			}
			case DataTypes.WideCharString:
			{
				string text = (string)data[i];
				int count = (int)Math.Min(text.Length * 2, base[i].ItemSize);
				Buffer.BlockCopy(Encoding.Unicode.GetBytes(text), 0, buffer, (int)FieldOffset[i], count);
				break;
			}
			}
		}
	}
}
