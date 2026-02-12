using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// The memory map accessor.
/// Provides methods to access mapped file data.
/// </summary>
internal class MemMapAccessor : MemMapReader, IReadWriteAccessor, IMemMapWriter, IDisposable, IDisposableReader, IMemoryReader
{
	private bool _disposed;

	/// <summary>
	/// This implementation doen't need large memory buffers 
	/// </summary>
	public int SuggestedChunkSize => 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemMapAccessor" /> class.
	/// </summary>
	/// <param name="mmf">The memory mapped raw file handler</param>
	/// <param name="streamId">The stream identifier.</param>
	/// <param name="viewAccessor">The view accessor.</param>
	/// <param name="initialOffset">The initial offset.</param>
	/// <param name="sizeOfView">The size of view.</param>
	public MemMapAccessor(MemoryMappedRawFile mmf, string streamId, MemoryMappedViewAccessor viewAccessor, long initialOffset, long sizeOfView)
		: base(mmf, streamId, viewAccessor, initialOffset, sizeOfView)
	{
		_disposed = false;
	}

	/// <summary>
	/// Writes the structure.
	/// </summary>
	/// <typeparam name="T">Struct type</typeparam>
	/// <param name="offset">The offset.</param>
	/// <param name="data">The data.</param>
	/// <returns>Number of bytes written</returns>
	public long WriteStruct<T>(long offset, T data) where T : struct
	{
		int numBytesWrite = Marshal.SizeOf(typeof(T));
		return WriteStruct(offset, data, numBytesWrite);
	}

	/// <summary>
	/// Writes the structure.
	/// </summary>
	/// <typeparam name="T">Struct type</typeparam>
	/// <param name="offset">The offset.</param>
	/// <param name="data">The data.</param>
	/// <param name="numBytesWrite">The size of structure.</param>
	/// <returns>Number of bytes written</returns>
	public long WriteStruct<T>(long offset, T data, int numBytesWrite) where T : struct
	{
		nint num = Marshal.AllocHGlobal(numBytesWrite);
		byte[] array = new byte[numBytesWrite];
		Marshal.StructureToPtr(data, num, fDeleteOld: false);
		Marshal.Copy(num, array, 0, numBytesWrite);
		base.Accessor.WriteArray(offset, array, 0, array.Length);
		Marshal.FreeHGlobal(num);
		return numBytesWrite;
	}

	/// <summary>
	/// Writes the float to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written (4)</returns>
	public long WriteFloat(long offset, float value)
	{
		base.Accessor.Write(offset, value);
		return 4L;
	}

	/// <summary>
	/// Writes the double to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>the number of bytes written (8)</returns>
	public long WriteDouble(long offset, double value)
	{
		base.Accessor.Write(offset, value);
		return 8L;
	}

	/// <summary>
	/// Writes the byte to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written (1)</returns>
	public long WriteByte(long offset, byte value)
	{
		base.Accessor.Write(offset, value);
		return 1L;
	}

	/// <summary>
	/// Writes the array of bytes to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written</returns>
	public long WriteBytes(long offset, byte[] value)
	{
		int num = value.Length;
		base.Accessor.WriteArray(offset, value, 0, num);
		return num;
	}

	/// <summary>
	/// Writes the short to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written (2)</returns>
	public long WriteShort(long offset, short value)
	{
		base.Accessor.Write(offset, value);
		return 2L;
	}

	/// <summary>
	/// Writes the integer to the memory mapped file.
	/// </summary>
	/// <param name="offset">The offset.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written (4)</returns>
	public long WriteInt(long offset, int value)
	{
		base.Accessor.Write(offset, value);
		return 4L;
	}

	/// <inheritdoc />
	public unsafe int IncrementInt(long offset)
	{
		byte* ptr = (byte*)((IntPtr)base.Accessor.SafeMemoryMappedViewHandle.DangerousGetHandle()).ToPointer();
		ptr += offset;
		return Interlocked.Increment(ref *(int*)ptr);
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <filterpriority>2</filterpriority>
	public new void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			base.Dispose();
		}
	}
}
