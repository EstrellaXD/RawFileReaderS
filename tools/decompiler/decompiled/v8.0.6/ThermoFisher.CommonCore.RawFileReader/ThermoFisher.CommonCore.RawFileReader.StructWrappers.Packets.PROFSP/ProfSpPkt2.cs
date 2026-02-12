using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PROFSP;

/// <summary>
/// The profile spectrum packet type 2.
/// </summary>
internal class ProfSpPkt2 : SimplePacketBase, IMsPacket, IPacket, IRawObjectBase
{
	private readonly int _numProfIndexes;

	private readonly bool _isSingleScan;

	private readonly ProfileIndexDataPacket[] _profIndexDataPkts;

	private readonly List<uint[]> _profDataPkt;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PROFSP.ProfSpPkt2" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">offset from start of memory reader</param>
	/// <param name="index">Index to scan who's data is needed</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="isSingleScan">set if the viewer has just 1 scan</param>
	public ProfSpPkt2(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess index, int fileRevision, bool isSingleScan = false)
	{
		int packetCount = index.PacketCount;
		_numProfIndexes = packetCount;
		_isSingleScan = isSingleScan;
		_profIndexDataPkts = new ProfileIndexDataPacket[_numProfIndexes];
		_profDataPkt = new List<uint[]>(_numProfIndexes - 1);
		base.LazySegmentPeaks = new Lazy<List<SegmentData>>(ExpandProfileBlob);
		Load(viewer, dataOffset, fileRevision);
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
		int num = profIndexDataPkts.Length;
		for (int i = 0; i < num; i++)
		{
			profIndexDataPkts[i] = viewer.LoadRawFileObjectExt(() => new ProfileIndexDataPacket(), fileRevision, ref startPos);
		}
		startOfPktPos = profIndexDataPkts[0].DataPos;
		if (isSingleScan)
		{
			startOfPktPos = 0L;
		}
		profIndexDataPkts[0].DataPos = 0L;
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
		int num = 1;
		for (int i = 0; i < _numProfIndexes - 1; i++)
		{
			ProfileIndexDataPacket profileIndexDataPacket = _profIndexDataPkts[i];
			int num2 = (int)((_profIndexDataPkts[i + 1].DataPos - profileIndexDataPacket.DataPos) / 4);
			_profDataPkt.Add(viewer.ReadUnsignedInts(startOfPktPos, num2));
			startOfPktPos += 4 * num2;
			uint[] array = _profDataPkt[i];
			int num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				if ((array[j] & 0x80000000u) != 0)
				{
					num3++;
				}
			}
			num += num3;
			profileIndexDataPacket.NumberPackets = num3;
			profileIndexDataPacket.TotalPackets = num2;
		}
		return 0L;
	}

	/// <summary>
	/// Expands the profile BLOB.
	/// </summary>
	/// <returns>The profile data</returns>
	private List<SegmentData> ExpandProfileBlob()
	{
		int num = _numProfIndexes - 1;
		List<SegmentData> list = new List<SegmentData>();
		for (int i = 0; i < num; i++)
		{
			ProfileIndexDataPacket profileIndexDataPacket = _profIndexDataPkts[i];
			int totalPackets = profileIndexDataPacket.TotalPackets;
			int numberPackets = profileIndexDataPacket.NumberPackets;
			double num2 = profileIndexDataPacket.LowMass;
			double massTick = profileIndexDataPacket.MassTick;
			uint[] array = _profDataPkt[i];
			List<DataPeak> list2 = new List<DataPeak>(totalPackets);
			int num3 = 0;
			int[] array2 = new int[4] { 1, 8, 64, 512 };
			for (int j = 1; j < totalPackets; j++)
			{
				uint num4 = array[j];
				if ((num4 & 0x80000000u) != 0)
				{
					long num5 = num4 & 0xFFFFFFF;
					int num6 = array2[(num4 & 0x30000000) >> 28];
					num5 *= num6;
					list2.Add(new DataPeak(num2, num5));
					num3++;
				}
				num2 += massTick;
				if (num3 >= numberPackets)
				{
					break;
				}
			}
			list.Add(new SegmentData
			{
				DataPeaks = list2,
				MassRange = new MassRangeStruct(profileIndexDataPacket.LowMass, profileIndexDataPacket.HighMass)
			});
			_profDataPkt[i] = null;
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
}
