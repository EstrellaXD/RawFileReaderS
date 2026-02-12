using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The temperature table, from legacy LCQ files
/// </summary>
internal sealed class TemperatureTable : IRawObjectBase
{
	/// <summary>
	/// Gets the temp table info.
	/// </summary>
	public TemperatureTableStruct[] TempTableInfo { get; private set; }

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		int bytesToRead = Marshal.SizeOf(typeof(TemperatureTableStruct));
		int num = viewer.ReadIntExt(ref startPos);
		TempTableInfo = new TemperatureTableStruct[num];
		for (int i = 0; i < num; i++)
		{
			TempTableInfo[i] = ReadTemperatureTableStruct(viewer, bytesToRead, ref startPos);
		}
		return startPos - dataOffset;
	}

	/// <summary>
	/// read temperature table struct.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="bytesToRead">
	/// The bytes to read.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <returns>
	/// The temperature table
	/// </returns>
	private static TemperatureTableStruct ReadTemperatureTableStruct(IMemoryReader viewer, int bytesToRead, ref long dataOffset)
	{
		byte[] value = viewer.ReadBytesExt(ref dataOffset, bytesToRead);
		return new TemperatureTableStruct
		{
			Rate = BitConverter.ToDouble(value, 0),
			Time = BitConverter.ToDouble(value, 8),
			Hold = BitConverter.ToDouble(value, 24),
			StartTemp = BitConverter.ToInt32(value, 32),
			EndTemp = BitConverter.ToInt32(value, 36)
		};
	}
}
