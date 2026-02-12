using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Indexing information to speed up MS data loading.
/// </summary>
public static class ExportDeviceMetadata
{
	private static readonly object LockObj = new object();

	private const string FilePostfixNameTrailScanEventIndicesInfo = "_tse.mb";

	private const int UniqueEventRecordSize = 20;

	/// <summary>Exports the trailer scan event indices information.</summary>
	/// <param name="detectorReader">The detector reader.</param>
	/// <param name="fileName">Name of the file.</param>
	public static bool ExportTrailScanEventIndicesInfo(this IDetectorReader detectorReader, string fileName)
	{
		string trailScanEventIndicesInfoFileName = GetTrailScanEventIndicesInfoFileName(fileName);
		List<object> trailerScanEventIndicesInfo = detectorReader.GetTrailerScanEventIndicesInfo();
		IReadOnlyCollection<(int, long, long)> uniqueScanEventIndices = trailerScanEventIndicesInfo[0] as IReadOnlyCollection<(int, long, long)>;
		int[] indexToUniqueEvents = trailerScanEventIndicesInfo[1] as int[];
		return ExportTrailScanEventIndicesInfo(trailScanEventIndicesInfoFileName, uniqueScanEventIndices, indexToUniqueEvents);
	}

	public static bool ExportTrailScanEventIndicesInfo(IMemoryReader viewer, IReadOnlyCollection<(int index, long startOffset, long endOffset)> uniqueScanEventIndices, IReadOnlyCollection<int> indexToUniqueEvents)
	{
		int num = viewer.StreamId.IndexOf("__");
		return ExportTrailScanEventIndicesInfo(GetTrailScanEventIndicesInfoFileName(viewer.StreamId.Substring(num + 2)), uniqueScanEventIndices, indexToUniqueEvents.ToArray());
	}

	public static bool ExportTrailScanEventIndicesInfo(string metadataFileName, IReadOnlyCollection<(int index, long startOffset, long endOffset)> uniqueScanEventIndices, int[] indexToUniqueEvents)
	{
		lock (LockObj)
		{
			if (indexToUniqueEvents == null || indexToUniqueEvents.Length == 0 || uniqueScanEventIndices == null || uniqueScanEventIndices.Count == 0)
			{
				return false;
			}
			using FileStream output = new FileStream(metadataFileName, FileMode.Create, FileAccess.ReadWrite);
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			byte[] array = new byte[uniqueScanEventIndices.Count * 20];
			binaryWriter.Write(array.Length);
			int num = 0;
			foreach (var uniqueScanEventIndex in uniqueScanEventIndices)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(uniqueScanEventIndex.index), 0, array, num, 4);
				num += 4;
				Buffer.BlockCopy(BitConverter.GetBytes(uniqueScanEventIndex.startOffset), 0, array, num, 8);
				num += 8;
				Buffer.BlockCopy(BitConverter.GetBytes(uniqueScanEventIndex.endOffset), 0, array, num, 8);
				num += 8;
			}
			binaryWriter.Write(array, 0, array.Length);
			array = new byte[indexToUniqueEvents.Length * 4];
			binaryWriter.Write(array.Length);
			Buffer.BlockCopy(indexToUniqueEvents, 0, array, 0, array.Length);
			binaryWriter.Write(array, 0, array.Length);
			binaryWriter.Flush();
			binaryWriter.Close();
			return true;
		}
	}

	public static bool ImportTrailScanEventIndicesInfo(IMemoryReader viewer, IList<(int index, long startOffset, long endOffset)> uniqueScanEventIndices, int[] indexToUniqueEvents)
	{
		int num = viewer.StreamId.IndexOf("__");
		return ImportTrailScanEventIndicesInfo(GetTrailScanEventIndicesInfoFileName(viewer.StreamId.Substring(num + 2)), uniqueScanEventIndices, indexToUniqueEvents);
	}

	/// <summary>Imports the trailer scan event indices information.</summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="uniqueScanEventIndicesBuffer">The unique scan event indices buffer.</param>
	/// <param name="indexToUniqueEvents">The index to unique events.</param>
	/// <returns>
	///   True if loaded successfully; otherwise false
	/// </returns>
	public static bool ImportTrailScanEventIndicesInfo(IMemoryReader viewer, out byte[] uniqueScanEventIndicesBuffer, int[] indexToUniqueEvents)
	{
		int num = viewer.StreamId.IndexOf("__");
		return ImportTrailScanEventIndicesInfo(GetTrailScanEventIndicesInfoFileName(viewer.StreamId.Substring(num + 2)), out uniqueScanEventIndicesBuffer, indexToUniqueEvents);
	}

	public static bool ImportTrailScanEventIndicesInfo(string metadataFileName, IList<(int index, long startOffset, long endOffset)> uniqueScanEventIndices, int[] indexToUniqueEvents)
	{
		if (!File.Exists(metadataFileName))
		{
			return false;
		}
		lock (LockObj)
		{
			using BinaryReader binaryReader = new BinaryReader(File.OpenRead(metadataFileName));
			int num = binaryReader.ReadInt32();
			byte[] array = new byte[num];
			binaryReader.Read(array, 0, num);
			int num2 = num / 20;
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				int item = BitConverter.ToInt32(array, num3);
				num3 += 4;
				int num4 = BitConverter.ToInt32(array, num3);
				num3 += 8;
				int num5 = BitConverter.ToInt32(array, num3);
				num3 += 8;
				uniqueScanEventIndices.Add((item, num4, num5));
			}
			num = binaryReader.ReadInt32();
			array = new byte[num];
			binaryReader.Read(array, 0, num);
			Buffer.BlockCopy(array, 0, indexToUniqueEvents, 0, num);
			binaryReader.Close();
			return true;
		}
	}

	/// <summary>Imports the trailer scan event indices information.</summary>
	/// <param name="metadataFileName">Name of the metadata file.</param>
	/// <param name="uniqueScanEventIndicesBuffer">The unique scan event indices buffer.</param>
	/// <param name="indexToUniqueEvents">The index to unique events.</param>
	/// <returns>
	///   True if loaded successfully; otherwise false
	/// </returns>
	public static bool ImportTrailScanEventIndicesInfo(string metadataFileName, out byte[] uniqueScanEventIndicesBuffer, int[] indexToUniqueEvents)
	{
		if (!File.Exists(metadataFileName))
		{
			uniqueScanEventIndicesBuffer = Array.Empty<byte>();
			return false;
		}
		lock (LockObj)
		{
			using BinaryReader binaryReader = new BinaryReader(File.OpenRead(metadataFileName));
			int num = binaryReader.ReadInt32();
			uniqueScanEventIndicesBuffer = new byte[num];
			binaryReader.Read(uniqueScanEventIndicesBuffer, 0, num);
			num = binaryReader.ReadInt32();
			byte[] array = new byte[num];
			binaryReader.Read(array, 0, num);
			Buffer.BlockCopy(array, 0, indexToUniqueEvents, 0, num);
			binaryReader.Close();
			return true;
		}
	}

	/// <summary>Gets the name of the trail scan event indices information file.</summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>
	///   trailer extra indexes file name
	/// </returns>
	internal static string GetTrailScanEventIndicesInfoFileName(string fileName)
	{
		return Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "_tse.mb");
	}
}
