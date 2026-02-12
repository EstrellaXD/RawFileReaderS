using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to find a chromatogram point, based on matching a pattern of masses
/// </summary>
public class PatternValueProvider : IScanValueProvider
{
	/// <summary>
	/// Gets or sets the isotope pattern tracer settings.
	/// </summary>
	public IsotopePatternTraceSettings Settings { get; set; }

	/// <summary>
	/// Scan value provider for XIC generation.
	/// </summary>
	/// <param name="scanData">The scan to search</param>
	/// <returns>The total intensity value of the function for a single scan.</returns>
	public double ValueForScan(ISimpleScanWithHeader scanData)
	{
		PatternFilter patternFilter = new PatternFilter(Settings.MassPatternFilter);
		List<Tuple<List<double>, List<double>>> list = new List<Tuple<List<double>, List<double>>>();
		ISimpleScanAccess data = scanData.Data;
		float[] noiseArray = null;
		if (data is ISimpleScanPlus simpleScanPlus)
		{
			noiseArray = simpleScanPlus.Noise;
		}
		double num = 0.0;
		for (int i = 0; i < data.Masses.Length; i++)
		{
			if (data.Intensities[i] < Settings.MinimumIntensity)
			{
				continue;
			}
			patternFilter.CompileFilter(data.Masses[i], data.Intensities[i]);
			if (!(Settings.MinimumIntensity > 0.0) || !patternFilter.IntensityRanges.Any((Range x) => x.High < Settings.MinimumIntensity))
			{
				Tuple<List<double>, List<double>> tuple = FindPatternInScan(patternFilter, data.Masses, data.Intensities, noiseArray);
				if (tuple.Item1.Count == patternFilter.Length)
				{
					list.Add(tuple);
				}
			}
		}
		RemoveOverlappingPatternsInScan(list);
		foreach (Tuple<List<double>, List<double>> item in list)
		{
			num += item.Item2.Sum();
		}
		return num;
	}

	/// <summary>
	/// find the first data value which is equal to or after start
	/// (for example, first value within a tolerance range)
	/// </summary>
	/// <param name="data">
	/// Array of increasing data (for example masses in a scan)
	/// </param>
	/// <param name="start">
	/// Position to look for (for example start of X axis range)
	/// </param>
	/// <returns>
	/// The first value after start, or -1 if there are no points after start
	/// </returns>
	private static int FirstAfter(double[] data, double start)
	{
		if (data == null)
		{
			return -1;
		}
		int num = data.Length;
		if (num == 0)
		{
			return -1;
		}
		if (data[0] >= start)
		{
			return 0;
		}
		int num2 = 0;
		int num3 = num - 1;
		if (data[num3] < start)
		{
			return -1;
		}
		while (num3 > num2 + 1)
		{
			int num4 = (num3 + num2) / 2;
			if (data[num4] < start)
			{
				num2 = num4;
			}
			else
			{
				num3 = num4;
			}
		}
		return num3;
	}

	/// <summary>
	/// Find the nearest data value to "find".
	/// The array must be sorted in increasing order (for example, masses from a scan)
	/// </summary>
	/// <param name="data">
	/// Array of increasing values
	/// </param>
	/// <param name="find">
	/// The data value to search for
	/// </param>
	/// <returns>
	/// The index of the nearest value to "find".
	/// -1 if the array is null or empty. 
	/// </returns>
	private static int Nearest(double[] data, double find)
	{
		if (data == null)
		{
			return -1;
		}
		int num = data.Length;
		if (num == 0)
		{
			return -1;
		}
		int num2 = FirstAfter(data, find);
		if (num2 >= 0)
		{
			if (num2 == 0 || num == 1)
			{
				return 0;
			}
			int num3 = num2 - 1;
			if (Math.Abs(data[num3] - find) < Math.Abs(data[num2] - find))
			{
				return num3;
			}
			return num2;
		}
		return num - 1;
	}

	/// <summary>
	/// Find a single isotope pattern in scan.
	/// </summary>
	/// <param name="isotopeFilter"></param>
	/// <param name="massArray"></param>
	/// <param name="intensityArray"></param>
	/// <param name="noiseArray"></param>
	/// <returns></returns>
	private static Tuple<List<double>, List<double>> FindPatternInScan(PatternFilter isotopeFilter, double[] massArray, double[] intensityArray, float[] noiseArray = null)
	{
		List<double> list = new List<double>(isotopeFilter.Length);
		List<double> list2 = new List<double>(isotopeFilter.Length);
		if (massArray.Length != intensityArray.Length)
		{
			return new Tuple<List<double>, List<double>>(list, list2);
		}
		if (noiseArray != null && massArray.Length != noiseArray.Length)
		{
			return new Tuple<List<double>, List<double>>(list, list2);
		}
		int num = massArray.Length - 1;
		list.Add(isotopeFilter.ReferenceMass);
		list2.Add(isotopeFilter.ReferenceIntensity);
		double val = 0.0;
		if (noiseArray != null)
		{
			int num2 = Nearest(massArray, isotopeFilter.ReferenceMass);
			if (num2 != -1)
			{
				val = noiseArray[num2];
			}
		}
		for (int i = 0; i < isotopeFilter.Length; i++)
		{
			if (isotopeFilter.IsReferenceList[i])
			{
				continue;
			}
			Range range = isotopeFilter.MassRanges[i];
			Range range2 = isotopeFilter.IntensityRanges[i];
			double low = range.Low;
			double high = range.High;
			double low2 = range2.Low;
			double high2 = range2.High;
			int num3 = massArray.FastBinarySearch(low);
			if (num3 < 0)
			{
				continue;
			}
			List<int> matchIndexList = new List<int>();
			double num4 = Math.Max(val, low2);
			for (int j = num3; j <= num && !(massArray[j] > high); j++)
			{
				double num5 = intensityArray[j];
				if (num5 >= num4 && num5 <= high2)
				{
					matchIndexList.Add(j);
				}
			}
			if (matchIndexList.Count == 0)
			{
				break;
			}
			list.Add(massArray.Where((double x, int index) => matchIndexList.Contains(index)).Average());
			list2.Add(intensityArray.Where((double x, int index) => matchIndexList.Contains(index)).Sum());
		}
		return new Tuple<List<double>, List<double>>(list, list2);
	}

	/// <summary>
	/// Remove overlapping patterns in a scan by keeping the most intense pattern.
	/// </summary>
	/// <param name="patternList"></param>
	/// <returns></returns>
	private static void RemoveOverlappingPatternsInScan(IList<Tuple<List<double>, List<double>>> patternList)
	{
		if (patternList.Count <= 1)
		{
			return;
		}
		List<int> list = new List<int>(patternList.Count);
		for (int i = 0; i < patternList.Count; i++)
		{
			Tuple<List<double>, List<double>> tuple = patternList[i];
			if (list.Contains(i))
			{
				continue;
			}
			for (int j = 0; j < patternList.Count; j++)
			{
				if (i == j)
				{
					continue;
				}
				Tuple<List<double>, List<double>> tuple2 = patternList[j];
				if (tuple2.Item1.Min() > tuple.Item1.Max() || tuple2.Item1.Max() < tuple.Item1.Min())
				{
					continue;
				}
				bool flag = false;
				foreach (double item2 in tuple.Item1)
				{
					if (tuple2.Item1.Contains(item2))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					int item = ((tuple.Item2.Sum() >= tuple2.Item2.Sum()) ? j : i);
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
		}
		if (list.Count != 0)
		{
			list.Sort((int a, int b) => a.CompareTo(b));
			for (int num = list.Count - 1; num >= 0; num--)
			{
				patternList.RemoveAt(list[num]);
			}
		}
	}
}
