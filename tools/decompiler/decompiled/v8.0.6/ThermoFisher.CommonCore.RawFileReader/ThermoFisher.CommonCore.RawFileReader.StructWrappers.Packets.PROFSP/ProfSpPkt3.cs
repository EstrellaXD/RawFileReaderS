using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PROFSP;

/// <summary>
/// The profile spectrum packet 3.
/// </summary>
internal class ProfSpPkt3 : SimplePacketBase, IMsPacket, IPacket, IRawObjectBase
{
	private readonly int _numProfIndexes;

	private readonly bool _isSingleScan;

	private readonly ProfileIndexDataPacket[] _profIndexDataPkts;

	private readonly List<float[]> _profDataPkt;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PROFSP.ProfSpPkt3" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">Offset from start of memory reader</param>
	/// <param name="scanIndex">Index to scan whose data is needed</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="isSingleScan">set if the viewer has just 1 scan</param>
	public ProfSpPkt3(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess scanIndex, int fileRevision, bool isSingleScan = false)
	{
		int packetCount = scanIndex.PacketCount;
		_numProfIndexes = packetCount;
		_isSingleScan = isSingleScan;
		_profIndexDataPkts = new ProfileIndexDataPacket[_numProfIndexes];
		_profDataPkt = new List<float[]>(_numProfIndexes - 1);
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
			_profDataPkt.Add(viewer.ReadFloats(startOfPktPos, num2));
			startOfPktPos += 4 * num2;
			num += num2;
			profileIndexDataPacket.NumberPackets = num2;
			profileIndexDataPacket.TotalPackets = num2;
		}
		return 0L;
	}

	/// <summary>
	/// Expands the profile BLOB.
	/// </summary>
	/// <returns>The expanded data</returns>
	protected List<SegmentData> ExpandProfileBlob()
	{
		int num = _numProfIndexes - 1;
		List<SegmentData> list = new List<SegmentData>();
		for (int i = 0; i < num; i++)
		{
			ProfileIndexDataPacket profileIndexDataPacket = _profIndexDataPkts[i];
			int numberPackets = profileIndexDataPacket.NumberPackets;
			double num2 = profileIndexDataPacket.LowMass;
			double massTick = profileIndexDataPacket.MassTick;
			float[] array = _profDataPkt[i];
			List<DataPeak> list2 = new List<DataPeak>(numberPackets);
			for (int j = 0; j < numberPackets; j++)
			{
				list2.Add(new DataPeak(num2, array[j]));
				num2 += massTick;
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
