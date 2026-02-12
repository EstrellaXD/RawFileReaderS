using System.Collections.Generic;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// Filter sorting, designed to match same ordering as previous Xcalibur code.
/// </summary>
internal static class FilterSorter
{
	/// <summary>
	/// Sort a list of events, using the <c>qsort</c> algorithm.
	/// This code is needed to precisely match results from legacy <c>fileio</c>.
	/// </summary>
	/// <param name="values">scan events to sort</param>
	public static void Qsort(IList<FilterScanEvent> values)
	{
		int[] array = new int[64];
		int[] array2 = new int[64];
		int count = values.Count;
		if (count < 2)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = count - 1;
		while (true)
		{
			int num4 = num3 - num2 + 1;
			if (num4 <= 8)
			{
				ShortSort(num2, num3, values);
			}
			else
			{
				int num5 = num2 + num4 / 2;
				if (values[num2].CompareExact(values[num5]) > 0)
				{
					Swap(num2, num5, values);
				}
				if (values[num2].CompareExact(values[num3]) > 0)
				{
					Swap(num2, num3, values);
				}
				if (values[num5].CompareExact(values[num3]) > 0)
				{
					Swap(num5, num3, values);
				}
				int num6 = num2;
				int num7 = num3;
				while (true)
				{
					if (num5 > num6)
					{
						do
						{
							num6++;
						}
						while (num6 < num5 && values[num6].CompareExact(values[num5]) <= 0);
					}
					if (num5 <= num6)
					{
						do
						{
							num6++;
						}
						while (num6 <= num3 && values[num6].CompareExact(values[num5]) <= 0);
					}
					do
					{
						num7--;
					}
					while (num7 > num5 && values[num7].CompareExact(values[num5]) > 0);
					if (num7 < num6)
					{
						break;
					}
					Swap(num6, num7, values);
					if (num5 == num7)
					{
						num5 = num6;
					}
				}
				num7++;
				if (num5 < num7)
				{
					do
					{
						num7--;
					}
					while (num7 > num5 && values[num7].CompareExact(values[num5]) == 0);
				}
				if (num5 >= num7)
				{
					do
					{
						num7--;
					}
					while (num7 > num2 && values[num7].CompareExact(values[num5]) == 0);
				}
				if (num7 - num2 >= num3 - num6)
				{
					if (num2 < num7)
					{
						array[num] = num2;
						array2[num] = num7;
						num++;
					}
					if (num6 < num3)
					{
						num2 = num6;
						continue;
					}
				}
				else
				{
					if (num6 < num3)
					{
						array[num] = num6;
						array2[num] = num3;
						num++;
					}
					if (num2 < num7)
					{
						num3 = num7;
						continue;
					}
				}
			}
			num--;
			if (num >= 0)
			{
				num2 = array[num];
				num3 = array2[num];
				continue;
			}
			break;
		}
	}

	/// <summary>
	/// Sort a small list of filters in a simple manner (about 8 items)
	/// </summary>
	/// <param name="low">Low index to sort from</param>
	/// <param name="high">High index to sort to</param>
	/// <param name="values">Data array to sort</param>
	private static void ShortSort(int low, int high, IList<FilterScanEvent> values)
	{
		while (high > low)
		{
			int num = low;
			for (int i = low + 1; i <= high; i++)
			{
				if (values[i].CompareExact(values[num]) > 0)
				{
					num = i;
				}
			}
			Swap(num, high, values);
			high--;
		}
	}

	/// <summary>
	/// Exchange two array elements
	/// </summary>
	/// <param name="a">First to exchange</param>
	/// <param name="b">Second to exchange</param>
	/// <param name="values">Array of data</param>
	private static void Swap(int a, int b, IList<FilterScanEvent> values)
	{
		if (a != b)
		{
			FilterScanEvent value = values[a];
			values[a] = values[b];
			values[b] = value;
		}
	}
}
