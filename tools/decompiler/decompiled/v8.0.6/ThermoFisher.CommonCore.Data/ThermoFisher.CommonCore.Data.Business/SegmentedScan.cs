using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a scan with mass range segments.
/// </summary>
[Serializable]
public class SegmentedScan : ICloneable, IChromatogramPointBuilder, IDeepCloneable<SegmentedScan>, ISegmentedScanAccess
{
	private PeakOptions[] _flags;

	private int[] _segmentSizes;

	private Range[] _ranges;

	private double[] _intensities;

	private double[] _positions;

	private int _segmentCount;

	private int _positionCount;

	/// <summary>
	/// Gets the Mass ranges for each scan segment
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<IRangeAccess> MassRanges => new ReadOnlyCollection<IRangeAccess>(_ranges);

	/// <summary>
	/// Gets or sets the Mass ranges for each scan segment
	/// </summary>
	public Range[] Ranges
	{
		get
		{
			return _ranges;
		}
		set
		{
			if (value != null)
			{
				_segmentCount = value.Length;
			}
			_ranges = value;
		}
	}

	/// <summary>
	/// Gets or sets The number of segments
	/// </summary>
	public int SegmentCount
	{
		get
		{
			return _segmentCount;
		}
		set
		{
			_segmentCount = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of data points in each segment
	/// </summary>
	public int[] SegmentSizes
	{
		get
		{
			return _segmentSizes;
		}
		set
		{
			if (value != null)
			{
				_segmentCount = value.Length;
			}
			_segmentSizes = value;
		}
	}

	/// <summary>
	/// Gets SegmentLengths.
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<int> SegmentLengths => new ReadOnlyCollection<int>(SegmentSizes);

	/// <summary>
	/// Gets or sets the positions (mass or wavelength) for each point in the scan
	/// </summary>
	public double[] Positions
	{
		get
		{
			return _positions;
		}
		set
		{
			if (value != null)
			{
				_positionCount = value.Length;
			}
			_positions = value;
		}
	}

	/// <summary>
	/// Gets or sets the Intensity (or absorbance) values for each point in the scan
	/// </summary>
	public double[] Intensities
	{
		get
		{
			return _intensities;
		}
		set
		{
			if (value != null)
			{
				_positionCount = value.Length;
			}
			_intensities = value;
		}
	}

	/// <summary>
	/// Gets or sets flags, such as "saturated" for each peak.
	/// </summary>
	public PeakOptions[] Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			if (value != null)
			{
				_positionCount = value.Length;
			}
			_flags = value;
		}
	}

	/// <summary>
	/// Gets or sets the The size of the position and intensity arrays.
	/// The number of peaks in the scan (total for all segments)
	/// </summary>
	public int PositionCount
	{
		get
		{
			return _positionCount;
		}
		set
		{
			_positionCount = value;
		}
	}

	/// <summary>
	/// Gets or sets the he number of this scan.
	/// </summary>
	public int ScanNumber { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" /> class. 
	/// Default constructor;  sets the size of data arrays to 0.
	/// </summary>
	public SegmentedScan()
	{
		SegmentCount = 0;
		PositionCount = 0;
		_ranges = new Range[0];
	}

	/// <summary>
	/// Create a scan from simple X,Y data. This method creates a scan with one segment.
	/// For efficiency, the references to the mass and intensity data are maintained within the
	/// constructed object. If this is not desired, clone the mass and intensity
	/// arrays on calling this constructor.
	/// Masses are assumed to be in ascending order.
	/// </summary>
	/// <param name="masses">Mass data for the scan</param>
	/// <param name="intensities">Intensity data for the scan</param>
	/// <returns>A scan with one segment</returns>
	/// <exception cref="T:System.ArgumentNullException"><c>masses</c> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException"><c>intensities</c> is null.</exception>
	/// <exception cref="T:System.ArgumentException">Intensities must have same length as masses</exception>
	public static SegmentedScan FromMassesAndIntensities(double[] masses, double[] intensities)
	{
		if (masses == null)
		{
			throw new ArgumentNullException("masses");
		}
		if (intensities == null)
		{
			throw new ArgumentNullException("intensities");
		}
		if (masses.Length != intensities.Length)
		{
			throw new ArgumentException("Intensities must have same length as masses");
		}
		SegmentedScan segmentedScan = new SegmentedScan
		{
			Intensities = intensities,
			Positions = masses,
			Flags = new PeakOptions[masses.Length],
			PositionCount = masses.Length,
			SegmentCount = 1,
			SegmentSizes = new int[1],
			Ranges = new Range[1]
		};
		segmentedScan.SegmentSizes[0] = masses.Length;
		if (masses.Length != 0)
		{
			segmentedScan.Ranges[0] = new Range(masses[0], masses[^1]);
		}
		else
		{
			segmentedScan.Ranges[0] = new Range(0.0, 0.0);
		}
		return segmentedScan;
	}

	/// <summary>
	/// Sum all masses within the ranges
	/// </summary>
	/// <param name="ranges">List of ranges to sum</param>
	/// <param name="tolerance">If the ranges have equal mass values,
	/// then tolerance is subtracted from low and added to high to search for matching masses</param>
	/// <returns>Sum of intensities in all ranges</returns>
	public double SumIntensities(IRangeAccess[] ranges, double tolerance)
	{
		double num = 0.0;
		for (int i = 0; i < ranges.Length; i++)
		{
			Range fixedRange = new Range(ranges[i], tolerance);
			num = SumIntensities(num, fixedRange);
		}
		return num;
	}

	/// <summary>
	/// Sum all masses within the ranges
	/// </summary>
	/// <param name="ranges">List of ranges to sum</param>
	/// <param name="toleranceOptions">If the ranges have equal mass values,
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses</param>
	/// <returns>Sum of intensities in all ranges</returns>
	public double SumIntensities(IRangeAccess[] ranges, MassOptions toleranceOptions)
	{
		if (ranges == null)
		{
			throw new ArgumentNullException("ranges");
		}
		double num = 0.0;
		for (int i = 0; i < ranges.Length; i++)
		{
			Range fixedRange = new Range(ranges[i], toleranceOptions);
			num = SumIntensities(num, fixedRange);
		}
		return num;
	}

	/// <summary>
	/// Return the largest intensity (base value) in the ranges supplies
	/// </summary>
	/// <param name="ranges">Ranges of positions</param>
	/// <returns>The largest intensity in all ranges</returns>
	/// <param name="tolerance">If the ranges have equal mass values,
	/// then tolerance is subtracted from low and added to high to search for matching masses</param>
	/// <returns>Sum of intensities in all ranges</returns>
	public double BaseIntensity(IRangeAccess[] ranges, double tolerance)
	{
		double num = 0.0;
		if (Positions != null && Intensities != null && Positions.Length == Intensities.Length)
		{
			for (int i = 0; i < ranges.Length; i++)
			{
				Range fixedRange = new Range(ranges[i], tolerance);
				num = GetBaseValue(fixedRange, num);
			}
		}
		return num;
	}

	/// <summary>
	/// Return the largest intensity (base value) in the ranges supplied
	/// </summary>
	/// <param name="ranges">Ranges of positions (masses, wavelengths)</param>
	/// <param name="toleranceOptions">If the ranges have equal mass values,
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses</param>
	/// <returns>Largest intensity in all ranges</returns>
	public double BaseIntensity(IRangeAccess[] ranges, MassOptions toleranceOptions)
	{
		if (ranges == null)
		{
			throw new ArgumentNullException("ranges");
		}
		double num = 0.0;
		if (Positions != null && Intensities != null && Positions.Length == Intensities.Length)
		{
			for (int i = 0; i < ranges.Length; i++)
			{
				Range fixedRange = new Range(ranges[i], toleranceOptions);
				num = GetBaseValue(fixedRange, num);
			}
		}
		return num;
	}

	/// <summary>
	/// Get the largest value in a range
	/// </summary>
	/// <param name="fixedRange">
	/// The fixed range.
	/// </param>
	/// <param name="baseValue">
	/// The base value so far.
	/// </param>
	/// <returns>
	/// The largest of, the passed in base value and the base value in the range.
	/// </returns>
	private double GetBaseValue(Range fixedRange, double baseValue)
	{
		int num = Array.BinarySearch(Positions, fixedRange.Low);
		if (num < 0)
		{
			num = ~num;
		}
		while (num < Positions.Length && Positions[num] <= fixedRange.High)
		{
			baseValue = Math.Max(baseValue, Intensities[num++]);
		}
		return baseValue;
	}

	/// <summary>
	/// Test if this is a valid object (all streams are not null. All data has same length)
	/// </summary>
	/// <returns>
	/// True if valid.
	/// </returns>
	public bool TryValidate()
	{
		try
		{
			Validate();
		}
		catch (ArgumentException)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Test if this is a valid object (all streams are not null. All data has same length)
	///  </summary>
	/// <exception cref="T:System.ArgumentException">is thrown if this instance does not contain valid data.</exception>
	public void Validate()
	{
		if (Flags == null)
		{
			throw new ArgumentException("Flags array is not initialized.");
		}
		if (Flags.Length != PositionCount)
		{
			throw new ArgumentException($"Length of Flags array({Flags.Length}) does not match PositionCount.");
		}
		if (Intensities == null)
		{
			throw new ArgumentException("Intensities array is not initialized.");
		}
		if (Intensities.Length != PositionCount)
		{
			throw new ArgumentException($"Length of Intensities array({Intensities.Length}) does not match PositionCount.");
		}
		if (Positions == null)
		{
			throw new ArgumentException("Positions array is not initialized.");
		}
		if (Positions.Length != PositionCount)
		{
			throw new ArgumentException($"Length of Positions array({Positions.Length}) does not match PositionCount.");
		}
		if (SegmentSizes == null)
		{
			throw new ArgumentException("SegmentSizes array is not initialized.");
		}
		if (SegmentSizes.Length != SegmentCount)
		{
			throw new ArgumentException($"Length of SegmentSizes array({SegmentSizes.Length}) does not match SegmentCount.");
		}
	}

	/// <summary>
	/// Sum all intensities of peaks within the ranges
	/// </summary>
	/// <param name="sum">
	/// The sum so far.
	/// </param>
	/// <param name="fixedRange">
	/// The fixed Range.
	/// </param>
	/// <returns>
	/// Sum of intensities in all ranges
	/// </returns>
	private double SumIntensities(double sum, Range fixedRange)
	{
		int num = Array.BinarySearch(Positions, fixedRange.Low);
		if (num < 0)
		{
			num = ~num;
		}
		while (num < Positions.Length && Positions[num] <= fixedRange.High)
		{
			sum += Intensities[num++];
		}
		return sum;
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	public object Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>
	/// Make a deep clone of this object.
	/// </summary>
	/// <returns>An object containing all data in this, and no shared references</returns>
	public SegmentedScan DeepClone()
	{
		return new SegmentedScan
		{
			Flags = (PeakOptions[])Flags.Clone(),
			Intensities = (double[])Intensities.Clone(),
			PositionCount = PositionCount,
			Positions = (double[])Positions.Clone(),
			Ranges = (Range[])Ranges.Clone(),
			SegmentSizes = (int[])SegmentSizes.Clone(),
			ScanNumber = ScanNumber,
			SegmentCount = SegmentCount
		};
	}

	/// <summary>
	/// Find the index of the first packet in a segment 
	/// </summary>
	/// <param name="segment">segment number (starting from 0)</param>
	/// <returns>the index of the first packet in a segment </returns>
	public int IndexOfSegmentStart(int segment)
	{
		int num = 0;
		for (int i = 0; i < segment; i++)
		{
			num += SegmentSizes[i];
		}
		return num;
	}

	/// <summary>
	/// Change the defined length of the scan (for internal use only)
	/// </summary>
	/// <param name="newSize">new scan size</param>
	internal void Resize(int newSize)
	{
		Array.Resize(ref _positions, newSize);
		Array.Resize(ref _intensities, newSize);
		Array.Resize(ref _flags, newSize);
		PositionCount = newSize;
	}

	/// <summary>
	/// Convert to simple scan.
	/// This permits calling code to free up references to unused parts of the scan data.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.ISimpleScanAccess" />.
	/// </returns>
	public ISimpleScanAccess ToSimpleScan()
	{
		return new SimpleScan
		{
			Masses = _positions,
			Intensities = _intensities
		};
	}

	/// <summary>
	/// Return only data within the supplied ranges.
	/// This internal method can assume that the ranges are in mass order, and do not overlap.
	/// </summary>
	/// <param name="compactedRange">ranges of data to include</param>
	/// <param name="isProfile">if set, then this scan is profile data.
	/// Zeros are added to terminate sliced profiles</param>
	/// <param name="processCentroidStream">If this is set:
	/// There must be a valid centroid stream.
	/// Profile points near a centroid must be included, even if outside of the range.
	/// </param>
	/// <param name="centroids">Centroid data, which may be used to extend profiles so that
	/// points which contribute to the centroid are shown</param>
	/// <returns>A new scan, with data in the desired ranges only</returns>
	internal SegmentedScan Slice(List<IRangeAccess> compactedRange, bool isProfile, bool processCentroidStream, CentroidStream centroids)
	{
		if (isProfile)
		{
			return ProcessProfile();
		}
		return ProcessCentroid();
		SegmentedScan ProcessCentroid()
		{
			List<double> list = new List<double>();
			List<double> list2 = new List<double>();
			List<PeakOptions> list3 = new List<PeakOptions>();
			foreach (IRangeAccess item in compactedRange)
			{
				int i = _positions.FastBinarySearch(item.Low);
				if (i >= 0 && _positions[i] <= item.High)
				{
					for (; i < _positionCount && _positions[i] <= item.High; i++)
					{
						bool flag = list.Count == 0;
						if (!flag)
						{
							flag = _positions[i] > list[list.Count - 1];
						}
						if (flag)
						{
							list.Add(_positions[i]);
							list2.Add(_intensities[i]);
							list3.Add(_flags[i]);
						}
					}
				}
			}
			SegmentedScan segmentedScan = FromMassesAndIntensities(list.ToArray(), list2.ToArray());
			segmentedScan.Flags = list3.ToArray();
			return segmentedScan;
		}
		SegmentedScan ProcessProfile()
		{
			double[] masses = (double[])_positions.Clone();
			double[] newIntensities = new double[_intensities.Length];
			PeakOptions[] flags = (PeakOptions[])_flags.Clone();
			double[] centroidMasses;
			if (processCentroidStream)
			{
				centroidMasses = centroids.Masses;
				processCentroidStream = centroidMasses != null;
			}
			else
			{
				centroidMasses = new double[0];
			}
			int largestIndexUsed = -1;
			foreach (IRangeAccess item2 in compactedRange)
			{
				IRangeAccess rangeAccess = item2;
				int firstInRange = _positions.FastBinarySearch(rangeAccess.Low);
				if (firstInRange >= 0 && _positions[firstInRange] <= rangeAccess.High)
				{
					if (_intensities[firstInRange] > 0.0 && processCentroidStream)
					{
						ProcessCentroidAtRangeStart();
					}
					bool flag = false;
					for (; firstInRange < _positionCount && _positions[firstInRange] <= rangeAccess.High; firstInRange++)
					{
						flag = true;
						newIntensities[firstInRange] = _intensities[firstInRange];
						largestIndexUsed = firstInRange;
					}
					if (flag && newIntensities[largestIndexUsed] > 0.0 && processCentroidStream)
					{
						ProcessCentroidsAtRangeEnd();
					}
				}
				void ProcessCentroidAtRangeStart()
				{
					int num = centroidMasses.FastBinarySearch(rangeAccess.Low);
					if (num >= 0)
					{
						double num2 = centroidMasses[num];
						int num3 = firstInRange;
						bool flag2 = _positions[num3] > num2;
						while (!flag2 && num3 < PositionCount - 1 && _intensities[num3 + 1] > 0.0 && num3 - firstInRange <= 10)
						{
							if (_positions[num3 + 1] > num2)
							{
								flag2 = true;
								break;
							}
							num3++;
						}
						if (flag2)
						{
							int num4 = firstInRange - 1;
							bool flag3 = false;
							bool flag4 = false;
							bool flag5 = false;
							double num5 = 0.0;
							while (num4 >= 0 && num4 > largestIndexUsed && firstInRange - num4 <= 10)
							{
								if (!(_intensities[num4] > 0.0))
								{
									flag3 = true;
									break;
								}
								if (flag5)
								{
									if (_intensities[num4] > num5)
									{
										flag4 = true;
										break;
									}
								}
								else
								{
									flag5 = true;
								}
								num5 = _intensities[num4];
								num4--;
							}
							if (flag3 || flag4)
							{
								for (int i = num4; i < firstInRange; i++)
								{
									if (i != num4 || !flag4)
									{
										newIntensities[i] = _intensities[i];
									}
									largestIndexUsed = i;
								}
							}
						}
					}
				}
				void ProcessCentroidsAtRangeEnd()
				{
					bool flag2 = false;
					int num = -1;
					int num2 = centroidMasses.FastBinarySearch(rangeAccess.High);
					if (num2 > 0)
					{
						double value = centroidMasses[num2 - 1];
						if (rangeAccess.Contains(value))
						{
							flag2 = true;
							num = num2 - 1;
						}
					}
					else
					{
						double value2 = centroidMasses[centroidMasses.Length - 1];
						if (rangeAccess.Contains(value2))
						{
							flag2 = true;
							num = centroidMasses.Length - 1;
						}
					}
					if (flag2)
					{
						double num3 = centroidMasses[num];
						int num4 = largestIndexUsed - 1;
						if (num4 >= 0)
						{
							bool flag3 = _positions[num4] < num3;
							while (!flag3 && num4 > 0 && _intensities[num4 - 1] > 0.0 && largestIndexUsed - num4 <= 10)
							{
								if (_positions[num4 - 1] < num3)
								{
									flag3 = true;
									break;
								}
								num4--;
							}
							if (flag3)
							{
								int i = firstInRange;
								bool flag4 = false;
								bool flag5 = false;
								bool flag6 = false;
								double num5 = 0.0;
								for (; i < _positionCount && i - firstInRange <= 10; i++)
								{
									if (!(_intensities[i] > 0.0))
									{
										flag4 = true;
										break;
									}
									if (flag6)
									{
										if (_intensities[i] > num5)
										{
											flag5 = true;
											break;
										}
									}
									else
									{
										flag6 = true;
									}
									num5 = _intensities[i];
								}
								if (flag4 || flag5)
								{
									for (int j = firstInRange; j <= i; j++)
									{
										newIntensities[j] = _intensities[j];
										largestIndexUsed = j;
									}
								}
							}
						}
					}
				}
			}
			SegmentedScan segmentedScan = FromMassesAndIntensities(masses, newIntensities);
			segmentedScan.Flags = flags;
			return segmentedScan;
		}
	}
}
