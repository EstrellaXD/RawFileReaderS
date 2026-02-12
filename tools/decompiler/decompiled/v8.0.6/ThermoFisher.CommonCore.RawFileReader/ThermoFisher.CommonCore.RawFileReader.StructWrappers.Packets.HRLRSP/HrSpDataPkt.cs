using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;

/// <summary>
/// The high resolution spectrum data packet.
/// </summary>
internal sealed class HrSpDataPkt : SimplePacketBase, IMsPacket, IPacket, IRawObjectBase
{
	private static readonly int HighResDataStructSize = Utilities.StructSizeLookup.Value[14];

	private static readonly byte[] FlagConvert = MakeByteFlagsTable();

	private readonly bool _includeRefPeaks;

	private readonly int _numIndexStruct;

	private byte[] _profileBlob;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP.HrSpDataPkt" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">offset from start of memory reader</param>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="includeRefPeaks">if set to <c>true</c> [include reference peaks].</param>
	public HrSpDataPkt(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess scanIndex, int fileRevision, bool includeRefPeaks = false)
	{
		HrSpDataPkt hrSpDataPkt = this;
		_includeRefPeaks = includeRefPeaks;
		_numIndexStruct = scanIndex.PacketCount;
		double lowMass = scanIndex.LowMass;
		double highMass = scanIndex.HighMass;
		base.LazySegmentPeaks = new Lazy<List<SegmentData>>(() => hrSpDataPkt.ExpandProfileBlob(lowMass, highMass));
		Load(viewer, dataOffset, fileRevision);
	}

	/// <summary>
	/// Compress the scan to binary format for writing to a raw file
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
		int highResDataStructSize = HighResDataStructSize;
		double[] intensities = segmentedScan.Intensities;
		double[] positions = segmentedScan.Positions;
		int num = positions.Length;
		PeakOptions[] flags = segmentedScan.Flags;
		byte[] array = new byte[highResDataStructSize * num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			byte b = 0;
			int num3 = num2;
			int num4 = num2 + 1;
			int destinationIndex = num2 + 4;
			int num5 = num2 + 11;
			int num6 = (int)intensities[i];
			if (num6 > 8388607)
			{
				num6 >>= 8;
				b |= 0x80;
			}
			array[num3] = b;
			array[num5] = FlagConvert[(int)(flags[i] & (PeakOptions.Saturated | PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Exception | PeakOptions.Reference | PeakOptions.Modified))];
			array[num4 + 2] = (byte)((num6 >> 16) & 0xFF);
			array[num4 + 1] = (byte)((num6 >> 8) & 0xFF);
			array[num4] = (byte)(num6 & 0xFF);
			Array.Copy(BitConverter.GetBytes(positions[i]), 2, array, destinationIndex, 6);
			num2 += highResDataStructSize;
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
		int num = HighResDataStructSize * _numIndexStruct;
		_profileBlob = viewer.ReadBytes(dataOffset, num);
		return num;
	}

	/// <summary>
	/// Converts the specified number index structure.
	/// </summary>
	/// <param name="numIndexStruct">The number index structure.</param>
	/// <param name="sizeOfHiResSpTypeStruct">The size of high resolution spectrum type structure.</param>
	/// <param name="includeRefPeaks">if set to <c>true</c> [include reference peaks].</param>
	/// <param name="profileBlob">The profile BLOB.</param>
	/// <returns>Data for the scan</returns>
	private static List<DataPeak> Convert(int numIndexStruct, int sizeOfHiResSpTypeStruct, bool includeRefPeaks, byte[] profileBlob)
	{
		List<DataPeak> list = new List<DataPeak>(numIndexStruct);
		for (int i = 0; i < numIndexStruct; i++)
		{
			int num = i * sizeOfHiResSpTypeStruct;
			int num2 = num + 1;
			int num3 = num + 2;
			int num4 = num + 3;
			byte b = profileBlob[num + 11];
			if (!includeRefPeaks && ((b & 0x10) != 0 || (b & 0x20) != 0))
			{
				continue;
			}
			byte num5 = profileBlob[num];
			int num6 = (profileBlob[num4] << 16) + (profileBlob[num3] << 8) + profileBlob[num2];
			profileBlob[num3] = 0;
			profileBlob[num4] = 0;
			double mass = BitConverter.ToDouble(profileBlob, num3);
			if ((num5 & 0x80) != 0)
			{
				num6 <<= 8;
			}
			DataPeak item = new DataPeak(mass, num6);
			bool flag = (num5 & 1) != 0;
			if ((b & 0x3F) != 0 || flag)
			{
				PeakOptions peakOptions = PeakOptions.None;
				if ((b & 4) != 0)
				{
					peakOptions |= PeakOptions.Merged;
				}
				if ((b & 1) != 0)
				{
					peakOptions |= PeakOptions.Saturated;
				}
				if ((b & 2) != 0)
				{
					peakOptions |= PeakOptions.Fragmented;
				}
				if ((b & 0x10) != 0)
				{
					peakOptions |= PeakOptions.Reference;
				}
				if ((b & 0x20) != 0)
				{
					peakOptions |= PeakOptions.Exception;
				}
				if ((b & 8) != 0)
				{
					peakOptions |= PeakOptions.Modified;
				}
				if (flag)
				{
					peakOptions |= PeakOptions.Saturated;
				}
				item.Options = peakOptions;
			}
			list.Add(item);
		}
		return list;
	}

	/// <summary>
	/// Map peak options to flag bytes, with a static table
	/// </summary>
	/// <returns>
	/// The table of bytes for each combination of options
	/// </returns>
	private static byte[] MakeByteFlagsTable()
	{
		byte[] array = new byte[64];
		for (int i = 0; i < 64; i++)
		{
			byte b = 0;
			if (((ulong)i & 2uL) != 0L)
			{
				b |= 2;
			}
			if (((ulong)i & 4uL) != 0L)
			{
				b |= 4;
			}
			if (((ulong)i & 1uL) != 0L)
			{
				b |= 1;
			}
			if (((ulong)i & 8uL) != 0L)
			{
				b |= 0x20;
			}
			if (((ulong)i & 0x10uL) != 0L)
			{
				b |= 0x10;
			}
			if (((ulong)i & 0x20uL) != 0L)
			{
				b |= 8;
			}
			array[i] = b;
		}
		return array;
	}

	/// <summary>
	/// Expands the profile blob.
	/// </summary>
	/// <param name="lowMass">The low mass.</param>
	/// <param name="highMass">The high mass.</param>
	/// <returns>The converted data</returns>
	private List<SegmentData> ExpandProfileBlob(double lowMass, double highMass)
	{
		List<SegmentData> list = new List<SegmentData>(1);
		if (_profileBlob == null || _profileBlob.Length == 0)
		{
			return list;
		}
		List<DataPeak> dataPeaks = Convert(_numIndexStruct, HighResDataStructSize, _includeRefPeaks, _profileBlob);
		list.Add(new SegmentData
		{
			DataPeaks = dataPeaks,
			MassRange = new MassRangeStruct(lowMass, highMass)
		});
		_profileBlob = null;
		return list;
	}

	/// <summary>
	/// find the byte length of a scan
	/// </summary>
	/// <param name="scanIndex">index (used to calculate the scan size)</param>
	/// <returns>bytes used for this scan</returns>
	public static long Size(IMsScanIndexAccess scanIndex)
	{
		return HighResDataStructSize * scanIndex.PacketCount;
	}
}
