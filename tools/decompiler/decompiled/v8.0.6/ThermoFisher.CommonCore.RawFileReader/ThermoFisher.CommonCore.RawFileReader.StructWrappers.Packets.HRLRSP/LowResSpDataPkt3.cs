using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;

/// <summary>
/// The low resolution spectrum data packet 3.
/// </summary>
internal sealed class LowResSpDataPkt3 : SimplePacketBase, IMsPacket, IPacket, IRawObjectBase
{
	private const int DataSize = 8;

	private readonly int _numProfIndexes;

	private readonly long _dataOffset;

	private readonly bool _isSingleScan;

	private readonly ProfileIndexDataPacket[] _profIndexDataPkts;

	private float[] _massIntensityPairs;

	/// <summary>
	/// Gets the scan index.
	/// </summary>
	public IScanIndex Index { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP.LowResSpDataPkt3" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">offset from start of memory reader</param>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="isSingleScan">set if the viewer has just 1 scan</param>
	public LowResSpDataPkt3(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess scanIndex, int fileRevision, bool isSingleScan = false)
	{
		_numProfIndexes = scanIndex.PacketCount;
		_dataOffset = dataOffset;
		_isSingleScan = isSingleScan;
		_profIndexDataPkts = new ProfileIndexDataPacket[_numProfIndexes];
		base.LazySegmentPeaks = new Lazy<List<SegmentData>>(ExpandProfileBlob);
		Load(viewer, dataOffset, fileRevision);
	}

	/// <summary>
	/// Compresses the low resolution spec data packet #3 segmented scan.
	/// </summary>
	/// <param name="segmentedScan">The segmented scan.</param>
	/// <returns>The compressed segmented scans in byte array.</returns>
	/// <exception cref="T:System.ArgumentNullException">segmented Scan</exception>
	public static byte[] Compress(ISegmentedScanAccess segmentedScan)
	{
		if (WriterHelper.HasNoScan(segmentedScan))
		{
			return Array.Empty<byte>();
		}
		double[] positions = segmentedScan.Positions;
		double[] intensities = segmentedScan.Intensities;
		int num = positions.Length;
		float[] array = new float[num * 2];
		byte[] array2 = new byte[8 * num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			array[num2++] = (float)positions[i];
			array[num2++] = (float)intensities[i];
		}
		Buffer.BlockCopy(array, 0, array2, 0, array2.Length);
		return array2;
	}

	/// <summary>
	/// Calculate the number of bytes used for this scan
	/// </summary>
	/// <param name="viewer">access to data for the scan</param>
	/// <param name="dataOffset">offset to "index records"</param>
	/// <param name="scanIndex">scan index</param>
	/// <param name="fileRevision">file rev</param>
	/// <param name="startAddress">actual scan start address (first data byte in scan)</param>
	/// <returns>bytes in this scan</returns>
	public static long Size(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess scanIndex, int fileRevision, out long startAddress)
	{
		int packetCount = scanIndex.PacketCount;
		long startPos = dataOffset;
		ProfileIndexDataPacket[] array = new ProfileIndexDataPacket[packetCount];
		startPos = GetProfileIndexPackets(viewer, fileRevision, startPos, array, out var startOfPktPos);
		long num = startPos - dataOffset;
		int num2 = (int)(array[packetCount - 1].DataPos - array[0].DataPos);
		startAddress = startOfPktPos;
		return num2 + num;
	}

	/// <summary>
	/// Read the index records
	/// </summary>
	/// <param name="viewer">memory reader</param>
	/// <param name="fileRevision">raw file rev</param>
	/// <param name="startPos">start of first index record</param>
	/// <param name="profIndexDataPkts">array to fill with index records</param>
	/// <param name="startOfPktPos">calculated address of first data packet</param>
	/// <param name="isSingleScan">set if the memory reader is for 1 scan (else it may be an entire file of scans)</param>
	/// <returns>the updated start pos (after these records)</returns>
	private static long GetProfileIndexPackets(IMemoryReader viewer, int fileRevision, long startPos, ProfileIndexDataPacket[] profIndexDataPkts, out long startOfPktPos, bool isSingleScan = false)
	{
		long num = startPos;
		int num2 = profIndexDataPkts.Length;
		for (int i = 0; i < num2; i++)
		{
			profIndexDataPkts[i] = viewer.LoadRawFileObjectExt(() => new ProfileIndexDataPacket(), fileRevision, ref startPos);
		}
		startOfPktPos = profIndexDataPkts[0].DataPos;
		profIndexDataPkts[0].DataPos = 0L;
		if (isSingleScan)
		{
			startOfPktPos = 0L;
		}
		profIndexDataPkts[num2 - 1].DataPos = num - startOfPktPos;
		return startPos;
	}

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
		startPos = GetProfileIndexPackets(viewer, fileRevision, startPos, _profIndexDataPkts, out var startOfPktPos, _isSingleScan);
		int num = (int)((_profIndexDataPkts[_numProfIndexes - 1].DataPos - _profIndexDataPkts[0].DataPos) / 8);
		startPos = startOfPktPos;
		_massIntensityPairs = viewer.ReadFloatsExt(ref startPos, num * 2);
		return 0L;
	}

	/// <summary>
	/// Expands the profile BLOB.
	/// </summary>
	/// <returns>The expanded data</returns>
	private List<SegmentData> ExpandProfileBlob()
	{
		int num = _numProfIndexes - 1;
		int num2 = (int)((_profIndexDataPkts[_numProfIndexes - 1].DataPos - _profIndexDataPkts[0].DataPos) / 8);
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		List<SegmentData> list = new List<SegmentData>();
		for (int i = 0; i < num; i++)
		{
			ProfileIndexDataPacket profileIndexDataPacket = _profIndexDataPkts[i];
			float lowMass = profileIndexDataPacket.LowMass;
			float highMass = profileIndexDataPacket.HighMass;
			int num6 = 0;
			while (num3 < num2 && _massIntensityPairs[num4] >= lowMass && _massIntensityPairs[num4] <= highMass)
			{
				num3++;
				num6++;
				num4 += 2;
			}
			profileIndexDataPacket.NumberPackets = num6;
			profileIndexDataPacket.TotalPackets = num6;
			List<DataPeak> dataPeaks = OnConvert(_massIntensityPairs, num5, num6);
			list.Add(new SegmentData
			{
				DataPeaks = dataPeaks,
				MassRange = new MassRangeStruct(lowMass, highMass)
			});
			num5 += num6 * 2;
		}
		return list;
	}

	/// <summary>
	/// Calculate where the index records are based on the length of the scan buffer and the number of index records
	/// </summary>
	/// <param name="length">length of scan</param>
	/// <param name="packetCount">number of index records</param>
	/// <param name="version">file version</param>
	/// <returns>offset into scan where profile index packets are</returns>
	internal static int GetOffset(long length, int packetCount, int version)
	{
		return (int)(length - packetCount * ProfileIndexDataPacket.Size(version));
	}

	/// <summary>
	/// Called when [convert].
	/// </summary>
	/// <param name="massIntensityPairs">The mass intensity pairs.</param>
	/// <param name="startPosition">The mi start position.</param>
	/// <param name="numPkts">The number PKTS.</param>
	/// <returns>The converted data</returns>
	private List<DataPeak> OnConvert(float[] massIntensityPairs, int startPosition, int numPkts)
	{
		List<DataPeak> list = new List<DataPeak>(numPkts);
		for (int i = 0; i < numPkts; i++)
		{
			int num = startPosition + i * 2;
			list.Add(new DataPeak(massIntensityPairs[num], massIntensityPairs[num + 1]));
		}
		return list;
	}
}
