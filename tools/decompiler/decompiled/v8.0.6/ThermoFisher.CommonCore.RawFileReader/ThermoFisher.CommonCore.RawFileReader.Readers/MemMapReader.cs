using System;
using System.Collections;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// The memory map reader.
/// </summary>
internal class MemMapReader : IDisposableReader, IMemoryReader, IDisposable
{
	private bool _disposed;

	/// <summary>
	/// Gets the accessor.
	/// </summary>
	protected MemoryMappedViewAccessor Accessor { get; }

	/// <summary>
	/// Gets or sets the memory mapped file.
	/// </summary>
	private MemoryMappedRawFile MemoryMappedFile { get; set; }

	/// <summary>
	/// Gets the initial offset.
	/// </summary>
	public long InitialOffset { get; }

	/// <summary>
	/// Gets the size of view.
	/// </summary>
	/// <value>
	/// The size of view.
	/// </value>
	public long Length { get; }

	/// <summary>
	/// Gets the stream id.
	/// </summary>
	public string StreamId { get; }

	/// <summary>
	/// Gets a value indicating whether large reads are needed for performance.
	/// This (lagacy) reader does not require large reads.
	/// memory mapping mode is left "untouched" for full backwards comaptibilty with
	/// apps that do not wish to use alterante reading modes.
	/// In general, small object reads from this are efficient.
	/// There could be greater efficiency in some cases by readign larger blocks, but:
	/// Mmemory mapping already creates a large working set, so adding extra memmoy buffering
	/// would risk inceasing memory consumption for limited benefits.
	/// </summary>
	public bool PreferLargeReads => false;

	/// <summary>
	/// Gets a value indicating whether sub-views can be created.
	/// This (lagacy) reader does not support this feature.
	/// memory mapping mode is left "untouched" for full backwards comaptibilty with
	/// apps that do not wish to use alterante reading modes.
	/// </summary>
	public bool SupportsSubViews => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemMapReader" /> class.
	/// </summary>
	/// <param name="mmf">
	/// The memory Mapped File.
	/// </param>
	/// <param name="streamId">
	/// The stream id.
	/// </param>
	/// <param name="viewAccessor">
	/// The view accessor.
	/// </param>
	/// <param name="initialOffset">
	/// The initial offset.
	/// </param>
	/// <param name="sizeOfView">
	/// The size of view.
	/// </param>
	public MemMapReader(MemoryMappedRawFile mmf, string streamId, MemoryMappedViewAccessor viewAccessor, long initialOffset, long sizeOfView)
	{
		MemoryMappedFile = mmf;
		Accessor = viewAccessor;
		StreamId = streamId;
		InitialOffset = initialOffset;
		Length = sizeOfView;
	}

	/// <summary>
	/// Reads the structure array.
	/// </summary>
	/// <typeparam name="T">Type of structure</typeparam>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="numberOfBytesRead">The number of bytes read.</param>
	/// <returns>Array of structures</returns>
	public T[] ReadStructArray<T>(long offset, out long numberOfBytesRead) where T : struct
	{
		long num = offset;
		int num2 = Accessor.ReadInt32(offset);
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
		return Accessor.ReadByte(offset);
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
		Accessor.ReadArray(offset, array, 0, count);
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
		return Accessor.ReadDouble(offset);
	}

	/// <summary>
	/// Reads an array of double.
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">The count.</param>
	/// <returns>The array of doubles</returns>
	public double[] ReadDoubles(long offset, int count)
	{
		double[] array = new double[count];
		switch (count)
		{
		case 1:
		case 2:
		case 3:
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = Accessor.ReadDouble(offset);
				offset += 8;
			}
			break;
		}
		default:
			Accessor.ReadArray(offset, array, 0, count);
			break;
		case 0:
			break;
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
		return Accessor.ReadSingle(offset);
	}

	/// <summary>
	/// Reads floats, as an array
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">The count.</param>
	/// <returns>The array of floats</returns>
	public float[] ReadFloats(long offset, int count)
	{
		float[] array = new float[count];
		switch (count)
		{
		case 1:
		case 2:
		case 3:
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = Accessor.ReadSingle(offset);
				offset += 4;
			}
			break;
		}
		default:
			Accessor.ReadArray(offset, array, 0, count);
			break;
		case 0:
			break;
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
		return Accessor.ReadInt32(offset);
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
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = Accessor.ReadInt32(offset);
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
		ulong[] array = new ulong[count];
		Accessor.ReadArray(offset, array, 0, count);
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
		uint[] array = new uint[count];
		switch (count)
		{
		case 1:
		case 2:
		case 3:
		{
			for (int i = 0; i < count; i++)
			{
				array[i] = Accessor.ReadUInt32(offset);
				offset += 4;
			}
			break;
		}
		default:
			Accessor.ReadArray(offset, array, 0, count);
			break;
		case 0:
			break;
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
		return Accessor.ReadInt16(offset);
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
		uint stringSize = (uint)Accessor.ReadInt32(offset);
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
		uint stringSize = (uint)Accessor.ReadInt32(offset);
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
		Accessor.ReadArray(offset, array, 0, (int)stringSize);
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
		int num2 = Accessor.ReadInt32(num);
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
		Accessor.Read<T>(offset, out var structure);
		return structure;
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
		Accessor.ReadArray(offset, array, 0, count);
		return array;
	}

	/// <summary>
	/// read an array of bytes from a view, expecting a large data array.
	/// This uses 64 bit items, to cut down Marshalling overheads
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
		if (count >= 8)
		{
			int num = count / 8;
			int num2 = num * 8;
			ulong[] src = ReadUnsignedLongInts(startPos, num);
			byte[] array = new byte[count];
			Buffer.BlockCopy(src, 0, array, 0, num2);
			if (num2 < count)
			{
				int count2 = count - num2;
				Buffer.BlockCopy(ReadBytes(num2 + startPos, count2), 0, array, num2, count2);
			}
			return array;
		}
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
		Accessor.ReadArray(offset, array, 0, (int)numOfBytesRead);
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
		Accessor.ReadArray(offset, array, 0, numOfBytesRead);
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
		return Accessor.ReadUInt32(offset);
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
		return Accessor.ReadUInt16(offset);
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

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Accessor?.Dispose();
			if (MemoryMappedFile != null)
			{
				MemoryMappedFile.DecrementRefCount();
				MemoryMappedFile = null;
			}
		}
	}

	public IReadWriteAccessor CreateSubView(long dataOffset, long blobSize)
	{
		throw new NotImplementedException();
	}
}
