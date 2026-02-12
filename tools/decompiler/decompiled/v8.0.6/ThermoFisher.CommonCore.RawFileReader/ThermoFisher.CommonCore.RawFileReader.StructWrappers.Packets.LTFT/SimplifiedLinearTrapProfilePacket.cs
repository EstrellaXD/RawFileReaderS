using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.FTProfile;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

/// <summary>
/// The Linear Trap profile packet.
/// </summary>
internal class SimplifiedLinearTrapProfilePacket : AdvancedPacketBase, ISimpleMsPacket
{
	private struct ProfileSegment
	{
		public readonly double BaseAbscissa;

		public readonly double AbscissaSpacing;

		public readonly uint NumSubSegments;

		/// <summary>
		/// The number of masses in a segment?
		/// </summary>
		private uint NumExpandedWords;

		public ProfileSegment(double baseAbscissa, double abscissaSpacing, int numSubSegments, int numMassesInSegments)
		{
			this = default(ProfileSegment);
			BaseAbscissa = baseAbscissa;
			AbscissaSpacing = abscissaSpacing;
			NumSubSegments = (uint)numSubSegments;
			NumExpandedWords = (uint)numMassesInSegments;
		}
	}

	private static readonly int _sizeOfProfileSegmentStruct = Marshal.SizeOf(typeof(ProfileSegmentStruct));

	private static readonly int _sizeOfProfileSubsegmentStruct = Marshal.SizeOf(typeof(ProfileSubsegmentStruct));

	/// <summary>
	/// Gets the segmented peaks. retrieve a DataPeak from the profile packet buffer.
	/// </summary>
	public override List<SegmentData> SegmentPeaks => base.SegmentPeakList;

	public double[] Mass { get; set; }

	public double[] Intensity { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.SimplifiedLinearTrapProfilePacket" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="offset">offset from start of the memory reader for this scan</param>
	/// <param name="fileRevision">Raw file version</param>
	/// <param name="includeRefPeaks">The include ref peaks.</param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required</param>
	public SimplifiedLinearTrapProfilePacket(IMemoryReader viewer, long offset, int fileRevision, bool includeRefPeaks = false, PacketFeatures packetScanDataFeatures = PacketFeatures.All)
		: base(viewer, offset, fileRevision, includeRefPeaks, packetScanDataFeatures)
	{
		if ((packetScanDataFeatures & PacketFeatures.Profile) != PacketFeatures.None)
		{
			ExpandProfileData();
		}
	}

	/// <summary>
	/// Expands the profile data.
	/// Fills the simplified mass/intensity arrays
	/// </summary>
	private void ExpandProfileData()
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
		Span<ProfileSegment> span = stackalloc ProfileSegment[(int)numSegments];
		int num2 = 0;
		for (int i = 0; i < numSegments; i++)
		{
			double baseAbscissa = BitConverter.ToDouble(profileData, num);
			double abscissaSpacing = BitConverter.ToDouble(profileData, num + 8);
			int num3 = BitConverter.ToInt32(profileData, num + 16);
			int numMassesInSegments = BitConverter.ToInt32(profileData, num + 20);
			int num4 = 1;
			span[i] = new ProfileSegment(baseAbscissa, abscissaSpacing, num3, numMassesInSegments);
			num += sizeOfProfileSegmentStruct;
			for (int j = 0; j < num3; j++)
			{
				uint num5 = BitConverter.ToUInt32(profileData, num) + 1;
				uint num6 = BitConverter.ToUInt32(profileData, num + 4);
				num += sizeOfProfileSubsegmentStruct;
				long num7 = 0L;
				if (num5 >= num4)
				{
					num4 = (int)num5;
				}
				else
				{
					num7 = num4 - num5;
				}
				num2 += (int)num6 - (int)num7;
				num += (int)(4 * num6);
				num4 += (int)num6;
			}
		}
		double[] array = new double[num2];
		double[] array2 = new double[num2];
		num = base.ProfileOffset;
		int num8 = 0;
		for (int k = 0; k < numSegments; k++)
		{
			int num9 = 1;
			ProfileSegment profileSegment = span[k];
			double baseAbscissa2 = profileSegment.BaseAbscissa;
			double abscissaSpacing2 = profileSegment.AbscissaSpacing;
			num += sizeOfProfileSegmentStruct;
			for (int l = 0; l < profileSegment.NumSubSegments; l++)
			{
				uint num10 = BitConverter.ToUInt32(profileData, num) + 1;
				uint num11 = BitConverter.ToUInt32(profileData, num + 4);
				num += sizeOfProfileSubsegmentStruct;
				if (num10 >= num9)
				{
					num9 = (int)num10;
				}
				else
				{
					long num12 = num9 - num10;
					num8 -= (int)num12;
					if (num8 < 0)
					{
						num8 = 0;
					}
				}
				uint num13 = num11 / 3 * 3;
				int m;
				for (m = 0; m < num13; m += 3)
				{
					double num14 = (array[num8] = baseAbscissa2 + (double)num9 * abscissaSpacing2);
					array2[num8++] = BitConverter.ToSingle(profileData, num);
					num14 = (array[num8] = num14 + abscissaSpacing2);
					array2[num8++] = BitConverter.ToSingle(profileData, num + 4);
					array[num8] = num14 + abscissaSpacing2;
					array2[num8++] = BitConverter.ToSingle(profileData, num + 8);
					num += 12;
					num9 += 3;
				}
				for (; m < num11; m++)
				{
					array[num8] = baseAbscissa2 + (double)num9 * abscissaSpacing2;
					array2[num8++] = BitConverter.ToSingle(profileData, num);
					num += 4;
					num9++;
				}
			}
			Mass = array;
			Intensity = array2;
		}
	}
}
