using System;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The Memory Map Writer interface.
/// </summary>
public interface IMemMapWriter : IDisposable
{
	/// <summary>
	/// Writes the structure.
	/// </summary>
	/// <typeparam name="T">
	/// Type of structure
	/// </typeparam>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="data">
	/// The data.
	/// </param>
	/// <returns>
	/// The number of bytes written
	/// </returns>
	long WriteStruct<T>(long offset, T data) where T : struct;

	/// <summary>
	/// write a struct.
	/// </summary>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <param name="data">
	/// The data.
	/// </param>
	/// <param name="numBytesWrite">
	/// The number of bytes to write.
	/// </param>
	/// <typeparam name="T">Type of structure to write
	/// </typeparam>
	/// <returns>
	/// The number of bytes written
	/// </returns>
	long WriteStruct<T>(long offset, T data, int numBytesWrite) where T : struct;

	/// <summary>
	/// Writes the float to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written</returns>
	long WriteFloat(long offset, float value);

	/// <summary>
	/// Writes the double to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written</returns>
	long WriteDouble(long offset, double value);

	/// <summary>
	/// Writes the byte to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written</returns>
	long WriteByte(long offset, byte value);

	/// <summary>
	/// Writes the array of bytes to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written</returns>
	long WriteBytes(long offset, byte[] value);

	/// <summary>
	/// Writes the short to the memory mapped file.
	/// </summary>
	/// <param name="offset">The start position.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written</returns>
	long WriteShort(long offset, short value);

	/// <summary>
	/// Writes the integer to the memory mapped file.
	/// </summary>
	/// <param name="offset">The offset.</param>
	/// <param name="value">The value.</param>
	/// <returns>The number of bytes written</returns>
	long WriteInt(long offset, int value);

	/// <summary>
	/// Using Interlocked operation to increment the specified INT (4-byte) variable and
	/// stores the result, as an atomic operation against memory-mapped files in .NET.
	/// Note: This method is marked as UNSAFE, which is required for operating
	/// memory-mapped file pointer.
	/// </summary>
	/// <param name="offset">The number of bytes into the accessor at which to begin writing.</param>
	/// <returns>The incremented value</returns>
	int IncrementInt(long offset);
}
