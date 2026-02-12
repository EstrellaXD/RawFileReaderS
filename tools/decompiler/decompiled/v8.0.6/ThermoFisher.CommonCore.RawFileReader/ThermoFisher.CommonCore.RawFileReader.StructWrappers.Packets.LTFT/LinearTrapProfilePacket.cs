using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.FTProfile;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

/// <summary>
/// The Linear Trap profile packet.
/// </summary>
internal class LinearTrapProfilePacket : AdvancedPacketBase
{
	private static int _sizeOfProfileSegmentStruct = Marshal.SizeOf(typeof(ProfileSegmentStruct));

	private static int _sizeOfProfileSubsegmentStruct = Marshal.SizeOf(typeof(ProfileSubsegmentStruct));

	/// <summary>
	/// Gets the segmented peaks. retrieve a DataPeak from the profile packet buffer.
	/// </summary>
	public override List<SegmentData> SegmentPeaks => base.SegmentPeakList;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.LinearTrapProfilePacket" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="offset">offset from start of the memory reader for this scan</param>
	/// <param name="fileRevision">Raw file version</param>
	/// <param name="includeRefPeaks">The include ref peaks.</param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required</param>
	/// <param name="zeroPadding">forces zero values between profile peaks</param>
	public LinearTrapProfilePacket(IMemoryReader viewer, long offset, int fileRevision, bool includeRefPeaks = false, PacketFeatures packetScanDataFeatures = PacketFeatures.All, bool zeroPadding = true)
		: base(viewer, offset, fileRevision, includeRefPeaks, packetScanDataFeatures)
	{
		if ((packetScanDataFeatures & PacketFeatures.Profile) != PacketFeatures.None)
		{
			ExpandProfileData(zeroPadding);
		}
	}

	/// <summary>
	/// Copies the lt profile to packet buffer.
	/// </summary>
	/// <param name="totalProfilePoints">The total profile points.</param>
	/// <param name="segProfilePointsCounter">The segmented profile points counter.</param>
	/// <param name="profileSegmentInfo">The profile segment information.</param>
	/// <param name="bytes">The bytes.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="compProfileSubSegmentsNumWords">The comp profile sub segments number words.</param>
	/// <param name="compProfilePoints">The comp profile points.</param>
	/// <returns>The number of bytes copied to the packet buffer.</returns>
	private static int CopyLtProfileToPacketBuffer(int totalProfilePoints, int[] segProfilePointsCounter, ProfileSegmentStruct[] profileSegmentInfo, byte[] bytes, int dataOffset, uint[] compProfileSubSegmentsNumWords, float[][] compProfilePoints)
	{
		int num = dataOffset;
		int num2 = segProfilePointsCounter.Length;
		if (totalProfilePoints > 0)
		{
			uint[] array = new uint[2];
			for (int i = 0; i < num2; i++)
			{
				int num3 = segProfilePointsCounter[i] * 4;
				Buffer.BlockCopy(WriterHelper.StructToByteArray(profileSegmentInfo[i], AdvancedPacketBase.ProfileSegmentStructSize), 0, bytes, num, AdvancedPacketBase.ProfileSegmentStructSize);
				num += AdvancedPacketBase.ProfileSegmentStructSize;
				array[0] = 0u;
				array[1] = compProfileSubSegmentsNumWords[i];
				Buffer.BlockCopy(array, 0, bytes, num, 8);
				num += 8;
				Buffer.BlockCopy(compProfilePoints[i], 0, bytes, num, num3);
				num += num3;
			}
		}
		return num - dataOffset;
	}

	/// <summary>
	/// Compresses the LT Profiles.
	/// The following algorithm assumes that:
	/// - The packet buffer is not compressed (ALL packets in the DataPeak buffer match one in the packet buffer).
	/// - Each segment contains exactly one sub segment which in turn contains all data points of this segment.
	/// - The position of a DataPeak and a packet share the same unit (mass), i.e. no conversion (e.g. mass to frequency) is necessary.
	/// - The low mass of the segment's mass range is == the base abscissa of the segment.
	/// - The position of the first packet in the mass range == base abscissa + 1 * abscissa spacing.
	/// <para />
	/// This is true with Endeavor spectra (up to now, at least) but not with spectra from
	/// Jupiter (does compression, stores frequencies, multiple sub segments).
	/// Therefore this function must be re-implemented in the LT_PROF packet class.
	/// </summary>
	/// <param name="instData">The mass spec instrument data.</param>
	/// <returns>The compressed packet in byte array. </returns>
	/// <exception cref="T:System.NotImplementedException">Compress Profile</exception>
	public static byte[] CompressProfiles(IMsInstrumentData instData)
	{
		byte[] array = Array.Empty<byte>();
		ISegmentedScanAccess segmentedScanAccess = instData.ScanData;
		bool flag = false;
		if (WriterHelper.HasNoScan(segmentedScanAccess))
		{
			segmentedScanAccess = AdvancedPacketBase.CreateAnEmptySegmentedScan();
			flag = true;
		}
		uint segmentCount = (uint)segmentedScanAccess.SegmentCount;
		NoiseAndBaseline[] array2 = instData.NoiseData ?? Array.Empty<NoiseAndBaseline>();
		double[] array3 = segmentedScanAccess.Intensities ?? Array.Empty<double>();
		double[] array4 = segmentedScanAccess.Positions ?? Array.Empty<double>();
		ReadOnlyCollection<IRangeAccess> massRanges = segmentedScanAccess.MassRanges;
		bool hasWidths;
		ThermoFisher.CommonCore.Data.Business.LabelPeak[] labelPeaks = AdvancedPacketBase.GetLabelPeaks(instData.CentroidData ?? new CentroidStream(), out hasWidths);
		int num = 0;
		int num2 = 0;
		int numNoisePackets = array2.Length;
		int[] array5 = new int[segmentCount];
		for (int i = 0; i < segmentCount; i++)
		{
			num2 += (array5[i] = segmentedScanAccess.SegmentLengths[i]);
		}
		if (num2 > 0 || flag)
		{
			ProfileSegmentStruct[] array6 = new ProfileSegmentStruct[segmentCount];
			float[][] array7 = new float[segmentCount][];
			uint[] array8 = new uint[segmentCount];
			for (int j = 0; j < segmentCount; j++)
			{
				int num3 = array5[j];
				array7[j] = new float[array5[j]];
				array6[j].NumSubSegments = 1u;
				array6[j].NumExpandedWords = (uint)num3;
				if (num3 > 0)
				{
					double low = massRanges[j].Low;
					array6[j].BaseAbscissa = low;
					array6[j].AbscissaSpacing = array4[0] - low;
				}
				array8[j] = (uint)num3;
				int num4 = 0;
				for (int k = 0; k < num3; k++)
				{
					array7[j][num4++] = (float)array3[k];
				}
				num++;
			}
			int[] array9 = new int[segmentCount];
			int num5 = labelPeaks.Length;
			SimpleScan simpleScan = new SimpleScan
			{
				Intensities = new double[num5],
				Masses = new double[num5]
			};
			double[] intensities = simpleScan.Intensities;
			double[] masses = simpleScan.Masses;
			for (int l = 0; l < num5; l++)
			{
				ThermoFisher.CommonCore.Data.Business.LabelPeak labelPeak = labelPeaks[l];
				masses[l] = labelPeak.Mass;
				intensities[l] = labelPeak.Intensity;
			}
			AdvancedPacketBase.ExtractLabelsInfo(labelPeaks, hasWidths, out var features, out var widths);
			AdvancedPacketBase.CalculateLabelsPerSegment(segmentCount, new ReadOnlyCollection<ThermoFisher.CommonCore.Data.Business.LabelPeak>(labelPeaks), massRanges, array9);
			PacketHeaderStruct packetHeaderInfo = AdvancedPacketBase.CreatePacketHeader(segmentCount, num5, num5, numNoisePackets, num2, num, hasWidths, 65536u, 8);
			array = AdvancedPacketBase.CreatePacketBuffer(segmentCount, packetHeaderInfo);
			int num6 = 0;
			num6 += AdvancedPacketBase.CopyPacketHeaderToPacketBuffer(packetHeaderInfo, array, num6);
			num6 += AdvancedPacketBase.CopyMassRangesToPacketBuffer(massRanges, array, num6);
			num6 += CopyLtProfileToPacketBuffer(num2, array5, array6, array, num6, array8, array7);
			num6 += AdvancedPacketBase.CopyCentroidToPacketBuffer(new ReadOnlyCollection<int>(array9), segmentCount, simpleScan, array, num6);
			num6 += AdvancedPacketBase.CopyLabelsToPacketBuffer(num5, array, num6, features, hasWidths, widths);
			num6 += AdvancedPacketBase.CopyNoiseInfoToPacketBuffer(numNoisePackets, array2, array, num6);
			AdvancedPacketBase.CopyExtensions(instData.ExtendedData, array, num6);
		}
		return array;
	}

	/// <summary>
	/// Expands the profile data.
	/// </summary>
	private void ExpandProfileData(bool zeroPadding)
	{
		if (base.Header.NumProfileWords == 0)
		{
			return;
		}
		byte[] profileData = base.ProfileData;
		uint numSegments = base.Header.NumSegments;
		int sizeOfProfileSegmentStruct = _sizeOfProfileSegmentStruct;
		int sizeOfProfileSubsegmentStruct = _sizeOfProfileSubsegmentStruct;
		int num = base.ProfileOffset;
		for (int i = 0; i < numSegments; i++)
		{
			double num2 = BitConverter.ToDouble(profileData, num);
			double num3 = BitConverter.ToDouble(profileData, num + 8);
			int num4 = BitConverter.ToInt32(profileData, num + 16);
			int num5 = BitConverter.ToInt32(profileData, num + 20);
			int j = 1;
			num += sizeOfProfileSegmentStruct;
			List<DataPeak> list = null;
			for (int k = 0; k < num4; k++)
			{
				int num6 = BitConverter.ToInt32(profileData, num) + 1;
				int num7 = BitConverter.ToInt32(profileData, num + 4);
				if (k == 0)
				{
					list = new List<DataPeak>(num7 * num4);
				}
				num += sizeOfProfileSubsegmentStruct;
				if (num6 > j)
				{
					if (zeroPadding)
					{
						for (; j < num6; j++)
						{
							list.Add(new DataPeak(num2 + (double)j * num3));
						}
					}
					else
					{
						j = num6;
					}
				}
				else if (num6 < j)
				{
					int num8 = j - num6;
					int index = list.Count - num8;
					list.RemoveRange(index, num8);
					j = num6;
				}
				int num9 = num7 / 3 * 3;
				int l;
				for (l = 0; l < num9; l += 3)
				{
					double num10 = num2 + (double)j * num3;
					list.Add(new DataPeak(num10, BitConverter.ToSingle(profileData, num)));
					num10 += num3;
					list.Add(new DataPeak(num10, BitConverter.ToSingle(profileData, num + 4)));
					list.Add(new DataPeak(num10 + num3, BitConverter.ToSingle(profileData, num + 8)));
					num += 12;
					j += 3;
				}
				for (; l < num7; l++)
				{
					list.Add(new DataPeak(num2 + (double)j * num3, BitConverter.ToSingle(profileData, num)));
					num += 4;
					j++;
				}
			}
			if (zeroPadding)
			{
				for (; j < num5; j++)
				{
					list.Add(new DataPeak(num2 + (double)j * num3));
				}
			}
			base.SegmentPeakList[i].DataPeaks = list;
		}
	}
}
