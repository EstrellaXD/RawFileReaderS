using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to represent a scan
/// </summary>
[Serializable]
public class Scan : CommonCoreDataObject, IDeepCloneable<Scan>, IScanAccess
{
	/// <summary>
	/// The fragment result.
	/// </summary>
	private class FragmentResult
	{
		/// <summary>
		/// Gets or sets Intensity.
		/// </summary>
		public double Intensity { get; set; }

		/// <summary>
		/// Gets or sets Mass.
		/// </summary>
		public double Mass { get; set; }

		/// <summary>
		/// Gets or sets Options.
		/// </summary>
		public PeakOptions Options { get; set; }
	}

	[NonSerialized]
	private readonly MassToFrequencyConverter _massToFrequencyConverter;

	[NonSerialized]
	private readonly NoiseAndBaseline[] _rawFileNoise;

	private double _massFuzz = 0.5;

	private bool _preferCentroids = true;

	private bool[] _segmentSimilar;

	private bool _similarFlag;

	private ToleranceMode _toleranceUnit;

	private bool _userTolerance;

	/// <summary>
	/// Gets or sets A second data stream for the scan
	/// </summary>
	public CentroidStream CentroidScan { get; set; }

	/// <summary>
	/// Gets A second data stream for the scan
	/// </summary>
	public ICentroidStreamAccess CentroidStreamAccess => CentroidScan;

	/// <summary>
	/// Gets a value indicating whether this scan has a centroid stream.
	/// </summary>
	public bool HasCentroidStream => CentroidScan?.Masses != null;

	/// <summary>
	/// Gets a value indicating whether this scan has a noise table.
	/// This will be true only if the scan was constructed with the overload containing this table.
	/// Note that this is not related to having "noise and baseline" values with centroid stream data.
	/// This is a separate table, used for spectrum averaging and subtraction of orbitrap data
	/// </summary>
	public bool HasNoiseTable => _rawFileNoise != null;

	/// <summary>
	/// Gets or sets a value indicating whether the User Tolerance value is being used.
	/// </summary>
	public bool IsUserTolerance
	{
		get
		{
			return _userTolerance;
		}
		set
		{
			_userTolerance = value;
		}
	}

	/// <summary>
	/// Gets or sets the mass resolution for all scan arithmetic operations
	/// </summary>
	public double MassResolution
	{
		get
		{
			return _massFuzz;
		}
		set
		{
			_massFuzz = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether, when requesting "Preferred data", the centroid stream will be returned.
	/// For example "<see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.PreferredMasses" />", "<see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.PreferredIntensities" />".
	/// If this property is false, or there is no centroid stream, then these methods will return
	/// the data from <see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.SegmentedScan" />. For greater efficiency, callers should cache the return of "<see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.PreferredMasses" />".
	/// Typically data processing, such as elemental compositions, should use these methods.
	/// </summary>
	public bool PreferCentroids
	{
		get
		{
			return _preferCentroids;
		}
		set
		{
			_preferCentroids = value;
		}
	}

	/// <summary>
	/// Gets peak flags (such as saturated) for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double PreferredBasePeakIntensity
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.BasePeakIntensity;
			}
			return ScanStatistics.BasePeakIntensity;
		}
	}

	/// <summary>
	/// Gets Mass of base peak default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double PreferredBasePeakMass
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.BasePeakMass;
			}
			return ScanStatistics.BasePeakMass;
		}
	}

	/// <summary>
	/// Gets Noise of base peak for default data stream (usually centroid stream, if present).
	/// Falls back to zero if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double PreferredBasePeakNoise
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.BasePeakNoise;
			}
			return 0.0;
		}
	}

	/// <summary>
	/// Gets Resolution of base peak for default data stream (usually centroid stream, if present).
	/// Falls back to zero if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double PreferredBasePeakResolution
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.BasePeakResolution;
			}
			return 0.0;
		}
	}

	/// <summary>
	/// Gets peak flags (such as saturated) for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public PeakOptions[] PreferredFlags
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.Flags;
			}
			return SegmentedScan.Flags;
		}
	}

	/// <summary>
	/// Gets Intensity for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double[] PreferredIntensities
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.Intensities;
			}
			return SegmentedScan.Intensities;
		}
	}

	/// <summary>
	/// Gets the Mass for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="P:ThermoFisher.CommonCore.Data.Business.Scan.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double[] PreferredMasses
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.Masses;
			}
			return SegmentedScan.Positions;
		}
	}

	/// <summary>
	/// Gets Noises for default data stream (usually centroid stream, if present).
	/// Returns an empty array if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double[] PreferredNoises
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.Noises;
			}
			return new double[0];
		}
	}

	/// <summary>
	/// Gets Baselines for default data stream (usually centroid stream, if present).
	/// Returns an empty array if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double[] PreferredBaselines
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.Baselines;
			}
			return new double[0];
		}
	}

	/// <summary>
	/// Gets Resolutions for default data stream (usually centroid stream, if present).
	/// Returns an empty array if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	public double[] PreferredResolutions
	{
		get
		{
			if (PreferCentroids && HasCentroidStream)
			{
				return CentroidScan.Resolutions;
			}
			return new double[0];
		}
	}

	/// <summary>
	/// Gets or sets the number of scans which were combined to create this scan.
	/// For example: By the scan averager.
	/// This can be zero if this is a "scan read from a file"
	/// </summary>
	public int ScansCombined { get; set; }

	/// <summary>
	/// Gets or sets Header information for the scan
	/// </summary>
	public ScanStatistics ScanStatistics { get; set; }

	/// <summary>
	/// Gets Header information for the scan
	/// </summary>
	public IScanStatisticsAccess ScanStatisticsAccess => ScanStatistics;

	/// <summary>
	/// Gets or sets Type of scan (for filtering)
	/// </summary>
	public string ScanType { get; set; }

	/// <summary>
	/// Gets or sets The data for the scan
	/// </summary>
	public SegmentedScan SegmentedScan { get; set; }

	/// <summary>
	/// Gets The data for the scan
	/// </summary>
	public ISegmentedScanAccess SegmentedScanAccess => SegmentedScan;

	/// <summary>
	/// Gets or sets IScanSubtract interface pointer.
	/// </summary>
	/// <returns>Interface to perform subtraction</returns>
	[XmlIgnore]
	public IScanSubtract SubtractionPointer { get; set; }

	/// <summary>
	/// Gets or sets IScanAdd interface.
	/// This delegates addition of FT profile scans.
	/// </summary>
	/// <returns>Interface to perform addition</returns>
	[XmlIgnore]
	public IScanAdd ScanAdder { get; set; }

	/// <summary>
	/// Gets or sets the Tolerance value.
	/// </summary>
	public ToleranceMode ToleranceUnit
	{
		get
		{
			return _toleranceUnit;
		}
		set
		{
			_toleranceUnit = value;
		}
	}

	/// <summary>
	/// Get or sets a value indicating whether scan + and - operators will merge data from scans
	/// which were not scanned over a similar range.
	/// Only applicable when scans only have a single segment.
	/// By default: Scans are considered incompatible if:
	/// The span of the scanned mass range differs by 10%
	/// The start or end of the scanned mass range differs by 10%
	/// If this is set as "true" then any mass ranges will be merged.
	/// </summary>
	public bool AlwaysMergeSegments { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Scan" /> class.
	/// Default construction.
	/// </summary>
	public Scan()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Scan" /> class.
	/// Overload for processed data (such as averaged or subtracted)
	/// </summary>
	/// <param name="converter">
	///     The converter from mass to frequency (used by FT data averaging and exporting).
	/// </param>
	/// <param name="rawFileNoise">Optional: Noise and baseline table</param>
	public Scan(MassToFrequencyConverter converter, NoiseAndBaseline[] rawFileNoise = null)
	{
		_massToFrequencyConverter = converter;
		_rawFileNoise = rawFileNoise;
	}

	/// <summary>
	/// Copy scan data. This protected method is inteded for derived object use only.
	/// It is not a "deep clone" of a scan.
	/// </summary>
	/// <param name="scan">Scan to copy</param>
	protected Scan(Scan scan)
	{
		_massToFrequencyConverter = scan._massToFrequencyConverter;
		_rawFileNoise = scan._rawFileNoise;
		CentroidScan = scan.CentroidScan;
		IsUserTolerance = scan.IsUserTolerance;
		MassResolution = scan.MassResolution;
		PreferCentroids = scan.PreferCentroids;
		ScanStatistics = scan.ScanStatistics;
		ScanType = scan.ScanType;
		SegmentedScan = scan.SegmentedScan;
		ToleranceUnit = scan.ToleranceUnit;
	}

	/// <summary>
	/// Create a scan object from a file and a retention time.
	/// </summary>
	/// <param name="rawFile">
	/// File to read from
	/// </param>
	/// <param name="time">
	/// time of Scan number to read
	/// </param>
	/// <returns>
	/// The scan read, or null of the scan number if not valid
	/// </returns>
	public static Scan AtTime(IRawData rawFile, double time)
	{
		if (rawFile == null)
		{
			throw new ArgumentNullException("rawFile");
		}
		int scanNumber = rawFile.ScanNumberFromRetentionTime(time);
		return FromFile(rawFile, scanNumber);
	}

	/// <summary>
	/// Create a scan object from a detector reader and a retention time.
	/// </summary>
	/// <param name="detectorReader">
	/// Detector to read from
	/// </param>
	/// <param name="time">
	/// time of Scan number to read
	/// </param>
	/// <returns>
	/// The scan read, or null of the scan number if not valid
	/// </returns>
	public static Scan AtTime(IDetectorReader detectorReader, double time)
	{
		if (detectorReader == null)
		{
			throw new ArgumentNullException("detectorReader");
		}
		ScanStatistics scanStatsForRetentionTime = detectorReader.GetScanStatsForRetentionTime(time);
		if (scanStatsForRetentionTime == null || scanStatsForRetentionTime.ScanNumber <= 0)
		{
			return null;
		}
		Scan scan = new Scan();
		scan.ReadFromDetector(detectorReader, scanStatsForRetentionTime.ScanNumber, scanStatsForRetentionTime);
		return scan;
	}

	/// <summary>
	/// test if 2 scans can be averaged or subtracted.
	/// </summary>
	/// <param name="identicalFlag">
	/// Returned as "true" if all segments are the same
	/// </param>
	/// <param name="currentScan">
	/// Current scan object
	/// </param>
	/// <param name="toMerge">
	/// The scan to possibly add
	/// </param>
	/// <returns>
	/// true if scans can be merged
	/// </returns>
	public static bool CanMergeScan(ref bool identicalFlag, Scan currentScan, Scan toMerge)
	{
		if (currentScan == null)
		{
			throw new ArgumentNullException("currentScan");
		}
		if (toMerge == null)
		{
			throw new ArgumentNullException("toMerge");
		}
		bool flag = true;
		if (identicalFlag)
		{
			identicalFlag = false;
		}
		int packetType = currentScan.ScanStatistics.PacketType;
		int packetType2 = toMerge.ScanStatistics.PacketType;
		int num = CommonData.LOWord(packetType);
		int num2 = CommonData.LOWord(packetType2);
		if (num == 20 && num2 == 20)
		{
			return true;
		}
		if (num == 21 && num2 == 21)
		{
			return true;
		}
		bool flag2 = CommonData.IsProfileScan(packetType);
		bool flag3 = CommonData.IsProfileScan(packetType2);
		if ((flag2 || flag3) && (IsKnownProfileType(num) || IsKnownProfileType(num2)))
		{
			return true;
		}
		bool flag4;
		if (flag2 && flag3)
		{
			flag4 = false;
			flag = (identicalFlag = currentScan.SegmentedScan.SegmentCount == toMerge.SegmentedScan.SegmentCount);
		}
		else
		{
			flag4 = num == 24 && num2 == 24;
			if (flag4)
			{
				flag = (identicalFlag = currentScan.SegmentedScan.SegmentCount == toMerge.SegmentedScan.SegmentCount);
			}
			else
			{
				identicalFlag = false;
			}
		}
		if (identicalFlag)
		{
			int counter = CheckForIdentical(out identicalFlag, currentScan, toMerge);
			if (!identicalFlag)
			{
				flag = TryToMergeSimilarScans(currentScan, toMerge, flag4, counter, flag);
			}
		}
		return flag;
	}

	/// <summary>
	/// Create an object which can be used to read scans from a file, with optional caching.
	/// This is valuable if repeated operations (such as averaging) are expected over the same region of data.
	/// Scans returned from each call are unique objects, even if called repeatedly with the same scan number.
	/// </summary>
	/// <param name="cacheSize">
	/// Number of scans cached. 
	/// When set to 1 or more, this creates a FIFO, keeping track of the most recently read scans.
	/// If a scan in the FIFO is requested again, it is pulled from the cache.
	/// If a scan is not in the cache, then a new scan is read from the file.
	/// If the cache is full, the oldest scan is dropped. The newly read scan is that added to the FIFO cache.
	/// If size is set to 0, this makes a trivial object
	/// with no overheads, that directly gets scans from the file.
	/// </param>
	/// <returns>
	/// Object to read scans from a file
	/// </returns>
	public static IScanReader CreateScanReader(int cacheSize)
	{
		if (cacheSize > 0)
		{
			return new CachedScanProvider(cacheSize);
		}
		return new DefaultScanProvider();
	}

	/// <summary>
	/// Create a scan object from a file and a scan number.
	/// </summary>
	/// <param name="rawFile">
	/// File to read from
	/// </param>
	/// <param name="scanNumber">
	/// Scan number to read
	/// </param>
	/// <param name="formatProvider">defined culture, for number formatting (default Invariant)</param>
	/// <returns>
	/// The scan read, or null of the scan number if not valid
	/// </returns>
	public static Scan FromFile(IRawData rawFile, int scanNumber, IFormatProvider formatProvider = null)
	{
		if (rawFile == null)
		{
			throw new ArgumentNullException("rawFile");
		}
		if (formatProvider == null)
		{
			formatProvider = CultureInfo.InvariantCulture;
		}
		Scan scan = null;
		if (scanNumber > 0)
		{
			ScanStatistics scanStatsForScanNumber = rawFile.GetScanStatsForScanNumber(scanNumber);
			if (scanStatsForScanNumber != null)
			{
				scan = new Scan();
				scan.ReadFromFile(rawFile, scanNumber, scanStatsForScanNumber);
			}
		}
		return scan;
	}

	/// <summary>
	/// Create a scan object from a detector Reader and a scan number.
	/// </summary>
	/// <param name="detectorReader">
	/// File to read from
	/// </param>
	/// <param name="scanNumber">
	/// Scan number to read
	/// </param>
	/// <returns>
	/// The scan read, or null of the scan number if not valid
	/// </returns>
	public static Scan FromDetector(IDetectorReader detectorReader, int scanNumber)
	{
		if (detectorReader == null)
		{
			throw new ArgumentNullException("detectorReader");
		}
		Scan scan = null;
		if (scanNumber > 0)
		{
			ScanStatistics scanStatsForScanNumber = detectorReader.GetScanStatsForScanNumber(scanNumber);
			if (scanStatsForScanNumber != null)
			{
				scan = new Scan();
				scan.ReadFromDetector(detectorReader, scanNumber, scanStatsForScanNumber);
			}
		}
		return scan;
	}

	/// <summary>
	/// Merge the two scans.
	/// </summary>
	/// <param name="currentScanObject">current scan object(this).</param>
	/// <param name="inputScan">scan object to merge.</param>
	/// <returns>returns the sum of two scans.</returns>
	public static Scan operator +(Scan currentScanObject, Scan inputScan)
	{
		bool identicalFlag = false;
		currentScanObject._similarFlag = false;
		int segmentCount = currentScanObject.SegmentedScan.SegmentCount;
		currentScanObject._segmentSimilar = new bool[segmentCount];
		if (!CanMergeScan(ref identicalFlag, currentScanObject, inputScan))
		{
			return currentScanObject;
		}
		int num = CommonData.LOWord(currentScanObject.ScanStatistics.PacketType);
		int num2 = CommonData.LOWord(inputScan.ScanStatistics.PacketType);
		if (num == 20 && num2 == 20)
		{
			return AddFtCentroidData(currentScanObject, inputScan);
		}
		if (num == 21 && num2 == 21)
		{
			return AddFTProfileData(currentScanObject, inputScan);
		}
		if ((CommonData.IsProfileScan(currentScanObject.ScanStatistics.PacketType) || CommonData.IsProfileScan(inputScan.ScanStatistics.PacketType)) && (IsKnownProfileType(num) || IsKnownProfileType(num2)))
		{
			identicalFlag = false;
		}
		if (currentScanObject.HasCentroidStream && inputScan.HasCentroidStream && (currentScanObject.CentroidScan.Length > 0 || inputScan.CentroidScan.Length > 0))
		{
			AddCentroids(currentScanObject, inputScan);
		}
		return AddSegments(identicalFlag, currentScanObject, inputScan);
	}

	/// <summary>
	/// Averages the two scans.
	/// </summary>
	/// <param name="inputScan">scan object to average.</param>
	/// <param name="size">divider (or) common factor.</param>
	/// <returns>returns the average scan.</returns>
	public static Scan operator /(Scan inputScan, double size)
	{
		if (inputScan != null)
		{
			if (inputScan.SegmentedScan != null && inputScan.SegmentedScan.Intensities != null && inputScan.SegmentedScan.Intensities.Length != 0)
			{
				for (int i = 0; i < inputScan.SegmentedScan.Positions.Length; i++)
				{
					inputScan.SegmentedScan.Intensities[i] /= size;
				}
			}
			if (inputScan.ScanStatistics != null)
			{
				inputScan.ScanStatistics.BasePeakIntensity /= size;
				inputScan.ScanStatistics.TIC /= size;
			}
			inputScan.CentroidScan = ScaleIntensities(1.0 / size, inputScan.CentroidScan);
		}
		return inputScan;
	}

	/// <summary>
	/// Subtract method for MS data.
	/// </summary>
	/// <param name="currentScanObject">current scan object.</param>
	/// <param name="inputScan">scan object to subtract.</param>
	/// <returns>returns the subtracted scan object.</returns>
	public static Scan operator -(Scan currentScanObject, Scan inputScan)
	{
		bool identicalFlag = false;
		currentScanObject._similarFlag = false;
		int segmentCount = currentScanObject.SegmentedScan.SegmentCount;
		currentScanObject._segmentSimilar = new bool[segmentCount];
		if (!CanMergeScan(ref identicalFlag, currentScanObject, inputScan))
		{
			currentScanObject._segmentSimilar = null;
			return currentScanObject;
		}
		int num = CommonData.LOWord(currentScanObject.ScanStatistics.PacketType);
		int num2 = CommonData.LOWord(inputScan.ScanStatistics.PacketType);
		if (num == 20 && num2 == 20)
		{
			return SubFTCentroidData(currentScanObject, inputScan);
		}
		if (num == 21 && num2 == 21)
		{
			return SubFTProfileData(currentScanObject, inputScan);
		}
		if ((CommonData.IsProfileScan(currentScanObject.ScanStatistics.PacketType) || CommonData.IsProfileScan(inputScan.ScanStatistics.PacketType)) && (IsKnownProfileType(num) || IsKnownProfileType(num2)))
		{
			currentScanObject._similarFlag = false;
		}
		if (currentScanObject.CentroidScan != null && inputScan.CentroidScan != null && (currentScanObject.CentroidScan.Length > 0 || inputScan.CentroidScan.Length > 0))
		{
			SubCentroids(currentScanObject, inputScan);
		}
		return SubtractSegments(identicalFlag, currentScanObject, inputScan);
	}

	/// <summary>
	/// Converts the segmented scan to centroid scan.
	/// Used to centroid profile data.
	/// </summary>
	/// <param name="currentScan">
	/// The scan to centroid
	/// </param>
	/// <returns>The centroided version of the scan</returns>
	public static Scan ToCentroid(Scan currentScan)
	{
		if (currentScan.ScanStatistics == null)
		{
			return currentScan;
		}
		if (!CommonData.IsProfileScan(currentScan.ScanStatistics.PacketType))
		{
			return currentScan;
		}
		int segmentCount = currentScan.SegmentedScan.SegmentCount;
		List<FragmentResult>[] array = new List<FragmentResult>[segmentCount];
		for (int i = 0; i < segmentCount; i++)
		{
			array[i] = currentScan.Fragment(i);
		}
		Scan scan = new Scan
		{
			CentroidScan = new CentroidStream(),
			MassResolution = currentScan.MassResolution,
			ScanStatistics = (currentScan.ScanStatistics.Clone() as ScanStatistics),
			ScanType = currentScan.ScanType,
			SegmentedScan = new SegmentedScan(),
			ToleranceUnit = currentScan.ToleranceUnit
		};
		scan.SegmentedScan.Ranges = OrderSegments(currentScan.SegmentedScan.Ranges, array);
		scan.SegmentedScan.SegmentCount = scan.SegmentedScan.Ranges.Length;
		MergeSegments(scan, array);
		int num = CommonData.LOWord(currentScan.ScanStatistics.PacketType);
		if (BasicProfileType(num))
		{
			scan.ScanStatistics.PacketType = 1;
			scan.ScanStatistics.IsCentroidScan = true;
		}
		else if (num == 8 || num == 10)
		{
			scan.ScanStatistics.PacketType = 12;
		}
		else if (currentScan.ScanStatistics.SpectrumPacketType == SpectrumPacketType.HighResolutionSpectrum)
		{
			scan.ScanStatistics.PacketType = 2;
			scan.ScanStatistics.IsCentroidScan = true;
		}
		return scan;
	}

	/// <summary>
	/// Make a deep clone of this scan.
	/// </summary>
	/// <returns>
	/// An object containing all data in the input, and no shared references
	/// </returns>
	public Scan DeepClone()
	{
		return new Scan
		{
			CentroidScan = CentroidScan.DeepClone(),
			IsUserTolerance = IsUserTolerance,
			MassResolution = MassResolution,
			PreferCentroids = PreferCentroids,
			ScanStatistics = (ScanStatistics)ScanStatistics.Clone(),
			ScanType = ScanType,
			SegmentedScan = SegmentedScan.DeepClone(),
			ToleranceUnit = ToleranceUnit
		};
	}

	/// <summary>
	/// Deep Copy all data from another scan into this scan.
	/// Intended to assist in cloning derived objects.
	/// </summary>
	protected void DeepCopyFrom(Scan other)
	{
		CentroidScan = other.CentroidScan.DeepClone();
		IsUserTolerance = other.IsUserTolerance;
		MassResolution = other.MassResolution;
		PreferCentroids = other.PreferCentroids;
		ScanStatistics = (ScanStatistics)other.ScanStatistics.Clone();
		ScanType = other.ScanType;
		SegmentedScan = other.SegmentedScan.DeepClone();
		ToleranceUnit = other.ToleranceUnit;
	}

	/// <summary>
	/// Return a slice of a scan which only contains data within the supplied mass Range or ranges.
	/// For example: For a scan with data from m/z 200 to 700, and a single mass range of 300 to 400:
	/// This returns a new scan containing all data with the range 300 to 400.
	/// </summary>
	/// <param name="massRanges">The mass ranges, where data should be retained. When multiple ranges are supplied,
	/// all data which is in at least one range is included in the returned scan</param>
	/// <param name="trimMassRange">If this is true, then the scan will reset the
	/// scan's mass range to the bounds of the supplied mass ranges </param>
	/// <param name="expandProfiles">This setting only applies when the scan has both profile and centroid data.
	/// If true: When there isa centroid near the start or end of a range, and the first or
	/// final "above zero" section of the profile includes that peak, then the profile is extended, to include the points
	/// which contribute to that peak. A maximum of 10 points may be added</param>
	/// <returns>A copy of the scan, with only the data in the supplied ranges</returns>
	public Scan Slice(IRangeAccess[] massRanges, bool trimMassRange = false, bool expandProfiles = true)
	{
		if (massRanges == null)
		{
			throw new ArgumentNullException("massRanges");
		}
		if (massRanges.Length < 1)
		{
			throw new ArgumentOutOfRangeException("massRanges", "Must have at least 1 mass range");
		}
		Array.Sort(massRanges, OrderByLowest);
		List<IRangeAccess> list = Compact(massRanges);
		ScanStatistics scanStatistics = (ScanStatistics)ScanStatistics.Clone();
		if (trimMassRange)
		{
			scanStatistics.LowMass = list[0].Low;
			scanStatistics.HighMass = list[list.Count - 1].High;
		}
		CentroidStream centroidStream = CentroidScan.Slice(list);
		SegmentedScan segmentedScan = SegmentedScan.Slice(list, CommonData.IsProfileScan(ScanStatistics.PacketType), HasCentroidStream && expandProfiles, centroidStream);
		return new Scan(_massToFrequencyConverter, _rawFileNoise)
		{
			CentroidScan = centroidStream,
			IsUserTolerance = IsUserTolerance,
			MassResolution = MassResolution,
			PreferCentroids = PreferCentroids,
			ScanStatistics = scanStatistics,
			ScanType = ScanType,
			SegmentedScan = segmentedScan,
			ToleranceUnit = ToleranceUnit
		};
	}

	private static List<IRangeAccess> Compact(IRangeAccess[] massRanges)
	{
		List<IRangeAccess> list = new List<IRangeAccess>(massRanges.Length);
		foreach (IRangeAccess rangeAccess in massRanges)
		{
			if (list.Count == 0)
			{
				list.Add(rangeAccess);
				continue;
			}
			IRangeAccess rangeAccess2 = list[list.Count - 1];
			if (rangeAccess.Low <= rangeAccess2.High)
			{
				list[list.Count - 1] = RangeFactory.Create(rangeAccess2.Low, Math.Max(rangeAccess2.High, rangeAccess.High));
			}
			else
			{
				list.Add(rangeAccess);
			}
		}
		return list;
	}

	/// <summary>
	/// Order mass ranges by lowest mass.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	private static int OrderByLowest(IRangeAccess x, IRangeAccess y)
	{
		if (x.Low < y.Low)
		{
			return -1;
		}
		if (x.Low > y.Low)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>
	/// generate frequency table for this scan.
	/// This method only applied to "FT" format scans
	/// which have mass to frequency calibration data.
	/// When a scan in constructed from processing algorithms, such 
	/// as averaging, a frequency to mass converter is used to
	/// create this scan. This same converter can be used to create
	/// a frequency table, which would be needed when writing averaged (or subtracted) data to a raw file.
	/// </summary>
	/// <returns>
	/// The frequency table.
	/// </returns>
	public double[] GenerateFrequencyTable()
	{
		if (_massToFrequencyConverter == null)
		{
			return Array.Empty<double>();
		}
		double[] positions = SegmentedScan.Positions;
		double[] array = new double[positions.Length];
		for (int i = 0; i < positions.Length; i++)
		{
			double num = _massToFrequencyConverter.ConvertMassToFrequency(positions[i]);
			array[i] = num;
		}
		return array;
	}

	/// <summary>
	/// Generates a "noise and baseline table".
	/// This table is only relevant to FT format data.
	/// For other data, an empty list is returned.
	/// This table is intended for use when exporting processed (averaged, subtracted) scans to a raw file.
	/// If this scan is the result of a calculation such as "average of subtract" it may be constructed using
	/// an overload which includes a noise and baseline table.
	/// If so: that tale is returned.
	/// Otherwise, a table is generated by extracting data from the scan.
	/// </summary>
	/// <returns>
	/// The nose and baseline data
	/// </returns>
	public NoiseAndBaseline[] GenerateNoiseTable()
	{
		if (_rawFileNoise != null)
		{
			return _rawFileNoise;
		}
		NoiseAndBaseline[] array = new NoiseAndBaseline[0];
		if (HasCentroidStream)
		{
			double[] masses = CentroidScan.Masses;
			double[] noises = CentroidScan.Noises;
			double[] baselines = CentroidScan.Baselines;
			int num = masses.Length;
			if (baselines.Length == num && noises.Length == num)
			{
				array = new NoiseAndBaseline[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = new NoiseAndBaseline
					{
						Baseline = (float)baselines[i],
						Noise = (float)noises[i],
						Mass = (float)masses[i]
					};
				}
			}
		}
		return array;
	}

	/// <summary>
	/// The add centroids.
	/// </summary>
	/// <param name="currentScan">
	///     The current scan.
	/// </param>
	/// <param name="inputScan">
	///     The input scan.
	/// </param>
	private static void AddCentroids(Scan currentScan, Scan inputScan)
	{
		CentroidStream centroidScan = currentScan.CentroidScan;
		CentroidStream centroidScan2 = inputScan.CentroidScan;
		ValidateCentroidScan(centroidScan);
		ValidateCentroidScan(centroidScan2);
		int packetType = currentScan.ScanStatistics.PacketType;
		int packetType2 = inputScan.ScanStatistics.PacketType;
		CentroidStream centroidStream = CreateCentroidStream(centroidScan.Length + centroidScan2.Length, centroidScan.ScanNumber);
		long num = centroidScan.Length;
		long num2 = centroidScan2.Length;
		int num3 = 0;
		int num4 = 0;
		bool usePeakWidthAsMassFuzz = CommonData.IsProfileScan(packetType) && CommonData.IsProfileScan(packetType2);
		int outputIndex = 0;
		while (num > 0 || num2 > 0)
		{
			double resolutionFactor = 1.0;
			if (num > 0 && num2 > 0)
			{
				if (centroidScan2.Resolutions != null && centroidScan2.Resolutions[num4] > 1.0)
				{
					resolutionFactor = 1.0 / centroidScan2.Resolutions[num4];
				}
				double[] masses = centroidScan.Masses;
				double[] masses2 = centroidScan2.Masses;
				if (masses != null && masses2 != null)
				{
					double num5 = ((!(masses[num3] < masses2[num4])) ? ConvertCentroidStreamTolerance(currentScan, centroidScan2, num4, usePeakWidthAsMassFuzz, resolutionFactor) : ConvertCentroidStreamTolerance(currentScan, centroidScan, num3, usePeakWidthAsMassFuzz, resolutionFactor));
					double num6 = masses[num3] - masses2[num4];
					if (num6 < 0.0 - num5)
					{
						PutLabel(ref outputIndex, centroidStream, centroidScan, num3, num5);
						num--;
						num3++;
					}
					else if (num6 > num5)
					{
						PutLabel(ref outputIndex, centroidStream, centroidScan2, num4, num5);
						num2--;
						num4++;
					}
					else
					{
						PutLabel(ref outputIndex, centroidStream, centroidScan, num3, num5);
						centroidStream.Masses[outputIndex] = centroidStream.Masses[outputIndex] * centroidStream.Intensities[outputIndex] + centroidScan2.Masses[num4] * centroidScan2.Intensities[num4] / (centroidStream.Intensities[outputIndex] + centroidScan2.Intensities[num4]);
						centroidStream.Intensities[outputIndex] += centroidScan2.Intensities[num4];
						centroidStream.Flags[outputIndex] |= centroidScan2.Flags[num4];
						if (centroidStream.Charges[outputIndex] != centroidScan2.Charges[num4])
						{
							centroidStream.Charges[outputIndex] = 0.0;
						}
						num--;
						num2--;
						num4++;
						num3++;
					}
				}
			}
			else if (num2 > 0)
			{
				if (centroidScan2.Resolutions[num4] > 1.0)
				{
					resolutionFactor = 1.0 / centroidScan2.Resolutions[num4];
				}
				double num5 = ConvertCentroidStreamTolerance(currentScan, centroidScan2, num4, usePeakWidthAsMassFuzz, resolutionFactor);
				PutLabel(ref outputIndex, centroidStream, centroidScan2, num4, num5);
				num2--;
				num4++;
			}
			else if (num > 0)
			{
				double num5 = ConvertCentroidStreamTolerance(currentScan, centroidScan, num3, usePeakWidthAsMassFuzz, resolutionFactor);
				PutLabel(ref outputIndex, centroidStream, centroidScan, num3, num5);
				num3++;
				num--;
			}
			if (centroidStream.Intensities[outputIndex] > 0.0)
			{
				outputIndex++;
			}
		}
		_ = 0;
		currentScan.CentroidScan = centroidStream;
	}

	/// <summary>
	/// Addition of FTMS centroid scans (called by operator +=)
	/// </summary>
	/// <param name="currentScan">
	/// current scan object(this).
	/// </param>
	/// <param name="rightScan">
	/// scan to right of + operator (that)
	/// </param>
	/// <returns>
	/// The sum of the two scans
	/// </returns>
	private static Scan AddFtCentroidData(Scan currentScan, Scan rightScan)
	{
		CentroidStream centroidScan = currentScan.CentroidScan;
		CentroidStream centroidScan2 = rightScan.CentroidScan;
		CentroidStream centroidStream = CreateCentroidStream(centroidScan.Length + centroidScan2.Length, centroidScan.ScanNumber);
		double num = 0.0;
		double baseMass = 0.0;
		SegmentedScan segmentedScan = currentScan.SegmentedScan;
		SegmentedScan segmentedScan2 = rightScan.SegmentedScan;
		bool isCentroidScan = currentScan.ScanStatistics.IsCentroidScan;
		SegmentedScan segmentedScan3 = null;
		if (!isCentroidScan)
		{
			currentScan._segmentSimilar[0] = true;
			AddSegments(identicalFlag: false, currentScan, rightScan, processSegments: false);
		}
		else
		{
			segmentedScan3 = CreateSegmentedScan(segmentedScan.PositionCount + segmentedScan2.PositionCount);
		}
		int length = centroidScan.Length;
		int length2 = centroidScan2.Length;
		double toleranceFactor = 0.0;
		toleranceFactor = GetToleranceFactor(currentScan, rightScan, centroidScan, centroidScan2, length2, toleranceFactor, isOrbitrapData: false, length);
		int outputIndex = 0;
		int num2 = centroidScan.Length;
		int num3 = centroidScan2.Length;
		int num4 = 0;
		int num5 = 0;
		while (num2 > 0 || num3 > 0)
		{
			if (num2 > 0 && num3 > 0)
			{
				double num6 = CalculateMassFuzz(currentScan, centroidScan, centroidScan2, toleranceFactor, isOrbitrapData: false, num4, num5, isCentroidScan);
				double num7 = centroidScan.Masses[num4] - centroidScan2.Masses[num5];
				if (num7 < 0.0 - num6)
				{
					PutLabel(ref outputIndex, centroidStream, centroidScan, num4, num6);
					num2--;
					num4++;
				}
				else if (num7 > num6)
				{
					PutLabel(ref outputIndex, centroidStream, centroidScan2, num5, num6);
					num3--;
					num5++;
				}
				else
				{
					PutLabel(ref outputIndex, centroidStream, centroidScan, num4, num6);
					centroidStream.Masses[outputIndex] = (centroidStream.Masses[outputIndex] * centroidStream.Intensities[outputIndex] + centroidScan2.Masses[num5] * centroidScan2.Intensities[num5]) / (centroidStream.Intensities[outputIndex] + centroidScan2.Intensities[num5]);
					centroidStream.Intensities[outputIndex] += centroidScan2.Intensities[num5];
					centroidStream.Flags[outputIndex] |= centroidScan2.Flags[num5];
					if (centroidStream.Charges[outputIndex] != centroidScan2.Charges[num5])
					{
						centroidStream.Charges[outputIndex] = 0.0;
					}
					num2--;
					num4++;
					num3--;
					num5++;
				}
			}
			else if (num3 > 0)
			{
				double num6 = CalculateMassFuzzForCentroidPeak(centroidScan2.Masses[num5], toleranceFactor, currentScan._massFuzz, isOrbitraData: false, currentScan._toleranceUnit);
				PutLabel(ref outputIndex, centroidStream, centroidScan2, num5, num6);
				num3--;
				num5++;
			}
			else if (num2 > 0)
			{
				double num6 = CalculateMassFuzzForCentroidPeak(centroidScan.Masses[num4], toleranceFactor, currentScan._massFuzz, isOrbitraData: false, currentScan._toleranceUnit);
				PutLabel(ref outputIndex, centroidStream, centroidScan, num4, num6);
				num2--;
				num4++;
			}
			if (centroidStream.Intensities[outputIndex] > 0.0)
			{
				outputIndex++;
			}
		}
		int resultCount = 0;
		if (isCentroidScan)
		{
			int num8 = segmentedScan.PositionCount;
			int num9 = segmentedScan2.PositionCount;
			double[] positions = segmentedScan.Positions;
			double[] positions2 = segmentedScan2.Positions;
			int num10 = 0;
			int num11 = 0;
			while (num8 > 0 || num9 > 0)
			{
				if (num8 > 0 && num9 > 0)
				{
					double num12 = CalculateMassFuzzForCentroidPeak(positions[num10], toleranceFactor, currentScan._massFuzz, isOrbitraData: false, currentScan._toleranceUnit);
					double num13 = positions[num10] - positions2[num11];
					if (num13 < 0.0 - num12)
					{
						PutPeak(ref resultCount, segmentedScan3, segmentedScan, num10, num12);
						num8--;
						num10++;
					}
					else if (num13 > num12)
					{
						PutPeak(ref resultCount, segmentedScan3, segmentedScan2, num11, num12);
						num9--;
						num11++;
					}
					else
					{
						PutPeak(ref resultCount, segmentedScan3, segmentedScan, num10, num12);
						segmentedScan3.Positions[resultCount] = (segmentedScan3.Positions[resultCount] * segmentedScan3.Intensities[resultCount] + segmentedScan2.Positions[num11] * segmentedScan2.Intensities[num11]) / (segmentedScan3.Intensities[resultCount] + segmentedScan2.Intensities[num11]);
						segmentedScan3.Intensities[resultCount] += segmentedScan2.Intensities[num11];
						num8--;
						num10++;
						num9--;
						num11++;
					}
				}
				else if (num9 > 0)
				{
					double num12 = CalculateMassFuzzForCentroidPeak(positions2[num11], toleranceFactor, currentScan._massFuzz, isOrbitraData: false, currentScan._toleranceUnit);
					PutPeak(ref resultCount, segmentedScan3, segmentedScan2, num11, num12);
					num9--;
					num11++;
				}
				else if (num8 > 0)
				{
					double num12 = CalculateMassFuzzForCentroidPeak(positions[num10], toleranceFactor, currentScan._massFuzz, isOrbitraData: false, currentScan._toleranceUnit);
					PutPeak(ref resultCount, segmentedScan3, segmentedScan, num10, num12);
					num8--;
					num10++;
				}
				if (segmentedScan3.Intensities[resultCount] > num)
				{
					num = segmentedScan3.Intensities[resultCount];
					baseMass = segmentedScan3.Positions[resultCount];
				}
				if (segmentedScan3.Intensities[resultCount] > 0.0)
				{
					resultCount++;
				}
			}
		}
		if (resultCount > 0 && isCentroidScan)
		{
			FillSegmentRange(segmentedScan, segmentedScan2, segmentedScan3);
		}
		if (isCentroidScan)
		{
			segmentedScan3.SegmentCount = 1;
			segmentedScan3.SegmentSizes = new int[1] { resultCount };
			segmentedScan3.Ranges = new Range[1];
			Range range = currentScan.SegmentedScan.Ranges[0];
			segmentedScan3.Ranges[0] = new Range(range.Low, range.High);
			RemoveZeros(segmentedScan3, resultCount);
		}
		RemoveZeros(centroidStream, outputIndex);
		currentScan.CentroidScan = centroidStream;
		if (isCentroidScan)
		{
			currentScan.SegmentedScan = segmentedScan3;
		}
		if (isCentroidScan)
		{
			ScanStatistics sumIndex = GetSumIndex(currentScan, rightScan, resultCount, num, baseMass);
			currentScan.ScanStatistics = sumIndex;
		}
		return currentScan;
	}

	/// <summary>
	/// The add scan stats.
	/// </summary>
	/// <param name="leftScan">
	/// The left scan.
	/// </param>
	/// <param name="rightScan">
	/// The right scan.
	/// </param>
	/// <param name="scanStats">
	/// The scan stats.
	/// </param>
	/// <param name="baseHeight">
	/// The base height.
	/// </param>
	/// <param name="baseMass">
	/// The base mass.
	/// </param>
	private static void AddScanStats(Scan leftScan, Scan rightScan, ScanStatistics scanStats, double baseHeight, double baseMass)
	{
		if (leftScan.ScanStatistics != null && rightScan.ScanStatistics != null)
		{
			scanStats.HighMass = Math.Max(leftScan.ScanStatistics.HighMass, rightScan.ScanStatistics.HighMass);
			scanStats.LowMass = Math.Min(leftScan.ScanStatistics.LowMass, rightScan.ScanStatistics.LowMass);
			scanStats.PacketCount = leftScan.SegmentedScan.PositionCount;
			scanStats.TIC += rightScan.ScanStatistics.TIC;
			scanStats.BasePeakIntensity = Math.Max(scanStats.BasePeakIntensity, baseHeight);
			scanStats.BasePeakMass = Math.Max(scanStats.BasePeakMass, baseMass);
		}
	}

	/// <summary>
	/// The add segment.
	/// </summary>
	/// <param name="leftScan">
	/// The left scan.
	/// </param>
	/// <param name="leftSegment">
	/// The left segment.
	/// </param>
	/// <param name="rightSegment">
	/// The right segment.
	/// </param>
	/// <param name="iterator">
	/// The segment number.
	/// </param>
	/// <param name="resultScanSegments">
	/// The result scan segments.
	/// </param>
	/// <param name="baseHeight">
	/// The base height.
	/// </param>
	/// <param name="baseMass">
	/// The base mass.
	/// </param>
	/// <param name="processSegments">
	/// The process segments.
	/// </param>
	private static void AddSegment(Scan leftScan, SegmentedScan leftSegment, SegmentedScan rightSegment, int iterator, SegmentedScan[] resultScanSegments, ref double baseHeight, ref double baseMass, bool processSegments)
	{
		long num = leftSegment.SegmentSizes[iterator];
		long num2 = rightSegment.SegmentSizes[iterator];
		long num3 = num + num2;
		SegmentedScan segmentedScan = (resultScanSegments[iterator] = CreateSegmentedScan((int)num3));
		FindSegmentStartPackets(leftSegment, rightSegment, iterator, out var leftScanCount, out var rightScanCount);
		int resultCount = 0;
		while (num > 0 || num2 > 0)
		{
			double num4 = ((num <= 0) ? leftScan.ConvertTolerance(rightScanCount, rightSegment.Positions) : leftScan.ConvertTolerance(leftScanCount, leftSegment.Positions));
			if (num > 0 && num2 > 0)
			{
				double num5 = leftSegment.Positions[leftScanCount] - rightSegment.Positions[rightScanCount];
				if (num5 < 0.0 - num4)
				{
					PutPeak(ref resultCount, segmentedScan, leftSegment, leftScanCount, num4, processSegments);
					leftScanCount++;
					num--;
				}
				else if (num5 > num4)
				{
					PutPeak(ref resultCount, segmentedScan, rightSegment, rightScanCount, num4, processSegments);
					rightScanCount++;
					num2--;
				}
				else
				{
					PutPeak(ref resultCount, segmentedScan, leftSegment, leftScanCount, num4, processSegments);
					segmentedScan.Positions[resultCount] = (segmentedScan.Positions[resultCount] * segmentedScan.Intensities[resultCount] + rightSegment.Positions[rightScanCount] * rightSegment.Intensities[rightScanCount]) / (segmentedScan.Intensities[resultCount] + rightSegment.Intensities[rightScanCount]);
					segmentedScan.Intensities[resultCount] += rightSegment.Intensities[rightScanCount];
					leftScanCount++;
					rightScanCount++;
					num--;
					num2--;
				}
			}
			else if (num2 > 0)
			{
				PutPeak(ref resultCount, segmentedScan, rightSegment, rightScanCount, num4, processSegments);
				rightScanCount++;
				num2--;
			}
			else if (num > 0)
			{
				PutPeak(ref resultCount, segmentedScan, leftSegment, leftScanCount, num4);
				leftScanCount++;
				num--;
			}
			if (segmentedScan.Intensities[resultCount] > baseHeight)
			{
				baseHeight = segmentedScan.Intensities[resultCount];
				baseMass = segmentedScan.Positions[resultCount];
			}
			if (segmentedScan.Intensities[resultCount] > 0.0)
			{
				resultCount++;
				continue;
			}
			segmentedScan.Intensities[resultCount] = 0.0;
			segmentedScan.Positions[resultCount] = 0.0;
		}
		double[] array = SelectNonZeroValues(segmentedScan.Positions);
		double[] array2 = SelectNonZeroValues(segmentedScan.Intensities);
		if (array.Length == array2.Length)
		{
			segmentedScan = CreateSegmentedScan(array.Length);
			segmentedScan.Intensities = array2;
			segmentedScan.Positions = array;
			segmentedScan.Ranges = new Range[1] { rightSegment.Ranges[iterator] };
			resultScanSegments[iterator] = segmentedScan;
		}
	}

	/// <summary>
	/// Add segments.
	/// </summary>
	/// <param name="identicalFlag">
	/// The _identical flag.
	/// </param>
	/// <param name="leftScan">
	/// The left scan.
	/// </param>
	/// <param name="rightScan">
	/// The right scan.
	/// </param>
	/// <param name="processSegments">
	/// The process segments.
	/// </param>
	/// <returns>
	/// The sum of the two scans, which is also in "leftScan".
	/// </returns>
	private static Scan AddSegments(bool identicalFlag, Scan leftScan, Scan rightScan, bool processSegments = true)
	{
		double baseHeight = 0.0;
		double baseMass = 0.0;
		SegmentedScan segmentedScan = leftScan.SegmentedScan;
		SegmentedScan segmentedScan2 = rightScan.SegmentedScan;
		ScanStatistics scanStatistics = leftScan.ScanStatistics;
		ValidateSegmentedScan(segmentedScan);
		ValidateSegmentedScan(segmentedScan2);
		int num = Math.Min(segmentedScan.SegmentCount, segmentedScan2.SegmentCount);
		if ((identicalFlag || leftScan._segmentSimilar[0]) && CommonData.LOWord(scanStatistics.PacketType) != 24)
		{
			ProfileDataForSimilarSegments(identicalFlag, leftScan, rightScan);
		}
		else
		{
			if (!leftScan._userTolerance)
			{
				leftScan = ToCentroid(leftScan);
				if (rightScan.ScanStatistics != null && CommonData.IsProfileScan(rightScan.ScanStatistics.PacketType))
				{
					rightScan = ToCentroid(rightScan);
				}
			}
			SegmentedScan[] array = new SegmentedScan[num];
			for (int i = 0; i < num; i++)
			{
				AddSegment(leftScan, segmentedScan, segmentedScan2, i, array, ref baseHeight, ref baseMass, processSegments);
			}
			ReplaceSegmentData(leftScan, array);
			AddScanStats(leftScan, rightScan, scanStatistics, baseHeight, baseMass);
			leftScan.ScanStatistics = scanStatistics;
		}
		return leftScan;
	}

	/// <summary>
	/// The append peak.
	/// </summary>
	/// <param name="results">
	/// The results.
	/// </param>
	/// <param name="newPeak">
	/// The new peak.
	/// </param>
	/// <param name="massFuzz">
	/// The mass fuzz (tolerance).
	/// </param>
	private static void AppendPeak(List<FragmentResult> results, FragmentResult newPeak, double massFuzz)
	{
		if (results.Count > 0 && newPeak.Mass - results[results.Count - 1].Mass < massFuzz)
		{
			CombinePeaks(results[results.Count - 1], newPeak);
		}
		else
		{
			results.Add(newPeak);
		}
	}

	/// <summary>
	/// Determine if this is a basic profile type.
	/// </summary>
	/// <param name="packetType">
	/// The packet type.
	/// </param>
	/// <returns>
	/// True if it is basic profile type.
	/// </returns>
	private static bool BasicProfileType(int packetType)
	{
		if (packetType != 0 && packetType != 14 && packetType != 16)
		{
			return packetType == 7;
		}
		return true;
	}

	/// <summary>
	/// Attempt to find the tolerance factor, from the base peak of a scan
	/// return "true" on fail.
	/// </summary>
	/// <param name="centroidCount">
	/// The centroid count.
	/// </param>
	/// <param name="centroidStream">
	/// The centroid stream.
	/// </param>
	/// <param name="baseMass">
	/// The base mass.
	/// </param>
	/// <param name="toleranceFactor">
	/// The tolerance factor.
	/// </param>
	/// <param name="isOrbitrapData">
	/// The is orbitrap data.
	/// </param>
	/// <returns>
	/// The calculated tolerance factor.
	/// </returns>
	private static bool CalculateToleranceFactor(int centroidCount, CentroidStream centroidStream, double baseMass, ref double toleranceFactor, bool isOrbitrapData)
	{
		if (centroidCount > 0 && centroidStream.Resolutions != null && centroidStream.Resolutions[0] > 0.0)
		{
			int position = SearchBasePeakIndex(baseMass, centroidStream, centroidCount);
			toleranceFactor = CalculateToleranceFactorForLabels(centroidStream, position, isOrbitrapData);
			return false;
		}
		return true;
	}

	/// <summary>
	/// Returns the default tolerance factor.
	/// </summary>
	/// <param name="isOrbitraData">
	/// flag to Check if we have LTQ-FT or Orbitrap data
	/// </param>
	/// <returns>
	/// The calculate default tolerance factor.
	/// </returns>
	private static double CalculateDefaultToleranceFactor(bool isOrbitraData)
	{
		if (isOrbitraData)
		{
			return 1E-07;
		}
		return 2E-06;
	}

	/// <summary>
	/// The calculate mass fuzz.
	/// </summary>
	/// <param name="currentScan">
	/// The current scan.
	/// </param>
	/// <param name="leftCentroid">
	/// The left centroid.
	/// </param>
	/// <param name="rightCentroid">
	/// The right centroid.
	/// </param>
	/// <param name="toleranceFactor">
	/// The tolerance factor.
	/// </param>
	/// <param name="isOrbitrapData">
	/// The is orbitrap data.
	/// </param>
	/// <param name="leftIterator">
	/// The left iterator.
	/// </param>
	/// <param name="rightIterator">
	/// The right iterator.
	/// </param>
	/// <param name="processSegments">
	/// The process segments.
	/// </param>
	/// <returns>
	/// The calculated mass fuzz.
	/// </returns>
	private static double CalculateMassFuzz(Scan currentScan, CentroidStream leftCentroid, CentroidStream rightCentroid, double toleranceFactor, bool isOrbitrapData, int leftIterator, int rightIterator, bool processSegments)
	{
		if (leftCentroid.Masses[leftIterator] < rightCentroid.Masses[rightIterator])
		{
			if (processSegments)
			{
				return CalculateMassFuzzForCentroidPeak(leftCentroid.Masses[leftIterator], toleranceFactor, currentScan._massFuzz, isOrbitrapData, currentScan._toleranceUnit);
			}
			return CalculateMassFuzzForProfilePeak(leftCentroid.Masses[leftIterator], toleranceFactor, currentScan._massFuzz, isOrbitrapData, currentScan._toleranceUnit);
		}
		if (processSegments)
		{
			return CalculateMassFuzzForCentroidPeak(rightCentroid.Masses[rightIterator], toleranceFactor, currentScan._massFuzz, isOrbitrapData, currentScan._toleranceUnit);
		}
		return CalculateMassFuzzForProfilePeak(rightCentroid.Masses[rightIterator], toleranceFactor, currentScan._massFuzz, isOrbitrapData, currentScan._toleranceUnit);
	}

	/// <summary>
	/// Calculates the Mass tolerance for centroid peak.
	/// </summary>
	/// <param name="mass">
	/// current mass tolerance value.
	/// </param>
	/// <param name="toleranceFactor">
	/// tolerance factor
	/// </param>
	/// <param name="previouMassFuzz">
	/// previous mass tolerance value
	/// </param>
	/// <param name="isOrbitraData">
	/// flag to check if we have LTQ-FT or Orbitrap data.
	/// </param>
	/// <param name="tolUnit">
	/// tolerance unit
	/// </param>
	/// <returns>
	/// The calculate mass fuzz for centroid peak.
	/// </returns>
	private static double CalculateMassFuzzForCentroidPeak(double mass, double toleranceFactor, double previouMassFuzz, bool isOrbitraData, ToleranceMode tolUnit)
	{
		switch (tolUnit)
		{
		case ToleranceMode.Ppm:
			return mass * 1E-06 * previouMassFuzz;
		case ToleranceMode.Mmu:
			return 0.001 * previouMassFuzz;
		default:
			if (isOrbitraData)
			{
				return mass * Math.Sqrt(mass) * toleranceFactor;
			}
			return mass * mass * toleranceFactor;
		}
	}

	/// <summary>
	/// Calculates the Mass tolerance for profile peak.
	/// </summary>
	/// <param name="mass">
	/// current mass tolerance value.
	/// </param>
	/// <param name="toleranceFactor">
	/// tolerance factor
	/// </param>
	/// <param name="defaultMassFuzz">
	/// previous mass tolerance value
	/// </param>
	/// <param name="isOrbitraData">
	/// flag to check if we have LTQ-FT or Orbitrap data.
	/// </param>
	/// <param name="tolUnit">
	/// tolerance unit
	/// </param>
	/// <returns>
	/// The calculated mass fuzz for profile peak.
	/// </returns>
	private static double CalculateMassFuzzForProfilePeak(double mass, double toleranceFactor, double defaultMassFuzz, bool isOrbitraData, ToleranceMode tolUnit)
	{
		switch (tolUnit)
		{
		case ToleranceMode.Ppm:
		{
			double num = mass * 1E-06 * defaultMassFuzz;
			double num3 = mass * mass * toleranceFactor;
			return (num3 > num) ? num3 : num;
		}
		case ToleranceMode.Mmu:
		{
			double num = 0.001 * defaultMassFuzz;
			double num2 = mass * mass * toleranceFactor;
			return (num2 > num) ? num2 : num;
		}
		default:
			if (isOrbitraData)
			{
				return mass * Math.Sqrt(mass) * toleranceFactor;
			}
			return mass * mass * toleranceFactor;
		}
	}

	/// <summary>
	/// Calculates the tolerance factor
	/// </summary>
	/// <param name="stream">
	/// CentroidStream object
	/// </param>
	/// <param name="position">
	/// calculates the tolerance factor at specified index.
	/// </param>
	/// <param name="isOrbitraData">
	/// flag to Check if we have LTQ-FT or Orbitrap data
	/// </param>
	/// <returns>
	/// The calculate tolerance factor for labels.
	/// </returns>
	private static double CalculateToleranceFactorForLabels(CentroidStream stream, int position, bool isOrbitraData)
	{
		if (isOrbitraData)
		{
			return 0.5 / (stream.Resolutions[position] * Math.Sqrt(stream.Masses[position]));
		}
		return 0.5 / (stream.Resolutions[position] * stream.Masses[position]);
	}

	/// <summary>
	/// Calculate total scan size.
	/// </summary>
	/// <param name="segments">
	/// The segments.
	/// </param>
	/// <returns>
	/// The total scan size.
	/// </returns>
	private static int CalculateTotalScanSize(IEnumerable<SegmentedScan> segments)
	{
		int num = 0;
		foreach (SegmentedScan segment in segments)
		{
			num += segment.PositionCount;
		}
		return num;
	}

	/// <summary>
	/// Check for identical.
	/// </summary>
	/// <param name="identicalFlag">
	/// The identical flag.
	/// </param>
	/// <param name="currentScanObject">
	/// The current scan object.
	/// </param>
	/// <param name="inputScanObject">
	/// The input scan object.
	/// </param>
	/// <returns>
	/// The number of segments found
	/// </returns>
	private static int CheckForIdentical(out bool identicalFlag, Scan currentScanObject, Scan inputScanObject)
	{
		SegmentedScan segmentedScan = currentScanObject.SegmentedScan;
		SegmentedScan segmentedScan2 = inputScanObject.SegmentedScan;
		Range[] ranges = segmentedScan.Ranges;
		Range[] ranges2 = segmentedScan2.Ranges;
		int[] segmentSizes = segmentedScan.SegmentSizes;
		int[] segmentSizes2 = segmentedScan2.SegmentSizes;
		int segmentCount = segmentedScan.SegmentCount;
		int i;
		for (i = 0; i < segmentCount && ranges[i].Low == ranges2[i].Low && ranges[i].High == ranges2[i].High && segmentSizes[i] == segmentSizes2[i]; i++)
		{
		}
		identicalFlag = i == segmentCount;
		return i;
	}

	/// <summary>
	/// The combine peaks.
	/// </summary>
	/// <param name="left">
	/// The left.
	/// </param>
	/// <param name="right">
	/// The right.
	/// </param>
	private static void CombinePeaks(FragmentResult left, FragmentResult right)
	{
		left.Mass = (left.Mass * left.Intensity + right.Mass * right.Intensity) / (left.Intensity + right.Intensity);
		if ((right.Options & PeakOptions.Saturated) == PeakOptions.Saturated)
		{
			left.Options |= PeakOptions.Saturated;
		}
		left.Options |= PeakOptions.Merged;
		left.Intensity += right.Intensity;
	}

	/// <summary>
	/// The convert centroid stream tolerance.
	/// </summary>
	/// <param name="currentScan">
	/// The current scan.
	/// </param>
	/// <param name="centroidStream">
	/// The centroid stream.
	/// </param>
	/// <param name="counter">
	/// The counter.
	/// </param>
	/// <param name="usePeakWidthAsMassFuzz">
	/// The use peak width as mass fuzz.
	/// </param>
	/// <param name="resolutionFactor">
	/// The resolution factor.
	/// </param>
	/// <returns>
	/// The converted centroid stream tolerance.
	/// </returns>
	private static double ConvertCentroidStreamTolerance(Scan currentScan, CentroidStream centroidStream, int counter, bool usePeakWidthAsMassFuzz, double resolutionFactor)
	{
		double num;
		if (currentScan._toleranceUnit == ToleranceMode.Ppm)
		{
			num = centroidStream.Masses[counter] * 1E-06 * currentScan._massFuzz;
			if (usePeakWidthAsMassFuzz)
			{
				double num2 = 0.5 * centroidStream.Masses[counter] * resolutionFactor;
				num = ((num2 < currentScan._massFuzz && num2 > num) ? num2 : num);
			}
		}
		else if (currentScan._toleranceUnit == ToleranceMode.Mmu)
		{
			num = 0.001 * currentScan._massFuzz;
			if (usePeakWidthAsMassFuzz)
			{
				double num3 = 0.5 * centroidStream.Masses[counter] * resolutionFactor;
				num = ((num3 < currentScan._massFuzz && num3 > num) ? num3 : num);
			}
		}
		else
		{
			double num4 = 0.05 * centroidStream.Masses[counter] * resolutionFactor;
			num = ((num4 < currentScan._massFuzz) ? num4 : currentScan._massFuzz);
		}
		return num;
	}

	/// <summary>
	/// Create an empty centroid stream.
	/// </summary>
	/// <param name="size">
	/// The size.
	/// </param>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// A centroid stream which has "size" data packets.
	/// </returns>
	private static CentroidStream CreateCentroidStream(int size, int scanNumber)
	{
		return new CentroidStream
		{
			Baselines = new double[size],
			Charges = new double[size],
			Coefficients = new double[0],
			CoefficientsCount = 0,
			Intensities = new double[size],
			Masses = new double[size],
			Flags = new PeakOptions[size],
			Length = size,
			Noises = new double[size],
			Resolutions = new double[size],
			ScanNumber = scanNumber
		};
	}

	/// <summary>
	/// Create a segmented scan.
	/// </summary>
	/// <param name="size">
	/// The number of peaks in the scan.
	/// </param>
	/// <returns>
	/// The created scan
	/// </returns>
	private static SegmentedScan CreateSegmentedScan(int size)
	{
		return new SegmentedScan
		{
			Intensities = new double[size],
			Flags = new PeakOptions[size],
			Positions = new double[size]
		};
	}

	/// <summary>
	/// Assuming that the methods ReplaceScanDataArrays and UpdateSegmentSizes
	/// have already been called, the result scan
	/// is formatted ready to accept new data
	/// Merge the data from the results of adding each segment
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="segments">
	/// The segments.
	/// </param>
	private static void FillScanData(SegmentedScan scan, SegmentedScan[] segments)
	{
		int num = segments.Length;
		for (int i = 0; i < num; i++)
		{
			int destinationIndex = scan.IndexOfSegmentStart(i);
			SegmentedScan segmentedScan = segments[i];
			Array.Copy(segmentedScan.Positions, 0, scan.Positions, destinationIndex, segmentedScan.PositionCount);
			Array.Copy(segmentedScan.Intensities, 0, scan.Intensities, destinationIndex, segmentedScan.PositionCount);
			Array.Copy(segmentedScan.Flags, 0, scan.Flags, destinationIndex, segmentedScan.PositionCount);
		}
	}

	/// <summary>
	/// The fill segment range.
	/// </summary>
	/// <param name="leftSegment">
	/// The left segment.
	/// </param>
	/// <param name="rightSegment">
	/// The right segment.
	/// </param>
	/// <param name="outputSegmentedScan">
	/// The output segmented scan.
	/// </param>
	private static void FillSegmentRange(SegmentedScan leftSegment, SegmentedScan rightSegment, SegmentedScan outputSegmentedScan)
	{
		double low = Math.Min(leftSegment.Ranges[0].Low, rightSegment.Ranges[0].Low);
		double high = Math.Max(leftSegment.Ranges[0].High, rightSegment.Ranges[0].High);
		outputSegmentedScan.Ranges = new Range[1];
		outputSegmentedScan.Ranges[0] = new Range(low, high);
	}

	/// <summary>
	/// find segment start packet index number.
	/// </summary>
	/// <param name="leftSegment">
	/// The left segment.
	/// </param>
	/// <param name="rightSegment">
	/// The right segment.
	/// </param>
	/// <param name="iterator">
	/// The iterator.
	/// </param>
	/// <param name="leftScanCount">
	/// The count of peaks before this segment in the left scan.
	/// </param>
	/// <param name="rightScanCount">
	/// The count of peaks before this segment in the right scan.
	/// </param>
	private static void FindSegmentStartPackets(SegmentedScan leftSegment, SegmentedScan rightSegment, int iterator, out int leftScanCount, out int rightScanCount)
	{
		leftScanCount = 0;
		rightScanCount = 0;
		if (iterator > 0)
		{
			for (int i = 0; i < iterator; i++)
			{
				leftScanCount += leftSegment.SegmentSizes[i];
				rightScanCount += rightSegment.SegmentSizes[i];
			}
		}
	}

	/// <summary>
	/// Fix the length of segment tables after a merge
	/// </summary>
	/// <param name="scanSegments">
	/// scan which will contain a merge
	/// </param>
	/// <param name="segments">
	/// segments which are result of addition
	/// </param>
	private static void FixSegmentTables(SegmentedScan scanSegments, SegmentedScan[] segments)
	{
		int num = segments.Length;
		if (scanSegments.Ranges.Length != num)
		{
			Range[] array = scanSegments.Ranges;
			Array.Resize(ref array, num);
			scanSegments.Ranges = array;
		}
		if (scanSegments.SegmentSizes.Length != num)
		{
			int[] array2 = scanSegments.SegmentSizes;
			Array.Resize(ref array2, num);
			scanSegments.SegmentSizes = array2;
		}
		Range[] ranges = scanSegments.Ranges;
		for (int i = 0; i < num; i++)
		{
			Range range = ranges[i];
			Range[] ranges2 = segments[i].Ranges;
			if (ranges2 != null && ranges2.Length >= 1)
			{
				Range range2 = ranges2[0];
				ranges[i] = new Range(Math.Min(range.Low, range2.Low), Math.Max(range.High, range2.High));
			}
		}
	}

	/// <summary>
	/// The get subtracted index.
	/// </summary>
	/// <param name="currentScan">
	/// The current scan.
	/// </param>
	/// <param name="rightScan">
	/// The right scan.
	/// </param>
	/// <param name="outputPeakCount">
	/// The output peak count.
	/// </param>
	/// <param name="baseHeight">
	/// The base height.
	/// </param>
	/// <param name="baseMass">
	/// The base mass.
	/// </param>
	/// <returns>
	/// Index of the subtracted scan
	/// </returns>
	private static ScanStatistics GetSubtractedIndex(Scan currentScan, Scan rightScan, int outputPeakCount, double baseHeight, double baseMass)
	{
		ScanStatistics scanStatistics = new ScanStatistics(currentScan.ScanStatistics);
		if (currentScan.ScanStatistics != null && rightScan.ScanStatistics != null)
		{
			scanStatistics.LowMass = Math.Min(currentScan.ScanStatistics.LowMass, rightScan.ScanStatistics.LowMass);
			scanStatistics.HighMass = Math.Max(currentScan.ScanStatistics.HighMass, rightScan.ScanStatistics.HighMass);
			scanStatistics.PacketCount = outputPeakCount;
			scanStatistics.TIC -= rightScan.ScanStatistics.TIC;
			scanStatistics.BasePeakIntensity = baseHeight;
			scanStatistics.BasePeakMass = baseMass;
		}
		return scanStatistics;
	}

	/// <summary>
	/// get the sum index.
	/// </summary>
	/// <param name="currentScan">
	/// The current scan.
	/// </param>
	/// <param name="rightScan">
	/// The right scan.
	/// </param>
	/// <param name="outputPeakCount">
	/// The output peak count.
	/// </param>
	/// <param name="baseHeight">
	/// The base height.
	/// </param>
	/// <param name="baseMass">
	/// The base mass.
	/// </param>
	/// <returns>
	/// The scan index data for the summed scans
	/// </returns>
	private static ScanStatistics GetSumIndex(Scan currentScan, Scan rightScan, int outputPeakCount, double baseHeight, double baseMass)
	{
		ScanStatistics scanStatistics = new ScanStatistics(currentScan.ScanStatistics);
		if (currentScan.ScanStatistics != null && rightScan.ScanStatistics != null)
		{
			scanStatistics.LowMass = Math.Min(currentScan.ScanStatistics.LowMass, rightScan.ScanStatistics.LowMass);
			scanStatistics.HighMass = Math.Max(currentScan.ScanStatistics.HighMass, rightScan.ScanStatistics.HighMass);
			scanStatistics.PacketCount = outputPeakCount;
			scanStatistics.TIC += rightScan.ScanStatistics.TIC;
			scanStatistics.BasePeakIntensity = baseHeight;
			scanStatistics.BasePeakMass = baseMass;
		}
		return scanStatistics;
	}

	/// <summary>
	/// get the tolerance factor.
	/// </summary>
	/// <param name="currentScan">
	/// The current scan.
	/// </param>
	/// <param name="rightScan">
	/// The right scan.
	/// </param>
	/// <param name="leftCentroid">
	/// The left centroid.
	/// </param>
	/// <param name="rightCentroid">
	/// The right centroid.
	/// </param>
	/// <param name="rightCentroidCount">
	/// The right centroid count.
	/// </param>
	/// <param name="toleranceFactor">
	/// Get the tolerance factor.
	/// </param>
	/// <param name="isOrbitrapData">
	/// The is orbitrap data.
	/// </param>
	/// <param name="leftCentroidCount">
	/// The left centroid count.
	/// </param>
	/// <returns>
	/// The tolerance factor.
	/// </returns>
	private static double GetToleranceFactor(Scan currentScan, Scan rightScan, CentroidStream leftCentroid, CentroidStream rightCentroid, int rightCentroidCount, double toleranceFactor, bool isOrbitrapData, int leftCentroidCount)
	{
		if (CalculateToleranceFactor(rightCentroidCount, rightCentroid, rightScan.CentroidScan.BasePeakMass, ref toleranceFactor, isOrbitrapData) && CalculateToleranceFactor(leftCentroidCount, leftCentroid, currentScan.CentroidScan.BasePeakMass, ref toleranceFactor, isOrbitrapData))
		{
			toleranceFactor = CalculateDefaultToleranceFactor(isOrbitrapData);
		}
		return toleranceFactor;
	}

	/// <summary>
	/// Test if is known profile type.
	/// </summary>
	/// <param name="packetType">
	/// The packet type.
	/// </param>
	/// <returns>
	/// true if this is known to be profile
	/// </returns>
	private static bool IsKnownProfileType(int packetType)
	{
		if (packetType != 23 && packetType != 22 && packetType != 1)
		{
			return packetType == 2;
		}
		return true;
	}

	/// <summary>
	/// The merge segments.
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="allResults">
	/// The all results.
	/// </param>
	private static void MergeSegments(Scan scan, List<FragmentResult>[] allResults)
	{
		SegmentedScan segmentedScan = scan.SegmentedScan;
		int num = 0;
		for (int i = 0; i < segmentedScan.SegmentCount; i++)
		{
			num += allResults[i].Count;
		}
		List<FragmentResult> list = new List<FragmentResult>(num);
		double num2 = 0.0;
		double basePeakMass = 0.0;
		double num3 = 1E+20;
		double num4 = 0.0;
		List<FragmentResult> list2 = null;
		int num5 = 0;
		for (int i = 0; i < segmentedScan.SegmentCount; i++)
		{
			num2 = 0.0;
			basePeakMass = 0.0;
			List<FragmentResult> list3 = allResults[i];
			int num6 = 0;
			long num7 = list3.Count;
			List<FragmentResult> list4 = list2;
			if (list4 != null)
			{
				num5 = list4.Count;
			}
			int num8 = 0;
			while (num5 > 0 || num7 > 0)
			{
				double num9 = ((scan.ToleranceUnit == ToleranceMode.Ppm) ? ((list4 != null) ? (list4[num8].Mass * 1E-06 * scan._massFuzz) : (1E-06 * scan._massFuzz)) : ((scan.ToleranceUnit != ToleranceMode.Mmu) ? scan._massFuzz : (0.001 * scan._massFuzz)));
				FragmentResult fragmentResult3;
				if (num5 > 0)
				{
					if (num7 > 0)
					{
						FragmentResult fragmentResult = list4[num8];
						FragmentResult fragmentResult2 = list3[num6];
						double num10 = fragmentResult.Mass - fragmentResult2.Mass;
						if (num10 < 0.0 - num9)
						{
							fragmentResult3 = fragmentResult;
							num8++;
							num5--;
						}
						else if (num10 > num9)
						{
							fragmentResult3 = fragmentResult2;
							num6++;
							num7--;
						}
						else
						{
							fragmentResult3 = new FragmentResult
							{
								Intensity = fragmentResult.Intensity,
								Mass = fragmentResult.Mass,
								Options = fragmentResult.Options
							};
							CombinePeaks(fragmentResult3, fragmentResult2);
							num8++;
							num5--;
							num6++;
							num7--;
						}
					}
					else
					{
						fragmentResult3 = list4[num8];
						num8++;
						num5--;
					}
				}
				else
				{
					fragmentResult3 = list3[num6];
					num6++;
					num7--;
				}
				if (fragmentResult3.Intensity > num2)
				{
					num2 = fragmentResult3.Intensity;
					basePeakMass = fragmentResult3.Mass;
				}
				if (fragmentResult3.Intensity > 0.0)
				{
					AppendPeak(list, fragmentResult3, num9);
				}
			}
			list2 = list;
			list = new List<FragmentResult>(num);
			num3 = Math.Min(num3, segmentedScan.Ranges[i].Low);
			num4 = Math.Max(num4, segmentedScan.Ranges[i].High);
		}
		int count = list2.Count;
		segmentedScan.Positions = new double[count];
		segmentedScan.Intensities = new double[count];
		segmentedScan.Flags = new PeakOptions[count];
		for (int j = 0; j < count; j++)
		{
			segmentedScan.Positions[j] = list2[j].Mass;
			segmentedScan.Intensities[j] = list2[j].Intensity;
			segmentedScan.Flags[j] = list2[j].Options;
		}
		segmentedScan.PositionCount = count;
		segmentedScan.Ranges = new Range[1];
		segmentedScan.Ranges[0] = new Range(num3, num4);
		segmentedScan.SegmentCount = 1;
		segmentedScan.SegmentSizes = new int[1] { count };
		scan.ScanStatistics.BasePeakMass = basePeakMass;
		scan.ScanStatistics.BasePeakIntensity = num2;
	}

	/// <summary>
	/// Order the segment ranges of the scan, and order the "allResults" table to match
	/// This is part of the process to centroid a scan
	/// when this is done, it is more efficient to combine "allResults" into a single segment
	/// </summary>
	/// <param name="scanRanges">
	/// The scan ranges.
	/// </param>
	/// <param name="allResults">
	/// The all results.
	/// </param>
	/// <returns>
	/// The segment range table
	/// </returns>
	private static Range[] OrderSegments(Range[] scanRanges, List<FragmentResult>[] allResults)
	{
		Range[] array = new Range[scanRanges.Length];
		scanRanges.CopyTo(array, 0);
		int num = scanRanges.Length;
		if (num < 2)
		{
			return array;
		}
		bool flag;
		do
		{
			flag = false;
			for (int i = 0; i < num - 1; i++)
			{
				int num2 = i + 1;
				if (array[i].Low > array[num2].Low)
				{
					List<FragmentResult> list = allResults[i];
					Range range = array[i];
					allResults[i] = allResults[num2];
					allResults[num2] = list;
					array[i] = array[num2];
					array[num2] = range;
					flag = true;
				}
			}
		}
		while (flag);
		return array;
	}

	/// <summary>
	/// Addition of SegmentedScan's.
	/// this case for profile data,which matches with identical or similar segments.
	/// </summary>
	/// <param name="identicalFlag">
	/// set if the flags have the same segments
	/// </param>
	/// <param name="currentScan">
	/// left scan of addition "left + right"
	/// </param>
	/// <param name="inputScan">
	/// right scan of addition "left + right"
	/// </param>
	private static void ProfileDataForSimilarSegments(bool identicalFlag, Scan currentScan, Scan inputScan)
	{
		SegmentedScan segmentedScan = currentScan.SegmentedScan;
		SegmentedScan segmentedScan2 = inputScan.SegmentedScan;
		if (identicalFlag)
		{
			for (int i = 0; i < segmentedScan.PositionCount; i++)
			{
				segmentedScan.Intensities[i] += segmentedScan2.Intensities[i];
				if ((segmentedScan2.Flags[i] & PeakOptions.Saturated) != PeakOptions.None)
				{
					segmentedScan.Flags[i] |= PeakOptions.Saturated;
				}
			}
			return;
		}
		int num = 0;
		for (int j = 0; j < segmentedScan.SegmentCount; j++)
		{
			if (!currentScan._segmentSimilar[j])
			{
				continue;
			}
			num++;
			int num2 = num * segmentedScan.SegmentSizes[j];
			int num3 = num * segmentedScan2.SegmentSizes[j];
			int num4 = j * segmentedScan.SegmentSizes[j];
			int k = 0;
			int num5 = j * segmentedScan2.SegmentSizes[j];
			if (num2 <= 1)
			{
				break;
			}
			double num6 = segmentedScan.Positions[num5 + 1] - segmentedScan.Positions[num5];
			if (num6 < 0.0)
			{
				break;
			}
			double num7 = segmentedScan.Positions[num5];
			for (; k < num3; k++)
			{
				if (!(segmentedScan2.Positions[num4] < num7))
				{
					break;
				}
				num4++;
			}
			if (k >= num3)
			{
				break;
			}
			if (k > 0)
			{
				num4--;
				double num8 = num7 - segmentedScan2.Positions[num4];
				double num9 = segmentedScan2.Intensities[num4] * (1.0 - num8 / num6);
				segmentedScan.Intensities[num5] += num9;
				num4++;
			}
			while (num4 < num3)
			{
				if (num5 < num2 - 1)
				{
					if (segmentedScan2.Positions[num4] < segmentedScan.Positions[num5 + 1])
					{
						double num10 = segmentedScan.Positions[num5 + 1] - segmentedScan2.Positions[num4];
						double num11 = segmentedScan2.Intensities[num4] * num10 / num6;
						segmentedScan.Intensities[num5] += num11;
						segmentedScan.Intensities[num5 + 1] += segmentedScan2.Intensities[num4] - num11;
						num4++;
					}
					else
					{
						num5++;
					}
					continue;
				}
				double num12 = segmentedScan2.Positions[num4] - segmentedScan.Positions[num5];
				double num13 = segmentedScan2.Intensities[num4] * (1.0 - num12 / num6);
				segmentedScan.Intensities[num5] += num13;
				break;
			}
		}
	}

	/// <summary>
	/// Added for supporting addition of FTMS profile points
	/// </summary>
	/// <param name="resultCount">
	/// </param>
	/// <param name="outPut">
	/// </param>
	/// <param name="inPut">
	/// </param>
	/// <param name="position">Index into input data
	/// </param>
	/// <param name="massFuzz">Mass Tolerance
	/// </param>
	private static void PutFtPeak(ref int resultCount, SegmentedScan outPut, SegmentedScan inPut, int position, double massFuzz)
	{
		if (resultCount > 0 && inPut.Positions[position] - outPut.Positions[resultCount - 1] < massFuzz)
		{
			resultCount--;
			if (Math.Abs(outPut.Intensities[position] - 0.0) < 1E-07 && Math.Abs(inPut.Intensities[position] - 0.0) < 1E-07)
			{
				outPut.Positions[resultCount] = 0.5 * (outPut.Positions[resultCount] + inPut.Positions[position]);
			}
			else
			{
				outPut.Positions[resultCount] = (outPut.Positions[resultCount] * outPut.Intensities[resultCount] + inPut.Positions[position] * inPut.Intensities[position]) / (outPut.Intensities[resultCount] + inPut.Intensities[position]);
			}
			outPut.Intensities[resultCount] += inPut.Intensities[position];
		}
		else
		{
			outPut.Positions[resultCount] = inPut.Positions[position];
			outPut.Intensities[resultCount] = inPut.Intensities[position];
		}
	}

	/// <summary>
	/// Adds the two CentroidSteam's
	/// </summary>
	/// <param name="outputIndex">
	/// Current position in the output buffer
	/// </param>
	/// <param name="outPut">
	/// previous output CentroidStream.
	/// </param>
	/// <param name="input">
	/// current CentroidStream.
	/// </param>
	/// <param name="position">
	/// index.
	/// </param>
	/// <param name="massFuzz">
	/// mass value.
	/// </param>
	private static void PutLabel(ref int outputIndex, CentroidStream outPut, CentroidStream input, int position, double massFuzz)
	{
		if (outputIndex > 0 && input.Masses[position] - outPut.Masses[outputIndex - 1] < massFuzz)
		{
			outputIndex--;
			outPut.Masses[outputIndex] = (outPut.Masses[outputIndex] * outPut.Intensities[outputIndex] + input.Masses[position] * input.Intensities[position]) / (outPut.Intensities[outputIndex] + input.Intensities[position]);
			outPut.Intensities[outputIndex] += input.Intensities[position];
			outPut.Flags[outputIndex] |= input.Flags[position];
			if (outPut.Charges[outputIndex] != input.Charges[position])
			{
				outPut.Charges[outputIndex] = 0.0;
			}
		}
		else
		{
			outPut.Intensities[outputIndex] = input.Intensities[position];
			outPut.Baselines[outputIndex] = input.Baselines[position];
			outPut.Masses[outputIndex] = input.Masses[position];
			outPut.Noises[outputIndex] = input.Noises[position];
			outPut.Resolutions[outputIndex] = input.Resolutions[position];
		}
	}

	/// <summary>
	/// The put peak.
	/// </summary>
	/// <param name="resultCount">
	/// The result count.
	/// </param>
	/// <param name="outPut">
	/// The out put.
	/// </param>
	/// <param name="inPut">
	/// The in put.
	/// </param>
	/// <param name="position">
	/// The position.
	/// </param>
	/// <param name="massFuzz">
	/// The mass fuzz.
	/// </param>
	/// <param name="processSegments">
	/// The process segments.
	/// </param>
	private static void PutPeak(ref int resultCount, SegmentedScan outPut, SegmentedScan inPut, int position, double massFuzz, bool processSegments)
	{
		if (processSegments)
		{
			PutPeak(ref resultCount, outPut, inPut, position, massFuzz);
		}
		else
		{
			PutFtPeak(ref resultCount, outPut, inPut, position, massFuzz);
		}
	}

	/// <summary>
	/// Adds the two segmented scan.
	/// </summary>
	/// <param name="resultCount">
	/// index into output array
	/// </param>
	/// <param name="outPut">
	/// previous output peak.
	/// </param>
	/// <param name="inPut">
	/// current peak object.
	/// </param>
	/// <param name="position">
	/// index.
	/// </param>
	/// <param name="massFuzz">
	/// mass value.
	/// </param>
	private static void PutPeak(ref int resultCount, SegmentedScan outPut, SegmentedScan inPut, int position, double massFuzz)
	{
		if (resultCount > 0 && inPut.Positions[position] - outPut.Positions[resultCount - 1] < massFuzz)
		{
			resultCount--;
			outPut.Positions[resultCount] = (outPut.Positions[resultCount] * outPut.Intensities[resultCount] + inPut.Positions[position] * inPut.Intensities[position]) / (outPut.Intensities[resultCount] + inPut.Intensities[position]);
			outPut.Intensities[resultCount] += inPut.Intensities[position];
		}
		else
		{
			outPut.Positions[resultCount] = inPut.Positions[position];
			outPut.Intensities[resultCount] = inPut.Intensities[position];
		}
	}

	/// <summary>
	/// The remove zeros.
	/// </summary>
	/// <param name="stream">
	/// The stream.
	/// </param>
	/// <param name="newSize">
	/// The new size.
	/// </param>
	private static void RemoveZeros(CentroidStream stream, int newSize)
	{
		if (stream.Length != newSize)
		{
			stream.Resize(newSize);
		}
	}

	/// <summary>
	/// The remove zeros.
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="newSize">
	/// The new size.
	/// </param>
	private static void RemoveZeros(SegmentedScan scan, int newSize)
	{
		if (scan.Positions.Length == newSize)
		{
			scan.PositionCount = newSize;
		}
		else
		{
			scan.Resize(newSize);
		}
	}

	/// <summary>
	/// create the arrays for the scan, with a new length
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="total">
	/// The total.
	/// </param>
	private static void ReplaceScanDataArrays(SegmentedScan scan, int total)
	{
		scan.PositionCount = total;
		scan.Positions = new double[total];
		scan.Intensities = new double[total];
		scan.Flags = new PeakOptions[total];
	}

	/// <summary>
	/// Create a new scan out of the data in resultScanSegments
	/// in the private method, the scan is assumed to have valid segment ranges
	/// and the correct size of the segment tables
	/// only the data for the segments needs to be replaced
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="segments">
	/// The segments.
	/// </param>
	private static void ReplaceSegmentData(Scan scan, SegmentedScan[] segments)
	{
		SegmentedScan segmentedScan = scan.SegmentedScan;
		FixSegmentTables(segmentedScan, segments);
		int total = CalculateTotalScanSize(segments);
		ReplaceScanDataArrays(segmentedScan, total);
		UpdateSegmentSizes(segmentedScan, segments);
		FillScanData(segmentedScan, segments);
	}

	/// <summary>
	/// Scale the label intensities by a common factor.
	/// </summary>
	/// <param name="scale">
	/// common factor
	/// </param>
	/// <param name="stream">
	/// CentroidStream object to scale.
	/// </param>
	/// <returns>
	/// returns the scaled Centroid Stream object.
	/// </returns>
	private static CentroidStream ScaleIntensities(double scale, CentroidStream stream)
	{
		for (int i = 0; i < stream.Length; i++)
		{
			stream.Intensities[i] *= scale;
			stream.Noises[i] *= scale;
			stream.Baselines[i] *= scale;
		}
		return stream;
	}

	/// <summary>
	/// Search for the base peak resolution.
	/// </summary>
	/// <param name="baseMass">
	/// base mass value
	/// </param>
	/// <param name="centroidStream">
	/// CentroidStream object to search.
	/// </param>
	/// <param name="peaks">
	/// peak value.
	/// </param>
	/// <returns>
	/// The search base peak index.
	/// </returns>
	private static int SearchBasePeakIndex(double baseMass, CentroidStream centroidStream, int peaks)
	{
		int num = peaks - 1;
		int num2 = 0;
		while (num > num2 + 1)
		{
			int num3 = (num + num2) / 2;
			if (centroidStream.Masses != null && centroidStream.Masses[num3] < baseMass)
			{
				num2 = num3;
			}
			else
			{
				num = num3;
			}
		}
		int result = 0;
		if (num2 < peaks - 1)
		{
			result = ((centroidStream.Intensities[num2] > centroidStream.Intensities[num2 + 1]) ? num2 : (num2 + 1));
		}
		return result;
	}

	/// <summary>
	/// Select only non zero values from an array.
	/// </summary>
	/// <param name="values">
	/// The values.
	/// </param>
	/// <returns>
	/// The array of non zero values
	/// </returns>
	private static double[] SelectNonZeroValues(IEnumerable<double> values)
	{
		List<double> list = new List<double>();
		foreach (double value in values)
		{
			if (value != 0.0)
			{
				list.Add(value);
				continue;
			}
			break;
		}
		return list.ToArray();
	}

	/// <summary>
	/// The sub centroids.
	/// </summary>
	/// <param name="currentScan">
	///     The current scan.
	/// </param>
	/// <param name="inputScan">
	///     The input scan.
	/// </param>
	private static void SubCentroids(Scan currentScan, Scan inputScan)
	{
		CentroidStream centroidScan = currentScan.CentroidScan;
		CentroidStream centroidScan2 = inputScan.CentroidScan;
		int packetType = currentScan.ScanStatistics.PacketType;
		int packetType2 = inputScan.ScanStatistics.PacketType;
		ValidateCentroidScan(centroidScan);
		ValidateCentroidScan(centroidScan2);
		CentroidStream centroidStream = centroidScan;
		long num = 0L;
		long num2 = centroidScan.Length;
		long num3 = centroidScan2.Length;
		int num4 = 0;
		bool usePeakWidthAsMassFuzz = CommonData.IsProfileScan(packetType) && CommonData.IsProfileScan(packetType2);
		int outputIndex = 0;
		while (num2 > 0 || num3 > 0)
		{
			double massFuzz = currentScan._massFuzz;
			double resolutionFactor = 1.0;
			if (num2 > 0 && num3 > 0)
			{
				if (centroidScan2.Resolutions != null && centroidScan2.Resolutions[num4] > 1.0)
				{
					resolutionFactor = 1.0 / centroidScan2.Resolutions[num4];
				}
				massFuzz = ((centroidScan.Masses == null || centroidScan2.Masses == null || !(centroidScan.Masses[num4] < centroidScan2.Masses[num4])) ? ConvertCentroidStreamTolerance(currentScan, centroidScan2, num4, usePeakWidthAsMassFuzz, resolutionFactor) : ConvertCentroidStreamTolerance(currentScan, centroidScan, num4, usePeakWidthAsMassFuzz, resolutionFactor));
				double num5 = centroidScan.Masses[num4] - centroidScan2.Masses[num4];
				if (num5 < 0.0 - massFuzz)
				{
					PutLabel(ref outputIndex, centroidStream, centroidScan, num4, massFuzz);
					num2--;
					num3--;
				}
				else if (num5 > massFuzz)
				{
					SubLabel(ref outputIndex, centroidStream, centroidScan2, num4, massFuzz);
					num2--;
					num3--;
				}
				else
				{
					PutLabel(ref outputIndex, centroidStream, centroidScan, num4, massFuzz);
					centroidStream.Masses[num4] = centroidStream.Masses[num4] * centroidStream.Intensities[num4] + centroidScan2.Masses[num4] * centroidScan2.Intensities[num4] / (centroidStream.Intensities[num4] + centroidScan2.Intensities[num4]);
					centroidStream.Intensities[num4] -= centroidScan2.Intensities[num4];
					centroidStream.Flags[num4] |= centroidScan2.Flags[num4];
					if (centroidStream.Charges[num4] != centroidScan2.Charges[num4])
					{
						centroidStream.Charges[num4] = 0.0;
					}
					num2--;
					num3--;
				}
			}
			else if (num3 > 0)
			{
				if (centroidScan2.Resolutions[num4] > 1.0)
				{
					resolutionFactor = 1.0 / centroidScan2.Resolutions[num4];
				}
				ConvertCentroidStreamTolerance(currentScan, centroidScan2, num4, usePeakWidthAsMassFuzz, resolutionFactor);
				SubLabel(ref outputIndex, centroidStream, centroidScan2, num4, massFuzz);
			}
			else if (num2 > 0)
			{
				ConvertCentroidStreamTolerance(currentScan, centroidScan, num4, usePeakWidthAsMassFuzz, resolutionFactor);
				PutLabel(ref outputIndex, centroidStream, centroidScan, num4, massFuzz);
			}
			if (centroidStream.Intensities[num4] < 0.0)
			{
				centroidStream.Intensities[num4] = 0.0;
			}
			if (centroidStream.Intensities[num4] > 0.0)
			{
				num++;
			}
		}
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				centroidStream.Resolutions[i] = 0.0;
			}
		}
		currentScan.CentroidScan = centroidStream;
	}

	/// <summary>
	/// The sub ft centroid data.
	/// </summary>
	/// <param name="currentScan">
	/// The current scan.
	/// </param>
	/// <param name="rightScan">
	/// The right scan.
	/// </param>
	/// <returns>
	/// The subtracted scan
	/// </returns>
	private static Scan SubFTCentroidData(Scan currentScan, Scan rightScan)
	{
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = currentScan.CentroidScan.Length;
		int num4 = rightScan.CentroidScan.Length;
		CentroidStream centroidScan = currentScan.CentroidScan;
		CentroidStream centroidScan2 = rightScan.CentroidScan;
		CentroidStream centroidStream = CreateCentroidStream(centroidScan.Length + centroidScan2.Length, centroidScan.ScanNumber);
		double num5 = 0.0;
		double baseMass = 0.0;
		int outputIndex = 0;
		int resultCount = 0;
		SegmentedScan segmentedScan = currentScan.SegmentedScan;
		SegmentedScan segmentedScan2 = rightScan.SegmentedScan;
		SegmentedScan segmentedScan3 = CreateSegmentedScan(segmentedScan.PositionCount + segmentedScan2.PositionCount);
		double toleranceFactor = 0.0;
		toleranceFactor = GetToleranceFactor(currentScan, rightScan, centroidScan, centroidScan2, num4, toleranceFactor, flag, num3);
		while (num3 > 0 || num4 > 0)
		{
			if (num3 > 0 && num4 > 0)
			{
				double num6 = CalculateMassFuzz(currentScan, centroidScan, centroidScan2, toleranceFactor, flag, num, num2, processSegments: false);
				double num7 = centroidScan.Masses[num] - centroidScan2.Masses[num2];
				if (num7 < 0.0 - num6)
				{
					PutLabel(ref outputIndex, centroidStream, centroidScan, num, num6);
					PutPeak(ref resultCount, segmentedScan3, segmentedScan, num, num6);
					num3--;
					num++;
				}
				else if (num7 > num6)
				{
					SubLabel(ref outputIndex, centroidStream, centroidScan2, num2, num6);
					SubPeak(ref resultCount, segmentedScan3, segmentedScan2, num2, num6);
					num4--;
					num2++;
				}
				else
				{
					PutLabel(ref outputIndex, centroidStream, centroidScan, num, num6);
					PutPeak(ref resultCount, segmentedScan3, segmentedScan, num, num6);
					centroidStream.Masses[outputIndex] = (centroidStream.Masses[outputIndex] * centroidStream.Intensities[outputIndex] + centroidScan2.Masses[num2] * centroidScan2.Intensities[num2]) / (centroidStream.Intensities[outputIndex] + centroidScan2.Intensities[num2]);
					centroidStream.Intensities[outputIndex] -= centroidScan2.Intensities[num2];
					centroidStream.Flags[outputIndex] |= centroidScan2.Flags[num2];
					if (centroidStream.Charges[outputIndex] != centroidScan2.Charges[num2])
					{
						centroidStream.Charges[outputIndex] = 0.0;
					}
					segmentedScan3.Positions[resultCount] = (segmentedScan3.Positions[resultCount] * segmentedScan3.Intensities[resultCount] + segmentedScan2.Positions[num2] * segmentedScan2.Intensities[num2]) / (segmentedScan3.Intensities[resultCount] + segmentedScan2.Intensities[num2]);
					segmentedScan3.Intensities[resultCount] -= segmentedScan2.Intensities[num2];
					num3--;
					num4--;
					num++;
					num2++;
				}
			}
			else if (num4 > 0)
			{
				double num6 = CalculateMassFuzzForCentroidPeak(centroidScan2.Masses[num2], toleranceFactor, currentScan._massFuzz, flag, currentScan._toleranceUnit);
				SubLabel(ref outputIndex, centroidStream, centroidScan2, num2, num6);
				SubPeak(ref resultCount, segmentedScan3, segmentedScan2, num2, num6);
				num4--;
				num2++;
			}
			else if (num3 > 0)
			{
				double num6 = CalculateMassFuzzForCentroidPeak(centroidScan.Masses[num], toleranceFactor, currentScan._massFuzz, flag, currentScan._toleranceUnit);
				PutLabel(ref outputIndex, centroidStream, centroidScan, num, num6);
				PutPeak(ref resultCount, segmentedScan3, segmentedScan, num, num6);
				num3--;
				num++;
			}
			if (segmentedScan3.Intensities[resultCount] > num5)
			{
				num5 = segmentedScan3.Intensities[resultCount];
				baseMass = segmentedScan3.Positions[resultCount];
			}
			if (centroidStream.Intensities[outputIndex] < 0.0)
			{
				centroidStream.Intensities[outputIndex] = 0.0;
				segmentedScan3.Intensities[outputIndex] = 0.0;
			}
			if (centroidStream.Intensities[outputIndex] > 0.0)
			{
				resultCount++;
				outputIndex++;
			}
		}
		if (outputIndex > 0)
		{
			for (int i = 0; i < outputIndex; i++)
			{
				centroidStream.Resolutions[i] = 0.0;
			}
			FillSegmentRange(segmentedScan, segmentedScan2, segmentedScan3);
		}
		segmentedScan3.SegmentCount = 1;
		segmentedScan3.SegmentSizes = new int[1] { resultCount };
		segmentedScan3.Ranges = new Range[1];
		Range range = currentScan.SegmentedScan.Ranges[0];
		segmentedScan3.Ranges[0] = new Range(range.Low, range.High);
		RemoveZeros(segmentedScan3, resultCount);
		RemoveZeros(centroidStream, resultCount);
		currentScan.CentroidScan = centroidStream;
		currentScan.SegmentedScan = segmentedScan3;
		currentScan.ScanStatistics = GetSubtractedIndex(currentScan, rightScan, resultCount, num5, baseMass);
		return currentScan;
	}

	/// <summary>
	/// Subtract FT Profile Data
	/// </summary>
	/// <param name="currentScan">
	/// The object subtract from this
	/// </param>
	/// <param name="inputScan">
	/// </param>
	/// <returns>
	/// The result of the subtraction of the two scans
	/// </returns>
	private static Scan SubFTProfileData(Scan currentScan, Scan inputScan)
	{
		string message = "IScanSubtract interface should not be null.";
		if (currentScan.SubtractionPointer != null)
		{
			return currentScan.SubtractionPointer.Subtract(currentScan, inputScan);
		}
		throw new InvalidOperationException(message);
	}

	/// <summary>
	/// Add FT Profile Data
	/// </summary>
	/// <param name="currentScan">
	/// The object subtract from this
	/// </param>
	/// <param name="inputScan">
	/// </param>
	/// <returns>
	/// The result of the subtraction of the two scans
	/// </returns>
	private static Scan AddFTProfileData(Scan currentScan, Scan inputScan)
	{
		string message = "IScanAdd interface should not be null.";
		IScanAdd scanAdder = currentScan.ScanAdder;
		if (scanAdder != null)
		{
			return scanAdder.Add(currentScan, inputScan);
		}
		throw new InvalidOperationException(message);
	}

	/// <summary>
	/// The sub label.
	/// </summary>
	/// <param name="outputIndex">
	/// The output index.
	/// </param>
	/// <param name="outPut">
	/// The out put.
	/// </param>
	/// <param name="input">
	/// The input.
	/// </param>
	/// <param name="position">
	/// The position.
	/// </param>
	/// <param name="massFuzz">
	/// The mass fuzz.
	/// </param>
	private static void SubLabel(ref int outputIndex, CentroidStream outPut, CentroidStream input, int position, double massFuzz)
	{
		if (outputIndex > 0 && input.Masses[position] - outPut.Masses[position] < massFuzz)
		{
			outputIndex--;
			outPut.Masses[outputIndex] = (outPut.Masses[outputIndex] * outPut.Intensities[outputIndex] + input.Masses[position] * input.Intensities[position]) / (outPut.Intensities[outputIndex] + input.Intensities[position]);
			outPut.Intensities[outputIndex] -= input.Intensities[position];
			outPut.Flags[outputIndex] |= input.Flags[position];
			if (outPut.Charges[outputIndex] != input.Charges[position])
			{
				outPut.Charges[outputIndex] = 0.0;
			}
		}
		else
		{
			outPut.Intensities[outputIndex] = 0.0;
			outPut.Baselines[outputIndex] = input.Baselines[position];
			outPut.Masses[outputIndex] = input.Masses[position];
			outPut.Noises[outputIndex] = input.Noises[position];
			outPut.Resolutions[outputIndex] = input.Resolutions[position];
		}
	}

	/// <summary>
	/// Subtract a background peak.
	/// </summary>
	/// <param name="outCounter">
	/// The output (foreground scan) counter.
	/// </param>
	/// <param name="outPut">
	/// The output (foreground scan).
	/// </param>
	/// <param name="inPut">
	/// The input (background scan).
	/// </param>
	/// <param name="inputCounter">
	/// The input (background scan) counter.
	/// </param>
	/// <param name="massFuzz">
	/// The mass fuzz (tolerance).
	/// </param>
	private static void SubPeak(ref int outCounter, SegmentedScan outPut, SegmentedScan inPut, int inputCounter, double massFuzz)
	{
		if (outCounter > 0 && inPut.Positions[inputCounter] - outPut.Positions[outCounter - 1] < massFuzz)
		{
			outCounter--;
			outPut.Positions[outCounter] = (outPut.Positions[outCounter] * outPut.Intensities[outCounter] + inPut.Positions[inputCounter] * inPut.Intensities[inputCounter]) / (outPut.Intensities[outCounter] + inPut.Intensities[inputCounter]);
			outPut.Intensities[outCounter] -= inPut.Intensities[inputCounter];
		}
		else
		{
			outPut.Positions[outCounter] = inPut.Positions[inputCounter];
			outPut.Intensities[outCounter] = 0.0;
		}
	}

	/// <summary>
	/// The subtract profile segments.
	/// </summary>
	/// <param name="identicalFlag">
	/// The identical flag.
	/// </param>
	/// <param name="currentScan">
	/// The current scan.
	/// </param>
	/// <param name="inputScan">
	/// The input scan.
	/// </param>
	private static void SubtractProfileSegments(bool identicalFlag, Scan currentScan, Scan inputScan)
	{
		SegmentedScan segmentedScan = currentScan.SegmentedScan;
		SegmentedScan segmentedScan2 = inputScan.SegmentedScan;
		int num = 0;
		currentScan.ScanStatistics.TIC = 0.0;
		if (identicalFlag)
		{
			double num2 = 0.0;
			for (int i = 0; i < segmentedScan.PositionCount; i++)
			{
				segmentedScan.Intensities[i] = Math.Max(0.0, segmentedScan.Intensities[i] - segmentedScan2.Intensities[i]);
				if ((segmentedScan2.Flags[i] & PeakOptions.Saturated) != PeakOptions.None)
				{
					segmentedScan.Flags[i] |= PeakOptions.Saturated;
				}
				num2 += segmentedScan.Intensities[i];
			}
			currentScan.ScanStatistics.TIC = num2;
			return;
		}
		for (int j = 0; j < segmentedScan.SegmentCount; j++)
		{
			currentScan.ScanStatistics.TIC = 0.0;
			if (!currentScan._segmentSimilar[j])
			{
				continue;
			}
			num++;
			int num3 = num * segmentedScan.SegmentSizes[j];
			int num4 = num * segmentedScan2.SegmentSizes[j];
			int num5 = j * segmentedScan.SegmentSizes[j];
			int k = 0;
			int num6 = j * segmentedScan2.SegmentSizes[j];
			if (num3 <= 1)
			{
				break;
			}
			double num7 = segmentedScan.Positions[num6 + 1] - segmentedScan.Positions[num6];
			if (num7 < 0.0)
			{
				break;
			}
			double num8 = segmentedScan.Positions[num6];
			for (; k < num4; k++)
			{
				if (!(segmentedScan2.Positions[num5] < num8))
				{
					break;
				}
				num5++;
			}
			if (k >= num4)
			{
				break;
			}
			if (k > 0)
			{
				num5--;
				double num9 = num8 - segmentedScan2.Positions[num5];
				double num10 = segmentedScan2.Intensities[num5] * (1.0 - num9 / num7);
				segmentedScan.Intensities[num6] -= num10;
				num5++;
			}
			while (num5 < num4)
			{
				if (num6 < num3 - 1)
				{
					if (segmentedScan2.Positions[num5] < segmentedScan.Positions[num6 + 1])
					{
						double num11 = segmentedScan.Positions[num6 + 1] - segmentedScan2.Positions[num5];
						double num12 = segmentedScan2.Intensities[num5] * num11 / num7;
						segmentedScan.Intensities[num6] -= num12;
						segmentedScan.Intensities[num6 + 1] -= segmentedScan2.Intensities[num5] - num12;
						num5++;
					}
					else
					{
						num6++;
					}
					continue;
				}
				double num13 = segmentedScan2.Positions[num5] - segmentedScan.Positions[num6];
				double num14 = segmentedScan2.Intensities[num5] * (1.0 - num13 / num7);
				segmentedScan.Intensities[num6] -= num14;
				break;
			}
		}
	}

	/// <summary>
	/// The subtract segment.
	/// </summary>
	/// <param name="leftScan">
	/// The left scan.
	/// </param>
	/// <param name="leftSegment">
	/// The left segment.
	/// </param>
	/// <param name="rightSegment">
	/// The right segment.
	/// </param>
	/// <param name="segmentNumber">
	/// The segment number.
	/// </param>
	/// <param name="resultScanSegments">
	/// The result scan segments.
	/// </param>
	/// <param name="baseHeight">
	/// The base height.
	/// </param>
	/// <param name="baseMass">
	/// The base mass.
	/// </param>
	private static void SubtractSegment(Scan leftScan, SegmentedScan leftSegment, SegmentedScan rightSegment, int segmentNumber, SegmentedScan[] resultScanSegments, ref double baseHeight, ref double baseMass)
	{
		long num = leftSegment.SegmentSizes[segmentNumber];
		long num2 = rightSegment.SegmentSizes[segmentNumber];
		long num3 = num + num2;
		SegmentedScan segmentedScan = (resultScanSegments[segmentNumber] = CreateSegmentedScan((int)num3));
		FindSegmentStartPackets(leftSegment, rightSegment, segmentNumber, out var leftScanCount, out var rightScanCount);
		int resultCount = 0;
		while (num > 0 || num2 > 0)
		{
			double num4 = ((num <= 0) ? leftScan.ConvertTolerance(rightScanCount, rightSegment.Positions) : leftScan.ConvertTolerance(leftScanCount, leftSegment.Positions));
			if (num > 0 && num2 > 0)
			{
				double num5 = leftSegment.Positions[leftScanCount] - rightSegment.Positions[rightScanCount];
				if (num5 < 0.0 - num4)
				{
					PutPeak(ref resultCount, segmentedScan, leftSegment, leftScanCount, num4);
					leftScanCount++;
					num--;
				}
				else if (num5 > num4)
				{
					SubPeak(ref resultCount, segmentedScan, rightSegment, rightScanCount, num4);
					rightScanCount++;
					num2--;
				}
				else
				{
					PutPeak(ref resultCount, segmentedScan, leftSegment, leftScanCount, num4);
					segmentedScan.Positions[resultCount] = (segmentedScan.Positions[resultCount] * segmentedScan.Intensities[resultCount] + rightSegment.Positions[rightScanCount] * rightSegment.Intensities[rightScanCount]) / (segmentedScan.Intensities[resultCount] + rightSegment.Intensities[rightScanCount]);
					segmentedScan.Intensities[resultCount] -= rightSegment.Intensities[rightScanCount];
					leftScanCount++;
					rightScanCount++;
					num--;
					num2--;
				}
			}
			else if (num2 > 0)
			{
				SubPeak(ref resultCount, segmentedScan, rightSegment, rightScanCount, num4);
				rightScanCount++;
				num2--;
			}
			else if (num > 0)
			{
				PutPeak(ref resultCount, segmentedScan, leftSegment, leftScanCount, num4);
				leftScanCount++;
				num--;
			}
			if (segmentedScan.Intensities[resultCount] > baseHeight)
			{
				baseHeight = segmentedScan.Intensities[resultCount];
				baseMass = segmentedScan.Positions[resultCount];
			}
			if (segmentedScan.Intensities[resultCount] > 0.0)
			{
				resultCount++;
				continue;
			}
			segmentedScan.Intensities[resultCount] = 0.0;
			segmentedScan.Positions[resultCount] = 0.0;
		}
		double[] array = SelectNonZeroValues(segmentedScan.Positions);
		double[] array2 = SelectNonZeroValues(segmentedScan.Intensities);
		if (array.Length == array2.Length)
		{
			segmentedScan = CreateSegmentedScan(array.Length);
			segmentedScan.Intensities = array2;
			segmentedScan.Positions = array;
			resultScanSegments[segmentNumber] = segmentedScan;
		}
	}

	/// <summary>
	/// The subtract segments.
	/// </summary>
	/// <param name="identicalFlag">
	/// The identical flag.
	/// </param>
	/// <param name="leftScan">
	/// The left scan.
	/// </param>
	/// <param name="rightScan">
	/// The right scan.
	/// </param>
	/// <returns>
	/// The subtraction result
	/// </returns>
	private static Scan SubtractSegments(bool identicalFlag, Scan leftScan, Scan rightScan)
	{
		double baseHeight = 0.0;
		double baseMass = 0.0;
		SegmentedScan segmentedScan = leftScan.SegmentedScan;
		SegmentedScan segmentedScan2 = rightScan.SegmentedScan;
		ScanStatistics scanStatistics = leftScan.ScanStatistics;
		ValidateSegmentedScan(segmentedScan);
		ValidateSegmentedScan(segmentedScan2);
		int num = Math.Min(segmentedScan.SegmentCount, segmentedScan2.SegmentCount);
		if ((identicalFlag || leftScan._segmentSimilar[0]) && CommonData.LOWord(scanStatistics.PacketType) != 24)
		{
			SubtractProfileSegments(identicalFlag, leftScan, rightScan);
		}
		else
		{
			if (!leftScan._userTolerance)
			{
				leftScan = ToCentroid(leftScan);
				if (rightScan.ScanStatistics != null && CommonData.IsProfileScan(rightScan.ScanStatistics.PacketType))
				{
					rightScan = ToCentroid(rightScan);
				}
			}
			SegmentedScan[] array = new SegmentedScan[num];
			for (int i = 0; i < num; i++)
			{
				SubtractSegment(leftScan, segmentedScan, segmentedScan2, i, array, ref baseHeight, ref baseMass);
			}
			ReplaceSegmentData(leftScan, array);
			if (leftScan.ScanStatistics != null && rightScan.ScanStatistics != null)
			{
				scanStatistics.HighMass = Math.Max(leftScan.ScanStatistics.HighMass, rightScan.ScanStatistics.HighMass);
				scanStatistics.LowMass = Math.Min(leftScan.ScanStatistics.LowMass, rightScan.ScanStatistics.LowMass);
				scanStatistics.PacketCount = leftScan.SegmentedScan.PositionCount;
				scanStatistics.TIC -= rightScan.ScanStatistics.TIC;
				scanStatistics.BasePeakIntensity = baseHeight;
				scanStatistics.BasePeakMass = baseMass;
			}
			leftScan.ScanStatistics = scanStatistics;
		}
		return leftScan;
	}

	/// <summary>
	/// Try to merge similar scans.
	/// </summary>
	/// <param name="currentScanObject">
	/// The current scan object.
	/// </param>
	/// <param name="inputScanObject">
	/// The input scan object.
	/// </param>
	/// <param name="bothCentroids">
	/// The both centroids.
	/// </param>
	/// <param name="counter">
	/// The counter.
	/// </param>
	/// <param name="canMerge">
	/// The can merge.
	/// </param>
	/// <returns>
	/// True if can merge similar scans.
	/// </returns>
	private static bool TryToMergeSimilarScans(Scan currentScanObject, Scan inputScanObject, bool bothCentroids, int counter, bool canMerge)
	{
		currentScanObject._similarFlag = false;
		SegmentedScan segmentedScan = currentScanObject.SegmentedScan;
		currentScanObject._segmentSimilar = new bool[segmentedScan.SegmentCount];
		bool flag = segmentedScan.SegmentCount > 1 || !currentScanObject.AlwaysMergeSegments;
		for (int i = 0; i < segmentedScan.SegmentCount; i++)
		{
			currentScanObject._segmentSimilar[i] = true;
			Range range = segmentedScan.Ranges[i];
			Range range2 = inputScanObject.SegmentedScan.Ranges[i];
			double num = range.High - range.Low;
			double num2 = range2.High - range2.Low;
			if (num2 > 0.0)
			{
				double num3 = num / num2;
				if (flag && (num3 > 1.1 || num3 < 0.9))
				{
					currentScanObject._segmentSimilar[i] = false;
				}
				else
				{
					double num4 = num / 10.0;
					if (flag && (Math.Abs(range.High - range2.High) > num4 || Math.Abs(range.Low - range2.Low) > num4))
					{
						currentScanObject._segmentSimilar[i] = false;
					}
					else if (!bothCentroids)
					{
						int segmentCount = currentScanObject.SegmentedScan.SegmentCount;
						int segmentCount2 = inputScanObject.SegmentedScan.SegmentCount;
						if (segmentCount > 1 && segmentCount2 > 1)
						{
							double num5 = num / (double)(segmentCount - 1);
							double num6 = num2 / (double)(segmentCount2 - 1);
							double num7 = Math.Abs(num5 / num6);
							if (num7 > 1.1 || num7 < 0.9)
							{
								currentScanObject._segmentSimilar[i] = false;
							}
						}
						else
						{
							currentScanObject._segmentSimilar[i] = false;
						}
					}
				}
			}
			else
			{
				currentScanObject._segmentSimilar[counter] = false;
			}
			currentScanObject._similarFlag = currentScanObject._similarFlag || currentScanObject._segmentSimilar[i];
		}
		if (!currentScanObject._similarFlag)
		{
			currentScanObject._segmentSimilar = null;
			canMerge = false;
		}
		return canMerge;
	}

	/// <summary>
	/// The update segment sizes.
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="segments">
	/// The segments.
	/// </param>
	private static void UpdateSegmentSizes(SegmentedScan scan, SegmentedScan[] segments)
	{
		for (int i = 0; i < segments.Length; i++)
		{
			scan.SegmentSizes[i] = segments[i].PositionCount;
		}
	}

	/// <summary>
	/// The validate centroid scan.
	/// </summary>
	/// <param name="stream">
	/// The stream.
	/// </param>
	/// <exception cref="T:System.InvalidOperationException">
	/// </exception>
	private static void ValidateCentroidScan(CentroidStream stream)
	{
		if (stream == null)
		{
			throw new InvalidOperationException("CentroidStream object should not be null");
		}
		stream.Validate();
	}

	/// <summary>
	/// The validate segmented scan.
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	private static void ValidateSegmentedScan(SegmentedScan scan)
	{
		scan.Validate();
	}

	/// <summary>
	/// Convert tolerance into mass.
	/// </summary>
	/// <param name="prof">
	/// The prof.
	/// </param>
	/// <param name="positions">
	/// The positions.
	/// </param>
	/// <returns>
	/// The converted tolerance.
	/// </returns>
	private double ConvertTolerance(int prof, double[] positions)
	{
		if (ToleranceUnit == ToleranceMode.Ppm)
		{
			return positions[prof] * 1E-06 * _massFuzz;
		}
		if (ToleranceUnit == ToleranceMode.Mmu)
		{
			return 0.001 * _massFuzz;
		}
		return _massFuzz;
	}

	/// <summary>
	/// Find peaks in the current segment (part of centroiding)
	/// </summary>
	/// <param name="segment">The scan segment (SIM/SRM range)
	/// </param>
	private List<FragmentResult> Fragment(int segment)
	{
		if (SegmentedScan.SegmentSizes[segment] < 2)
		{
			return null;
		}
		int num = SegmentedScan.SegmentSizes[segment];
		List<FragmentResult> list = new List<FragmentResult>();
		int i = SegmentedScan.IndexOfSegmentStart(segment);
		int num2 = i + num - 2;
		double[] positions = SegmentedScan.Positions;
		double[] intensities = SegmentedScan.Intensities;
		PeakOptions[] flags = SegmentedScan.Flags;
		int num3 = positions.Length;
		int num4 = i + 1;
		if (num4 > num3)
		{
			return null;
		}
		double num5 = positions[num4] - positions[i];
		if (num5 < 1E-30)
		{
			num4++;
			if (num4 > num3)
			{
				return null;
			}
			num5 = positions[num4] - positions[num4 - 1];
			if (num5 < 1E-30)
			{
				return null;
			}
		}
		double num6 = ConvertTolerance(i, positions);
		int num7 = 2 * (int)decimal.Ceiling(new decimal(num6 / num5));
		while (i < num2)
		{
			while (intensities[i] < 1.0 && i++ < num2)
			{
			}
			bool flag2;
			bool flag3;
			bool flag = (flag2 = (flag3 = false));
			int num8 = 0;
			double num10;
			double num9 = (num10 = 0.0);
			for (; i < num2; i++)
			{
				if (flag)
				{
					break;
				}
				if (intensities[i] >= 1.0)
				{
					num9 += intensities[i];
					num10 += num9;
					num8++;
					flag2 |= (flags[i] & PeakOptions.Saturated) == PeakOptions.Saturated;
				}
				else
				{
					flag = true;
				}
				if (num8 > 3)
				{
					flag |= (flag3 |= intensities[i - 2] >= intensities[i + 1] && intensities[i - 1] < intensities[i + 2] && num8 >= 3);
					flag |= (flag3 |= intensities[i - 1] * 2.0 < intensities[i - 2]);
					flag |= (flag3 |= intensities[i + 1] * 2.0 < intensities[i + 2]);
				}
				flag = flag || num8 > num7;
			}
			if (flag3 && i < num2 && intensities[i] > 1.0)
			{
				num9 += intensities[i];
				num10 += num9;
				num8++;
				flag2 |= (flags[i] & PeakOptions.Saturated) == PeakOptions.Saturated;
				i++;
			}
			if (num9 > 0.0)
			{
				double intensity = num9;
				double num11 = (((num9 > 0.0) ? (num10 / num9) : 0.0) + 1.0) * num5;
				double mass = positions[i] - num11;
				PeakOptions peakOptions = PeakOptions.None;
				if (flag3)
				{
					peakOptions = PeakOptions.Fragmented;
				}
				if (flag2)
				{
					peakOptions |= PeakOptions.Saturated;
				}
				list.Add(new FragmentResult
				{
					Intensity = intensity,
					Mass = mass,
					Options = peakOptions
				});
			}
		}
		return list;
	}

	/// <summary>
	/// The read from file.
	/// </summary>
	/// <param name="detectorReader">
	/// The detector containing this scan.
	/// </param>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="stats">
	/// The stats.
	/// </param>
	protected void ReadFromDetector(IDetectorReader detectorReader, int scanNumber, ScanStatistics stats)
	{
		IConfiguredDetector configuredDetector = detectorReader.ConfiguredDetector;
		if (configuredDetector == null)
		{
			SegmentedScan = new SegmentedScan();
			CentroidScan = new CentroidStream();
			ScanStatistics = new ScanStatistics();
			ScanType = string.Empty;
		}
		else
		{
			SegmentedScan = detectorReader.GetSegmentedScanFromScanNumber(scanNumber, stats) ?? new SegmentedScan();
			CentroidScan = ((configuredDetector.DeviceType == Device.MS) ? detectorReader.GetCentroidStream(scanNumber, configuredDetector.UseReferenceAndExceptionData) : new CentroidStream());
			ScanStatistics = new ScanStatistics(stats);
			ScanType = stats.ScanType;
			ScanStatistics.ScanType = ScanType;
			IRunHeader runHeaderEx = detectorReader.RunHeaderEx;
			ConvertTolerance(runHeaderEx);
		}
	}

	/// <summary>
	/// Covert tolerance (for detector reader)
	/// </summary>
	/// <param name="header"></param>
	private void ConvertTolerance(IRunHeader header)
	{
		switch (header.ToleranceUnit)
		{
		case ToleranceUnits.mmu:
			ToleranceUnit = ToleranceMode.Mmu;
			break;
		case ToleranceUnits.ppm:
			ToleranceUnit = ToleranceMode.Ppm;
			break;
		case ToleranceUnits.amu:
			ToleranceUnit = ToleranceMode.Amu;
			break;
		default:
			ToleranceUnit = ToleranceMode.None;
			break;
		}
		MassResolution = header.MassResolution;
	}

	/// <summary>
	/// Legacy version for code paths using IRawData
	/// </summary>
	/// <param name="header"></param>
	private void ConvertTolerance(IRunHeaderAccess header)
	{
		switch (header.ToleranceUnit)
		{
		case ToleranceUnits.mmu:
			ToleranceUnit = ToleranceMode.Mmu;
			break;
		case ToleranceUnits.ppm:
			ToleranceUnit = ToleranceMode.Ppm;
			break;
		case ToleranceUnits.amu:
			ToleranceUnit = ToleranceMode.Amu;
			break;
		default:
			ToleranceUnit = ToleranceMode.None;
			break;
		}
		MassResolution = header.MassResolution;
	}

	/// <summary>
	/// The read from file.
	/// </summary>
	/// <param name="rawFile">
	/// The raw file.
	/// </param>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="stats">
	/// The stats.
	/// </param>
	protected void ReadFromFile(IRawData rawFile, int scanNumber, ScanStatistics stats)
	{
		InstrumentSelection selectedInstrument = rawFile.SelectedInstrument;
		if (selectedInstrument == null)
		{
			SegmentedScan = new SegmentedScan();
			CentroidScan = new CentroidStream();
			ScanStatistics = new ScanStatistics();
			ScanType = string.Empty;
		}
		else
		{
			SegmentedScan = rawFile.GetSegmentedScanFromScanNumber(scanNumber) ?? new SegmentedScan();
			CentroidScan = ((selectedInstrument.DeviceType == Device.MS) ? rawFile.GetCentroidStream(scanNumber, rawFile.IncludeReferenceAndExceptionData) : new CentroidStream());
			ScanStatistics = new ScanStatistics(stats);
			ScanType = stats.ScanType;
			ScanStatistics.ScanType = ScanType;
			IRunHeaderAccess runHeader = rawFile.RunHeader;
			ConvertTolerance(runHeader);
		}
	}
}
