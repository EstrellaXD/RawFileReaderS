using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// The peak charge calculator.
/// Calculates charge for one peak in the centroid list.
/// </summary>
internal class PeakChargeCalculator
{
	/// <summary>
	/// The average isotopic delta.
	/// </summary>
	private const double AverageIsotopicDelta = 1.002;

	/// <summary>
	/// The average isotopic delta deviation.
	/// </summary>
	private const double AverageIsotopicDeltaDeviation = 0.006;

	/// <summary>
	/// The charge evaluation score separation.
	/// </summary>
	private const double ChargeEvaluationScoreSepartion = 2.0;

	/// <summary>
	/// The charge evaluation tolerance.
	/// </summary>
	private const double ChargeEvaluationTolerance = 0.35;

	/// <summary>
	/// The charge map density.
	/// </summary>
	private const double ChargeMapDensity = 5.0;

	/// <summary>
	/// The default mass accuracy. PPM.
	/// </summary>
	private const double DefaultMassAccuracy = 5.0;

	/// <summary>
	/// The delta charge.
	/// </summary>
	private const double DeltaCharge = 0.2;

	/// <summary>
	/// The max high mass range for charge determination.
	/// </summary>
	private const double MaxHighMassRangeForChargeDetermination = 2.1;

	/// <summary>
	/// The max isotopic peaks per cluster.
	/// </summary>
	private const int MaxIsotopicPeaksPerCluster = 50;

	/// <summary>
	/// The max low mass range for charge determination.
	/// </summary>
	private const double MaxLowMassRangeForChargeDetermination = 1.2;

	/// <summary>
	/// The max order for FT.
	/// </summary>
	private const int MaxOrder = 14;

	/// <summary>
	/// The min order for FT.
	/// </summary>
	private const int MinOrder = 4;

	private readonly PattersonChargeCalculator _pattersonChargeCalculator;

	/// <summary>
	/// Gets or sets the mass accuracy, in PPM. Default 5 PPM.
	/// </summary>
	private double MassAccuracy { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.PeakChargeCalculator" /> class.
	/// Default constructor
	/// </summary>
	public PeakChargeCalculator()
	{
		MassAccuracy = 5.0;
		_pattersonChargeCalculator = new PattersonChargeCalculator();
	}

	/// <summary>
	/// Find index of first mass in range.
	/// </summary>
	/// <param name="massSortedCentroidList">
	/// The mass sorted centroid list.
	/// </param>
	/// <param name="centroidPositionComparer">
	/// The centroid position comparer.
	/// </param>
	/// <param name="lowerCentroid">
	/// The lower centroid.
	/// </param>
	/// <returns>
	/// The index of first mass in range.
	/// </returns>
	private static int CentroidBegin(List<CentroidStreamPoint> massSortedCentroidList, SpectrumAverager.CentroidPositionComparer centroidPositionComparer, CentroidStreamPoint lowerCentroid)
	{
		int num = massSortedCentroidList.BinarySearch(lowerCentroid, centroidPositionComparer);
		if (num < 0)
		{
			num = ~num;
		}
		return num;
	}

	/// <summary>
	/// Calculate charge for peak.
	/// </summary>
	/// <param name="segmentList">
	/// The segment list.
	/// </param>
	/// <param name="massSortedCentroidList">
	/// The mass sorted centroid list.
	/// </param>
	/// <param name="centroidPositionComparer">
	/// The centroid position comparer.
	/// </param>
	/// <param name="centroid">
	/// The peak whose charge is needed.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.ChargeResult" />.
	/// </returns>
	internal ChargeResult CalculateChargeForPeak(SpectrumAverager.ProfileData segmentList, List<CentroidStreamPoint> massSortedCentroidList, SpectrumAverager.CentroidPositionComparer centroidPositionComparer, CentroidStreamPoint centroid)
	{
		double num = centroid.Position - Math.Min(1.2, centroid.Position / 1200.0);
		double num2 = centroid.Position + 2.1;
		double massAccuracy = 1E-06 * centroid.Position * MassAccuracy;
		int begin = LowerBound(segmentList, num);
		int end = UpperBound(segmentList, num2);
		double profileSpacing = FindProfileSpacing(segmentList, begin, end);
		CentroidStreamPoint lowerCentroid = new CentroidStreamPoint
		{
			Position = num
		};
		CentroidStreamPoint upperCentroid = new CentroidStreamPoint
		{
			Position = num2
		};
		int centroidBegin = CentroidBegin(massSortedCentroidList, centroidPositionComparer, lowerCentroid);
		int centroidEnd = CentroidEnd(massSortedCentroidList, centroidPositionComparer, upperCentroid);
		int charge = CalculateCharge(segmentList, massSortedCentroidList, end, begin, profileSpacing, centroidBegin, massAccuracy, centroidEnd);
		return IdentifyIsotopicCluster(massSortedCentroidList, centroid.Index, charge, massAccuracy);
	}

	/// <summary>
	/// The calculate charge map scores.
	/// </summary>
	/// <param name="fftVector">
	/// The FFT vector.
	/// </param>
	/// <param name="halfSize">
	/// The half size.
	/// </param>
	/// <param name="fftSpacing">
	/// The FFT spacing.
	/// </param>
	/// <param name="localChargeMap">
	/// The local charge map.
	/// </param>
	private static void CalculateChargeMapScores(double[] fftVector, int halfSize, double fftSpacing, double[] localChargeMap)
	{
		if (fftSpacing > 0.2)
		{
			for (int i = 0; i < localChargeMap.Length; i++)
			{
				double num = (double)i * 0.2 / fftSpacing;
				int num2 = (int)(num + 0.5);
				if (num2 < halfSize)
				{
					double num3 = fftVector[num2];
					localChargeMap[i] = num3 + (fftVector[num2 + 1] - num3) * (num - (double)num2);
				}
			}
			return;
		}
		for (int j = 0; j < localChargeMap.Length; j++)
		{
			double num4 = (double)j * 0.2;
			double num5 = 0.0;
			int num6 = 0;
			int num7 = (int)(0.5 + 0.2 / fftSpacing);
			int num8 = (int)(0.5 + num4 / fftSpacing);
			for (int k = num8 - num7; k <= num8 + num7; k++)
			{
				if (k >= 0 && k < halfSize)
				{
					num5 += fftVector[k];
					num6++;
				}
			}
			localChargeMap[j] = ((num6 == 0) ? num5 : (num5 / (double)num6));
		}
	}

	/// <summary>
	/// The calculate complex modulus.
	/// </summary>
	/// <param name="fftVector">
	/// The FFT vector.
	/// </param>
	/// <param name="halfSize">
	/// The half size.
	/// </param>
	private static void CalculateComplexModulus(double[] fftVector, int halfSize)
	{
		for (int i = 0; i < halfSize; i++)
		{
			int num = i + i;
			double num2 = fftVector[num];
			double num3 = fftVector[num + 1];
			fftVector[i] = Math.Sqrt(num2 * num2 + num3 * num3);
		}
	}

	/// <summary>
	/// Calculates a distribution of possible charge states based upon
	/// an FFT analysis of the input profile spectrum.
	/// </summary>
	/// <param name="chargeMap">
	/// charge map object to fill the scores for the Patterson calculation info
	/// </param>
	/// <param name="segments">
	/// Profile data to be analyzed
	/// </param>
	/// <param name="startIndex">
	/// start of the profile segment to analyze
	/// </param>
	/// <param name="endIndex">
	/// end of the profile segment to analyze.
	/// </param>
	/// <param name="profileSpacing">
	/// The spacing of profile points. This spacing is used to calculate the maximum theoretical charge
	/// for which the isotopic pattern could be resolved.
	/// </param>
	/// <returns>
	/// The map
	/// </returns>
	private static double[] CalculateFftCharge(double[] chargeMap, SpectrumAverager.ProfileData segments, int startIndex, int endIndex, double profileSpacing)
	{
		if (chargeMap == null)
		{
			return null;
		}
		double[] intensities = segments.Intensities;
		int num = endIndex - startIndex;
		int i;
		for (i = 4; 1 << i < num; i++)
		{
		}
		if (i > 14)
		{
			i = 14;
		}
		int num2 = 1 << i;
		double[] array = new double[num2];
		Array.Copy(intensities, startIndex, array, 0, Math.Min(num2, num));
		MathHelpers.CalculateRealFFT(array, num2, 1);
		int halfSize = num2 / 2;
		CalculateComplexModulus(array, halfSize);
		double fftSpacing = 1.002 / (profileSpacing * (double)num2);
		double[] localChargeMap = new double[chargeMap.Length];
		CalculateChargeMapScores(array, halfSize, fftSpacing, localChargeMap);
		NormalizeChargeMap(chargeMap, localChargeMap);
		return chargeMap;
	}

	/// <summary>
	/// Find the centroid end.
	/// </summary>
	/// <param name="massSortedCentroidList">
	/// The mass sorted centroid list.
	/// </param>
	/// <param name="centroidPositionComparer">
	/// The centroid position comparer.
	/// </param>
	/// <param name="upperCentroid">
	/// The upper centroid.
	/// </param>
	/// <returns>
	/// The centroid end.
	/// </returns>
	private static int CentroidEnd(List<CentroidStreamPoint> massSortedCentroidList, SpectrumAverager.CentroidPositionComparer centroidPositionComparer, CentroidStreamPoint upperCentroid)
	{
		int num = massSortedCentroidList.BinarySearch(upperCentroid, centroidPositionComparer);
		if (num < 0)
		{
			num = ~num;
		}
		if (num >= massSortedCentroidList.Count)
		{
			num = massSortedCentroidList.Count - 1;
		}
		return num;
	}

	/// <summary>
	/// Evaluate the given charge map and report the best scored charge
	/// if it is both, valid and significant.
	/// </summary>
	/// <param name="chargeMapList">
	/// charge map list to evaluate.
	/// </param>
	/// <returns>
	/// highest scored charge in the map
	/// </returns>
	private static int EvaluateChargeMap(double[] chargeMapList)
	{
		int num = IndexOfMax(chargeMapList);
		double num2 = chargeMapList[num];
		double num3 = (double)num * 0.2;
		double num4 = 2.0;
		if (num3 > 4.5)
		{
			num4 *= 1.25;
		}
		int num5 = (int)(num3 + 0.5);
		if (num5 == 0 || Math.Abs((double)num5 - num3) > 0.35)
		{
			return 0;
		}
		double num6 = 0.0;
		double num7 = 0.0;
		int num8 = 3;
		int num9 = chargeMapList.Length - num;
		if (num8 < num9)
		{
			int num10 = IndexOfMinimum(chargeMapList, num + num8, chargeMapList.Length - 1);
			int num11 = IndexOfMax(chargeMapList, num10, chargeMapList.Length - 1);
			num7 = chargeMapList[num11];
		}
		if (num7 * num4 > num2)
		{
			return 0;
		}
		if (num8 <= num)
		{
			int num12 = IndexOfMinimum(chargeMapList, num - num8, 0);
			int num13 = num12;
			if (num8 < num12)
			{
				num13 = IndexOfMax(chargeMapList, num8, num12);
			}
			num6 = chargeMapList[num13];
		}
		if (num6 * num4 > num2)
		{
			return 0;
		}
		return num5;
	}

	/// <summary>
	/// find profile spacing.
	/// </summary>
	/// <param name="segmentList">
	/// The segment list.
	/// </param>
	/// <param name="begin">
	/// The begin.
	/// </param>
	/// <param name="end">
	/// The end.
	/// </param>
	/// <returns>
	/// The profile spacing.
	/// </returns>
	private static double FindProfileSpacing(SpectrumAverager.ProfileData segmentList, int begin, int end)
	{
		double[] masses = segmentList.Masses;
		if (end - begin <= 1)
		{
			if (end < segmentList.Length - 1)
			{
				return masses[end + 1] - masses[end];
			}
			if (begin > 0)
			{
				return masses[begin] - masses[begin - 1];
			}
			return 1.0;
		}
		return (masses[end - 1] - masses[begin]) / (double)(end - 1 - begin);
	}

	/// <summary>
	/// Identifies a possible isotopic cluster.
	/// </summary>
	/// <param name="centroidList">
	/// The list of centroids to process
	/// </param>
	/// <param name="centerPointer">
	/// The center of the isotopic cluster to identify
	/// </param>
	/// <param name="charge">
	/// The charge of the isotopic cluster.
	/// </param>
	/// <param name="massAccuracy">
	/// The mass accuracy to apply for the identification of the isotopic cluster peaks.
	/// </param>
	/// <returns>
	/// The identify isotopic cluster and assign charge.
	/// </returns>
	private static ChargeResult IdentifyIsotopicCluster(List<CentroidStreamPoint> centroidList, int centerPointer, int charge, double massAccuracy)
	{
		if (charge == 0)
		{
			return new ChargeResult();
		}
		int num = 0;
		int num2 = centroidList.Count - 1;
		if (num >= num2)
		{
			return new ChargeResult();
		}
		double num3 = centroidList[centerPointer].Position - 25.05 / (double)charge;
		int num4 = centerPointer;
		while (num4 != 0 && centroidList[num4].Position > num3)
		{
			num4--;
		}
		if (centroidList[num4].Position <= num3)
		{
			num = num4 + 1;
		}
		num3 = centroidList[centerPointer].Position + 25.05 / (double)charge;
		int i;
		for (i = centerPointer; i != num2 && centroidList[i].Position < num3; i++)
		{
		}
		num2 = i;
		double minStep = 0.996 / (double)charge;
		double maxStep = 1.008 / (double)charge;
		List<int> list = new List<int>(50);
		List<int> tempIsotopes = new List<int>(50);
		list.Add(centerPointer);
		LookBelowCenterMass(centroidList, charge, num, centerPointer, list, tempIsotopes, maxStep, massAccuracy, minStep);
		LookAboveCenterMass(centroidList, charge, num2, centerPointer, list, tempIsotopes, maxStep, massAccuracy, minStep);
		if ((charge < 3 && list.Count >= 2) || (charge >= 3 && list.Count >= 3))
		{
			return new ChargeResult
			{
				Charge = charge,
				Isotopes = list
			};
		}
		return new ChargeResult();
	}

	/// <summary>
	/// The index of max element.
	/// </summary>
	/// <param name="listofDoubles">
	/// The list of doubles.
	/// </param>
	/// <param name="from">
	/// The from.
	/// </param>
	/// <param name="to">
	/// The to.
	/// </param>
	/// <returns>
	/// The index.
	/// </returns>
	private static int IndexOfMax(double[] listofDoubles, int from, int to)
	{
		int num = listofDoubles.Length;
		if (to >= num || from > to)
		{
			return num - 1;
		}
		int num2 = from;
		double num3 = listofDoubles[num2];
		for (int i = from + 1; i <= to; i++)
		{
			if (listofDoubles[i] > num3)
			{
				num2 = i;
				num3 = listofDoubles[i];
			}
		}
		return num2;
	}

	/// <summary>
	/// Find the index of max value in a list of doubles
	/// </summary>
	/// <param name="listofDoubles">Numbers to examine</param>
	/// <returns>Index of max, or -1 if list is empty</returns>
	private static int IndexOfMax(double[] listofDoubles)
	{
		int num = listofDoubles.Length;
		if (num <= 0)
		{
			return -1;
		}
		int num2 = 0;
		double num3 = listofDoubles[num2];
		for (int i = 1; i < num; i++)
		{
			if (listofDoubles[i] > num3)
			{
				num2 = i;
				num3 = listofDoubles[i];
			}
		}
		return num2;
	}

	/// <summary>
	/// Gets the index of minimum.
	/// </summary>
	/// <param name="listofDoubles">
	/// The list of doubles.
	/// </param>
	/// <param name="from">
	/// The from index.
	/// </param>
	/// <param name="to">
	/// The to index.
	/// </param>
	/// <returns>
	/// The index of min.
	/// </returns>
	private static int IndexOfMinimum(double[] listofDoubles, int from, int to)
	{
		int num = listofDoubles.Length;
		if (to >= num || from >= num)
		{
			return num - 1;
		}
		if (to == from)
		{
			return to;
		}
		int num2 = ((to > from) ? 1 : (-1));
		int num3 = from;
		double num4 = listofDoubles[num3];
		int num5 = to + num2;
		for (int i = from + num2; i != num5; i += num2)
		{
			if (listofDoubles[i] < num4)
			{
				num3 = i;
				num4 = listofDoubles[i];
			}
		}
		return num3;
	}

	/// <summary>
	/// Look above center mass.
	/// </summary>
	/// <param name="centroidList">
	/// The centroid list.
	/// </param>
	/// <param name="charge">
	/// The charge.
	/// </param>
	/// <param name="endPointer">
	/// The end pointer.
	/// </param>
	/// <param name="centerPointer">
	/// The center pointer.
	/// </param>
	/// <param name="isotopes">
	/// The isotopes.
	/// </param>
	/// <param name="tempIsotopes">
	/// The temp isotopes.
	/// </param>
	/// <param name="maxStep">
	/// The max step.
	/// </param>
	/// <param name="massAccuracy">
	/// The mass accuracy.
	/// </param>
	/// <param name="minStep">
	/// The min step.
	/// </param>
	private static void LookAboveCenterMass(List<CentroidStreamPoint> centroidList, int charge, int endPointer, int centerPointer, List<int> isotopes, List<int> tempIsotopes, double maxStep, double massAccuracy, double minStep)
	{
		CentroidStreamPoint centroidStreamPoint = centroidList[centerPointer];
		int num = 0;
		int num2 = 0;
		double num3 = centroidStreamPoint.Position;
		double num4 = centroidStreamPoint.Position + 0.006 / (double)charge;
		int num5 = ((centerPointer < endPointer) ? (centerPointer + 1) : centerPointer);
		tempIsotopes.Clear();
		double num6 = centroidStreamPoint.Intensity;
		double num7 = num6;
		double num8 = num6;
		while (num2 < 2 && num5 < endPointer && num5 != centerPointer)
		{
			if (centroidList[num5].Position > num4)
			{
				if (num7 > 0.04 * num6)
				{
					double num9 = Math.Max(num7, num6);
					if (num9 > num8)
					{
						break;
					}
					isotopes.AddRange(tempIsotopes);
					num8 = num9;
					num6 = num7;
					num2 = 0;
				}
				else
				{
					num2++;
				}
				num++;
				tempIsotopes.Clear();
				num7 = 0.0;
				num3 = centroidStreamPoint.Position + (double)num * minStep - massAccuracy;
				num4 = centroidStreamPoint.Position + (double)num * maxStep + massAccuracy;
				continue;
			}
			if (centroidList[num5].Position > num3 && centroidList[num5].Charge == 0)
			{
				tempIsotopes.Add(num5);
				if (centroidList[num5].Intensity > num7)
				{
					num7 = centroidList[num5].Intensity;
				}
			}
			num5++;
		}
		if (num7 > 0.04 * num6)
		{
			double num9 = Math.Max(num7, num6);
			if (num9 <= num8)
			{
				isotopes.AddRange(tempIsotopes);
			}
		}
	}

	/// <summary>
	/// Look below the center mass.
	/// </summary>
	/// <param name="centroidList">
	/// The centroid list.
	/// </param>
	/// <param name="charge">
	/// The charge.
	/// </param>
	/// <param name="beginPointer">
	/// The begin pointer.
	/// </param>
	/// <param name="centerPointer">
	/// The center pointer.
	/// </param>
	/// <param name="isotopes">
	/// The isotopes.
	/// </param>
	/// <param name="tempIsotopes">
	/// The temp isotopes.
	/// </param>
	/// <param name="maxStep">
	/// The max step.
	/// </param>
	/// <param name="massAccuracy">
	/// The mass accuracy.
	/// </param>
	/// <param name="minStep">
	/// The min step.
	/// </param>
	private static void LookBelowCenterMass(List<CentroidStreamPoint> centroidList, int charge, int beginPointer, int centerPointer, List<int> isotopes, List<int> tempIsotopes, double maxStep, double massAccuracy, double minStep)
	{
		int num = 0;
		int num2 = 0;
		CentroidStreamPoint centroidStreamPoint = centroidList[centerPointer];
		double num3 = centroidStreamPoint.Position - 0.006 / (double)charge;
		double num4 = centroidStreamPoint.Position;
		int num5 = ((centerPointer > beginPointer) ? (centerPointer - 1) : centerPointer);
		double num6 = centroidStreamPoint.Intensity;
		double num7 = centroidStreamPoint.Intensity;
		double num8 = centroidStreamPoint.Intensity;
		while (num2 < 2 && num5 >= beginPointer && num5 != centerPointer)
		{
			if (centroidList[num5].Position < num3)
			{
				if (num7 > 0.04 * num6)
				{
					double num9 = Math.Max(num7, num6);
					if (num9 > num8)
					{
						break;
					}
					isotopes.AddRange(tempIsotopes);
					num8 = num9;
					num6 = num7;
					num2 = 0;
				}
				else
				{
					num2++;
				}
				num++;
				tempIsotopes.Clear();
				num7 = 0.0;
				num3 = centroidStreamPoint.Position - (double)num * maxStep - massAccuracy;
				num4 = centroidStreamPoint.Position - (double)num * minStep + massAccuracy;
				continue;
			}
			if (centroidList[num5].Position < num4 && centroidList[num5].Charge == 0)
			{
				tempIsotopes.Add(num5);
				if (centroidList[num5].Intensity > num7)
				{
					num7 = centroidList[num5].Intensity;
				}
			}
			if (num5 == beginPointer)
			{
				break;
			}
			num5--;
		}
		if (num7 > 0.04 * num6)
		{
			double num9 = Math.Max(num7, num6);
			if (num9 <= num8)
			{
				isotopes.AddRange(tempIsotopes);
			}
		}
	}

	/// <summary>
	/// Lower bound. This method returns the first value which is within range
	/// </summary>
	/// <param name="segmentList">
	/// The segment list.
	/// </param>
	/// <param name="lower">
	/// The lower.
	/// </param>
	/// <returns>
	/// The lower bound.
	/// </returns>
	private static int LowerBound(SpectrumAverager.ProfileData segmentList, double lower)
	{
		int num = Array.BinarySearch(segmentList.Masses, lower);
		if (num < 0)
		{
			num = ~num;
		}
		return num;
	}

	/// <summary>
	/// Normalize the charge map.
	/// </summary>
	/// <param name="chargeMap">
	/// The charge map.
	/// </param>
	/// <param name="localChargeMap">
	/// The local charge map.
	/// </param>
	private static void NormalizeChargeMap(double[] chargeMap, double[] localChargeMap)
	{
		double num = 1.0;
		if (localChargeMap.Length != 0)
		{
			num = 1.0 / PattersonChargeCalculator.FastMax(localChargeMap);
		}
		for (int i = 0; i < localChargeMap.Length && (double)i * 0.2 < 0.8; i++)
		{
			localChargeMap[i] = 0.0;
		}
		for (int j = 0; j < localChargeMap.Length; j++)
		{
			chargeMap[j] *= num * localChargeMap[j];
		}
	}

	/// <summary>
	/// Upper bound. This method returns the first value which is out of range
	/// </summary>
	/// <param name="segmentList">
	/// The segment list.
	/// </param>
	/// <param name="upper">
	/// The upper mass limit.
	/// </param>
	/// <returns>
	/// The upper bound.
	/// </returns>
	private static int UpperBound(SpectrumAverager.ProfileData segmentList, double upper)
	{
		int num = Array.BinarySearch(segmentList.Masses, upper);
		num = ((num >= 0) ? (num + 1) : (~num));
		if (num >= segmentList.Length)
		{
			num = segmentList.Length - 1;
		}
		return num;
	}

	/// <summary>
	/// Calculate the charge.
	/// </summary>
	/// <param name="segmentList">
	///     The segment list.
	/// </param>
	/// <param name="massSortedCentroidList">
	///     The mass sorted centroid list.
	/// </param>
	/// <param name="end">
	///     The end.
	/// </param>
	/// <param name="begin">
	///     The begin.
	/// </param>
	/// <param name="profileSpacing">
	///     The profile spacing.
	/// </param>
	/// <param name="centroidBegin">
	///     The centroid begin.
	/// </param>
	/// <param name="massAccuracy">
	///     The mass accuracy.
	/// </param>
	/// <param name="centroidEnd">
	///     The centroid end.
	/// </param>
	/// <returns>
	/// The calculated charge.
	/// </returns>
	private int CalculateCharge(SpectrumAverager.ProfileData segmentList, List<CentroidStreamPoint> massSortedCentroidList, int end, int begin, double profileSpacing, int centroidBegin, double massAccuracy, int centroidEnd)
	{
		return EvaluateChargeMap(CalculateFftCharge(_pattersonChargeCalculator.CalculatePattersonCharge(massSortedCentroidList, centroidBegin, centroidEnd, profileSpacing, massAccuracy), segmentList, begin, end, profileSpacing));
	}
}
