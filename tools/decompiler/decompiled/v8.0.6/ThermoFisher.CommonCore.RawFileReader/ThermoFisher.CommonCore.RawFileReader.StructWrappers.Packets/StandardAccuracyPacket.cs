using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// The standard accuracy packet.
/// </summary>
internal sealed class StandardAccuracyPacket : SimplePacketBase, IMsPacket, IPacket, IRawObjectBase
{
	private static readonly int SizeOfStandardAccuracyStruct = Utilities.StructSizeLookup.Value[24];

	private readonly double[] _massCalibrators;

	private readonly int _numberOfPackets;

	private byte[] _profileBlob;

	/// <summary>
	/// find the byte length of a scan
	/// </summary>
	/// <param name="scanIndex">index (used to calcualate the scan size)</param>
	/// <returns>bytes used for this scan</returns>
	public static long Size(IMsScanIndexAccess scanIndex)
	{
		return SizeOfStandardAccuracyStruct * (scanIndex.PacketCount + 1) / 2;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.StandardAccuracyPacket" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">Offset from start of memory reader</param>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="massCalibrators">if set to <c>true</c> [include reference peaks].</param>
	public StandardAccuracyPacket(IMemoryReader viewer, long dataOffset, IMsScanIndexAccess scanIndex, int fileRevision, double[] massCalibrators)
	{
		StandardAccuracyPacket standardAccuracyPacket = this;
		_massCalibrators = massCalibrators;
		_numberOfPackets = scanIndex.PacketCount;
		double lowMass = scanIndex.LowMass;
		double highMass = scanIndex.HighMass;
		base.LazySegmentPeaks = new Lazy<List<SegmentData>>(() => standardAccuracyPacket.ExpandProfileBlob(lowMass, highMass));
		Load(viewer, dataOffset, fileRevision);
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
		int num = SizeOfStandardAccuracyStruct * (_numberOfPackets + 1) / 2;
		_profileBlob = viewer.ReadBytes(dataOffset, num);
		return num;
	}

	/// <summary>
	/// Converts the specified number of packets into (mass, intensity).
	/// </summary>
	/// <param name="numberOfPackets">The number of (mass, intensity) packets.</param>
	/// <param name="sizeOfStandardAccuracyStruct">The size of high resolution spectrum type structure.</param>
	/// <param name="massCalibrators">mass calibration data</param>
	/// <param name="profileBlob">The profile blob.</param>
	/// <returns>Data for the scan</returns>
	private static List<DataPeak> Convert(int numberOfPackets, int sizeOfStandardAccuracyStruct, double[] massCalibrators, byte[] profileBlob)
	{
		List<DataPeak> list = new List<DataPeak>(numberOfPackets);
		int num = 0;
		int num2 = massCalibrators.Length - 1;
		for (int i = 0; i < numberOfPackets; i++)
		{
			double intensity;
			double num6;
			if ((i & 1) != 0)
			{
				ushort num3 = profileBlob[num + 2];
				intensity = ((profileBlob[num + 3] << 5) + (num3 >> 3)) * (1 << ((num3 & 7) << 1));
				uint num4 = profileBlob[num + 7];
				uint num5 = profileBlob[num + 8];
				num6 = (double)((uint)((profileBlob[num + 9] << 16) + (int)(num5 << 8)) + num4) / 1024.0;
				num += sizeOfStandardAccuracyStruct;
			}
			else
			{
				ushort num7 = profileBlob[num];
				intensity = ((profileBlob[num + 1] << 5) + (num7 >> 3)) * (1 << ((num7 & 7) << 1));
				uint num8 = profileBlob[num + 4];
				uint num9 = profileBlob[num + 5];
				num6 = (double)((uint)((profileBlob[num + 6] << 16) + (int)(num9 << 8)) + num8) / 1024.0;
			}
			double num10 = massCalibrators[num2];
			int num11 = num2;
			while (num11 > 0)
			{
				num10 = num10 * num6 + massCalibrators[--num11];
			}
			DataPeak item = new DataPeak(num10, intensity);
			if (list.Count == 0 || num10 >= list[list.Count - 1].Position)
			{
				list.Add(item);
			}
		}
		return list;
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
		List<DataPeak> dataPeaks = Convert(_numberOfPackets, SizeOfStandardAccuracyStruct, _massCalibrators, _profileBlob);
		list.Add(new SegmentData
		{
			DataPeaks = dataPeaks,
			MassRange = new MassRangeStruct(lowMass, highMass)
		});
		_profileBlob = null;
		return list;
	}
}
