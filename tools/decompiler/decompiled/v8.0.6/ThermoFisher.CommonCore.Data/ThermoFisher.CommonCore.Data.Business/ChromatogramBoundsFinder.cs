using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to find which scans belong in the RT range of a chromatogram.
/// This includes the feature that scans may be "entirely within" a given range, or go one scan beyond,
/// such that the range is extended allowing a chromatogram plot can be generated over the entire range.
/// </summary>
public class ChromatogramBoundsFinder
{
	/// <summary>
	/// Gets or sets the available scans.
	/// </summary>
	public IList<ISimpleScanHeader> AvailableScans { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the chromatograms have a strict time range.
	/// By default, the retention time range of a chromatogram is considered as a "display range",
	/// such that the first value before the range, and the first value after the range is included
	/// in the data, permitting a continuous line, if plotted, to the edge of the time window.
	/// If this property is true, then only points which are within the supplied RT range are returned.
	/// </summary>
	public bool StrictTimeRange { get; set; }

	/// <summary>
	/// Find scan index for a given retention time.
	/// </summary>
	/// <param name="time">
	/// The time.
	/// </param>
	/// <returns>
	/// The index into the scan table for this time.
	/// </returns>
	private int FindIndexForTime(double time)
	{
		Comparer<ISimpleScanHeader> comparer = new Comparer<ISimpleScanHeader>((ISimpleScanHeader a, ISimpleScanHeader b) => a.RetentionTime.CompareTo(b.RetentionTime));
		int num = AvailableScans.BinarySearch(new SimpleScanHeader
		{
			RetentionTime = time
		}, comparer);
		if (num < 0)
		{
			num = ~num;
		}
		return num;
	}

	/// <summary>
	/// Find the scan index range for chromatogram, based on the retention time range.
	/// </summary>
	/// <param name="range">
	/// The RT range of the chromatogram.
	/// </param>
	/// <returns>
	/// The start and end index (items 1 and 2).
	/// </returns>
	public Tuple<int, int> FindIndexRangeForChromatogram(IRangeAccess range)
	{
		int num = FindIndexForTime(range.Low);
		int num2 = FindIndexForTime(range.High);
		if (StrictTimeRange)
		{
			if (num2 < AvailableScans.Count && range.High < AvailableScans[num2].RetentionTime)
			{
				num2--;
			}
		}
		else if (num > 0 && num < AvailableScans.Count && range.Low < AvailableScans[num].RetentionTime)
		{
			num--;
		}
		if (num2 >= AvailableScans.Count)
		{
			num2 = AvailableScans.Count - 1;
		}
		return new Tuple<int, int>(num, num2);
	}
}
