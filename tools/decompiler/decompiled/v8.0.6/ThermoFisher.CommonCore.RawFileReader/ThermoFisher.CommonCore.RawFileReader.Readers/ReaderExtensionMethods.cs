using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// The reader extension methods.
/// </summary>
internal static class ReaderExtensionMethods
{
	/// <summary>
	/// read a byte, updating the position.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Byte" />.
	/// </returns>
	public static byte ReadByteExt(this IMemoryReader viewer, ref long startPos)
	{
		byte result = viewer.ReadByte(startPos);
		startPos++;
		return result;
	}

	/// <summary>
	/// read a short, updating position.
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int16" />.
	/// </returns>
	public static short ReadShortExt(this IMemoryReader viewer, ref long startPos)
	{
		short result = viewer.ReadShort(startPos);
		startPos += 2L;
		return result;
	}

	/// <summary>
	/// read double, updating position.
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Double" />.
	/// </returns>
	public static double ReadDoubleExt(this IMemoryReader viewer, ref long startPos)
	{
		double result = viewer.ReadDouble(startPos);
		startPos += 8L;
		return result;
	}

	/// <summary>
	/// read an integer, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	public static int ReadIntExt(this IMemoryReader viewer, ref long startPos)
	{
		int result = viewer.ReadInt(startPos);
		startPos += 4L;
		return result;
	}

	/// <summary>
	/// read a float from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Single" />.
	/// </returns>
	public static float ReadFloatExt(this IMemoryReader viewer, ref long startPos)
	{
		float result = viewer.ReadFloat(startPos);
		startPos += 4L;
		return result;
	}

	/// <summary>
	/// read an unsigned short from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.UInt16" />.
	/// </returns>
	public static ushort ReadUnsignedShortExt(this IMemoryReader viewer, ref long startPos)
	{
		ushort result = viewer.ReadUnsignedShort(startPos);
		startPos += 2L;
		return result;
	}

	/// <summary>
	/// read an unsigned integer from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.UInt32" />.
	/// </returns>
	public static uint ReadUnsignedIntExt(this IMemoryReader viewer, ref long startPos)
	{
		uint result = viewer.ReadUnsignedInt(startPos);
		startPos += 4L;
		return result;
	}

	/// <summary>
	/// read a string from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	public static string ReadStringExt(this IMemoryReader viewer, ref long startPos)
	{
		long numOfBytesRead;
		string result = viewer.ReadString(startPos, out numOfBytesRead);
		startPos += numOfBytesRead;
		return result;
	}

	/// <summary>
	/// Read a number of wide characters, and convert to string.
	/// If there is a '0' in the string, the string length will be adjusted.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="startPos">Offset into the view.</param>
	/// <returns>Converted string</returns>
	public static string ReadWideCharsExt(this IMemoryReader viewer, ref long startPos)
	{
		long numOfBytesRead;
		string result = viewer.ReadWideChars(startPos, out numOfBytesRead);
		startPos += numOfBytesRead;
		return result;
	}

	/// <summary>
	/// Read a structure ext, updating the position
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <typeparam name="T">Type of struct
	/// </typeparam>
	/// <returns>
	/// The struct which was read
	/// </returns>
	public static T ReadStructureExt<T>(this IMemoryReader viewer, ref long startPos) where T : struct
	{
		long numberOfBytesRead;
		T result = viewer.ReadStructure<T>(startPos, out numberOfBytesRead);
		startPos += numberOfBytesRead;
		return result;
	}

	/// <summary>
	/// Reads the previous revision and convert.
	/// </summary>
	/// <typeparam name="T">Current Type</typeparam>
	/// <typeparam name="TPrev">Previous Type</typeparam>
	/// <param name="viewer">The viewer.</param>
	/// <param name="startPos">The start position.</param>
	/// <returns>The object which was read</returns>
	public static T ReadPreviousRevisionAndConvertExt<T, TPrev>(this IMemoryReader viewer, ref long startPos) where T : struct where TPrev : struct
	{
		int num = Marshal.SizeOf(typeof(TPrev));
		T result = viewer.ReadPreviousRevisionAndConvert<T>(startPos, num);
		startPos += num;
		return result;
	}

	/// <summary>
	/// read an array of bytes from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <param name="count">The count of bytes to read</param>
	/// <returns>
	/// The array
	/// </returns>
	public static byte[] ReadBytesExt(this IMemoryReader viewer, ref long startPos, int count)
	{
		byte[] result = viewer.ReadBytes(startPos, count);
		startPos += count;
		return result;
	}

	/// <summary>
	/// read an array of bytes from a view, expecting a large data array.
	/// This uses 64 bit items, to cut down Marshalling overheads
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <param name="count">The count of bytes to read</param>
	/// <returns>
	/// The array
	/// </returns>
	public static byte[] ReadLargeData(this IMemoryReader viewer, ref long startPos, int count)
	{
		byte[] result = viewer.ReadLargeData(startPos, count);
		startPos += count;
		return result;
	}

	/// <summary>
	/// read an array of integer from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// Data stored in integer array
	/// </returns>
	public static int[] ReadIntsExt(this IMemoryReader viewer, ref long startPos)
	{
		int num = viewer.ReadInt(startPos);
		startPos += 4L;
		if (num == 0)
		{
			return Array.Empty<int>();
		}
		int[] result = viewer.ReadInts(startPos, num);
		startPos += num * 4;
		return result;
	}

	/// <summary>
	/// read an array of unsigned integers from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// Data stored in unsigned integer array
	/// </returns>
	public static uint[] ReadUnsignedIntsExt(this IMemoryReader viewer, ref long startPos)
	{
		int num = viewer.ReadInt(startPos);
		startPos += 4L;
		if (num == 0)
		{
			return Array.Empty<uint>();
		}
		uint[] result = viewer.ReadUnsignedInts(startPos, num);
		startPos += num * 4;
		return result;
	}

	/// <summary>
	/// read an array of bytes from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <param name="count">
	/// The count of values to read.
	/// </param>
	/// <returns>
	/// Data stored unsigned integer array
	/// </returns>
	public static uint[] ReadUnsignedIntsExt(this IMemoryReader viewer, ref long startPos, int count)
	{
		uint[] result = viewer.ReadUnsignedInts(startPos, count);
		startPos += count * 4;
		return result;
	}

	/// <summary>
	/// Reads the doubles ext.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="startPos">The start position.</param>
	/// <returns>Data stored in double array</returns>
	public static double[] ReadDoublesExt(this IMemoryReader viewer, ref long startPos)
	{
		int num = viewer.ReadInt(startPos);
		startPos += 4L;
		if (num == 0)
		{
			return Array.Empty<double>();
		}
		double[] result = viewer.ReadDoubles(startPos, num);
		startPos += num * 8;
		return result;
	}

	/// <summary>
	/// Read a set of floats, and return as an array.
	/// </summary>
	/// <param name="viewer">
	/// The viewer. (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <param name="count">
	/// The count of objects to load.
	/// </param>
	/// <returns>
	/// The array
	/// </returns>
	public static float[] ReadFloatsExt(this IMemoryReader viewer, ref long startPos, int count)
	{
		float[] result = viewer.ReadFloats(startPos, count);
		startPos += count * 4;
		return result;
	}

	/// <summary>
	/// read an list of strings from a view, updating the start position
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The strings
	/// </returns>
	public static string[] ReadStringsExt(this IMemoryReader viewer, ref long startPos)
	{
		long numOfBytesRead;
		string[] result = viewer.ReadStrings(startPos, out numOfBytesRead);
		startPos += numOfBytesRead;
		return result;
	}

	/// <summary>
	/// Read struct array, updating the position.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <typeparam name="T">Type of array element
	/// </typeparam>
	/// <returns>
	/// The array
	/// </returns>
	public static T[] ReadStructArrayExt<T>(this IMemoryReader viewer, ref long startPos) where T : struct
	{
		long numberOfBytesRead;
		T[] result = viewer.ReadStructArray<T>(startPos, out numberOfBytesRead);
		startPos += numberOfBytesRead;
		return result;
	}

	/// <summary>
	/// load a raw file object, updating the start position.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="func">
	/// The function to construct the type.
	/// </param>
	/// <param name="fileVersion">
	/// The file version.
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <typeparam name="T">Type to load
	/// </typeparam>
	/// <returns>
	/// The object loaded
	/// </returns>
	public static T LoadRawFileObjectExt<T>(this IMemoryReader viewer, Func<T> func, int fileVersion, ref long startPos) where T : IRawObjectBase
	{
		T result = func();
		long num = result.Load(viewer, startPos, fileVersion);
		startPos += num;
		return result;
	}

	/// <summary>
	/// load a raw file object array.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="fileVersion">
	/// The file version.
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <typeparam name="T">Type of data in array
	/// </typeparam>
	/// <returns>
	/// The array of objects
	/// </returns>
	public static T[] LoadRawFileObjectArray<T>(this IMemoryReader viewer, int fileVersion, ref long startPos) where T : IRawObjectBase, new()
	{
		int num = viewer.ReadIntExt(ref startPos);
		T[] array = new T[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = viewer.LoadRawFileObjectExt(() => new T(), fileVersion, ref startPos);
		}
		return array;
	}

	/// <summary>
	/// load raw file object extended.
	/// </summary>
	/// <param name="viewer">
	/// The viewer. (memory map)
	/// </param>
	/// <param name="fileVersion">
	/// The file version.
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <typeparam name="T">Type of object to read
	/// </typeparam>
	/// <returns>
	/// The object
	/// </returns>
	public static T LoadRawFileObjectExt<T>(this IMemoryReader viewer, int fileVersion, ref long startPos) where T : IRawObjectBase, new()
	{
		T result = new T();
		long num = result.Load(viewer, startPos, fileVersion);
		startPos += num;
		return result;
	}
}
