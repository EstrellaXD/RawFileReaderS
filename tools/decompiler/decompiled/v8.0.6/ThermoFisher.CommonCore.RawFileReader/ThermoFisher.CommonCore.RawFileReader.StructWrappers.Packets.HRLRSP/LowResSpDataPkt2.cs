using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;

/// <summary>
/// The low res spectrum data packet 2.
/// </summary>
internal sealed class LowResSpDataPkt2 : SimplePacketBase, IMsPacket, IPacket, IRawObjectBase
{
	private static readonly int LowResDataStructSize = Utilities.StructSizeLookup.Value[15];

	private static readonly uint[] Scales = new uint[8] { 1u, 8u, 64u, 512u, 4096u, 32768u, 262144u, 2097152u };

	private static readonly byte[] FlagConvert = MakeByteFlagsTable();

	private readonly int _numIndexStruct;

	private readonly IMsScanIndexAccess _scanIndex;

	private byte[] _profileBlob;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP.LowResSpDataPkt2" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">Offset from start of memory reader</param>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="fileRevision">The file revision.</param>
	public LowResSpDataPkt2(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess scanIndex, int fileRevision)
	{
		_scanIndex = scanIndex;
		_numIndexStruct = scanIndex.PacketCount;
		base.LazySegmentPeaks = new Lazy<List<SegmentData>>(ExpandProfileBlob);
		Load(viewer, dataOffset, fileRevision);
	}

	/// <summary>
	/// find the byte length of a scan
	/// </summary>
	/// <param name="scanIndex">index (used to calculate the scan size)</param>
	/// <returns>bytes used for this scan</returns>
	public static long Size(IMsScanIndexAccess scanIndex)
	{
		return LowResDataStructSize * scanIndex.PacketCount;
	}

	/// <summary>
	/// Called when [compress].
	/// </summary>
	/// <param name="segmentedScan">The segmented scan.</param>
	/// <returns>The compressed segmented scans in byte array</returns>
	/// <exception cref="T:System.ArgumentNullException">segmented Scan</exception>
	public static byte[] Compress(ISegmentedScanAccess segmentedScan)
	{
		if (WriterHelper.HasNoScan(segmentedScan))
		{
			return Array.Empty<byte>();
		}
		double[] intensities = segmentedScan.Intensities;
		double[] positions = segmentedScan.Positions;
		PeakOptions[] flags = segmentedScan.Flags;
		int num = positions.Length;
		byte[] array = new byte[LowResDataStructSize * num];
		for (int i = 0; i < num; i++)
		{
			double num2 = intensities[i];
			uint num3 = 0u;
			if (num2 > 16777215.0)
			{
				byte b = 0;
				while (num2 > 16777215.0)
				{
					num2 /= 8.0;
					b++;
				}
				num3 |= b;
			}
			uint num4 = (uint)num2;
			int num5 = i * LowResDataStructSize;
			array[num5 + 3] = (byte)((num4 >> 16) & 0xFF);
			array[num5 + 2] = (byte)((num4 >> 8) & 0xFF);
			array[num5 + 1] = (byte)(num4 & 0xFF);
			ushort num6 = (ushort)Math.Floor(positions[i]);
			ushort value = (ushort)((positions[i] - (double)(int)num6) * 65536.0);
			Buffer.BlockCopy(BitConverter.GetBytes(num6), 0, array, num5 + 4, 2);
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, array, num5 + 6, 2);
			num3 |= FlagConvert[(byte)flags[i] & 7];
			array[num5] = Convert.ToByte(num3);
		}
		return array;
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
		int num = LowResDataStructSize * _numIndexStruct;
		_profileBlob = viewer.ReadBytes(dataOffset, LowResDataStructSize * _numIndexStruct);
		return num;
	}

	/// <summary>
	/// Map peak options to flags, with a static table
	/// </summary>
	/// <returns>
	/// The table of bytes for each combination of options
	/// </returns>
	private static byte[] MakeByteFlagsTable()
	{
		byte[] array = new byte[8];
		for (int i = 0; i < 8; i++)
		{
			byte b = 0;
			if (((ulong)i & 2uL) != 0L)
			{
				b |= 0x40;
			}
			if (((ulong)i & 4uL) != 0L)
			{
				b |= 0x20;
			}
			if (((ulong)i & 1uL) != 0L)
			{
				b |= 0x80;
			}
			array[i] = b;
		}
		return array;
	}

	/// <summary>
	/// Expands the profile BLOB.
	/// </summary>
	/// <returns>The profile data</returns>
	private List<SegmentData> ExpandProfileBlob()
	{
		List<SegmentData> list = new List<SegmentData>(1);
		List<DataPeak> list2 = new List<DataPeak>(_numIndexStruct);
		for (int i = 0; i < _numIndexStruct; i++)
		{
			int num = i * LowResDataStructSize;
			byte b = _profileBlob[num];
			uint num2 = (uint)((_profileBlob[num + 3] << 16) + (_profileBlob[num + 2] << 8) + _profileBlob[num + 1]);
			ushort num3 = BitConverter.ToUInt16(_profileBlob, num + 4);
			ushort num4 = BitConverter.ToUInt16(_profileBlob, num + 6);
			DataPeak item = new DataPeak((double)(int)num3 + (double)(int)num4 / 65536.0, num2 * Scales[b & 7]);
			if ((b & 0xE0) != 0)
			{
				PeakOptions peakOptions = PeakOptions.None;
				if ((b & 0x80) > 0)
				{
					peakOptions |= PeakOptions.Saturated;
				}
				if ((b & 0x40) > 0)
				{
					peakOptions |= PeakOptions.Fragmented;
				}
				if ((b & 0x20) > 0)
				{
					peakOptions |= PeakOptions.Merged;
				}
				item.Options = peakOptions;
			}
			list2.Add(item);
		}
		list.Add(new SegmentData
		{
			DataPeaks = list2,
			MassRange = new MassRangeStruct(_scanIndex.LowMass, _scanIndex.HighMass)
		});
		_profileBlob = null;
		return list;
	}
}
