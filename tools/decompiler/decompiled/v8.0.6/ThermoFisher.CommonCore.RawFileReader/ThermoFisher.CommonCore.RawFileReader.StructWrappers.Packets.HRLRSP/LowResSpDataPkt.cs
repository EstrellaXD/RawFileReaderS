using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;

/// <summary>
/// The low resolution spectrum data packet.
/// </summary>
/// <seealso cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.IMsPacket" />
/// <seealso cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRawObjectBase" />
internal sealed class LowResSpDataPkt : SimplePacketBase, IMsPacket, IPacket, IRawObjectBase
{
	private static readonly int LowResDataStructSize = Utilities.StructSizeLookup.Value[15];

	private readonly int _numIndexStruct;

	private byte[] _profileBlob;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP.LowResSpDataPkt" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset"></param>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="fileRevision">The file revision.</param>
	public LowResSpDataPkt(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess scanIndex, int fileRevision)
	{
		LowResSpDataPkt lowResSpDataPkt = this;
		_numIndexStruct = scanIndex.PacketCount;
		double lowMass = scanIndex.LowMass;
		double highMass = scanIndex.HighMass;
		base.LazySegmentPeaks = new Lazy<List<SegmentData>>(() => lowResSpDataPkt.ExpandProfileBlob(lowMass, highMass));
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
		int num = positions.Length;
		byte[] array = new byte[LowResDataStructSize * num];
		for (int i = 0; i < num; i++)
		{
			uint num2 = (uint)intensities[i];
			uint num3 = 0u;
			if (num2 > 8388607)
			{
				num2 >>= 8;
				num3 |= 0x80;
			}
			else
			{
				num3 &= 0xFFFFFF7Fu;
			}
			int num4 = i * LowResDataStructSize;
			array[num4] = Convert.ToByte(num3);
			array[num4 + 3] = (byte)((num2 >> 16) & 0xFF);
			array[num4 + 2] = (byte)((num2 >> 8) & 0xFF);
			array[num4 + 1] = (byte)(num2 & 0xFF);
			ushort num5 = (ushort)Math.Floor(positions[i]);
			ushort value = (ushort)((positions[i] - (double)(int)num5) * 65536.0);
			Buffer.BlockCopy(BitConverter.GetBytes(num5), 0, array, num4 + 4, 2);
			Buffer.BlockCopy(BitConverter.GetBytes(value), 0, array, num4 + 6, 2);
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
		_profileBlob = viewer.ReadBytes(dataOffset, num);
		return num;
	}

	/// <summary>
	/// Expands the profile BLOB.
	/// </summary>
	/// <param name="lowMass">
	/// The low Mass.
	/// </param>
	/// <param name="highMass">
	/// The high Mass.
	/// </param>
	/// <returns>
	/// The profile data
	/// </returns>
	private List<SegmentData> ExpandProfileBlob(double lowMass, double highMass)
	{
		List<SegmentData> list = new List<SegmentData>(1);
		List<DataPeak> list2 = new List<DataPeak>(_numIndexStruct);
		for (int i = 0; i < _numIndexStruct; i++)
		{
			int num = i * LowResDataStructSize;
			byte num2 = _profileBlob[num];
			uint num3 = (uint)((_profileBlob[num + 3] << 16) + (_profileBlob[num + 2] << 8) + _profileBlob[num + 1]);
			ushort num4 = BitConverter.ToUInt16(_profileBlob, num + 4);
			ushort num5 = BitConverter.ToUInt16(_profileBlob, num + 6);
			if ((num2 & 0x80) != 0)
			{
				num3 <<= 8;
			}
			DataPeak item = new DataPeak((double)(int)num4 + (double)(int)num5 / 65536.0, num3);
			if ((num2 & 1) != 0)
			{
				item.Options = PeakOptions.Saturated;
			}
			list2.Add(item);
		}
		list.Add(new SegmentData
		{
			DataPeaks = list2,
			MassRange = new MassRangeStruct(lowMass, highMass)
		});
		_profileBlob = null;
		return list;
	}
}
