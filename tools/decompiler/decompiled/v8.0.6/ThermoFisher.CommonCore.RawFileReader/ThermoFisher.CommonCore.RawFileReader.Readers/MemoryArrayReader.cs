using System;
using System.Collections;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// The MemoryArrayReader implements the IMemoryReader against a byte array.
/// Many operations become simple BitConverter calls
/// Some features of IMemMapReader, which imply file specic actions are 
/// redundant.
/// </summary>
public class MemoryArrayReader : IMemoryReader
{
	/// <summary>
	/// Gets the initial offset.
	/// (Offset into parent view)
	/// </summary>
	public long InitialOffset { get; }

	/// <summary>
	/// The data to be decoded
	/// </summary>
	protected internal byte[] Data { get; set; }

	/// <summary>
	/// Gets the stream id.
	/// </summary>
	public string StreamId { get; protected init; }

	/// <inheritdoc />
	public long Length => Data.Length;

	/// <summary>
	/// Determines if this needs large block reads for ecciciency.
	/// This reader can handle small data reads OK.
	/// </summary>
	public bool PreferLargeReads => false;

	/// <inheritdoc />
	public bool SupportsSubViews => true;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemoryArrayReader" /> class.
	/// </summary>
	/// <param name="data">data which needs to get decoded by this reader</param>
	/// <param name="initialOffset">the offset of this block into parent view.
	/// This offset is then subtracted from all passed in addresses, so that calling code can
	/// continue to address data based on te parent view</param>
	public MemoryArrayReader(byte[] data, long initialOffset = 0L)
	{
		Data = data;
		InitialOffset = initialOffset;
	}

	/// <summary>
	/// Reads an array of structures.
	/// </summary>
	/// <typeparam name="T">Type of structure</typeparam>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="numberOfBytesRead">The number of bytes read.</param>
	/// <returns>Array of structures</returns>
	public T[] ReadStructArray<T>(long offset, out long numberOfBytesRead) where T : struct
	{
		long num = offset - InitialOffset;
		int num2 = BitConverter.ToInt32(Data, (int)offset);
		T[] array = new T[num2];
		num += 4;
		for (int i = 0; i < num2; i++)
		{
			array[i] = ReadStructure<T>(num, out var numOfBytesRead);
			num += numOfBytesRead;
		}
		numberOfBytesRead = num - offset;
		return array;
	}

	/// <summary>
	/// The method reads a byte.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Byte" />.
	/// </returns>
	public byte ReadByte(long offset)
	{
		return Data[(int)(offset - InitialOffset)];
	}

	/// <summary>
	/// The method reads an array of bytes.
	/// </summary>
	/// <param name="offset">Offset from start of memory map</param>
	/// <param name="count">The count.</param>
	/// <returns>
	/// The byte array.
	/// </returns>
	public byte[] ReadBytes(long offset, int count)
	{
		byte[] array = new byte[count];
		Buffer.BlockCopy(Data, (int)(offset - InitialOffset), array, 0, count);
		return array;
	}

	/// <summary>
	/// The method reads a double.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Double" />.
	/// </returns>
	public double ReadDouble(long offset)
	{
		return BitConverter.ToDouble(Data, (int)(offset - InitialOffset));
	}

	/// <summary>
	/// Reads an array of double.
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">The count.</param>
	/// <returns>The array of doubles</returns>
	public double[] ReadDoubles(long offset, int count)
	{
		offset -= InitialOffset;
		double[] array = new double[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = BitConverter.ToDouble(Data, (int)offset);
			offset += 8;
		}
		return array;
	}

	/// <summary>
	/// The method reads a float.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Single" />.
	/// </returns>
	public float ReadFloat(long offset)
	{
		return BitConverter.ToSingle(Data, (int)(offset - InitialOffset));
	}

	/// <summary>
	/// Reads floats, as an array
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">The count.</param>
	/// <returns>The array of floats</returns>
	public float[] ReadFloats(long offset, int count)
	{
		offset -= InitialOffset;
		float[] array = new float[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = BitConverter.ToSingle(Data, (int)offset);
			offset += 4;
		}
		return array;
	}

	/// <summary>
	/// The method reads an integer.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	public int ReadInt(long offset)
	{
		return BitConverter.ToInt32(Data, (int)(offset - InitialOffset));
	}

	/// <summary>
	/// Reads the integers, where count is typically small
	/// </summary>
	/// <param name="offset">The offset.</param>
	/// <param name="count">The count.</param>
	/// <returns>
	/// The array of integers
	/// </returns>
	public int[] ReadInts(long offset, int count)
	{
		offset -= InitialOffset;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = BitConverter.ToInt32(Data, (int)offset);
			offset += 4;
		}
		return array;
	}

	/// <summary>
	/// read unsigned long integers, as an array.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <param name="count">
	/// The count.
	/// </param>
	/// <returns>
	/// The array of unsigned long integers.
	/// </returns>
	public ulong[] ReadUnsignedLongInts(long offset, int count)
	{
		offset -= InitialOffset;
		ulong[] array = new ulong[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = BitConverter.ToUInt64(Data, (int)offset);
			offset += 8;
		}
		return array;
	}

	/// <summary>
	/// read unsigned integers.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="count">
	/// The count.
	/// </param>
	/// <returns>
	/// The array
	/// </returns>
	public uint[] ReadUnsignedInts(long offset, int count)
	{
		offset -= InitialOffset;
		uint[] array = new uint[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = BitConverter.ToUInt32(Data, (int)offset);
			offset += 4;
		}
		return array;
	}

	/// <summary>
	/// Read a previous revision of a type and convert to latest.
	/// Previous struct is assumed to be at the start of the latest struct
	/// using sequential layout.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="sizeOfPreviousRevStruct">
	/// The size of previous rev struct.
	/// </param>
	/// <typeparam name="T">Type to return
	/// </typeparam>
	/// <returns>
	/// The converted object.
	/// </returns>
	public T ReadPreviousRevisionAndConvert<T>(long offset, int sizeOfPreviousRevStruct) where T : struct
	{
		offset -= InitialOffset;
		int num = Marshal.SizeOf(typeof(T));
		byte[] src = ReadBytes(offset, sizeOfPreviousRevStruct);
		byte[] array = new byte[num];
		Buffer.BlockCopy(src, 0, array, 0, sizeOfPreviousRevStruct);
		return ConvertBytesToStructure<T>(array);
	}

	/// <summary>
	/// Reads the short.
	/// </summary>
	/// <param name="offset">The offset into the memory map.</param>
	/// <returns>The short</returns>
	public short ReadShort(long offset)
	{
		return BitConverter.ToInt16(Data, (int)(offset - InitialOffset));
	}

	/// <summary>
	/// read string.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="numOfBytesRead">
	/// The number of bytes read.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	public string ReadString(long offset, out long numOfBytesRead)
	{
		offset -= InitialOffset;
		uint stringSize = (uint)BitConverter.ToInt32(Data, (int)offset);
		offset += (numOfBytesRead = 4L);
		return new string(ReadWideCharArray(offset, ref numOfBytesRead, stringSize));
	}

	/// <summary>
	/// Read a number of wide characters, and convert to string.
	/// If there is a '0' in the string, the string length will be adjusted.
	/// </summary>
	/// <param name="offset">The offset into the view.</param>
	/// <param name="numOfBytesRead">The number of bytes read.</param>
	/// <returns> The converted string </returns>
	public string ReadWideChars(long offset, out long numOfBytesRead)
	{
		uint stringSize = BitConverter.ToUInt32(Data, (int)(offset - InitialOffset));
		offset += (numOfBytesRead = 4L);
		return ReadWideChars(offset, ref numOfBytesRead, stringSize);
	}

	/// <summary>
	/// Read a number of wide characters, and convert to string.
	/// If there is a '0' in the string, the string length will be adjusted.
	/// </summary>
	/// <param name="offset">Offset into the view</param>
	/// <param name="numOfBytesRead">Number of bytes read by this call (added to input value)</param>
	/// <param name="stringSize">Number of 2 byte chars</param>
	/// <returns>converted string</returns>
	public string ReadWideChars(long offset, ref long numOfBytesRead, uint stringSize)
	{
		offset -= InitialOffset;
		char[] array = ReadWideCharArray(offset, ref numOfBytesRead, stringSize);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '\0')
			{
				if (i == 0)
				{
					return string.Empty;
				}
				return new string(array, 0, i);
			}
		}
		return new string(array);
	}

	/// <summary>
	/// read an array of wide (2 byte) char.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="numOfBytesRead">
	/// The number of bytes read.
	/// </param>
	/// <param name="stringSize">
	/// The string size.
	/// </param>
	/// <returns>
	/// The characters
	/// </returns>
	private char[] ReadWideCharArray(long offset, ref long numOfBytesRead, uint stringSize)
	{
		if (stringSize == 0)
		{
			return Array.Empty<char>();
		}
		char[] array = new char[stringSize];
		for (int i = 0; i < stringSize; i++)
		{
			array[i] = BitConverter.ToChar(Data, (int)offset);
			offset += 2;
		}
		numOfBytesRead += stringSize * 2;
		return array;
	}

	/// <summary>
	/// read a list of strings
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="numBytesRead">
	/// The number bytes read.
	/// </param>
	/// <returns>
	/// The strings
	/// </returns>
	public string[] ReadStrings(long offset, out long numBytesRead)
	{
		long num = offset;
		int num2 = BitConverter.ToInt32(Data, (int)(num - InitialOffset));
		num += 4;
		string[] array = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			array[i] = ReadString(num, out var numOfBytesRead);
			num += numOfBytesRead;
		}
		numBytesRead = num - offset;
		return array;
	}

	/// <summary>
	/// Read a simple structure.
	/// This must be a fixed size struct, with no embedded fixed arrays,
	/// ref types or any other marshaled items.
	/// Example: struct containing 2 doubles. 
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <typeparam name="T">type of struct
	/// </typeparam>
	/// <returns>
	/// The structure
	/// </returns>
	public T ReadSimpleStructure<T>(long offset) where T : struct
	{
		long numOfBytesRead;
		return ReadStructure<T>(offset, out numOfBytesRead);
	}

	/// <summary>
	/// read simple structure array.
	/// This must be a fixed size struct, with no embedded fixed arrays,
	/// ref types or any other marshaled items.
	/// Example: struct containing 2 doubles.
	/// </summary>
	/// <param name="offset">
	/// The offset into the memory map.
	/// </param>
	/// <param name="count">
	/// The count (array length).
	/// </param>
	/// <typeparam name="T">type of struct
	/// </typeparam>
	/// <returns>
	/// The struct array.
	/// </returns>
	public T[] ReadSimpleStructureArray<T>(long offset, int count) where T : struct
	{
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = ReadStructure<T>(offset, out var numOfBytesRead);
			offset += (int)numOfBytesRead;
		}
		return array;
	}

	/// <summary>
	/// read an array of bytes from a view, expecting a large data array.
	/// </summary>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <param name="count">The count of bytes to read</param>
	/// <returns>
	/// The array
	/// </returns>
	public byte[] ReadLargeData(long startPos, int count)
	{
		return ReadBytes(startPos, count);
	}

	/// <summary>
	/// read structure.
	/// </summary>
	/// <param name="offset">
	/// The offset into the memory map.
	/// </param>
	/// <param name="numOfBytesRead">
	/// The number of bytes read.
	/// </param>
	/// <typeparam name="T">type of structure
	/// </typeparam>
	/// <returns>
	/// The structure
	/// </returns>
	public T ReadStructure<T>(long offset, out long numOfBytesRead) where T : struct
	{
		numOfBytesRead = Marshal.SizeOf(typeof(T));
		byte[] array = new byte[numOfBytesRead];
		Buffer.BlockCopy(Data, (int)(offset - InitialOffset), array, 0, (int)numOfBytesRead);
		return ConvertBytesToStructure<T>(array);
	}

	/// <summary>
	/// read a structure.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="numOfBytesRead">
	/// The number of bytes read.
	/// </param>
	/// <typeparam name="T">Type to read
	/// </typeparam>
	/// <returns>
	/// The object which was read
	/// </returns>
	public T ReadStructure<T>(long offset, int numOfBytesRead) where T : struct
	{
		byte[] array = new byte[numOfBytesRead];
		Buffer.BlockCopy(Data, (int)(offset - InitialOffset), array, 0, numOfBytesRead);
		return ConvertBytesToStructure<T>(array);
	}

	/// <summary>
	/// read unsigned integer.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.UInt32" />.
	/// </returns>
	public uint ReadUnsignedInt(long offset)
	{
		return BitConverter.ToUInt32(Data, (int)(offset - InitialOffset));
	}

	/// <summary>
	/// read unsigned short.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.UInt16" />.
	/// </returns>
	public ushort ReadUnsignedShort(long offset)
	{
		return BitConverter.ToUInt16(Data, (int)(offset - InitialOffset));
	}

	/// <summary>
	/// The method converts an <see cref="T:System.Collections.IEnumerable" /> object to a structure.
	/// </summary>
	/// <param name="bytes">
	/// The bytes array.
	/// </param>
	/// <typeparam name="T">
	/// Structure to convert to.
	/// </typeparam>
	/// <returns>
	/// The structure.
	/// </returns>
	internal static T ConvertBytesToStructure<T>(IEnumerable bytes) where T : struct
	{
		GCHandle gCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
		gCHandle.Free();
		return result;
	}

	/// <inheritdoc />
	public IReadWriteAccessor CreateSubView(long dataOffset, long blobSize)
	{
		return new MemoryArrayAccessor(Data, InitialOffset + dataOffset);
	}
}
