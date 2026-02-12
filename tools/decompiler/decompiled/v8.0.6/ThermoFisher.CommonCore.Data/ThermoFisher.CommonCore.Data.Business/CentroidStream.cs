using System;
using System.Collections.Generic;
using System.Globalization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Centroid data making a second stream with profile scan.
/// </summary>
[Serializable]
public class CentroidStream : CommonCoreDataObject, ICloneable, IChromatogramPointBuilder, IDeepCloneable<CentroidStream>, ICentroidStreamAccess, ISimpleScanAccess
{
	/// <summary>
	/// this allows base peak mass and base peak intensity to search for the base value on first use
	/// </summary>
	private bool _basePeakFound;

	/// <summary>
	/// Intensity of largest peak (valid when _basePeakFound)
	/// </summary>
	private double _basePeakIntensity;

	/// <summary>
	/// Mass of largest peak
	/// </summary>
	private double _basePeakMass;

	/// <summary>
	/// Noise of largest peak (valid when _basePeakFound)
	/// </summary>
	private double _basePeakNoise;

	/// <summary>
	/// Resolution of largest peak (valid when _basePeakFound)
	/// </summary>
	private double _basePeakResolution;

	/// <summary>
	/// Mass calibration coefficients
	/// </summary>
	private double[] _coefficents;

	/// <summary>
	/// Gets or sets the list of baseline at each peak
	/// </summary>
	public double[] Baselines { get; set; }

	/// <summary>
	/// Gets the intensity of most intense peak
	/// </summary>
	public double BasePeakIntensity
	{
		get
		{
			if (!_basePeakFound)
			{
				FindBasePeak();
			}
			return _basePeakIntensity;
		}
	}

	/// <summary>
	/// Gets the mass of most intense peak
	/// </summary>
	public double BasePeakMass
	{
		get
		{
			if (!_basePeakFound)
			{
				FindBasePeak();
			}
			return _basePeakMass;
		}
	}

	/// <summary>
	/// Gets the noise of most intense peak
	/// </summary>
	public double BasePeakNoise
	{
		get
		{
			if (!_basePeakFound)
			{
				FindBasePeak();
			}
			return _basePeakNoise;
		}
	}

	/// <summary>
	/// Gets the resolution of most intense peak
	/// </summary>
	public double BasePeakResolution
	{
		get
		{
			if (!_basePeakFound)
			{
				FindBasePeak();
			}
			return _basePeakResolution;
		}
	}

	/// <summary>
	/// Gets or sets the list of charge calculated for peak
	/// </summary>
	public double[] Charges { get; set; }

	/// <summary>
	/// Gets or sets the calibration Coefficients
	/// </summary>
	public double[] Coefficients
	{
		get
		{
			return _coefficents;
		}
		set
		{
			_coefficents = value;
			if (_coefficents != null)
			{
				CoefficientsCount = _coefficents.Length;
			}
		}
	}

	/// <summary>
	/// Gets or sets the coefficients count.
	/// </summary>
	public int CoefficientsCount { get; set; }

	/// <summary>
	/// Gets or sets the flags for the peaks (such as reference)
	/// </summary>
	public PeakOptions[] Flags { get; set; }

	/// <summary>
	/// Gets or sets the list of Intensities for each centroid
	/// </summary>
	public double[] Intensities { get; set; }

	/// <summary>
	/// Gets or sets the number of centroids
	/// </summary>
	public int Length { get; set; }

	/// <summary>
	/// Gets or sets the list of masses of each centroid
	/// </summary>
	public double[] Masses { get; set; }

	/// <summary>
	/// Gets or sets the list of noise level near peak
	/// </summary>
	public double[] Noises { get; set; }

	/// <summary>
	/// Gets or sets resolution of each peak
	/// </summary>
	public double[] Resolutions { get; set; }

	/// <summary>
	/// Gets or sets the scan Number
	/// </summary>
	public int ScanNumber { get; set; }

	/// <summary>
	/// Creates a new instance of CentroidStream
	/// </summary>
	public CentroidStream()
	{
	}

	/// <summary>
	/// Deep copy constructor
	/// </summary>
	/// <param name="labelData"></param>
	protected CentroidStream(ICentroidStreamAccess labelData)
	{
		DeepCopyFrom(labelData);
	}

	/// <summary>
	/// Return the largest intensity (base value) in the ranges supplies
	/// </summary>
	/// <param name="ranges">Ranges of masses</param>
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
		for (int i = 0; i < ranges.Length; i++)
		{
			IRangeAccess fixedRange = RangeFactory.CreateFromRangeAndTolerance(ranges[i], toleranceOptions);
			num = GetBaseValue(fixedRange, num);
		}
		return num;
	}

	/// <summary>
	/// Clears all the data.
	/// </summary>
	public void Clear()
	{
		Length = 0;
		Masses = new double[0];
		Intensities = new double[0];
		Resolutions = new double[0];
		Baselines = new double[0];
		Noises = new double[0];
		Charges = new double[0];
		Flags = new PeakOptions[0];
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	public object Clone()
	{
		CentroidStream centroidStream = (CentroidStream)MemberwiseClone();
		centroidStream.Masses = new double[Length];
		centroidStream.Intensities = new double[Length];
		centroidStream.Resolutions = new double[Length];
		centroidStream.Baselines = new double[Length];
		centroidStream.Noises = new double[Length];
		centroidStream.Charges = new double[Length];
		centroidStream.Flags = new PeakOptions[Length];
		for (int i = 0; i < Length; i++)
		{
			centroidStream.Masses[i] = Masses[i];
			centroidStream.Intensities[i] = Intensities[i];
			centroidStream.Resolutions[i] = Resolutions[i];
			centroidStream.Baselines[i] = Baselines[i];
			centroidStream.Noises[i] = Noises[i];
			centroidStream.Charges[i] = Charges[i];
			centroidStream.Flags[i] = Flags[i];
		}
		if (Coefficients != null)
		{
			centroidStream.Coefficients = Coefficients.Clone() as double[];
		}
		else
		{
			centroidStream.Coefficients = Array.Empty<double>();
		}
		return centroidStream;
	}

	/// <summary>
	/// Make a deep clone of this object.
	/// </summary>
	/// <returns>An object containing all data in this, and no shared references</returns>
	public CentroidStream DeepClone()
	{
		CentroidStream centroidStream = new CentroidStream();
		centroidStream.DeepCopyFrom(this);
		return centroidStream;
	}

	internal void DeepCopyFrom(ICentroidStreamAccess that)
	{
		Length = that.Length;
		if (Length == 0)
		{
			Coefficients = ((that.CoefficientsCount > 0) ? ((double[])that.Coefficients.Clone()) : Array.Empty<double>());
			CoefficientsCount = that.CoefficientsCount;
			ScanNumber = that.ScanNumber;
			return;
		}
		Baselines = (double[])that.Baselines.Clone();
		Charges = (double[])that.Charges.Clone();
		Coefficients = ((that.CoefficientsCount > 0) ? ((double[])that.Coefficients.Clone()) : Array.Empty<double>());
		CoefficientsCount = that.CoefficientsCount;
		Flags = (PeakOptions[])that.Flags.Clone();
		Intensities = (double[])that.Intensities.Clone();
		Masses = (double[])that.Masses.Clone();
		Noises = (double[])that.Noises.Clone();
		Resolutions = (double[])that.Resolutions.Clone();
		ScanNumber = that.ScanNumber;
	}

	/// <summary>
	/// Get the list centroids.
	/// </summary>
	/// <returns>
	/// The centroids in the scan
	/// </returns>
	public IList<ICentroidPeak> GetCentroids()
	{
		ICentroidPeak[] array = new ICentroidPeak[Length];
		for (int i = 0; i < Length; i++)
		{
			array[i] = LabelPeak(i);
		}
		return array;
	}

	/// <summary>
	/// Convert the data for a given peak in this stream into a LabelPeak
	/// </summary>
	/// <param name="index">
	/// The index of the peak to convert.
	/// </param>
	/// <returns>
	/// Extracted data for the selected peak
	/// </returns>
	public LabelPeak GetLabelPeak(int index)
	{
		if (Length > 0 && index >= 0 && index < Length)
		{
			return new LabelPeak
			{
				Mass = Masses[index],
				Intensity = Intensities[index],
				Resolution = Resolutions[index],
				Baseline = Baselines[index],
				Noise = Noises[index],
				Charge = Charges[index],
				Flag = Flags[index]
			};
		}
		return null;
	}

	/// <summary>
	/// Convert the data into LabelPeak objects
	/// </summary>
	/// <returns>An array of LabelsPeaks, converted from this class</returns>
	public LabelPeak[] GetLabelPeaks()
	{
		LabelPeak[] array = null;
		if (Length > 0)
		{
			array = new LabelPeak[Length];
			for (int i = 0; i < Length; i++)
			{
				array[i] = LabelPeak(i);
			}
		}
		return array;
	}

	/// <summary>
	/// Forces re-computation of Base peaks , intensities.
	/// </summary>
	public void RefreshBaseDetails()
	{
		_basePeakFound = false;
	}

	/// <summary>
	/// Convert data into this object from an array of LabelPeaks
	/// </summary>
	/// <param name="labelPeaks">Data to populate the class</param>
	/// <returns>true on success. False if the labels peaks are null or empty</returns>
	public bool SetLabelPeaks(LabelPeak[] labelPeaks)
	{
		bool result = false;
		if (labelPeaks != null && labelPeaks.Length != 0)
		{
			int num = labelPeaks.Length;
			Masses = new double[num];
			Intensities = new double[num];
			Resolutions = new double[num];
			Baselines = new double[num];
			Noises = new double[num];
			Charges = new double[num];
			Flags = new PeakOptions[num];
			for (int i = 0; i < num; i++)
			{
				Masses[i] = labelPeaks[i].Mass;
				Intensities[i] = labelPeaks[i].Intensity;
				Resolutions[i] = labelPeaks[i].Resolution;
				Baselines[i] = labelPeaks[i].Baseline;
				Noises[i] = labelPeaks[i].Noise;
				Charges[i] = labelPeaks[i].Charge;
				Flags[i] = labelPeaks[i].Flag;
			}
			Length = num;
			result = true;
		}
		return result;
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
			IRangeAccess fixedRange = RangeFactory.CreateFromRangeAndTolerance(ranges[i], toleranceOptions);
			num = SumMasses(num, fixedRange);
		}
		return num;
	}

	/// <summary>
	/// Sum all masses within the ranges
	/// </summary>
	/// <param name="ranges">List of ranges to sum</param>
	/// <param name="tolerance">If the ranges have equal mass values,
	/// then tolerance is subtracted from low and added to high to search for matching masses</param>
	/// <returns>Sum of intensities in all ranges</returns>
	public double SumMasses(IRangeAccess[] ranges, double tolerance)
	{
		if (ranges == null)
		{
			throw new ArgumentNullException("ranges");
		}
		double num = 0.0;
		for (int i = 0; i < ranges.Length; i++)
		{
			IRangeAccess fixedRange = RangeFactory.CreateFromRangeAndTolerance(ranges[i], tolerance);
			num = SumMasses(num, fixedRange);
		}
		return num;
	}

	/// <summary>
	/// Convert to simple scan.
	/// Even though this class implements ISimpleScanAccess,
	/// there can be an advantage in doing this conversion, as when this object goes out of scope
	/// the converted object only holds the mass and intensity refs, and will need less memory.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SimpleScan" />.
	/// </returns>
	public SimpleScan ToSimpleScan()
	{
		return new SimpleScan
		{
			Masses = Masses,
			Intensities = Intensities
		};
	}

	/// <summary>
	/// Convert to segmented scan.
	/// This feature is intended for use where an application or algorithm in "SegmentedScan" format,
	/// such as typical centroid data from ITMS, but the data in this scan came from an FTMS detector,
	/// which would have the profile data in "SegmentedScan" and the centroid data in this object.
	/// Data from this object is duplicated (deep copy),
	/// such that changing values in the returned object will not affect data in
	/// this object.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" />.
	/// </returns>
	public SegmentedScan ToSegmentedScan()
	{
		SegmentedScan segmentedScan = new SegmentedScan();
		segmentedScan.Positions = Masses.Clone() as double[];
		segmentedScan.Intensities = Intensities.Clone() as double[];
		segmentedScan.Flags = Flags.Clone() as PeakOptions[];
		segmentedScan.Ranges = new Range[1] { GetMassRange() };
		segmentedScan.SegmentSizes = new int[1] { Masses.Length };
		return segmentedScan;
	}

	/// <summary>
	/// Convert to Scan.
	/// This feature is intended for use where an application or algorithm needs data in "Scan" format,
	/// with centroid information in the "SegmentedScan" property of the Scan.
	/// (such as typical centroid data from ITMS), but the data in this scan came from an FTMS detector,
	/// which would have the profile data in "SegmentedScan" and the centroid data in this object.
	/// The data is first converted to SegmentedScan format (using ToSegmentedScan) then a new Scan is made
	/// containing that data (with no data in "CentroidStream).
	/// Data from this object is duplicated (deep copy),
	/// such that changing values in the returned object will not affect data in
	/// this object.
	/// This initializes the returned scan's "ScanStatistics" based on the returned mass and intensity data.
	/// If the (optional) originalScanStats parameter is included, information from 
	/// that is used to initialize  RT, scan number and other fields
	/// which cannot be calculated from this data.
	/// The only values updated in the scan statistics are "BasePeakMass" and "BasePeakIntenity".
	/// All other values are either as copied from the supplied parameter, or defaults.
	/// Application should set any other values needed in the Scan,
	/// such as "ScansCombined, ToleranceUnit, MassResolution",
	/// which cannot be determined from the supplied parameters.
	/// </summary>
	/// <param name="originalScanStats">If this is supplied, the scan statistics are initialized as a deep clone
	/// of the supplied object (so that RT etc. get preserved) then the values of BasePeakMass and 
	/// BasePeakIntensity are updated from this object</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" />.
	/// </returns>
	public Scan ToScan(ScanStatistics originalScanStats = null)
	{
		Scan scan = new Scan();
		SegmentedScan segmentedScan = ToSegmentedScan();
		scan.SegmentedScan = segmentedScan;
		ScanStatistics scanStatistics = ((originalScanStats == null) ? new ScanStatistics() : originalScanStats.DeepClone());
		scanStatistics.BasePeakIntensity = BasePeakIntensity;
		scanStatistics.BasePeakMass = BasePeakMass;
		scan.ScanStatistics = scanStatistics;
		scan.ScanType = scanStatistics.ScanType;
		return scan;
	}

	/// <summary>
	/// Find the mass limits of this data
	/// </summary>
	/// <returns></returns>
	private Range GetMassRange()
	{
		if (Masses.Length != 0)
		{
			return new Range(Masses[0], Masses[^1]);
		}
		return new Range();
	}

	/// <summary>
	/// Test if this is a valid object (all streams are not null. All data has same length)
	/// </summary>
	/// <returns>
	/// true if the object has valid data in it.
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
		if (Baselines == null)
		{
			throw new ArgumentException("Baselines array is not initialized.");
		}
		if (Baselines.Length != Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Length of Baselines array({0}) does not match Length.", Baselines.Length));
		}
		if (Charges == null)
		{
			throw new ArgumentException("Charges array is not initialized.");
		}
		if (Charges.Length != Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Length of Charges array({0}) does not match Length.", Charges.Length));
		}
		if (Flags == null)
		{
			throw new ArgumentException("Flags array is not initialized.");
		}
		if (Flags.Length != Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Length of Flags array({0}) does not match Length.", Flags.Length));
		}
		if (Intensities == null)
		{
			throw new ArgumentException("Intensities array is not initialized.");
		}
		if (Intensities.Length != Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Length of Intensities array({0}) does not match Length.", Intensities.Length));
		}
		if (Masses == null)
		{
			throw new ArgumentException("Masses array is not initialized.");
		}
		if (Masses.Length != Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Length of Masses array({0}) does not match Length.", Masses.Length));
		}
		if (Noises == null)
		{
			throw new ArgumentException("Noises array is not initialized.");
		}
		if (Noises.Length != Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Length of Noises array({0}) does not match Length.", Noises.Length));
		}
		if (Resolutions == null)
		{
			throw new ArgumentException("Resolutions array is not initialized.");
		}
		if (Resolutions.Length != Length)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Length of Resolutions array({0}) does not match Length.", Resolutions.Length));
		}
	}

	/// <summary>
	/// Internal method, used to truncate long arrays
	/// </summary>
	/// <param name="newSize">The size to "resize" to</param>
	internal void Resize(int newSize)
	{
		Masses = ResizeArray(Masses, newSize);
		Intensities = ResizeArray(Intensities, newSize);
		Resolutions = ResizeArray(Resolutions, newSize);
		Baselines = ResizeArray(Baselines, newSize);
		Noises = ResizeArray(Noises, newSize);
		Charges = ResizeArray(Charges, newSize);
		Flags = ResizeArray(Flags, newSize);
		Length = newSize;
	}

	/// <summary>
	/// Change the size of an array. This exists because "Array.Resize" cannot be directly called on an array property.
	/// </summary>
	/// <param name="options">
	/// The original data.
	/// </param>
	/// <param name="newSize">
	/// The new size.
	/// </param>
	/// <returns>
	/// The resized options
	/// </returns>
	private static PeakOptions[] ResizeArray(PeakOptions[] options, int newSize)
	{
		Array.Resize(ref options, newSize);
		return options;
	}

	/// <summary>
	/// Change the size of an array. This exists because "Array.Resize" cannot be directly called on an array property.
	/// </summary>
	/// <param name="doubles">
	/// The original data.
	/// </param>
	/// <param name="newSize">
	/// The new size.
	/// </param>
	/// <returns>
	/// The resized array.
	/// </returns>
	private static double[] ResizeArray(double[] doubles, int newSize)
	{
		Array.Resize(ref doubles, newSize);
		return doubles;
	}

	/// <summary>
	/// Find the most intense peak
	/// </summary>
	private void FindBasePeak()
	{
		_basePeakFound = true;
		_basePeakMass = 0.0;
		_basePeakIntensity = 0.0;
		_basePeakResolution = 0.0;
		_basePeakNoise = 0.0;
		if (Masses == null || Masses.Length == 0)
		{
			return;
		}
		_basePeakMass = Masses[0];
		_basePeakIntensity = Intensities[0];
		_basePeakResolution = Resolutions[0];
		_basePeakNoise = Noises[0];
		for (int i = 1; i < Masses.Length; i++)
		{
			if (Intensities[i] > _basePeakIntensity)
			{
				_basePeakIntensity = Intensities[i];
				_basePeakMass = Masses[i];
				_basePeakResolution = Resolutions[i];
				_basePeakNoise = Noises[i];
			}
		}
	}

	/// <summary>
	/// Find the largest value in a given mass range, and combine with the value passed in.
	/// </summary>
	/// <param name="fixedRange">
	/// The mass range to analyze
	/// </param>
	/// <param name="baseValue">
	/// The base value so far.
	/// </param>
	/// <returns>
	/// The largest of the base value passed in, and the base value in this mass range.
	/// </returns>
	private double GetBaseValue(IRangeAccess fixedRange, double baseValue)
	{
		int num = Array.BinarySearch(Masses, fixedRange.Low);
		if (num < 0)
		{
			num = ~num;
		}
		while (num < Masses.Length && Masses[num] <= fixedRange.High)
		{
			baseValue = Math.Max(baseValue, Intensities[num++]);
		}
		return baseValue;
	}

	/// <summary>
	/// Create a label peak from data at a given index.
	/// </summary>
	/// <param name="labelIndex">
	/// The label index.
	/// </param>
	/// <returns>
	/// The <see cref="M:ThermoFisher.CommonCore.Data.Business.CentroidStream.LabelPeak(System.Int32)" />.
	/// </returns>
	private LabelPeak LabelPeak(int labelIndex)
	{
		double num = Intensities[labelIndex];
		double num2 = Noises[labelIndex];
		double num3 = Baselines[labelIndex];
		double num4 = num - num3;
		double num5 = num2 - num3;
		if (num4 < 0.0)
		{
			num4 = 0.0;
		}
		return new LabelPeak
		{
			Mass = Masses[labelIndex],
			Intensity = num,
			Resolution = Resolutions[labelIndex],
			Baseline = num3,
			Noise = num2,
			Charge = Charges[labelIndex],
			Flag = Flags[labelIndex],
			SignalToNoise = ((num5 > 0.0) ? (num4 / num5) : num4)
		};
	}

	/// <summary>
	/// Calculate the sum of intensities, for peaks within a mass range, and add to previous sum
	/// </summary>
	/// <param name="sum">
	/// The sum so far
	/// </param>
	/// <param name="fixedRange">
	/// The mass range.
	/// </param>
	/// <returns>
	/// This sum passed in, plus the sum of intensities of all peaks in range.
	/// </returns>
	private double SumMasses(double sum, IRangeAccess fixedRange)
	{
		int num = Array.BinarySearch(Masses, fixedRange.Low);
		if (num < 0)
		{
			num = ~num;
		}
		while (num < Masses.Length && Masses[num] <= fixedRange.High)
		{
			sum += Intensities[num++];
		}
		return sum;
	}

	/// <summary>
	/// slice the data, using a set of mass ranges.
	/// </summary>
	/// <param name="compactedRange">Ranges of data to include</param>
	/// <returns>sA scan which only has data in the requested ranges</returns>
	internal CentroidStream Slice(List<IRangeAccess> compactedRange)
	{
		if (!TryValidate())
		{
			return new CentroidStream();
		}
		List<LabelPeak> list = new List<LabelPeak>();
		double[] masses = Masses;
		int num = masses.Length;
		double[] intensities = Intensities;
		PeakOptions[] flags = Flags;
		double[] baselines = Baselines;
		double[] resolutions = Resolutions;
		double[] noises = Noises;
		double[] charges = Charges;
		foreach (IRangeAccess item in compactedRange)
		{
			int i = masses.FastBinarySearch(item.Low);
			if (i < 0 || !(masses[i] <= item.High))
			{
				continue;
			}
			for (; i < num && masses[i] <= item.High; i++)
			{
				bool flag = list.Count == 0;
				if (!flag)
				{
					flag = masses[i] > list[list.Count - 1].Mass;
				}
				if (flag)
				{
					list.Add(new LabelPeak
					{
						Baseline = baselines[i],
						Charge = charges[i],
						Flag = flags[i],
						Intensity = intensities[i],
						Mass = masses[i],
						Noise = noises[i],
						Resolution = resolutions[i]
					});
				}
			}
		}
		CentroidStream centroidStream = new CentroidStream();
		centroidStream.SetLabelPeaks(list.ToArray());
		return centroidStream;
	}
}
