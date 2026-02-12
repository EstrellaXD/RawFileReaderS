using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// This class is provides read/write access to an array of bytes
/// </summary>
public class MemoryArrayAccessor : MemoryArrayReader, IReadWriteAccessor, IMemMapWriter, IDisposable, IDisposableReader, IMemoryReader
{
	/// <summary>
	/// Gets a suggested minimum amount of memory to read and write
	/// </summary>
	public int SuggestedChunkSize => 0;

	/// <summary>
	/// Initializes a new instance of MemoryArrayAccessor
	/// </summary>
	/// <param name="data">Data to be read or written</param>
	/// <param name="initialOffset">Offset from parent view</param>
	/// <param name="streamId">The stream ID is not needed for a sub-viewer creation,
	/// but it's required for the main-viewer creation, ex. Random Access Reader </param>
	public MemoryArrayAccessor(byte[] data, long initialOffset = 0L, string streamId = null)
		: base(data, initialOffset)
	{
		base.StreamId = streamId;
	}

	/// <inheritdoc />
	public void Dispose()
	{
	}

	/// <inheritdoc />
	public int IncrementInt(long offset)
	{
		lock (this)
		{
			int num = ReadInt(offset);
			num++;
			WriteInt(offset, num);
			return num;
		}
	}

	/// <inheritdoc />
	public long WriteByte(long offset, byte value)
	{
		base.Data[(int)offset] = value;
		return 1L;
	}

	/// <inheritdoc />
	public long WriteBytes(long offset, byte[] value)
	{
		Array.Copy(value, 0L, base.Data, offset, value.Length);
		return value.Length;
	}

	/// <inheritdoc />
	public long WriteDouble(long offset, double value)
	{
		BitConverter.GetBytes(value).CopyTo(base.Data, offset);
		return 8L;
	}

	/// <inheritdoc />
	public long WriteFloat(long offset, float value)
	{
		BitConverter.GetBytes(value).CopyTo(base.Data, offset);
		return 4L;
	}

	/// <inheritdoc />
	public long WriteInt(long offset, int value)
	{
		BitConverter.GetBytes(value).CopyTo(base.Data, offset);
		return 4L;
	}

	/// <inheritdoc />
	public long WriteShort(long offset, short value)
	{
		BitConverter.GetBytes(value).CopyTo(base.Data, offset);
		return 2L;
	}

	/// <inheritdoc />
	public long WriteStruct<T>(long offset, T data) where T : struct
	{
		int numBytesWrite = Marshal.SizeOf(typeof(T));
		return WriteStruct(offset, data, numBytesWrite);
	}

	/// <inheritdoc />
	public long WriteStruct<T>(long offset, T data, int numBytesWrite) where T : struct
	{
		nint num = Marshal.AllocHGlobal(numBytesWrite);
		byte[] array = new byte[numBytesWrite];
		Marshal.StructureToPtr(data, num, fDeleteOld: false);
		Marshal.Copy(num, array, 0, numBytesWrite);
		return WriteBytes(offset, array);
	}
}
