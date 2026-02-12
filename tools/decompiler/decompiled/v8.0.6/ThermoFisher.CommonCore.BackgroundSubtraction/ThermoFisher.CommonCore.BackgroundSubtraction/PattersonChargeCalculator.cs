using System;
using System.Collections.Generic;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// <code>
/// This is a variant of the Patterson algorithm for calculating the charge from
/// a given set of measured points in a mass segment. The original algorithm
/// operates on profile points. Because this degrades the performance of the 
/// algorithm significantly for FTMS data (due to their high sampling rate), this
/// variant operates on the detected centroids in a given mass segment instead.
///
/// The algorithm steps through the given centroid segment and calculates the 
/// corresponding charge state for each two neighbors in the segment from their
/// mass distance. The calculation is made according to:
///
///     charge_i,n = dM_iso / (m(i+n) - m(i)) 
///
/// where dM_iso is the general average delta mass between two isotopic peaks.
///
/// For each calculated charge a score is calculated according to:
///
///     score_i,n = y(i+n) * y(i) / y2
///
/// where y(i), and y(i+n) are the intensities of the i-th, and the i-th plus one 
/// centroid, respectively, and y2 is the intensity of the most intense centroid
/// in between them.
///
/// All calculated scores are multiplied to the corresponding entry in the 
/// provided charge map.
///
/// The number of comparisons done in the Patterson charge calculation increases 
/// with the square of the number of centroids contained in the segment (exactly 
/// this is 0.5 * n * (n-1). To speed up the Patterson charge calculation in 
/// those cases where we have a large number of centroids, an intensity threshold 
/// is calculated that limits the number of centroid whose intensity is lying 
/// above. The ratio behind this is, that the more intense peaks should be the 
/// more important ones for the isotopic cluster.
///
///
/// POSSIBLE ENHANCEMENTS:
///
/// Currently the algorithm does not make benefit of the specific centroids that
/// have contributed to the score of a certain charge state in the charge map.
/// This information is available, but thrown away once that a charge is entered
/// into the map. After determining the charge we have to identify the isotopic 
/// cluster to which the charge should be applied. In this step we need again
/// the information about the position of the peaks that make up the isotopic 
/// cluster. Thus is might be beneficial if we would combine both.
/// </code>
/// </summary>
internal class PattersonChargeCalculator
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
	/// The charge map density.
	/// </summary>
	private const double ChargeMapDensity = 5.0;

	private const int ChargeMapLength = 255;

	/// <summary>
	/// The delta charge.
	/// </summary>
	private const double DeltaCharge = 0.2;

	/// <summary>
	/// The max centroids for patterson charge.
	/// </summary>
	private const int MaxCentroidsForPattersonCharge = 200;

	/// <summary>
	/// The max charge state.
	/// </summary>
	private const int MaxChargeState = 50;

	/// <summary>
	/// The patterson high charge upscale factor.
	/// </summary>
	private const double PattersonHighChargeUpscaleFactor = 5.0;

	private readonly double[] _emptyChargeMap;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.PattersonChargeCalculator" /> class.
	/// </summary>
	public PattersonChargeCalculator()
	{
		_emptyChargeMap = new double[255];
		for (int i = 0; i < 255; i++)
		{
			_emptyChargeMap[i] = 1.0;
		}
	}

	/// <summary>
	/// Calculates a distribution of possible charge states based upon
	/// the pairwise distances between the centroid peaks.
	/// </summary>
	/// <param name="centroids">
	///     data for scan
	/// </param>
	/// <param name="beginRangeIndex">
	///     The start of the centroid segment to analyze.
	/// </param>
	/// <param name="endRangeIndex">
	///     The end of the centroid segment to analyze.
	/// </param>
	/// <param name="profileSpacing">
	///     The spacing of profile points. This spacing is used to calculate the maximum theoretical charge for which the isotopic pattern could be resolved.
	/// </param>
	/// <param name="massAccuracy">
	///     The mass accuracy to apply for the identification of the isotopic cluster peaks.
	/// </param>
	/// <returns>
	/// The calculated patterson charge.
	/// </returns>
	public double[] CalculatePattersonCharge(List<CentroidStreamPoint> centroids, int beginRangeIndex, int endRangeIndex, double profileSpacing, double massAccuracy)
	{
		if (endRangeIndex - beginRangeIndex < 2)
		{
			return _emptyChargeMap.Clone() as double[];
		}
		double num = CalculateIntensityThreshold(200, centroids, beginRangeIndex, endRangeIndex);
		double num2 = Math.Min(1.002 / (2.0 * profileSpacing), 50.5);
		double[] array = new double[255];
		for (int i = beginRangeIndex; i + 1 != endRangeIndex; i++)
		{
			CentroidStreamPoint centroidStreamPoint = centroids[i];
			if (centroidStreamPoint.Intensity < num || centroidStreamPoint.Charge != 0)
			{
				continue;
			}
			double num3 = 2.0 * massAccuracy;
			double position = centroidStreamPoint.Position;
			for (int j = i + 1; j != endRangeIndex; j++)
			{
				CentroidStreamPoint centroidStreamPoint2 = centroids[j];
				if (centroidStreamPoint2.Intensity < num || centroidStreamPoint2.Charge != 0)
				{
					continue;
				}
				double num4 = centroidStreamPoint2.Position - position;
				if (num4 > 1.008 + num3)
				{
					break;
				}
				double num5 = 1.002 / num4;
				if (num5 > num2)
				{
					continue;
				}
				double num6 = 0.05 * Math.Max(centroidStreamPoint.Intensity, centroidStreamPoint2.Intensity);
				double num7 = position + 0.25 * num4;
				double num8 = position + 0.75 * num4;
				double num9 = Math.Max(num6, num);
				int num10 = j - 1;
				int k;
				for (k = i + 1; k < num10; k++)
				{
					CentroidStreamPoint centroidStreamPoint3 = centroids[k];
					if (centroidStreamPoint3.Intensity < num9 || centroidStreamPoint3.Position < num7 || centroidStreamPoint3.Charge != 0)
					{
						k++;
						centroidStreamPoint3 = centroids[k];
						if (centroidStreamPoint3.Intensity < num9 || centroidStreamPoint3.Position < num7 || centroidStreamPoint3.Charge != 0)
						{
							continue;
						}
					}
					if (centroidStreamPoint3.Position > num8)
					{
						break;
					}
					num6 = (num9 = centroidStreamPoint3.Intensity);
				}
				for (; k < j; k++)
				{
					CentroidStreamPoint centroidStreamPoint4 = centroids[k];
					if (!(centroidStreamPoint4.Intensity < num9) && !(centroidStreamPoint4.Position < num7) && centroidStreamPoint4.Charge == 0)
					{
						if (centroidStreamPoint4.Position > num8)
						{
							break;
						}
						num6 = (num9 = centroidStreamPoint4.Intensity);
					}
				}
				array[GetChargeMapIndexForCharge(num5)] += centroidStreamPoint.Intensity * centroidStreamPoint2.Intensity / num6;
			}
		}
		if ((double)IndexOfMax(array) * 0.2 >= 10.0)
		{
			for (int num11 = (int)num2; num11 >= 10; num11--)
			{
				int chargeMapIndexForCharge = GetChargeMapIndexForCharge(num11);
				int chargeMapIndexForCharge2 = GetChargeMapIndexForCharge(0.5 * (double)num11);
				double num12 = Math.Max(array[chargeMapIndexForCharge2], array[chargeMapIndexForCharge2 - 1]);
				if (num12 > 0.15 * array[chargeMapIndexForCharge] && num12 < 0.6 * array[chargeMapIndexForCharge])
				{
					int chargeMapIndexForCharge3 = GetChargeMapIndexForCharge(0.25 * (double)num11);
					double num13 = Math.Max(array[chargeMapIndexForCharge3], array[chargeMapIndexForCharge3 - 1]);
					if (num13 > 0.15 * array[chargeMapIndexForCharge] && num13 < 0.6 * array[chargeMapIndexForCharge])
					{
						array[chargeMapIndexForCharge] *= 5.0;
					}
					array[chargeMapIndexForCharge] *= 5.0;
				}
			}
		}
		return NormalizeChargeMap(array);
	}

	/// <summary>
	/// Find the max value in an array of doubles.
	/// (don't use slow LINQ extension Max() )
	/// </summary>
	/// <param name="arrayOfDoubles">Numbers to examine</param>
	/// <returns>max, or 0 if empty</returns>
	internal static double FastMax(double[] arrayOfDoubles)
	{
		int num = arrayOfDoubles.Length;
		if (num <= 0)
		{
			return 0.0;
		}
		double num2 = arrayOfDoubles[0];
		for (int i = 1; i < num; i++)
		{
			if (arrayOfDoubles[i] > num2)
			{
				num2 = arrayOfDoubles[i];
			}
		}
		return num2;
	}

	/// <summary>
	/// Calculate an intensity threshold for a given range of centroids.
	/// </summary>
	/// <param name="maxCentroidsAboveThreshold">
	/// The maximum number of centroids that should lie above the calculated  threshold.
	/// </param>
	/// <param name="centroids">
	/// data for scan
	/// </param>
	/// <param name="beginRangeIndex">
	/// begin of the centroid
	/// </param>
	/// <param name="endRangeIndex">
	/// end of the centroid
	/// </param>
	/// <returns>
	/// The calculated intensity threshold.
	/// </returns>
	private static double CalculateIntensityThreshold(int maxCentroidsAboveThreshold, List<CentroidStreamPoint> centroids, int beginRangeIndex, int endRangeIndex)
	{
		int num = endRangeIndex - beginRangeIndex + 1;
		if (num <= maxCentroidsAboveThreshold)
		{
			return 0.0;
		}
		double num2 = 0.0;
		double intensity = SpectrumAverager.FindMaxElement(centroids, beginRangeIndex, endRangeIndex).Intensity;
		double num3 = 0.05 * intensity;
		while (num > maxCentroidsAboveThreshold)
		{
			num2 += num3;
			num = FindCount(centroids, beginRangeIndex, endRangeIndex, num2);
		}
		return num2;
	}

	/// <summary>
	/// Finds the count of values above the threshold
	/// </summary>
	/// <param name="centroids">
	/// List of Centroids
	/// </param>
	/// <param name="startIndex">
	/// Start of array slice to analyze
	/// </param>
	/// <param name="endIndex">
	/// End of array slice to analyze
	/// </param>
	/// <param name="intensityThreshold">The smallest counted value
	/// </param>
	/// <returns>
	/// The count above threshold
	/// </returns>
	private static int FindCount(List<CentroidStreamPoint> centroids, int startIndex, int endIndex, double intensityThreshold)
	{
		int num = 0;
		if (startIndex >= 0 && startIndex <= endIndex && endIndex < centroids.Count)
		{
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (centroids[i].Intensity > intensityThreshold)
				{
					num++;
				}
			}
		}
		return num;
	}

	/// <summary>
	/// Get the charge map index for a specific charge.
	/// </summary>
	/// <param name="charge">
	/// The charge.
	/// </param>
	/// <returns>
	/// The charge map index for this charge.
	/// </returns>
	private static int GetChargeMapIndexForCharge(double charge)
	{
		return (int)Math.Floor(charge * 5.0 + 0.5);
	}

	/// <summary>
	/// Find the index of max value in an array of doubles
	/// </summary>
	/// <param name="arrayOfDoubles">Numbers to examine</param>
	/// <returns>Index of max, or -1 if list is empty</returns>
	private static int IndexOfMax(double[] arrayOfDoubles)
	{
		int num = arrayOfDoubles.Length;
		if (num <= 0)
		{
			return -1;
		}
		int num2 = 0;
		double num3 = arrayOfDoubles[num2];
		for (int i = 1; i < num; i++)
		{
			if (arrayOfDoubles[i] > num3)
			{
				num2 = i;
				num3 = arrayOfDoubles[i];
			}
		}
		return num2;
	}

	/// <summary>
	/// Smooth, normalize and transfer the local map to the result charge map.
	/// </summary>
	/// <param name="localChargeMap">
	/// The local charge map.
	/// </param>
	/// <returns>
	/// The smoothed map.
	/// </returns>
	private double[] NormalizeChargeMap(double[] localChargeMap)
	{
		double[] array = new double[255];
		double num = 1.0 / FastMax(localChargeMap);
		int num2 = localChargeMap.Length - 1;
		array[0] = 0.0;
		array[num2] = num * localChargeMap[num2];
		for (int i = 1; i < num2 && (double)i * 0.2 < 0.8; i++)
		{
			localChargeMap[i] = 0.0;
		}
		double num3 = localChargeMap[0];
		double num4 = localChargeMap[1];
		for (int j = 1; j < num2; j++)
		{
			double num5;
			array[j] = num * (0.3 * (num3 + (num5 = localChargeMap[j + 1])) + 0.4 * num4);
			num3 = num4;
			num4 = num5;
		}
		return array;
	}
}
