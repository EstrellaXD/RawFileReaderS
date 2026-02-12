using System;
using System.IO;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The UV scan index extension.
/// </summary>
internal static class UvScanIndexExtension
{
	private static readonly int UvScanIndexStructSize = Utilities.StructSizeLookup.Value[5];

	private static readonly int ScanIndexStructSize = Utilities.StructSizeLookup.Value[19];

	/// <summary>
	/// Saves the UV scan index structure information to disk.
	/// </summary>
	/// <param name="uvScanIndexStructInfo">The UV scan index structure information.</param>
	/// <param name="writer">The stream writer.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True if UV scan index saved to disk, false otherwise. </returns>
	public static bool Save(this UvScanIndexStruct uvScanIndexStructInfo, BinaryWriter writer, DeviceErrors errors)
	{
		try
		{
			writer.Write(WriterHelper.StructToByteArray(uvScanIndexStructInfo, UvScanIndexStructSize));
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the mass spec scan index structure to disk.
	/// </summary>
	/// <param name="scanIndex">The mass spec scan index structure.</param>
	/// <param name="writer">The stream writer.</param>
	/// <param name="errors">Stack trace on errors.</param>
	/// <returns>True if mass spec scan index saved to disk, false otherwise.</returns>
	public static bool Save(this ScanIndexStruct scanIndex, BinaryWriter writer, DeviceErrors errors)
	{
		byte[] array = new byte[ScanIndexStructSize];
		Array.Copy(BitConverter.GetBytes(scanIndex.DataSize), array, 4);
		Array.Copy(BitConverter.GetBytes(scanIndex.TrailerOffset), 0, array, 4, 4);
		Array.Copy(BitConverter.GetBytes(scanIndex.ScanTypeIndex), 0, array, 8, 4);
		Array.Copy(BitConverter.GetBytes(scanIndex.ScanNumber), 0, array, 12, 4);
		Array.Copy(BitConverter.GetBytes(scanIndex.PacketType), 0, array, 16, 4);
		Array.Copy(BitConverter.GetBytes(scanIndex.NumberPackets), 0, array, 20, 4);
		Array.Copy(BitConverter.GetBytes(scanIndex.StartTime), 0, array, 24, 8);
		Array.Copy(BitConverter.GetBytes(scanIndex.TIC), 0, array, 32, 8);
		Array.Copy(BitConverter.GetBytes(scanIndex.BasePeakIntensity), 0, array, 40, 8);
		Array.Copy(BitConverter.GetBytes(scanIndex.BasePeakMass), 0, array, 48, 8);
		Array.Copy(BitConverter.GetBytes(scanIndex.LowMass), 0, array, 56, 8);
		Array.Copy(BitConverter.GetBytes(scanIndex.HighMass), 0, array, 64, 8);
		Array.Copy(BitConverter.GetBytes(scanIndex.DataOffset), 0, array, 72, 8);
		Array.Copy(BitConverter.GetBytes(scanIndex.CycleNumber), 0, array, 80, 4);
		try
		{
			writer.Write(array);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Copy all the fields of the current CommonCore UV ScanIndex object to the new internal UV ScanIndex object.
	/// </summary>
	/// <param name="uvDataIndex">Index of the UV instrument scan.</param>
	/// <returns>Create a copy of the UV ScanIndex object</returns>
	public static UvScanIndex ConvertToUvScanIndex(this IUvScanIndex uvDataIndex)
	{
		return new UvScanIndex
		{
			UvScanIndexStructInfo = new UvScanIndexStruct
			{
				NumberOfChannels = uvDataIndex.NumberOfChannels,
				StartTime = uvDataIndex.StartTime,
				TIC = uvDataIndex.TIC,
				UniformTime = (uvDataIndex.IsUniformTime ? 1 : 0),
				Frequency = uvDataIndex.Frequency,
				NumberPackets = 0,
				ScanNumber = 0,
				PacketType = 25,
				DataOffset = 0L,
				DataOffset32Bit = 0u
			}
		};
	}

	/// <summary>
	/// Copy all the fields of the current CommonCore PDA ScanIndex object to the new internal UV ScanIndex object.
	/// </summary>
	/// <param name="pdaDataIndex">Index of the PDA data.</param>
	/// <returns>Create a copy of the UV ScanIndex object</returns>
	public static UvScanIndex ConvertToUvScanIndex(this IPdaScanIndex pdaDataIndex)
	{
		return new UvScanIndex
		{
			UvScanIndexStructInfo = new UvScanIndexStruct
			{
				NumberOfChannels = 1,
				UniformTime = 1,
				Frequency = 0.0,
				StartTime = pdaDataIndex.StartTime,
				ShortWavelength = pdaDataIndex.ShortWavelength,
				LongWavelength = pdaDataIndex.LongWavelength,
				TIC = pdaDataIndex.TIC,
				NumberPackets = 0,
				ScanNumber = 0,
				PacketType = 25,
				DataOffset = 0L,
				DataOffset32Bit = 0u
			}
		};
	}

	/// <summary>
	/// Converts all the fields of the current ScanStatistics objet to a new internal scan index object
	/// </summary>
	/// <param name="scanStatistics">The scan statistics.</param>
	/// <returns>Create a copy of the mass spec scan index object</returns>
	public static ScanIndex ConvertToScanIndex(this IScanStatisticsAccess scanStatistics)
	{
		return new ScanIndex
		{
			ScanIndexStructInfo = new ScanIndexStruct
			{
				ScanTypeIndex = scanStatistics.ScanEventNumber,
				ScanNumber = scanStatistics.ScanNumber,
				PacketType = (uint)scanStatistics.PacketType,
				NumberPackets = scanStatistics.PacketCount,
				StartTime = scanStatistics.StartTime,
				TIC = scanStatistics.TIC,
				BasePeakIntensity = scanStatistics.BasePeakIntensity,
				BasePeakMass = scanStatistics.BasePeakMass,
				HighMass = scanStatistics.HighMass,
				LowMass = scanStatistics.LowMass,
				CycleNumber = scanStatistics.CycleNumber,
				TrailerOffset = -1
			}
		};
	}
}
