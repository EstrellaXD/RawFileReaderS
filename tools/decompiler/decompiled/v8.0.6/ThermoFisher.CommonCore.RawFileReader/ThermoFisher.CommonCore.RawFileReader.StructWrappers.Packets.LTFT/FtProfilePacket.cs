using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.FTProfile;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

/// <summary>
/// The FTMS profile packet.
/// </summary>
internal sealed class FtProfilePacket : AdvancedPacketBase
{
	/// <summary>
	/// The create data peak delegate is used to convert from un-calibrated data
	/// to calibrated mass/intensity values
	/// </summary>
	/// <param name="massOffset">
	/// The mass offset.
	/// </param>
	/// <param name="freq">The frequency</param>
	/// <returns>The mass converted peak</returns>
	private delegate double CreateDataPeakDelegate(float massOffset, double freq);

	/// <summary>
	/// The half zero packets.
	/// </summary>
	private const int HalfZeroPackets = 4;

	private readonly double _coeff1;

	private readonly double _coeff2;

	private readonly double _coeff3;

	private int _isProfileBlobLoaded;

	/// <summary>
	/// Gets the segment peaks.
	/// </summary>
	public override List<SegmentData> SegmentPeaks
	{
		get
		{
			if (Interlocked.CompareExchange(ref _isProfileBlobLoaded, 1, 0) == 0)
			{
				ExpandProfileBlob();
			}
			return base.SegmentPeakList;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.FtProfilePacket" /> class.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="offset">offset from start of the memory reader for this scan</param>
	/// <param name="calibrators">
	/// Delegate to get the mass calibrators, whne profiles are needed
	/// </param>
	/// <param name="fileRevision">Raw file version</param>
	/// <param name="includeRefPeaks">
	/// The include ref peaks.
	/// </param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required</param>
	/// <exception cref="T:System.Exception">
	/// Thrown if not enough mass calibrators was passed.
	/// </exception>
	public FtProfilePacket(IMemoryReader viewer, long offset, Func<double[]> calibrators, int fileRevision, bool includeRefPeaks = false, PacketFeatures packetScanDataFeatures = PacketFeatures.All)
		: base(viewer, offset, fileRevision, includeRefPeaks, packetScanDataFeatures)
	{
		if ((packetScanDataFeatures & PacketFeatures.Profile) != PacketFeatures.None)
		{
			double[] array = calibrators();
			int num = array.Length;
			if (num < 4)
			{
				throw new Exception("Not enough mass calibration coefficients!");
			}
			if (num >= 5)
			{
				_coeff1 = array[2];
				_coeff2 = array[3];
				_coeff3 = array[4];
			}
			else if (num == 4)
			{
				_coeff1 = array[2];
				_coeff2 = array[3];
			}
		}
		_isProfileBlobLoaded = 0;
	}

	/// <summary>
	/// Compresses the FT Profiles.
	/// The following algorithm assumes that:
	/// - The packet buffer is not compressed (ALL packets in the DataPeak buffer match one in the packet buffer).
	/// - Each segment contains exactly one sub segment which in turn contains all data points of this segment.
	/// - The position of a DataPeak and a packet share the same unit (mass), i.e. no conversion (e.g. mass to frequency) is necessary.
	/// - The low mass of the segment's mass range is == the base abscissa of the segment.
	/// - The position of the first packet in the mass range == base abscissa + 1 * abscissa spacing.
	/// <para />
	/// This is true with Endeavor spectra (up to now, at least) but not with spectra from
	/// Jupiter (does compression, stores frequencies, multiple sub segments).
	/// Therefore this function must be re-implemented in the FT_PROF packet class.
	/// </summary>
	/// <param name="instData">The mass spec instrument data.</param>
	/// <returns>The compressed packet in byte array. </returns>
	/// <exception cref="T:System.NotImplementedException">Compress Profile</exception>
	public static byte[] CompressProfiles(IMsInstrumentData instData)
	{
		IScanEvent eventData = instData.EventData;
		ISegmentedScanAccess segmentedScanAccess = instData.ScanData;
		if (WriterHelper.HasNoScan(segmentedScanAccess))
		{
			segmentedScanAccess = AdvancedPacketBase.CreateAnEmptySegmentedScan();
		}
		uint segmentCount = (uint)segmentedScanAccess.SegmentCount;
		NoiseAndBaseline[] noiseData = instData.NoiseData;
		double[] array = segmentedScanAccess.Intensities ?? Array.Empty<double>();
		double[] array2 = segmentedScanAccess.Positions ?? Array.Empty<double>();
		ReadOnlyCollection<IRangeAccess> massRanges = segmentedScanAccess.MassRanges;
		double[] array3 = instData.Frequencies ?? Array.Empty<double>();
		bool hasWidths;
		ThermoFisher.CommonCore.Data.Business.LabelPeak[] labelPeaks = AdvancedPacketBase.GetLabelPeaks(instData.CentroidData, out hasWidths);
		int[] array4 = new int[segmentCount];
		int[] array5 = new int[segmentCount];
		ProfileSubsegment[][] array6 = new ProfileSubsegment[segmentCount][];
		uint[][] array7 = new uint[segmentCount][];
		uint num = 0u;
		int num2 = 0;
		int num3 = 0;
		int numNoisePackets = ((noiseData != null) ? noiseData.Length : 0);
		double coeff = 0.0;
		double coeff2 = 0.0;
		double coeff3 = 0.0;
		double coeff4 = 0.0;
		AdvancedPacketBase.GetCoeffValues(eventData, ref coeff, ref coeff2, ref coeff3, ref coeff4);
		for (int i = 0; i < segmentCount; i++)
		{
			int num4 = segmentedScanAccess.SegmentLengths[i];
			uint[] array8 = (array7[i] = new uint[num4]);
			ProfileSubsegment[] array9 = (array6[i] = (from h in Enumerable.Repeat(0, num4)
				select new ProfileSubsegment(0u, 0f)).ToArray());
			bool flag = false;
			int num5 = 0;
			double num6 = 0.0;
			uint num7 = 0u;
			while (num7 < num4)
			{
				double num8 = array3[num];
				double num9 = num8 * num8;
				double num10 = coeff2 / num8 + coeff3 / num9 + coeff4 / (num9 * num9);
				double num11 = array2[num] - num10;
				bool flag2 = num11 - num6 > 5E-05;
				if (flag)
				{
					if (array[num] > 0.0)
					{
						if (flag2)
						{
							array4[i] += (int)array9[num5].ProfilePoints;
							array9[num5].MassOffset = (float)num6;
							num5++;
							array8[num5] = num;
							array9[num5].ProfilePoints++;
							num6 = num11;
						}
						else
						{
							array9[num5].ProfilePoints++;
						}
					}
					else
					{
						array4[i] += (int)array9[num5].ProfilePoints;
						num5++;
						flag = false;
					}
				}
				else if (array[num] > 0.0)
				{
					array8[num5] = num;
					array9[num5].MassOffset = (float)num6;
					array9[num5].ProfilePoints++;
					num6 = num11;
					flag = true;
				}
				num7++;
				num++;
			}
			if (flag)
			{
				array4[i] += (int)array9[num5].ProfilePoints;
			}
			if (array4[i] != 0)
			{
				array5[i] = (flag ? (num5 + 1) : num5);
				num2 += array5[i];
				num3 += array4[i];
			}
		}
		float[][] compProfilePoints = Array.Empty<float[]>();
		Tuple<uint[][], ProfileSubsegment[][]> compBufferProfileSubSegments = new Tuple<uint[][], ProfileSubsegment[][]>(new uint[segmentCount][], new ProfileSubsegment[segmentCount][]);
		ProfileSegmentStruct[] profileSegmentInfo = new ProfileSegmentStruct[segmentCount];
		if (num3 > 0)
		{
			ComputeProfilePoints(array5, compBufferProfileSubSegments, array4, profileSegmentInfo, array3, array6, array7, array, out compProfilePoints);
		}
		int num12 = labelPeaks.Length;
		SimpleScan simpleScan = new SimpleScan
		{
			Intensities = new double[num12],
			Masses = new double[num12]
		};
		int[] array10 = new int[segmentCount];
		double[] intensities = simpleScan.Intensities;
		double[] masses = simpleScan.Masses;
		for (int num13 = 0; num13 < num12; num13++)
		{
			ThermoFisher.CommonCore.Data.Business.LabelPeak labelPeak = labelPeaks[num13];
			masses[num13] = labelPeak.Mass;
			intensities[num13] = labelPeak.Intensity;
		}
		AdvancedPacketBase.ExtractLabelsInfo(labelPeaks, hasWidths, out var features, out var widths);
		AdvancedPacketBase.CalculateLabelsPerSegment(segmentCount, labelPeaks, massRanges, array10);
		PacketHeaderStruct packetHeaderInfo = AdvancedPacketBase.CreatePacketHeader(segmentCount, num12, num12, numNoisePackets, num3, num2, hasWidths, 65664u, 12, instData.ExtendedData);
		byte[] array11 = AdvancedPacketBase.CreatePacketBuffer(segmentCount, packetHeaderInfo);
		int num14 = 0;
		num14 += AdvancedPacketBase.CopyPacketHeaderToPacketBuffer(packetHeaderInfo, array11, num14);
		num14 += AdvancedPacketBase.CopyMassRangesToPacketBuffer(massRanges, array11, num14);
		num14 += CopyFtProfileToPacketBuffer(num3, array5, compBufferProfileSubSegments, compProfilePoints, profileSegmentInfo, array11, num14);
		num14 += AdvancedPacketBase.CopyCentroidToPacketBuffer(new ReadOnlyCollection<int>(array10), segmentCount, simpleScan, array11, num14);
		num14 += AdvancedPacketBase.CopyLabelsToPacketBuffer(num12, array11, num14, features, hasWidths, widths);
		num14 += AdvancedPacketBase.CopyNoiseInfoToPacketBuffer(numNoisePackets, noiseData, array11, num14);
		AdvancedPacketBase.CopyExtensions(instData.ExtendedData, array11, num14);
		return array11;
	}

	/// <summary>
	/// add zeros with correction for out of order mass
	/// </summary>
	/// <param name="calibratedMassDelegate">
	/// The calibrated mass delegate.
	/// </param>
	/// <param name="baseAbscissa">
	/// The base abscissa.
	/// </param>
	/// <param name="abscissaSpacing">
	/// The abscissa spacing.
	/// </param>
	/// <param name="dataPeaks">
	/// The data peaks.
	/// </param>
	/// <param name="startPacketIndex">
	/// The start packet index.
	/// </param>
	/// <param name="massOffset">
	/// The mass offset.
	/// </param>
	/// <param name="minMass">
	/// The min mass.
	/// </param>
	/// <param name="freq">
	/// The frequency.
	/// </param>
	/// <param name="endPacket">
	/// The end packet.
	/// </param>
	/// <returns>
	/// The mass of the last zero
	/// </returns>
	private static double AddZerosWithCorrection(CreateDataPeakDelegate calibratedMassDelegate, double baseAbscissa, double abscissaSpacing, List<DataPeak> dataPeaks, int startPacketIndex, float massOffset, ref double minMass, double freq, int endPacket)
	{
		double num = (minMass = IncreaseMass(minMass));
		dataPeaks.Add(new DataPeak(num, freq, noIntensityOverload: true));
		while (startPacketIndex < endPacket)
		{
			freq = baseAbscissa + (double)startPacketIndex++ * abscissaSpacing;
			num = calibratedMassDelegate(massOffset, freq);
			if (num <= minMass)
			{
				num = (minMass = IncreaseMass(minMass));
			}
			dataPeaks.Add(new DataPeak(num, freq, noIntensityOverload: true));
		}
		return num;
	}

	/// <summary>
	/// Computes the profile points.
	/// </summary>
	/// <param name="subSegmentsCounter">The sub segments counter.</param>
	/// <param name="compBufferProfileSubSegments">The comp buffer profile sub segments.</param>
	/// <param name="segSubSegProfilePointsCounter">The segment's sub segment profile points counter.</param>
	/// <param name="profileSegmentInfo">The profile segment information.</param>
	/// <param name="frequencies">The frequencies.</param>
	/// <param name="profileSubSegments">The profile sub segments.</param>
	/// <param name="startProfilePacketIndex">Start index of the profile packet.</param>
	/// <param name="intensities">The intensities.</param>
	/// <param name="compProfilePoints">The comp profile points.</param>
	private static void ComputeProfilePoints(int[] subSegmentsCounter, Tuple<uint[][], ProfileSubsegment[][]> compBufferProfileSubSegments, int[] segSubSegProfilePointsCounter, ProfileSegmentStruct[] profileSegmentInfo, double[] frequencies, ProfileSubsegment[][] profileSubSegments, uint[][] startProfilePacketIndex, double[] intensities, out float[][] compProfilePoints)
	{
		int num = subSegmentsCounter.Length;
		compProfilePoints = new float[num][];
		for (int i = 0; i < num; i++)
		{
			int num2 = subSegmentsCounter[i];
			compBufferProfileSubSegments.Item1[i] = new uint[num2];
			compBufferProfileSubSegments.Item2[i] = (from h in Enumerable.Repeat(0, num2)
				select new ProfileSubsegment(0u, 0f)).ToArray();
			compProfilePoints[i] = new float[segSubSegProfilePointsCounter[i]];
			InitializeProfileSegmentInfo(segSubSegProfilePointsCounter, i, profileSegmentInfo, num2, frequencies);
			ProfileSubsegment[] array = profileSubSegments[i];
			uint[] array2 = startProfilePacketIndex[i];
			uint[] array3 = compBufferProfileSubSegments.Item1[i];
			ProfileSubsegment[] array4 = compBufferProfileSubSegments.Item2[i];
			int num3 = 0;
			for (int num4 = 0; num4 < num2; num4++)
			{
				ProfileSegmentStruct profileSegmentStruct = profileSegmentInfo[i];
				double num5 = frequencies[array2[num4]];
				array3[num4] = (uint)(0.5 + (profileSegmentStruct.BaseAbscissa - num5) / (-1.0 * profileSegmentStruct.AbscissaSpacing));
				array4[num4].ProfilePoints = array[num4].ProfilePoints;
				array4[num4].MassOffset = array[num4].MassOffset;
				uint num6 = array2[num4];
				uint num7 = num6 + array[num4].ProfilePoints;
				for (uint num8 = num6; num8 != num7; num8++)
				{
					compProfilePoints[i][num3++] = (float)intensities[num8];
				}
			}
		}
	}

	/// <summary>
	/// Copies the FT profile to packet buffer.
	/// </summary>
	/// <param name="totalProfilePoints">The total profile points.</param>
	/// <param name="subSegmentsCounter">The sub segments counter.</param>
	/// <param name="compBufferProfileSubSegments">The comp buffer profile sub segments.</param>
	/// <param name="compProfilePoints">The comp profile points.</param>
	/// <param name="profileSegmentInfo">The profile segment information.</param>
	/// <param name="bytes">The bytes.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <returns>The number of bytes copied to the packet buffer.</returns>
	private static int CopyFtProfileToPacketBuffer(int totalProfilePoints, IReadOnlyList<int> subSegmentsCounter, Tuple<uint[][], ProfileSubsegment[][]> compBufferProfileSubSegments, IReadOnlyList<float[]> compProfilePoints, IReadOnlyList<ProfileSegmentStruct> profileSegmentInfo, byte[] bytes, int dataOffset)
	{
		int num = dataOffset;
		int count = subSegmentsCounter.Count;
		if (totalProfilePoints > 0)
		{
			for (int i = 0; i < count; i++)
			{
				int num2 = subSegmentsCounter[i];
				uint[] array = compBufferProfileSubSegments.Item1[i];
				ProfileSubsegment[] array2 = compBufferProfileSubSegments.Item2[i];
				float[] src = compProfilePoints[i];
				Buffer.BlockCopy(WriterHelper.StructToByteArray(profileSegmentInfo[i], AdvancedPacketBase.ProfileSegmentStructSize), 0, bytes, num, AdvancedPacketBase.ProfileSegmentStructSize);
				num += AdvancedPacketBase.ProfileSegmentStructSize;
				int num3 = 0;
				uint[] array3 = new uint[2];
				float[] array4 = new float[1];
				for (int j = 0; j < num2; j++)
				{
					ProfileSubsegment profileSubsegment = array2[j];
					uint profilePoints = profileSubsegment.ProfilePoints;
					int num4 = (int)(profilePoints * 4);
					array3[0] = array[j];
					array3[1] = profilePoints;
					array4[0] = profileSubsegment.MassOffset;
					Buffer.BlockCopy(array3, 0, bytes, num, 8);
					num += 8;
					Buffer.BlockCopy(array4, 0, bytes, num, 4);
					num += 4;
					Buffer.BlockCopy(src, num3, bytes, num, num4);
					num3 += num4;
					num += num4;
				}
			}
		}
		return num - dataOffset;
	}

	/// <summary>
	/// Ensure that as zero pad values are added, the mass increases.
	/// </summary>
	/// <param name="minMass">Minimum mass: The Mass of last accepted peak, or zero</param>
	/// <returns>New value for minimum mass</returns>
	private static double IncreaseMass(double minMass)
	{
		return minMass + 1E-05;
	}

	/// <summary>
	/// Initializes the profile segment information.
	/// </summary>
	/// <param name="segSubSegProfilePointsCounter">The segment's sub segment profile points counter.</param>
	/// <param name="segIndex">Index of the segment.</param>
	/// <param name="profileSegmentInfo">The profile segment information.</param>
	/// <param name="numSubSegments">The number sub segments.</param>
	/// <param name="frequencies">The frequencies.</param>
	private static void InitializeProfileSegmentInfo(int[] segSubSegProfilePointsCounter, int segIndex, ProfileSegmentStruct[] profileSegmentInfo, int numSubSegments, double[] frequencies)
	{
		if (segSubSegProfilePointsCounter[segIndex] > 0)
		{
			ProfileSegmentStruct profileSegmentStruct = profileSegmentInfo[segIndex];
			profileSegmentStruct.NumSubSegments = (uint)numSubSegments;
			profileSegmentStruct.BaseAbscissa = frequencies[0];
			profileSegmentStruct.AbscissaSpacing = frequencies[1] - profileSegmentStruct.BaseAbscissa;
			profileSegmentStruct.BaseAbscissa -= profileSegmentStruct.AbscissaSpacing;
			int num = frequencies.Length;
			double num2 = frequencies[num - 1];
			profileSegmentStruct.NumExpandedWords = (uint)(0.5 + (profileSegmentStruct.BaseAbscissa - num2) / (-1.0 * profileSegmentStruct.AbscissaSpacing));
			profileSegmentInfo[segIndex] = profileSegmentStruct;
		}
	}

	/// <summary>
	/// The method adds zero packets.
	/// </summary>
	/// <param name="calibratedMassDelegate">
	/// Delegate to create a mass from the peak index, using calibration data
	/// </param>
	/// <param name="baseAbscissa">
	/// The base abscissa (start frequency)
	/// </param>
	/// <param name="abscissaSpacing">
	/// The abscissa (frequency) spacing between samples
	/// </param>
	/// <param name="dataPeaks">
	/// The data peaks.
	/// </param>
	/// <param name="startPacketIndex">
	/// The start packet index.
	/// </param>
	/// <param name="endPacketIndex">
	/// The end packet index.
	/// </param>
	/// <param name="massOffSet">Mass offset of the profile segment
	/// </param>
	/// <param name="minMass">minimum valid mass, so that masses are not added out of order</param>
	/// <param name="isAppending">
	/// The is appending.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	private int AddZeroPackets(CreateDataPeakDelegate calibratedMassDelegate, double baseAbscissa, double abscissaSpacing, List<DataPeak> dataPeaks, int startPacketIndex, int endPacketIndex, float massOffSet, ref double minMass, bool isAppending = false)
	{
		int num = endPacketIndex - startPacketIndex;
		double num2 = minMass;
		if (num <= 8)
		{
			while (startPacketIndex < endPacketIndex)
			{
				double num3 = baseAbscissa + (double)startPacketIndex++ * abscissaSpacing;
				num2 = calibratedMassDelegate(massOffSet, num3);
				if (num2 <= minMass)
				{
					num2 = (minMass = IncreaseMass(minMass));
				}
				dataPeaks.Add(new DataPeak(num2, num3, noIntensityOverload: true));
			}
		}
		else
		{
			int num4 = startPacketIndex + 4;
			double num5 = baseAbscissa + (double)startPacketIndex++ * abscissaSpacing;
			num2 = calibratedMassDelegate(massOffSet, num5);
			if (num2 <= minMass)
			{
				num2 = AddZerosWithCorrection(calibratedMassDelegate, baseAbscissa, abscissaSpacing, dataPeaks, startPacketIndex, massOffSet, ref minMass, num5, num4);
			}
			else
			{
				dataPeaks.Add(new DataPeak(num2, num5, noIntensityOverload: true));
				while (startPacketIndex < num4)
				{
					num5 = baseAbscissa + (double)startPacketIndex++ * abscissaSpacing;
					num2 = calibratedMassDelegate(massOffSet, num5);
					dataPeaks.Add(new DataPeak(num2, num5, noIntensityOverload: true));
				}
			}
			if (isAppending)
			{
				startPacketIndex = endPacketIndex - 4 + 1;
				while (startPacketIndex <= endPacketIndex)
				{
					double num6 = baseAbscissa + (double)startPacketIndex++ * abscissaSpacing;
					num2 = calibratedMassDelegate(massOffSet, num6);
					if (num2 <= minMass)
					{
						num2 = (minMass = IncreaseMass(minMass));
					}
					dataPeaks.Add(new DataPeak(num2, num6, noIntensityOverload: true));
				}
				startPacketIndex = endPacketIndex + 1;
			}
			else
			{
				startPacketIndex = endPacketIndex - 4;
				double num7 = baseAbscissa + (double)startPacketIndex++ * abscissaSpacing;
				num2 = calibratedMassDelegate(massOffSet, num7);
				if (num2 <= minMass)
				{
					num2 = AddZerosWithCorrection(calibratedMassDelegate, baseAbscissa, abscissaSpacing, dataPeaks, startPacketIndex, massOffSet, ref minMass, num7, endPacketIndex);
				}
				else
				{
					dataPeaks.Add(new DataPeak(num2, num7, noIntensityOverload: true));
					while (startPacketIndex < endPacketIndex)
					{
						num7 = baseAbscissa + (double)startPacketIndex++ * abscissaSpacing;
						num2 = calibratedMassDelegate(massOffSet, num7);
						dataPeaks.Add(new DataPeak(num2, num7, noIntensityOverload: true));
					}
				}
				startPacketIndex = endPacketIndex;
			}
		}
		minMass = num2;
		return startPacketIndex;
	}

	/// <summary>
	/// Creates the data peak.
	/// </summary>
	/// <param name="massOffset">The mass offset.</param>
	/// <param name="freq">The frequency</param>
	/// <returns>The (frequency to mass) converted peak</returns>
	private double CalculateMass(float massOffset, double freq)
	{
		double num = freq * freq;
		return _coeff1 / freq + _coeff2 / num + _coeff3 / (num * num) + (double)massOffset;
	}

	/// <summary>
	/// Creates the data peak without coefficient 3.
	/// </summary>
	/// <param name="massOffset">The mass offset.</param>
	/// <param name="freq">The frequency</param>
	/// <returns>The (frequency to mass) converted peak</returns>
	private double CalculateMassWithoutCoeff3(float massOffset, double freq)
	{
		return (_coeff1 + _coeff2 / freq) / freq + (double)massOffset;
	}

	/// <summary>
	/// Expand the profile blob.
	/// </summary>
	private void ExpandProfileBlob()
	{
		if (base.Header.NumProfileWords == 0)
		{
			return;
		}
		double minMass = -1.0;
		byte[] profileData = base.ProfileData;
		int num = base.ProfileOffset;
		int referencePeakIndex = 0;
		uint numSegments = base.Header.NumSegments;
		CreateDataPeakDelegate createDataPeakDelegate = ((Math.Abs(_coeff3) < double.Epsilon) ? new CreateDataPeakDelegate(CalculateMassWithoutCoeff3) : new CreateDataPeakDelegate(CalculateMass));
		for (int i = 0; i < numSegments; i++)
		{
			ProfileSegmentStruct segment = new ProfileSegmentStruct
			{
				BaseAbscissa = BitConverter.ToDouble(profileData, num),
				AbscissaSpacing = BitConverter.ToDouble(profileData, num + 8),
				NumSubSegments = BitConverter.ToUInt32(profileData, num + 16),
				NumExpandedWords = BitConverter.ToUInt32(profileData, num + 20)
			};
			List<DataPeak> dataPeaks = new List<DataPeak>((int)(segment.NumSubSegments * 20));
			num += 24;
			int numExpandedWords = (int)segment.NumExpandedWords;
			Tuple<float, int, double, int, int> tuple = ProcessSubsegments(profileData, createDataPeakDelegate, segment, num, i, dataPeaks, referencePeakIndex, minMass);
			num = tuple.Item5;
			int item = tuple.Item2;
			minMass = tuple.Item3;
			referencePeakIndex = tuple.Item4;
			if (item < numExpandedWords)
			{
				AddZeroPackets(createDataPeakDelegate, segment.BaseAbscissa, segment.AbscissaSpacing, dataPeaks, item, numExpandedWords, tuple.Item1, ref minMass, isAppending: true);
			}
			base.SegmentPeakList[i].DataPeaks = dataPeaks;
		}
	}

	/// <summary>
	/// flag reference peaks with "reference" or "exception", where the profile is part of
	/// an identified reference.
	/// </summary>
	/// <param name="dataPeaks">
	/// The data peaks.
	/// </param>
	/// <param name="refPeak">
	/// The reference peak.
	/// </param>
	/// <param name="startPeakIndex">
	/// The start peak index.
	/// </param>
	/// <param name="numDataPeaks">
	/// The number of data peaks.
	/// </param>
	private void FlagReferencePeaks(List<DataPeak> dataPeaks, LabelPeak refPeak, int startPeakIndex, int numDataPeaks)
	{
		PeakOptions peakOptions = PeakOptions.None;
		if (refPeak.IsReference)
		{
			peakOptions |= PeakOptions.Reference;
		}
		if (refPeak.IsException)
		{
			peakOptions |= PeakOptions.Exception;
		}
		for (int i = startPeakIndex; i < numDataPeaks; i++)
		{
			DataPeak value = dataPeaks[i];
			if ((double)refPeak.Intensity >= value.Intensity)
			{
				if (!base.IncludeRefPeaks)
				{
					value.Intensity = 0.0;
				}
				value.Options |= peakOptions;
				dataPeaks[i] = value;
			}
		}
	}

	/// <summary>
	/// The method processes the sub-segments.
	/// </summary>
	/// <param name="profBlob">Byte array containing profiles</param>
	/// <param name="calibrateMassDelegate">Delegate to create a peak</param>
	/// <param name="segment">
	/// The segment.
	/// </param>
	/// <param name="blobIndex">
	/// The blob index.
	/// </param>
	/// <param name="segIndex">
	/// The segment index.
	/// </param>
	/// <param name="dataPeaks">
	/// The data peaks.
	/// </param>
	/// <param name="referencePeakIndex">
	/// The reference peak index.
	/// </param>
	/// <param name="minMass">Minimum mass (or last mass processed) to prevent out of order data</param>
	/// <returns>
	/// Item1: The mass offset of the last sub-segment. Item2: The profile index after the last added value
	/// Item3 the updated minMass
	/// Item4: the updated reference peak index
	/// </returns>
	private Tuple<float, int, double, int, int> ProcessSubsegments(byte[] profBlob, CreateDataPeakDelegate calibrateMassDelegate, ProfileSegmentStruct segment, int blobIndex, int segIndex, List<DataPeak> dataPeaks, int referencePeakIndex, double minMass)
	{
		int num = 1;
		float num2 = 0f;
		uint numSubSegments = segment.NumSubSegments;
		double baseAbscissa = segment.BaseAbscissa;
		double abscissaSpacing = segment.AbscissaSpacing;
		float[] array = new float[100];
		LabelPeak[] referencePeakArray = base.ReferencePeakArray;
		bool flag = numSubSegments == 1;
		int num3 = referencePeakArray.Length;
		for (int i = 0; i < numSubSegments; i++)
		{
			uint num4 = BitConverter.ToUInt32(profBlob, blobIndex);
			uint num5 = BitConverter.ToUInt32(profBlob, blobIndex + 4);
			blobIndex += 8;
			if (base.UseFtProfileSubSegment)
			{
				num2 = BitConverter.ToSingle(profBlob, blobIndex);
				blobIndex += 4;
			}
			if (num5 == 0)
			{
				continue;
			}
			if (num4 < num)
			{
				if (dataPeaks.Count > 0)
				{
					dataPeaks.RemoveAt(dataPeaks.Count - 1);
					num = (int)num4;
				}
			}
			else
			{
				num = AddZeroPackets(calibrateMassDelegate, baseAbscissa, abscissaSpacing, dataPeaks, num, (int)num4, num2, ref minMass);
			}
			int count = dataPeaks.Count;
			long num6 = num + num5;
			if (array.Length < num5)
			{
				array = new float[num5 * 2];
			}
			int num7 = (int)(num5 * 4);
			Buffer.BlockCopy(profBlob, blobIndex, array, 0, num7);
			blobIndex += num7;
			int num8 = 0;
			double num9 = baseAbscissa + (double)num++ * abscissaSpacing;
			double num10 = calibrateMassDelegate(num2, num9);
			if (num10 <= minMass)
			{
				bool flag2 = true;
				minMass = IncreaseMass(minMass);
				dataPeaks.Add(new DataPeak(minMass, array[num8++], num9));
				while (num < num6 && flag2)
				{
					num9 = baseAbscissa + (double)num++ * abscissaSpacing;
					num10 = calibrateMassDelegate(num2, num9);
					if (num10 <= minMass)
					{
						num10 = (minMass = IncreaseMass(minMass));
					}
					else
					{
						flag2 = false;
					}
					dataPeaks.Add(new DataPeak(num10, array[num8++], num9));
				}
			}
			else
			{
				dataPeaks.Add(new DataPeak(num10, array[num8++], num9));
			}
			long num11 = num6 - 2;
			while (num < num11)
			{
				double num12 = baseAbscissa + (double)num * abscissaSpacing;
				dataPeaks.Add(new DataPeak(calibrateMassDelegate(num2, num12), array[num8], num12));
				num12 = baseAbscissa + (double)(num + 1) * abscissaSpacing;
				dataPeaks.Add(new DataPeak(calibrateMassDelegate(num2, num12), array[num8 + 1], num12));
				num12 = baseAbscissa + (double)(num + 2) * abscissaSpacing;
				dataPeaks.Add(new DataPeak(calibrateMassDelegate(num2, num12), array[num8 + 2], num12));
				num += 3;
				num8 += 3;
			}
			while (num < num6)
			{
				double num13 = baseAbscissa + (double)num++ * abscissaSpacing;
				dataPeaks.Add(new DataPeak(calibrateMassDelegate(num2, num13), array[num8++], num13));
			}
			minMass = dataPeaks[dataPeaks.Count - 1].Position;
			if (referencePeakIndex >= num3 || (flag && base.CentroidCounts[segIndex] > 1))
			{
				continue;
			}
			double position = dataPeaks[count].Position;
			if (referencePeakArray[referencePeakIndex].Mass < position)
			{
				while (referencePeakIndex < num3 && !(referencePeakArray[referencePeakIndex].Mass >= position))
				{
					referencePeakIndex++;
				}
			}
			int count2 = dataPeaks.Count;
			if (referencePeakIndex != referencePeakArray.Length)
			{
				LabelPeak refPeak = referencePeakArray[referencePeakIndex];
				if (position <= refPeak.Mass && minMass >= refPeak.Mass)
				{
					FlagReferencePeaks(dataPeaks, refPeak, count, count2);
				}
			}
		}
		return new Tuple<float, int, double, int, int>(num2, num, minMass, referencePeakIndex, blobIndex);
	}
}
