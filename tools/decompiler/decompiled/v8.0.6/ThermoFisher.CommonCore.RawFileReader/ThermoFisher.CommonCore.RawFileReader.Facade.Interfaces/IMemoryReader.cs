using System;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// Defines methods to decode data from any data source which has a concept of an offset and bytes
/// The source should support parallel calls (be stateless).
/// </summary>
public interface IMemoryReader
{
	/// <summary>
	/// Determines if this needs large block reads for efficiency.
	/// </summary>
	bool PreferLargeReads { get; }

	/// <summary>
	/// Gets the initial offset, for address translation.
	/// Some objects hold addesses relative to a larger view or file.
	/// This offset is used for address translation to a sub-view.
	/// </summary>
	long InitialOffset { get; }

	/// <summary>
	/// Gets the length of the memory
	/// </summary>
	/// <value>
	/// The length
	/// </value>
	long Length { get; }

	/// <summary>
	/// Gets the stream id.
	/// Optional.
	/// </summary>
	string StreamId => string.Empty;

	/// <summary>
	/// Gets a value indicating whether sub-views can be made from this reader.
	/// Sub-views are made by calling CreateSubView
	/// A sub-view can be used to rebase the address "0" to the start of an embedded stream.
	/// A sub-view is typically created as an in memory byte array, to avoid small reads
	/// causing too many IO operations.
	/// A sub-view may also be backed by the initial view, just applying an offset.
	/// When a sub-view is not available, calling code will need to create a new viewer for the data or stream.
	/// </summary>
	bool SupportsSubViews { get; }

	/// <summary>
	/// Reads an array of structures.
	/// </summary>
	/// <typeparam name="T">Type of structure</typeparam>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="numberOfBytesRead">The number of bytes read.</param>
	/// <returns>Array of structures</returns>
	T[] ReadStructArray<T>(long offset, out long numberOfBytesRead) where T : struct;

	/// <summary>
	/// The method reads a byte.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Byte" />.
	/// </returns>
	byte ReadByte(long offset);

	/// <summary>
	/// The method reads an array of bytes.
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">
	/// The count.
	/// </param>
	/// <returns>
	/// The byte array.
	/// </returns>
	byte[] ReadBytes(long offset, int count);

	/// <summary>
	/// The method reads an array of bytes, from a buffer pool
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">
	/// The count.
	/// </param>
	/// <param name="pool">Buffer pool, which reduces garbage collection</param>
	/// <returns>
	/// The byte array.
	/// </returns>
	byte[] RentBytes(long offset, int count, IBufferPool pool)
	{
		return ReadBytes(offset, count);
	}

	/// <summary>
	/// Returns rented data to a buffer pool.
	/// Once bytes are returned, the array must not be used again as the memory will be reused
	/// </summary>
	/// <param name="rented"></param>
	/// <param name="pool">Buffer pool, which reduces garbage collection</param>
	void ReturnRentedBytes(byte[] rented, IBufferPool pool)
	{
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
	double ReadDouble(long offset);

	/// <summary>
	/// Reads an array of double.
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">The count.</param>
	/// <returns>The array of doubles</returns>
	double[] ReadDoubles(long offset, int count);

	/// <summary>
	/// The method reads a float.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Single" />.
	/// </returns>
	float ReadFloat(long offset);

	/// <summary>
	/// Reads floats, as an array
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">The count.</param>
	/// <returns>The array of floats</returns>
	float[] ReadFloats(long offset, int count);

	/// <summary>
	/// Gets a sub view of a view.
	/// </summary>
	/// <param name="dataOffset">offset into this view</param>
	/// <param name="blobSize">size of sub view</param>
	/// <returns>a viewer for the data, or null if unsupported (too large?)</returns>
	IReadWriteAccessor CreateSubView(long dataOffset, long blobSize);

	/// <summary>
	/// The method reads an integer.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	int ReadInt(long offset);

	/// <summary>
	/// Reads the integers.
	/// </summary>
	/// <param name="offset">The offset.</param>
	/// <param name="count">The count.</param>
	/// <returns>The array of integers</returns>
	int[] ReadInts(long offset, int count);

	/// <summary>
	/// The method reads an array of unsigned integers.
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="count">
	/// The number of unsigned integers to read.
	/// </param>
	/// <returns>
	/// The array of unsigned integers.
	/// </returns>
	uint[] ReadUnsignedInts(long offset, int count);

	/// <summary>
	/// The method reads a previous revision structure struct and convert it to the current target revision.
	/// </summary>
	/// <param name="offset">
	/// The position from the beginning of the file to start reading.
	/// </param>
	/// <param name="sizeOfPreviousRevStruct">
	/// The size of previous revision structure.
	/// </param>
	/// <typeparam name="T">
	/// The type of the target structure.
	/// </typeparam>
	/// <returns>
	/// The target structure.
	/// </returns>
	T ReadPreviousRevisionAndConvert<T>(long offset, int sizeOfPreviousRevStruct) where T : struct;

	/// <summary>
	/// Reads the short.
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <returns>The short</returns>
	short ReadShort(long offset);

	/// <summary>
	/// Reads the string.
	/// </summary>
	/// <param name="offset">The offset into the map.</param>
	/// <param name="numOfBytesRead">The number of bytes read.</param>
	/// <returns>The string</returns>
	string ReadString(long offset, out long numOfBytesRead);

	/// <summary>
	/// The method reads a collection of strings starting at the supplied offset
	/// </summary>
	/// <param name="offset">
	/// The position to start reading.
	/// </param>
	/// <param name="numOfBytesRead">The number of bytes read</param>
	/// <returns>
	/// The collection of strings.
	/// </returns>
	string[] ReadStrings(long offset, out long numOfBytesRead);

	/// <summary>
	/// This method reads a simple structure at the start position.
	/// Simple structure should have only basic fields <c>(int, float etc)</c>
	/// and no internally marshaled data (such as fixed size arrays, or ref types)
	/// </summary>
	/// <param name="offset">
	/// The start position.
	/// </param>
	/// <typeparam name="T">
	/// The structure to read.
	/// </typeparam>
	/// <returns>
	/// The structure.
	/// </returns>
	T ReadSimpleStructure<T>(long offset) where T : struct;

	/// <summary>
	/// This method reads a simple structure array at the start position.
	/// Simple structure should have only basic fields <c>(int, float etc)</c>
	/// and no internally marshaled data (such as fixed size arrays, or ref types)
	/// </summary>
	/// <param name="offset">
	/// The start position.
	/// </param>
	/// <param name="count">Number of array elements</param>
	/// <typeparam name="T">
	/// The structure to read.
	/// </typeparam>
	/// <returns>
	/// The structure.
	/// </returns>
	T[] ReadSimpleStructureArray<T>(long offset, int count) where T : struct;

	/// <summary>
	/// The over loaded method reads the structure at the start position,
	/// using the Marshal class to determine the size.
	/// </summary>
	/// <param name="offset">
	/// The start position.
	/// </param>
	/// <param name="numberOfBytesRead">The number of byte read</param>
	/// <typeparam name="T">
	/// The structure to read.
	/// </typeparam>
	/// <returns>
	/// The structure.
	/// </returns>
	T ReadStructure<T>(long offset, out long numberOfBytesRead) where T : struct;

	/// <summary>
	/// The over loaded method reads the structure at the start position,
	/// The structure size is passed in.
	/// </summary>
	/// <param name="offset">
	/// The start position.
	/// </param>
	/// <param name="numberOfBytesToRead">The number of bytes to read</param>
	/// <typeparam name="T">
	/// The structure to read.
	/// </typeparam>
	/// <returns>
	/// The structure.
	/// </returns>
	T ReadStructure<T>(long offset, int numberOfBytesToRead) where T : struct;

	/// <summary>
	/// The method reads an unsigned integer.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.UInt32" />.
	/// </returns>
	uint ReadUnsignedInt(long offset);

	/// <summary>
	/// The method reads an unsigned short.
	/// </summary>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.UInt16" />.
	/// </returns>
	ushort ReadUnsignedShort(long offset);

	/// <summary>
	/// Read a number of wide characters, and convert to string.
	/// If there is a '0' in the string, the string length will be adjusted.
	/// </summary>
	/// <param name="offset">The offset into the view.</param>
	/// <param name="numOfBytesRead">The number of bytes read.</param>
	/// <returns>The converted string</returns>
	string ReadWideChars(long offset, out long numOfBytesRead);

	/// <summary>
	/// Read a number of wide characters, and convert to string
	/// </summary>
	/// <param name="offset">Offset into the view</param>
	/// <param name="numOfBytesRead">Number of bytes read by this call (added to input value)</param>
	/// <param name="stringSize">Number of 2 byte chars</param>
	/// <returns>converted string</returns>
	string ReadWideChars(long offset, ref long numOfBytesRead, uint stringSize);

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
	byte[] ReadLargeData(long startPos, int count);

	/// <summary>
	/// Attempt to read an array of bytes from a view, expecting a large data array.
	/// This uses 64 bit items, to cut down Marshalling overheads.
	/// The array may be shorter if there are not enough bytes in this view.
	/// </summary>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <param name="count">The count of bytes to read</param>
	/// <returns>
	/// The array of data with count bytes from startPos, or all bytes to end of view if smaller.
	/// </returns>
	byte[] SafeReadLargeData(long startPos, int count)
	{
		if (startPos >= Length)
		{
			return Array.Empty<byte>();
		}
		if (startPos + count <= Length)
		{
			return ReadLargeData(startPos, count);
		}
		return ReadLargeData(startPos, (int)(Length - startPos));
	}
}
