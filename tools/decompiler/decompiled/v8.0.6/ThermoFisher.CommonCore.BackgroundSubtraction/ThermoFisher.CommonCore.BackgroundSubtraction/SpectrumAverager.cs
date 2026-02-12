using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// Class for averaging a set of spectra.
/// </summary>
public class SpectrumAverager : IScanSubtract, IScanAdd
{
	/// <summary>
	/// Find a segmented scan.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>The found scan</returns>
	private delegate SegmentedScan FindSegmentedScan(int scanNumber);

	/// <summary>
	/// Find centroid data.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>The found centroids.</returns>
	private delegate CentroidStream FindCentroids(int scanNumber);

	/// <summary>
	/// Internal representation of re-sampled data
	/// </summary>
	internal class ProfileData
	{
		/// <summary>
		/// Gets or sets Masses.
		/// </summary>
		public double[] Masses { get; set; }

		/// <summary>
		/// Gets or sets Intensities.
		/// </summary>
		public double[] Intensities { get; set; }

		/// <summary>
		/// Gets Length.
		/// </summary>
		public int Length => Masses.Length;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.SpectrumAverager.ProfileData" /> class.
		/// </summary>
		/// <param name="totalPoints">
		/// The total points.
		/// </param>
		public ProfileData(int totalPoints)
		{
			Masses = new double[totalPoints];
			Intensities = new double[totalPoints];
		}
	}

	/// <summary>
	/// The part profile masses.
	/// </summary>
	private class PartProfileMasses
	{
		/// <summary>
		/// Gets or sets a set of masses for profile points.
		/// </summary>
		public double[] Mass { get; set; }

		/// <summary>
		/// Gets or sets number of filled array points.
		/// </summary>
		public int Filled { get; set; }
	}

	/// <summary>
	/// The re-sampler.
	/// </summary>
	private class Resampler : IExecute
	{
		private readonly double[] _binMasses;

		private readonly int _scans;

		private readonly SegmentedScan _data;

		private List<MergeActions> _actions;

		/// <summary>
		/// Gets Result.
		/// </summary>
		public IEnumerable<MergeActions> Result => _actions;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.SpectrumAverager.Resampler" /> class.
		/// </summary>
		/// <param name="masses">
		/// The masses.
		/// </param>
		/// <param name="scanData">
		/// The scan data.
		/// </param>
		/// <param name="scans">
		/// The scans.
		/// </param>
		public Resampler(double[] masses, SegmentedScan scanData, int scans)
		{
			_binMasses = masses;
			_data = scanData;
			_scans = scans;
		}

		/// <summary>
		/// Method executed in parallel
		/// </summary>
		public void Execute()
		{
			_actions = ResampleScan(_binMasses, _data, _scans);
		}

		/// <summary>
		/// re-sample a scan.
		/// </summary>
		/// <param name="resampledMasses">
		/// The re-sampled masses.
		/// </param>
		/// <param name="scanData">
		/// The scan data.
		/// </param>
		/// <param name="scansAveraged">
		/// The scans averaged.
		/// </param>
		/// <returns>
		/// The set of resampled peaks, which must be merged into the final scan.
		/// </returns>
		private static List<MergeActions> ResampleScan(double[] resampledMasses, SegmentedScan scanData, int scansAveraged)
		{
			double[] intensities = scanData.Intensities;
			double[] positions = scanData.Positions;
			int num = intensities.Length;
			if (num == 0)
			{
				return new List<MergeActions>
				{
					new MergeActions
					{
						Filed = 0,
						Indexes = new int[0],
						Values = new double[0]
					}
				};
			}
			List<MergeActions> list = new List<MergeActions>();
			int i = 0;
			int num2 = resampledMasses.Length;
			int num3 = 0;
			int[] array = new int[5000];
			double[] array2 = new double[5000];
			double num4 = intensities[0];
			double num5 = positions[0];
			bool flag = false;
			for (int j = 1; j < num; j++)
			{
				double num6;
				if ((num6 = intensities[j]) == 0.0 && num4 == 0.0)
				{
					if (++j >= num)
					{
						break;
					}
					if ((num6 = intensities[j]) == 0.0)
					{
						if (++j >= num)
						{
							break;
						}
						if ((num6 = intensities[j]) == 0.0)
						{
							if (++j >= num)
							{
								break;
							}
							if ((num6 = intensities[j]) == 0.0)
							{
								num5 = positions[j];
								flag = false;
								continue;
							}
						}
					}
					num5 = positions[j - 1];
					flag = false;
				}
				double num7 = positions[j];
				if (!flag)
				{
					int k;
					for (k = i + 32; k < num2 && resampledMasses[k] < num5; k += 32)
					{
					}
					if (k - 32 > i && k < num2)
					{
						i = ((resampledMasses[k - 16] < num5) ? ((resampledMasses[k - 8] < num5) ? ((!(resampledMasses[k - 4] < num5)) ? (k - 8) : (k - 4)) : ((!(resampledMasses[k - 12] < num5)) ? (k - 16) : (k - 12))) : ((resampledMasses[k - 24] < num5) ? ((!(resampledMasses[k - 20] < num5)) ? (k - 24) : (k - 20)) : ((!(resampledMasses[k - 28] < num5)) ? (k - 32) : (k - 28))));
					}
					for (; i != num2 && resampledMasses[i] < num5; i++)
					{
					}
				}
				while (i != num2 && resampledMasses[i] < num7)
				{
					array2[num3] = (num4 + (num6 - num4) / (num7 - num5) * (resampledMasses[i] - num5)) / (double)scansAveraged;
					array[num3] = i++;
					if (++num3 == 5000)
					{
						list.Add(new MergeActions
						{
							Filed = num3,
							Indexes = array,
							Values = array2
						});
						num3 = 0;
						array = new int[5000];
						array2 = new double[5000];
					}
				}
				num4 = num6;
				num5 = num7;
				flag = true;
			}
			if (num3 > 0)
			{
				list.Add(new MergeActions
				{
					Filed = num3,
					Indexes = array,
					Values = array2
				});
			}
			return list;
		}
	}

	/// <summary>
	/// private class to hold an index/value pair.
	/// This records the data which needs to be added to the re-sampled total.
	/// </summary>
	private class MergeActions
	{
		/// <summary>
		/// Gets or sets Indexes.
		/// </summary>
		public int[] Indexes { get; set; }

		/// <summary>
		/// Gets or sets Values.
		/// </summary>
		public double[] Values { get; set; }

		/// <summary>
		/// Gets or sets Filed.
		/// </summary>
		public int Filed { get; set; }
	}

	/// <summary>
	/// The noise information.
	/// </summary>
	private class NoiseInformation
	{
		/// <summary>
		/// Gets or sets the scan aligned noise.
		/// This is data for each mass in the centroid scan
		/// </summary>
		public NoisePackets ScanAlignedNoise { get; set; }

		/// <summary>
		/// Gets or sets the noise data as recorded in a raw file.
		/// This data need not be aligned with the scan masses.
		/// Values for each mass can be interpolated from this table.
		/// </summary>
		public NoiseAndBaseline[] RawFileNoise { get; set; }
	}

	/// <summary>
	/// Class to obtain noise from a 2 scan: foreground (0) or background (1).
	/// Designed to create a function delegate, needed by
	/// the noise generation algorithm.
	/// </summary>
	private class NoiseFromScans
	{
		private readonly Scan _foreground;

		private readonly Scan _background;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.SpectrumAverager.NoiseFromScans" /> class.
		/// </summary>
		/// <param name="foreground">
		/// The scan to subtract from.
		/// </param>
		/// <param name="background">
		/// The (background) scan to be subtracted.
		/// </param>
		public NoiseFromScans(Scan foreground, Scan background)
		{
			_foreground = foreground;
			_background = background;
		}

		/// <summary>
		/// Get The noise and baselines from a file.
		/// </summary>
		/// <param name="index">
		/// The index.
		/// </param>
		/// <returns>
		/// The Noise data based on the index
		/// </returns>
		public NoiseAndBaseline[] NoiseAndBaselinesFromScan(int index)
		{
			if (index == 0)
			{
				return _foreground.GenerateNoiseTable();
			}
			return _background.GenerateNoiseTable();
		}
	}

	/// <summary>
	/// Class to obtain noise from a file.
	/// Designed to create a function delegate, needed by
	/// the noise generation algorithm.
	/// </summary>
	private class NoiseFromFile
	{
		private readonly IDetectorReaderPlus _rawDataReaderPlus;

		private readonly List<int> _scanNumbers;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.SpectrumAverager.NoiseFromFile" /> class.
		/// </summary>
		/// <param name="rawDataReaderPlus">
		/// The file.
		/// </param>
		/// <param name="scanNumbers">
		/// The scan numbers.
		/// </param>
		public NoiseFromFile(IDetectorReaderPlus rawDataReaderPlus, List<int> scanNumbers)
		{
			_rawDataReaderPlus = rawDataReaderPlus;
			_scanNumbers = scanNumbers;
		}

		/// <summary>
		/// Get The noise and baselines from a file.
		/// </summary>
		/// <param name="index">
		/// The index.
		/// </param>
		/// <returns>
		/// The Noise data based on the index
		/// </returns>
		public NoiseAndBaseline[] NoiseAndBaselinesFromFile(int index)
		{
			return _rawDataReaderPlus.GetAdvancedPacketData(_scanNumbers[index]).NoiseData;
		}
	}

	/// <summary>
	/// The centroid position comparer.
	/// </summary>
	internal class CentroidPositionComparer : IComparer<CentroidStreamPoint>
	{
		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <returns>
		/// Value               Condition 
		/// Less than zero      <paramref name="x" /> is less than <paramref name="y" />.
		/// Zero                <paramref name="x" /> equals <paramref name="y" />.
		/// Greater than zero   <paramref name="x" /> is greater than <paramref name="y" />.
		/// </returns>
		/// <param name="x">
		/// The first object to compare.
		/// </param>
		/// <param name="y">
		/// The second object to compare.
		/// </param>
		public int Compare(CentroidStreamPoint x, CentroidStreamPoint y)
		{
			if (x.Position < y.Position)
			{
				return -1;
			}
			return 1;
		}
	}

	/// <summary>
	/// The max peak samples for detection.
	/// </summary>
	private const int MaxPeakSamplesForDetection = 10;

	/// <summary>
	/// The zero point compression limit.
	/// </summary>
	private const int ZeroPointCompressionLimit = 8;

	/// <summary>
	/// Gets or sets Options which can be used to control the Ft / Orbitrap averaging
	/// </summary>
	public FtAverageOptions FtOptions { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.SpectrumAverager" /> class. 
	/// Default constructor
	/// </summary>
	public SpectrumAverager()
	{
		FtOptions = new FtAverageOptions();
	}

	/// <summary>
	/// Create the average scan from a collection of raw scans.
	/// </summary>
	/// <param name="rawFile">The file containing scans to average.
	/// </param>
	/// <param name="scanStatistics">
	/// The collection of raw scans to average
	/// </param>
	/// <param name="firstScan">
	/// The first scan in the set to average.
	/// </param>
	/// <param name="cacheLimit">
	/// (optional) set the number of items which can be internally cached (default 20)  
	/// </param>
	/// <param name="scanCreator">optional tool to provide scans. 
	/// If this is not supplied, then scan data is read in a single threaded manner using the supplied raw
	/// data interface, and cached as per the supplied cache limits. </param>
	/// <returns>
	/// The averaged scan. 
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">
	/// <c>rawFile</c> is null.
	/// </exception>
	public Scan GetAverage(IDetectorReaderBase rawFile, List<ScanStatistics> scanStatistics, Scan firstScan, int cacheLimit = 20, IScanCreator scanCreator = null)
	{
		if (rawFile == null)
		{
			throw new ArgumentNullException("rawFile");
		}
		if (scanStatistics == null)
		{
			throw new ArgumentNullException("scanStatistics");
		}
		List<int> list = new List<int>(scanStatistics.Count);
		foreach (ScanStatistics scanStatistic in scanStatistics)
		{
			list.Add(scanStatistic.ScanNumber);
		}
		IScanCreator scanCreator2 = scanCreator ?? new ScanFromFileCreator(rawFile);
		scanCreator2.Initialize(list, cacheLimit);
		return GetAverage(rawFile, scanCreator2.CreateSegmentedScan, list, firstScan, scanCreator2.CreateCentroidStream);
	}

	/// <summary>
	/// Create the average scan from a collection of raw scans.
	/// </summary>
	/// <param name="rawFile">The file containing scans to average
	/// </param>
	/// <param name="scanNumbers">
	/// The collection of raw scans to average
	/// </param>
	/// <param name="firstScan">
	/// The first scan in the set to average.
	/// </param>
	/// <param name="cacheLimit">
	/// (optional) set the number of items which can be internally cached (default 20)  
	/// </param>
	/// <returns>
	/// The averaged scan.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">
	/// <c>rawFile</c> is null.
	/// </exception>
	public Scan GetAverage(IRawData rawFile, List<int> scanNumbers, Scan firstScan, int cacheLimit = 20)
	{
		if (rawFile == null)
		{
			throw new ArgumentNullException("rawFile");
		}
		if (scanNumbers == null)
		{
			throw new ArgumentNullException("scanNumbers");
		}
		ScanFromFileCreator scanFromFileCreator = new ScanFromFileCreator(rawFile);
		scanFromFileCreator.Initialize(scanNumbers, cacheLimit);
		return GetAverage(rawFile, scanFromFileCreator.CreateSegmentedScan, scanNumbers, firstScan, scanFromFileCreator.CreateCentroidStream);
	}

	/// <summary>
	/// Create the average scan from a collection of raw scans.
	/// </summary>
	/// <param name="rawDataReader">file which has data to average</param>
	/// <param name="segmentedScanCreator">
	/// method to return segmented scans when requested (by scan list index)
	/// </param>
	/// <param name="scanNumbers">
	/// The collection of raw scans to average
	/// </param>
	/// <param name="firstScan">
	/// The first scan in the set to average.
	/// </param>
	/// <param name="centroidStreamCreator">
	/// method to return centroids streams from the list of scans when requested
	/// </param>
	/// <returns>
	/// The number of scans that were actually averaged.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">
	/// <c>rawFile</c> is null.
	/// </exception>
	private Scan GetAverage(IDetectorReaderBase rawDataReader, FindSegmentedScan segmentedScanCreator, List<int> scanNumbers, Scan firstScan, FindCentroids centroidStreamCreator)
	{
		string paramName = "The scan number list must not be null or empty.";
		if (scanNumbers == null || scanNumbers.Count == 0)
		{
			throw new ArgumentNullException(paramName);
		}
		int count = scanNumbers.Count;
		if (firstScan == null)
		{
			throw new ArgumentNullException("firstScan");
		}
		paramName = "No scans, nothing to do.";
		if (count < 0)
		{
			throw new ArgumentNullException(paramName);
		}
		MassToFrequencyConverter converter;
		if (count == 1)
		{
			firstScan.ScansCombined = 1;
			converter = CalculateTargetSpectrumParameters(firstScan, segmentedScanCreator, count);
			NoiseAndBaseline[] rawFileNoise = null;
			if (rawDataReader is IDetectorReaderPlus rawDataReaderPlus && FtOptions.UseNoiseTableWhenAvailable)
			{
				rawFileNoise = CreateNoiseAndBaselines(new NoiseFromFile(rawDataReaderPlus, scanNumbers).NoiseAndBaselinesFromFile, count);
			}
			return new Scan(converter, rawFileNoise)
			{
				CentroidScan = firstScan.CentroidScan,
				SegmentedScan = firstScan.SegmentedScan,
				ScanStatistics = firstScan.ScanStatistics,
				MassResolution = firstScan.MassResolution,
				IsUserTolerance = firstScan.IsUserTolerance,
				ToleranceUnit = firstScan.ToleranceUnit,
				ScanType = firstScan.ScanType
			};
		}
		ProfileData segmentList = CreateTargetSpectrum(firstScan, segmentedScanCreator, count, out converter);
		segmentList = ((!FtOptions.MergeInParallel) ? SumUpSpectraResampled(segmentedScanCreator, segmentList, count) : SumUpSpectraResampledParallel2(segmentedScanCreator, segmentList, count, FtOptions.MergeTaskBatching));
		NoiseInformation noiseInformation;
		List<CentroidStreamPoint> centroidStream;
		if (FtOptions.MaxChargeDeterminations == 0)
		{
			segmentList = CompressProfileSpectrum(segmentList);
			centroidStream = CentroidizeProfileSpectrum(segmentList);
			noiseInformation = CalculateNoiseInfo(rawDataReader, centroidStreamCreator, centroidStream, scanNumbers, FtOptions.UseNoiseTableWhenAvailable);
		}
		else
		{
			centroidStream = CentroidizeProfileSpectrum(segmentList);
			noiseInformation = CalculateNoiseInfo(rawDataReader, centroidStreamCreator, centroidStream, scanNumbers, FtOptions.UseNoiseTableWhenAvailable);
			centroidStream = CalculateAndAssignChargeStates(segmentList, centroidStream);
			segmentList = CompressProfileSpectrum(segmentList);
		}
		Scan scan = FillAverageScan(firstScan, segmentList, centroidStream, noiseInformation, converter);
		scan.ScansCombined = scanNumbers.Count;
		return scan;
	}

	/// <summary>
	/// Creates a difference of two raw scans.
	/// The returned scans count of "scans combined" is
	/// the total number of scans combined in each of the
	/// foreground and background scans.
	/// </summary>
	/// <param name="foregroundScan">
	/// The scan containing signal.
	/// </param>
	/// <param name="backgroundScan">
	/// The scan containing background
	/// </param>
	/// <returns>
	/// foregroundScan - backgroundScan
	/// </returns>
	public Scan Subtract(Scan foregroundScan, Scan backgroundScan)
	{
		if (foregroundScan == null || backgroundScan == null)
		{
			throw new InvalidOperationException("The foreground (or) background scans must not be NULL.");
		}
		List<Scan> scanList = new List<Scan> { foregroundScan, backgroundScan };
		ScanFromListCreator scanFromListCreator = new ScanFromListCreator(scanList);
		ProfileData segmentList = CreateTargetSpectrum(foregroundScan, scanFromListCreator.CreateSegmentedScan, 2, out var massToFrequencyConverter);
		segmentList = SubtractSpectraResampled(scanList, segmentList);
		List<CentroidStreamPoint> list = CentroidizeProfileSpectrum(segmentList);
		List<int> list2 = new List<int>(2)
		{
			foregroundScan.ScanStatistics.ScanNumber,
			backgroundScan.ScanStatistics.ScanNumber
		};
		NoiseInformation noiseInformation;
		if (foregroundScan.HasNoiseTable && backgroundScan.HasNoiseTable && FtOptions.UseNoiseTableWhenAvailable)
		{
			NoiseAndBaseline[] array = CreateNoiseAndBaselines(scans: list2.Count, noiseReader: new NoiseFromScans(foregroundScan, backgroundScan).NoiseAndBaselinesFromScan);
			NoisePackets scanAlignedNoise = BuildNoiseDataForScan(list, array);
			noiseInformation = new NoiseInformation
			{
				ScanAlignedNoise = scanAlignedNoise,
				RawFileNoise = array
			};
		}
		else
		{
			noiseInformation = CalculateNoiseInfo(scanFromListCreator.CreateCentroidStream, list, list2);
		}
		list = CalculateAndAssignChargeStates(segmentList, list);
		segmentList = CompressProfileSpectrum(segmentList);
		Scan scan = FillAverageScan(foregroundScan, segmentList, list, noiseInformation, massToFrequencyConverter);
		scan.ScansCombined = Math.Max(1, foregroundScan.ScansCombined) + Math.Max(1, backgroundScan.ScansCombined);
		return scan;
	}

	/// <summary>
	/// Creates a sum of two scans.
	/// The returned scans count of "scans combined" is
	/// the total number of scans combined in each of the
	/// first and second scans.
	/// </summary>
	/// <param name="firstScan">
	/// The first scan.
	/// </param>
	/// <param name="secondScan">
	/// The second scan
	/// </param>
	/// <returns>
	/// first scan + second scan
	/// </returns>
	public Scan Add(Scan firstScan, Scan secondScan)
	{
		if (firstScan == null)
		{
			throw new ArgumentNullException("firstScan");
		}
		if (secondScan == null)
		{
			throw new ArgumentNullException("secondScan");
		}
		List<Scan> scanList = new List<Scan> { firstScan, secondScan };
		ScanFromListCreator scanFromListCreator = new ScanFromListCreator(scanList);
		ProfileData segmentList = CreateTargetSpectrum(firstScan, scanFromListCreator.CreateSegmentedScan, 2, out var massToFrequencyConverter);
		segmentList = SumSpectraResampled(scanList, segmentList);
		List<CentroidStreamPoint> list = CentroidizeProfileSpectrum(segmentList);
		List<int> list2 = new List<int>(2)
		{
			firstScan.ScanStatistics.ScanNumber,
			secondScan.ScanStatistics.ScanNumber
		};
		NoiseInformation noiseInformation;
		if (firstScan.HasNoiseTable && secondScan.HasNoiseTable && FtOptions.UseNoiseTableWhenAvailable)
		{
			NoiseAndBaseline[] array = CreateNoiseAndBaselines(scans: list2.Count, noiseReader: new NoiseFromScans(firstScan, secondScan).NoiseAndBaselinesFromScan);
			NoisePackets scanAlignedNoise = BuildNoiseDataForScan(list, array);
			noiseInformation = new NoiseInformation
			{
				ScanAlignedNoise = scanAlignedNoise,
				RawFileNoise = array
			};
		}
		else
		{
			noiseInformation = CalculateNoiseInfo(scanFromListCreator.CreateCentroidStream, list, list2);
		}
		list = CalculateAndAssignChargeStates(segmentList, list);
		segmentList = CompressProfileSpectrum(segmentList);
		Scan scan = FillAverageScan(firstScan, segmentList, list, noiseInformation, massToFrequencyConverter);
		scan.ScansCombined = Math.Max(1, firstScan.ScansCombined) + Math.Max(1, secondScan.ScansCombined);
		return scan;
	}

	/// <summary>
	/// Create the target array for keeping the averaged profile points.
	/// </summary>
	/// <param name="firstScan">
	/// The first Scan.
	/// </param>
	/// <param name="findSegmentedScan">
	/// The find Segmented Scan.
	/// </param>
	/// <param name="scans">
	/// The scans.
	/// </param>
	/// <param name="massToFrequencyConverter">
	/// The target Spectrum Parameters.
	/// </param>
	/// <returns>
	/// The created target spectrum.
	/// </returns>
	private static ProfileData CreateTargetSpectrum(Scan firstScan, FindSegmentedScan findSegmentedScan, int scans, out MassToFrequencyConverter massToFrequencyConverter)
	{
		massToFrequencyConverter = CalculateTargetSpectrumParameters(firstScan, findSegmentedScan, scans);
		return CreateProfilePoints(CreateMassTable(massToFrequencyConverter));
	}

	/// <summary>
	/// create profile points.
	/// </summary>
	/// <param name="massTable">
	/// The mass table.
	/// </param>
	/// <returns>
	/// Points containing masses and 0 intensity
	/// </returns>
	private static ProfileData CreateProfilePoints(List<PartProfileMasses> massTable)
	{
		ProfileData profileData = new ProfileData(massTable.Sum((PartProfileMasses partProfileMassese) => partProfileMassese.Filled));
		double[] masses = profileData.Masses;
		int num = 0;
		foreach (PartProfileMasses item in massTable)
		{
			int filled = item.Filled;
			Array.Copy(item.Mass, 0, masses, num, filled);
			num += filled;
		}
		return profileData;
	}

	/// <summary>
	/// The create mass table.
	/// </summary>
	/// <param name="massToFrequencyConverter">
	/// The target spectrum parameters.
	/// </param>
	/// <returns>
	/// The masses for each profile point.
	/// </returns>
	private static List<PartProfileMasses> CreateMassTable(MassToFrequencyConverter massToFrequencyConverter)
	{
		List<PartProfileMasses> list = new List<PartProfileMasses>(10);
		int num = 0;
		int num2 = 0;
		double num3 = massToFrequencyConverter.ConvertFrequenceToMass(0);
		double highestMass = massToFrequencyConverter.HighestMass;
		double[] array = new double[2000];
		double baseFrequency = massToFrequencyConverter.BaseFrequency;
		double num4 = massToFrequencyConverter.BaseFrequency;
		double deltaFrequency = massToFrequencyConverter.DeltaFrequency;
		double coefficient = massToFrequencyConverter.Coefficient1;
		double coefficient2 = massToFrequencyConverter.Coefficient2;
		double coefficient3 = massToFrequencyConverter.Coefficient3;
		if (coefficient != 0.0)
		{
			while (num3 <= highestMass)
			{
				if (num == 2000)
				{
					list.Add(new PartProfileMasses
					{
						Filled = num,
						Mass = array
					});
					num2 += num;
					num = 0;
					array = new double[2000];
				}
				array[num++] = num3;
				num4 -= deltaFrequency;
				double num5 = num4 * num4;
				num3 = coefficient / num4 + coefficient2 / num5 + coefficient3 / (num5 * num5);
			}
		}
		else
		{
			while (num3 <= highestMass)
			{
				if (num >= 1997)
				{
					list.Add(new PartProfileMasses
					{
						Filled = num,
						Mass = array
					});
					num2 += num;
					num = 0;
					array = new double[2000];
				}
				array[num] = num3;
				num4 = baseFrequency - deltaFrequency * (double)(num2 + num + 1);
				double num6 = num4 * num4;
				array[num + 1] = coefficient2 / num6 + coefficient3 / (num6 * num6);
				num4 -= deltaFrequency;
				num6 = num4 * num4;
				array[num + 2] = coefficient2 / num6 + coefficient3 / (num6 * num6);
				num4 -= deltaFrequency;
				num6 = num4 * num4;
				array[num + 3] = coefficient2 / num6 + coefficient3 / (num6 * num6);
				num4 -= deltaFrequency;
				num6 = num4 * num4;
				num3 = coefficient2 / num6 + coefficient3 / (num6 * num6);
				num += 4;
			}
		}
		if (num >= 4)
		{
			if (array[num - 1] > highestMass)
			{
				num--;
			}
			if (array[num - 1] > highestMass)
			{
				num--;
			}
			if (array[num - 1] > highestMass)
			{
				num--;
			}
			list.Add(new PartProfileMasses
			{
				Filled = num,
				Mass = array
			});
		}
		return list;
	}

	/// <summary>
	/// Create the average profile spectrum.
	/// </summary>
	/// <param name="scanCreator">
	/// tool to return scans
	/// </param>
	/// <param name="segmentList">
	/// segment List
	/// </param>
	/// <param name="scans">
	/// The number of scans to sum
	/// </param>
	/// <returns>
	/// The summed up spectra.
	/// </returns>
	private static ProfileData SumUpSpectraResampled(FindSegmentedScan scanCreator, ProfileData segmentList, int scans)
	{
		double[] masses = segmentList.Masses;
		double[] intensities = segmentList.Intensities;
		for (int i = 0; i < scans; i++)
		{
			SegmentedScan segmentedScan = scanCreator(i);
			double[] intensities2 = segmentedScan.Intensities;
			double[] positions = segmentedScan.Positions;
			int num = intensities2.Length;
			if (intensities2.Length == 0)
			{
				continue;
			}
			int j = 0;
			int length = segmentList.Length;
			double num2 = intensities2[0];
			for (int k = 1; k < num; k++)
			{
				double num3;
				if ((num3 = intensities2[k]) == 0.0 && num2 == 0.0)
				{
					continue;
				}
				double num4 = positions[k];
				double num5 = positions[k - 1];
				int l;
				for (l = j + 10; l < length && masses[l] < num5; l += 10)
				{
				}
				if (l < length)
				{
					l -= 10;
					if (l > j)
					{
						j = l;
					}
				}
				for (; j != length && masses[j] < num5; j++)
				{
				}
				double num6 = (num3 - num2) / (num4 - num5);
				double num7;
				while (j != length && (num7 = masses[j]) < num4)
				{
					intensities[j++] += (num2 + num6 * (num7 - num5)) / (double)scans;
				}
				num2 = num3;
			}
		}
		return segmentList;
	}

	/// <summary>
	/// Create the average profile spectrum.
	/// Each scan is analyzed: Determining mass regions which contain non-zero data,
	/// and re-sampling the intensity data aligned to a set of output bins.
	/// After all scans have been re-sampled, the re-sampled data has to be merged into the final output.
	/// </summary>
	/// <param name="scanCreator">
	/// tool to return scans
	/// </param>
	/// <param name="segmentList">
	/// segment List
	/// </param>
	/// <param name="scans">
	/// The number of scans to sum
	/// </param>
	/// <param name="taskBatching">
	/// The minimum number of Resample tasks per thread.
	/// Creating resampled data for profiles is a fairly fast task. It may be inefficient to queue workers to
	/// created the merged data for each scan in the batch.
	/// Setting this &gt;1 will reduce threading overheads, when averaging small batches of scans with low intensity peaks.
	/// This parameter only affects the re-sampling, as the final merge of the re-sampled data is single threaded.
	/// </param>
	/// <returns>
	/// The re-sampled and summed up spectra.
	/// </returns>
	private static ProfileData SumUpSpectraResampledParallel2(FindSegmentedScan scanCreator, ProfileData segmentList, int scans, int taskBatching)
	{
		List<Resampler> list = new List<Resampler>();
		int i = 0;
		while (i < scans)
		{
			list.Clear();
			for (; i < scans; i++)
			{
				SegmentedScan scanData = scanCreator(i);
				list.Add(new Resampler(segmentList.Masses, scanData, scans));
			}
			ParallelSmallTasks parallelSmallTasks = new ParallelSmallTasks();
			parallelSmallTasks.BatchSize = taskBatching;
			parallelSmallTasks.RunInParallel(list.ToArray());
			foreach (Resampler item in list)
			{
				foreach (MergeActions item2 in item.Result)
				{
					MergeData(item2, segmentList);
				}
			}
		}
		return segmentList;
	}

	/// <summary>
	/// The merge data.
	/// </summary>
	/// <param name="toMerge">
	/// The to merge.
	/// </param>
	/// <param name="resampled">
	/// The resampled.
	/// </param>
	private static void MergeData(MergeActions toMerge, ProfileData resampled)
	{
		double[] intensities = resampled.Intensities;
		int filed = toMerge.Filed;
		int[] indexes = toMerge.Indexes;
		double[] values = toMerge.Values;
		int num = filed / 10;
		int i = 0;
		for (int j = 0; j < num; j++)
		{
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
			intensities[indexes[i]] += values[i++];
		}
		for (; i < filed; i++)
		{
			intensities[indexes[i]] += values[i];
		}
	}

	/// <summary>
	/// Create the Subtracted profile spectrum.
	/// </summary>
	/// <param name="scanList">
	/// The foreground and background scans
	/// </param>
	/// <param name="segmentList">The re-sampled mass and intensity arrays.
	/// </param>
	/// <returns>
	/// The subtracted profile
	/// </returns>
	private static ProfileData SubtractSpectraResampled(List<Scan> scanList, ProfileData segmentList)
	{
		double[] masses = segmentList.Masses;
		double[] intensities = segmentList.Intensities;
		for (int i = 0; i <= 1; i++)
		{
			Scan scan = scanList[i];
			int num = scan.SegmentedScan.Intensities.Length;
			SegmentedScan segmentedScan = scan.SegmentedScan;
			int j = 0;
			int length = segmentList.Length;
			for (int k = 1; k < num; k++)
			{
				if (segmentedScan.Intensities[k - 1] == 0.0 && segmentedScan.Intensities[k] == 0.0)
				{
					continue;
				}
				for (; j != length && masses[j] < segmentedScan.Positions[k - 1]; j++)
				{
				}
				for (; j != length && masses[j] < segmentedScan.Positions[k]; j++)
				{
					double num2 = segmentedScan.Intensities[k - 1] + (segmentedScan.Intensities[k] - segmentedScan.Intensities[k - 1]) / (segmentedScan.Positions[k] - segmentedScan.Positions[k - 1]) * (masses[j] - segmentedScan.Positions[k - 1]);
					if (i == 0)
					{
						intensities[j] += num2;
					}
					else
					{
						intensities[j] -= num2;
					}
				}
			}
		}
		for (int l = 0; l < segmentList.Length; l++)
		{
			if (intensities[l] < 0.0)
			{
				intensities[l] = 0.0;
			}
		}
		return segmentList;
	}

	/// <summary>
	/// Create the Summed profile spectrum, for the "Add" method
	/// </summary>
	/// <param name="scanList">
	/// The scans to add
	/// </param>
	/// <param name="segmentList">The re-sampled mass and intensity arrays.
	/// </param>
	/// <returns>
	/// The subtracted profile
	/// </returns>
	private static ProfileData SumSpectraResampled(List<Scan> scanList, ProfileData segmentList)
	{
		double[] masses = segmentList.Masses;
		double[] intensities = segmentList.Intensities;
		for (int i = 0; i <= 1; i++)
		{
			Scan scan = scanList[i];
			int num = scan.SegmentedScan.Intensities.Length;
			SegmentedScan segmentedScan = scan.SegmentedScan;
			int j = 0;
			int length = segmentList.Length;
			for (int k = 1; k < num; k++)
			{
				if (segmentedScan.Intensities[k - 1] != 0.0 || segmentedScan.Intensities[k] != 0.0)
				{
					for (; j != length && masses[j] < segmentedScan.Positions[k - 1]; j++)
					{
					}
					for (; j != length && masses[j] < segmentedScan.Positions[k]; j++)
					{
						double num2 = segmentedScan.Intensities[k - 1] + (segmentedScan.Intensities[k] - segmentedScan.Intensities[k - 1]) / (segmentedScan.Positions[k] - segmentedScan.Positions[k - 1]) * (masses[j] - segmentedScan.Positions[k - 1]);
						intensities[j] += num2;
					}
				}
			}
		}
		for (int l = 0; l < segmentList.Length; l++)
		{
			if (intensities[l] < 0.0)
			{
				intensities[l] = 0.0;
			}
		}
		return segmentList;
	}

	/// <summary>
	/// Determines the centroids of all peaks in the given spectrum.
	/// <para>
	/// For the centroid the position, intensity and resolution are calculated.
	/// The resolution is the calculated as the quotient of the position and the 
	/// width of the peak.
	/// </para>
	/// <para>
	/// The following algorithm calculates the centroid as the vertex of a second 
	/// order parabola laid through the top three points of a peak. If the width 
	/// cannot be determined directly from the profile peak (e.g. because of 
	/// overlapping peaks), the full width at half height (FWHH) of the parabola
	/// fitted to determine the centroid position is used (see below). This width
	/// is usually smaller than the real peak width for FT data.
	/// </para>
	/// <para>
	/// This algorithm simplifies the calculation by moving the x coordinate of the 
	/// top three points to -1, 0 and +1, respectively. This is only applicable if 
	/// the points are equidistant, which would be exactly true for the spectrum
	/// in frequency domain, and is a valid approximation for the spectrum in mass
	/// domain.
	/// </para>
	/// <code>
	/// Parabola: y = a(x-xv)^2 + yv
	///
	/// 1. Find maximum and two surrounding points:
	///    Given  p1(x1,y1); p2(x2,y2); p3(x3,y3) 
	///
	///    if     dx = x2 - x1 = x3 - x2
	///    and    x2 = n * dx
	///
	/// 2. Map the three points to the unity interval [-1, 1]:
	///    p1'(-1,y1);p2'(0,y2);p3'(1,y3) - This simplifies the calculations.
	///
	/// 3. Calculate center offset dxc', height yc, and width w'
	///    (dxc' and w' are in unity units):
	///
	///    a    = 0.5 * (y1+y3) - y2;
	///    dxc' = (y1-y3) / 4a
	///    yc   = y2 - a * dxc'^2
	///    w'   = 2.0 * sqrt(yc/2|a|)
	///
	/// 4. Calculate center and width:
	///    xc = x2 + dxc' * dx
	///    w  = w' * dx
	/// </code>
	/// </summary>
	/// <param name="segmentScan">
	/// List of segments to be converted to centroids
	/// </param>
	/// <returns>
	/// The centroids detected.
	/// </returns>
	private static List<CentroidStreamPoint> CentroidizeProfileSpectrum(ProfileData segmentScan)
	{
		if (segmentScan.Length < 3)
		{
			return new List<CentroidStreamPoint>();
		}
		double[] masses = segmentScan.Masses;
		double[] intensities = segmentScan.Intensities;
		List<CentroidStreamPoint> list = new List<CentroidStreamPoint>();
		int num = 0;
		int num2 = 1;
		int num3 = 2;
		double num4 = intensities[num2];
		double num5 = intensities[num];
		int num6 = segmentScan.Length - 1;
		while (num2 != num6)
		{
			double num7 = intensities[num3];
			if (num4 > num5 && num4 > num7)
			{
				double num8 = masses[num];
				double num9 = masses[num3];
				double num10 = 0.5 * (num9 - num8);
				double num11 = 0.5 * (num5 + num7) - num4;
				double num12 = 0.25 * (num5 - num7) / num11;
				double num13 = num4 - num11 * num12 * num12;
				double num14 = 2.0 * Math.Sqrt(num13 / (-2.0 * num11));
				double num15 = 0.0;
				double num16 = 0.0;
				double num17 = 0.5 * num4;
				int num18 = num2;
				for (int i = 1; i < 10; i++)
				{
					int num19 = num18 - i;
					double num20;
					if (num15 > 0.0 || num18 < i || (num20 = intensities[num19]) > intensities[num19 + 1])
					{
						break;
					}
					if (num20 < num17)
					{
						double num21 = masses[num19];
						num15 += masses[num18] - num21 - (masses[num19 + 1] - num21) * (num17 - num20) / (intensities[num19 + 1] - num20);
					}
				}
				num18 = num2;
				for (int j = 1; j < 10; j++)
				{
					int num22 = num18 + j;
					double num23;
					if (num16 > 0.0 || segmentScan.Length - num18 <= j || (num23 = intensities[num22]) > intensities[num22 - 1])
					{
						break;
					}
					if (num23 < num17)
					{
						double num24 = masses[num22 - 1];
						double num25 = intensities[num22 - 1];
						num16 += num24 - masses[num18] + (masses[num22] - num24) * (num17 - num25) / (num23 - num25);
					}
				}
				double num26 = ((num15 > 0.0 && num16 > 0.0) ? 0.0 : ((num15 > 0.0 || num16 > 0.0) ? 0.5 : 1.0));
				double num27 = masses[num2] + num12 * num10;
				list.Add(new CentroidStreamPoint
				{
					Position = num27,
					Intensity = num13,
					Resolution = num27 / (num15 + num16 + num26 * num14 * num10)
				});
			}
			num = num2;
			num2 = num3;
			num3++;
			num5 = num4;
			num4 = num7;
		}
		return list;
	}

	/// <summary>
	/// Recalculates the baseline and noise information for the averaged spectrum.
	/// </summary>
	/// <param name="rawData">Raw file reader</param>
	/// <param name="scanCreator">
	/// The scan Creator.
	/// </param>
	/// <param name="centroidStream">
	/// The centroid Stream.
	/// </param>
	/// <param name="scanNumbers">
	/// The scans.
	/// </param>
	/// <param name="useOriginalNoiseAlgorithm">If true, use noise algorithm from Xcalibur</param>
	/// <returns>
	/// The calculated noise packets.
	/// </returns>
	private static NoiseInformation CalculateNoiseInfo(IDetectorReaderBase rawData, FindCentroids scanCreator, List<CentroidStreamPoint> centroidStream, List<int> scanNumbers, bool useOriginalNoiseAlgorithm)
	{
		if (useOriginalNoiseAlgorithm && rawData is IDetectorReaderPlus filePlus)
		{
			return CalculateNoiseInfoUsingNoiseTable(filePlus, centroidStream, scanNumbers);
		}
		return CalculateNoiseInfo(scanCreator, centroidStream, scanNumbers);
	}

	/// <summary>
	/// Recalculates the baseline and noise information for the averaged spectrum.
	/// </summary>
	/// <param name="scanCreator">
	/// The scan Creator.
	/// </param>
	/// <param name="centroidStream">
	/// The centroid Stream.
	/// </param>
	/// <param name="scanNumbers">
	/// The scans.
	/// </param>
	/// <returns>
	/// The calculated noise packets.
	/// </returns>
	private static NoiseInformation CalculateNoiseInfo(FindCentroids scanCreator, List<CentroidStreamPoint> centroidStream, List<int> scanNumbers)
	{
		int count = scanNumbers.Count;
		int count2 = centroidStream.Count;
		double[] array = new double[count2];
		double[] array2 = new double[count2];
		double[] array3 = new double[count2];
		for (int i = 0; i < count2; i++)
		{
			array[i] = centroidStream[i].Position;
		}
		for (int j = 0; j < count; j++)
		{
			CentroidStream centroidStream2 = scanCreator(j);
			int length = centroidStream2.Length;
			if (length <= 0)
			{
				continue;
			}
			double[] masses = centroidStream2.Masses;
			double[] baselines = centroidStream2.Baselines;
			double[] noises = centroidStream2.Noises;
			int k;
			for (k = 0; k < count2 && !(array[k] > masses[0]); k++)
			{
				array3[k] += baselines[0];
				array2[k] += noises[0];
			}
			double num = masses[0];
			double num2 = noises[0];
			double num3 = baselines[0];
			for (int l = 1; l < length; l++)
			{
				double num5;
				double num4 = (num5 = masses[l]) - num;
				double num7;
				double num6 = ((num7 = noises[l]) - num2) / num4;
				double num8 = num2 - num6 * num;
				double num10;
				double num9 = ((num10 = baselines[l]) - num3) / num4;
				double num11 = num3 - num9 * num;
				double num12;
				while (k < count2 && (num12 = array[k]) <= num5)
				{
					array2[k] += num6 * num12 + num8;
					array3[k++] += num9 * num12 + num11;
				}
				num = num5;
				num2 = num7;
				num3 = num10;
			}
			while (k < count2)
			{
				array2[k] += noises[length - 1];
				array3[k++] += baselines[length - 1];
			}
		}
		double num13 = Math.Sqrt(count);
		for (int m = 0; m < count2; m++)
		{
			double num14 = (array3[m] /= count);
			array2[m] = (array2[m] / (double)count - num14) / num13 + num14;
		}
		return new NoiseInformation
		{
			ScanAlignedNoise = new NoisePackets
			{
				BaseLine = array3,
				Mass = array,
				Noise = array2
			}
		};
	}

	/// <summary>
	/// Recalculates the baseline and noise information for the averaged spectrum.
	/// </summary>
	/// <param name="filePlus">Tool to read noise data</param>
	/// <param name="centroidStream">
	/// The centroid Stream.
	/// </param>
	/// <param name="scanNumbers">
	/// The scans.
	/// </param>
	/// <returns>
	/// The calculated noise packets.
	/// </returns>
	private static NoiseInformation CalculateNoiseInfoUsingNoiseTable(IDetectorReaderPlus filePlus, List<CentroidStreamPoint> centroidStream, List<int> scanNumbers)
	{
		int count = scanNumbers.Count;
		NoiseAndBaseline[] array = CreateNoiseAndBaselines(new NoiseFromFile(filePlus, scanNumbers).NoiseAndBaselinesFromFile, count);
		NoisePackets scanAlignedNoise = BuildNoiseDataForScan(centroidStream, array);
		return new NoiseInformation
		{
			ScanAlignedNoise = scanAlignedNoise,
			RawFileNoise = array
		};
	}

	/// <summary>
	/// build noise data for scan.
	/// Applies the noise and baseline data to a scan, making a noise table.
	/// </summary>
	/// <param name="centroidStream">
	/// The centroid stream.
	/// </param>
	/// <param name="noiseAndBaselines">
	/// The noise and baselines (input).
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.NoisePackets" />.
	/// </returns>
	private static NoisePackets BuildNoiseDataForScan(List<CentroidStreamPoint> centroidStream, NoiseAndBaseline[] noiseAndBaselines)
	{
		int count = centroidStream.Count;
		double[] array = new double[count];
		double[] array2 = new double[count];
		double[] array3 = new double[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = centroidStream[i].Position;
		}
		if (count > 0 && noiseAndBaselines.Length != 0)
		{
			NoiseAndBaseline noiseAndBaseline = noiseAndBaselines[0];
			float noise = noiseAndBaseline.Noise;
			float baseline = noiseAndBaseline.Baseline;
			float mass = noiseAndBaseline.Mass;
			int num = count;
			int j;
			for (j = 0; j < num && !(array[j] > (double)mass); j++)
			{
				array2[j] = noise;
				array3[j] = baseline;
			}
			NoiseAndBaseline noiseAndBaseline2 = noiseAndBaseline;
			for (int k = 1; k < noiseAndBaselines.Length; k++)
			{
				NoiseAndBaseline noiseAndBaseline3 = noiseAndBaselines[k];
				double slope;
				double num2 = InterpolateValues(noiseAndBaseline3.Noise, noiseAndBaseline2.Noise, noiseAndBaseline3.Mass, noiseAndBaseline2.Mass, out slope);
				double slope2;
				double num3 = InterpolateValues(noiseAndBaseline3.Baseline, noiseAndBaseline2.Baseline, noiseAndBaseline3.Mass, noiseAndBaseline2.Mass, out slope2);
				double num4 = noiseAndBaseline3.Mass;
				double num5;
				while (j < num && (num5 = array[j]) <= num4)
				{
					array2[j] = slope * num5 + num2;
					array3[j++] = slope2 * num5 + num3;
				}
				noiseAndBaseline2 = noiseAndBaseline3;
			}
			NoiseAndBaseline noiseAndBaseline4 = noiseAndBaselines[^1];
			while (j < num)
			{
				array2[j] = noiseAndBaseline4.Noise;
				array3[j++] = noiseAndBaseline4.Baseline;
			}
		}
		return new NoisePackets
		{
			BaseLine = array3,
			Mass = array,
			Noise = array2
		};
	}

	/// <summary>
	/// create noise and baselines.
	/// </summary>
	/// <param name="noiseReader">
	/// The noise Reader.
	/// </param>
	/// <param name="scans">
	/// The scans.
	/// </param>
	/// <returns>
	/// The noise and baselines (averaged)
	/// </returns>
	private static NoiseAndBaseline[] CreateNoiseAndBaselines(Func<int, NoiseAndBaseline[]> noiseReader, int scans)
	{
		NoiseAndBaseline[] array = Array.Empty<NoiseAndBaseline>();
		int i;
		for (i = 0; i < scans; i++)
		{
			NoiseAndBaseline[] array2 = noiseReader(i);
			if (array2.Length > 0)
			{
				array = array2.ToArray();
				break;
			}
		}
		while (++i < scans)
		{
			NoiseAndBaseline[] array3 = noiseReader(i);
			int num = array3.Length;
			if (num == 0)
			{
				continue;
			}
			int j;
			for (j = 0; j < array.Length; j++)
			{
				NoiseAndBaseline noiseAndBaseline = array3[0];
				NoiseAndBaseline noiseAndBaseline2 = array[j];
				if (noiseAndBaseline2.Mass > noiseAndBaseline.Mass)
				{
					break;
				}
				noiseAndBaseline2.Noise += noiseAndBaseline.Noise;
				noiseAndBaseline2.Baseline += noiseAndBaseline.Baseline;
			}
			for (int k = 1; k < num; k++)
			{
				NoiseAndBaseline noiseAndBaseline3 = array3[k - 1];
				NoiseAndBaseline noiseAndBaseline4 = array3[k];
				double num2 = (noiseAndBaseline4.Noise - noiseAndBaseline3.Noise) / (noiseAndBaseline4.Mass - noiseAndBaseline3.Mass);
				double num3 = (double)noiseAndBaseline3.Noise - num2 * (double)noiseAndBaseline3.Mass;
				double num4 = (noiseAndBaseline4.Baseline - noiseAndBaseline3.Baseline) / (noiseAndBaseline4.Mass - noiseAndBaseline3.Mass);
				double num5 = (double)noiseAndBaseline3.Baseline - num4 * (double)noiseAndBaseline3.Mass;
				for (; j < array.Length && array[j].Mass <= noiseAndBaseline4.Mass; j++)
				{
					array[j].Noise += (float)(num2 * (double)array[j].Mass + num3);
					array[j].Baseline += (float)(num4 * (double)array[j].Mass + num5);
				}
			}
			NoiseAndBaseline noiseAndBaseline5 = array3[num - 1];
			for (; j < array.Length; j++)
			{
				array[j].Noise += noiseAndBaseline5.Noise;
				array[j].Baseline += noiseAndBaseline5.Baseline;
			}
		}
		float num6 = (float)Math.Sqrt(scans);
		NoiseAndBaseline[] array4 = array;
		foreach (NoiseAndBaseline obj in array4)
		{
			float num7 = (obj.Baseline /= scans);
			obj.Noise = (obj.Noise / (float)scans - num7) / num6 + num7;
		}
		return array;
	}

	/// <summary>
	/// Interpolate noise or baseline value.
	/// </summary>
	/// <param name="currentValue">
	/// The current value.
	/// </param>
	/// <param name="previousValue">
	/// The previous value.
	/// </param>
	/// <param name="currentMass">
	/// The current mass.
	/// </param>
	/// <param name="previousMass">
	/// The previous mass.
	/// </param>
	/// <param name="slope">
	/// The slope.
	/// </param>
	/// <returns>
	/// The interpolated value.
	/// </returns>
	private static double InterpolateValues(double currentValue, double previousValue, double currentMass, double previousMass, out double slope)
	{
		slope = (currentValue - previousValue) / (currentMass - previousMass);
		return previousValue - slope * previousMass;
	}

	/// <summary>
	/// The function determines the charge state of the peaks in    
	/// the spectrum and assign the calculated charge to the 
	/// centroids of the corresponding isotopic cluster.
	/// <para>
	/// The functions tries to determine the charge states of the peaks in the 
	/// spectrum and to assign the calculated charges to the centroids of the 
	/// corresponding isotopic clusters in the spectrum.
	/// The algorithm tries to determine charges for the top peaks in the spectrum. 
	/// The algorithm tries at most MaxChargeDeterminations, or until the peak 
	/// intensity falls below a calculated noise level in the spectrum, whichever 
	/// comes first.
	/// </para>
	/// <para>
	/// The charge determination is done by going through the spectrum starting with
	/// the most intense peak. The calculation is done on a small interval around the 
	/// peak centroid ([centroid - 1.5 Da/1200.0, centroid + 1.5 Da]).
	/// Two independent approaches are used for the charge calculation: 
	/// </para>
	/// <code>
	/// 1. Calculation of an FFT on the profile points, and
	/// 2. Calculation by considering the centroid distances (Patterson charge 
	///    calculations).
	/// </code>
	/// <para>
	/// The two charge calculation algorithms fill a charge histogram map. This 
	/// histogram is then analyzed for exhibiting a top scored charge that has a 
	/// clear separation from the second best hit.
	/// If there is a clear best hit for the charge, this charge is assigned to the 
	/// corresponding isotopic cluster. This is the most difficult part of the charge
	/// assignment. For details how the isotopic cluster is identified see the 
	/// description on the charge assignment routine (-&gt; AssignCharge()).
	/// </para>
	/// </summary>
	/// <param name="segmentList">
	/// The profile for the scan.
	/// </param>
	/// <param name="massSortedCentroidList">
	/// List of centroids to assign charges to
	/// </param>
	/// <returns>
	/// The points with charge states.
	/// </returns>
	private List<CentroidStreamPoint> CalculateAndAssignChargeStates(ProfileData segmentList, List<CentroidStreamPoint> massSortedCentroidList)
	{
		int num = 0;
		foreach (CentroidStreamPoint massSortedCentroid in massSortedCentroidList)
		{
			massSortedCentroid.Index = num++;
		}
		if (FtOptions.MaxChargeDeterminations == 0)
		{
			return massSortedCentroidList;
		}
		List<CentroidStreamPoint> range = massSortedCentroidList.GetRange(0, massSortedCentroidList.Count);
		range.Sort(CompareIntenisities);
		int num2 = 0;
		CentroidPositionComparer centroidPositionComparer = new CentroidPositionComparer();
		PeakChargeCalculator peakChargeCalculator = new PeakChargeCalculator();
		for (int i = 0; i < range.Count; i++)
		{
			if (range[i].Charge == 0)
			{
				if (++num2 > FtOptions.MaxChargeDeterminations)
				{
					break;
				}
				ChargeResult result = peakChargeCalculator.CalculateChargeForPeak(segmentList, massSortedCentroidList, centroidPositionComparer, range[i]);
				AssignCharge(result, massSortedCentroidList);
			}
		}
		return massSortedCentroidList;
	}

	/// <summary>
	/// assign charge.
	/// </summary>
	/// <param name="result">
	/// The result of charge calculation.
	/// </param>
	/// <param name="massSortedCentroidList">
	/// The mass sorted centroid list.
	/// </param>
	private void AssignCharge(ChargeResult result, List<CentroidStreamPoint> massSortedCentroidList)
	{
		List<int> isotopes = result.Isotopes;
		int charge = result.Charge;
		if (isotopes != null && ((charge < 3 && isotopes.Count >= 2) || (charge >= 3 && isotopes.Count >= 3)))
		{
			for (int i = 0; i < isotopes.Count; i++)
			{
				massSortedCentroidList[isotopes[i]].Charge = charge;
			}
		}
	}

	/// <summary>
	/// Compress the averaged profile spectrum to reduce its memory footprint.
	/// The sampling rate of the FT instruments is very high. Due to the very high
	/// resolution of these instruments, the spectra are often quite empty. i.e. 
	/// that there are large areas of zero intensities between the two peaks.
	/// It is sufficient that we keep only a small number of consecutive zero intensity 
	/// points. We do not remove them entirely because this will have strange effects 
	/// on smoothing routines (currently we keep at most eight consecutive zero 
	/// intensity points. This is sufficient to enable 15-point smoothing over the 
	/// spectrum).
	/// </summary>
	/// <param name="segmentList">
	/// The segment list.
	/// </param>
	/// <returns>
	/// The compressed data.
	/// </returns>
	private static ProfileData CompressProfileSpectrum(ProfileData segmentList)
	{
		int i = 0;
		int length = segmentList.Length;
		int num = length - 4;
		int num2 = 0;
		double[] intensities = segmentList.Intensities;
		double[] masses = segmentList.Masses;
		for (; i < length && intensities[i] != 0.0; i++)
		{
		}
		if (i == length)
		{
			return segmentList;
		}
		int num3 = i;
		for (; i < length && intensities[i] == 0.0; i++)
		{
		}
		int num4 = i;
		if (i == length)
		{
			return segmentList;
		}
		while (num3 != length - 1)
		{
			if (num4 - num3 > 8)
			{
				int num5 = num3 + 4;
				int num6 = num4 - 4 - num5;
				if (num6 > 0)
				{
					num2 += num6;
					intensities[num5] = -1.0;
					masses[num5] = num6;
				}
			}
			for (; i < length && intensities[i] != 0.0; i++)
			{
			}
			num3 = i;
			if (num3 == length)
			{
				break;
			}
			for (i++; i < num && intensities[i] + intensities[i + 1] + intensities[i + 2] + intensities[i + 3] + intensities[i + 4] == 0.0; i += 5)
			{
			}
			for (; i < length && intensities[i] == 0.0; i++)
			{
			}
			num4 = i;
			if (num4 == length && num3 < segmentList.Length)
			{
				num4 = length;
			}
		}
		ProfileData profileData = new ProfileData(length - num2);
		int num7 = 0;
		double[] masses2 = profileData.Masses;
		double[] intensities2 = profileData.Intensities;
		for (int j = 0; j < length; j++)
		{
			double num8 = intensities[j];
			if (num8 != -1.0)
			{
				intensities2[num7] = num8;
				masses2[num7++] = masses[j];
			}
			else
			{
				j += (int)masses[j] - 1;
			}
		}
		return profileData;
	}

	/// <summary>
	/// Fill the target raw scan provided by the caller with the result of the average calculation.
	/// </summary>
	/// <param name="firstScan">
	/// the first scan in the set to average
	/// </param>
	/// <param name="segmentScan">
	/// List holding the averaged segmented scan
	/// </param>
	/// <param name="centroidStream">
	/// Holding the averaged centroid stream
	/// </param>
	/// <param name="noiseInformation">
	/// Noise and baseline data
	/// </param>
	/// <param name="massToFrequencyConverter">
	/// Mass calibration for target scan
	/// </param>
	/// <returns>
	/// The filled average scan.
	/// </returns>
	private static Scan FillAverageScan(Scan firstScan, ProfileData segmentScan, List<CentroidStreamPoint> centroidStream, NoiseInformation noiseInformation, MassToFrequencyConverter massToFrequencyConverter)
	{
		NoisePackets scanAlignedNoise = noiseInformation.ScanAlignedNoise;
		double[] masses = segmentScan.Masses;
		double[] intensities = segmentScan.Intensities;
		IRangeAccess segmentRange = massToFrequencyConverter.SegmentRange;
		if (masses.Length != 0)
		{
			segmentRange = RangeFactory.Create(masses[0], masses[^1]);
		}
		SegmentedScan segmentedScan = CreateSegmentedScan(segmentScan.Length, masses, intensities, segmentRange);
		CentroidStream centroidStream2 = CreateCentroidStream(centroidStream.Count);
		double[] baseLine = scanAlignedNoise.BaseLine;
		double[] noise = scanAlignedNoise.Noise;
		for (int i = 0; i < centroidStream.Count; i++)
		{
			CentroidStreamPoint centroidStreamPoint = centroidStream[i];
			centroidStream2.Masses[i] = centroidStreamPoint.Position;
			centroidStream2.Intensities[i] = centroidStreamPoint.Intensity;
			centroidStream2.Charges[i] = centroidStreamPoint.Charge;
			centroidStream2.Resolutions[i] = centroidStreamPoint.Resolution;
			centroidStream2.Baselines[i] = baseLine[i];
			centroidStream2.Noises[i] = noise[i];
		}
		if (firstScan.CentroidScan != null)
		{
			centroidStream2.CoefficientsCount = firstScan.CentroidScan.CoefficientsCount;
			if (centroidStream2.CoefficientsCount > 0)
			{
				centroidStream2.Coefficients = new double[centroidStream2.CoefficientsCount];
				Array.Copy(firstScan.CentroidScan.Coefficients, centroidStream2.Coefficients, centroidStream2.CoefficientsCount);
			}
		}
		double tic = GetTic(segmentScan, intensities);
		double basePeakMass = 0.0;
		double basePeakIntensity = 0.0;
		if (centroidStream.Count > 0)
		{
			CentroidStreamPoint centroidStreamPoint2 = FindMaxElement(centroidStream);
			basePeakMass = centroidStreamPoint2.Position;
			basePeakIntensity = centroidStreamPoint2.Intensity;
		}
		ScanStatistics scanStatistics = firstScan.ScanStatistics.DeepClone();
		scanStatistics.BasePeakMass = basePeakMass;
		scanStatistics.TIC = tic;
		scanStatistics.BasePeakIntensity = basePeakIntensity;
		scanStatistics.PacketCount = segmentScan.Length;
		if (masses.Length != 0)
		{
			scanStatistics.LowMass = masses[0];
			scanStatistics.HighMass = masses[segmentScan.Length - 1];
		}
		return new Scan(massToFrequencyConverter, noiseInformation.RawFileNoise)
		{
			CentroidScan = centroidStream2,
			SegmentedScan = segmentedScan,
			ScanStatistics = scanStatistics,
			MassResolution = firstScan.MassResolution,
			IsUserTolerance = firstScan.IsUserTolerance,
			ToleranceUnit = firstScan.ToleranceUnit,
			ScanType = scanStatistics.ScanType,
			SubtractionPointer = firstScan.SubtractionPointer,
			ScanAdder = firstScan.ScanAdder
		};
	}

	/// <summary>
	/// get tic.
	/// </summary>
	/// <param name="segmentScan">
	/// The segment scan.
	/// </param>
	/// <param name="resampledIntensities">
	/// The re-sampled intensities.
	/// </param>
	/// <returns>
	/// The Tic of the intensities
	/// </returns>
	private static double GetTic(ProfileData segmentScan, double[] resampledIntensities)
	{
		double num = 0.0;
		int length = segmentScan.Length;
		int num2 = length / 10;
		int i = 0;
		for (int j = 0; j < num2; j++)
		{
			num += resampledIntensities[i];
			num += resampledIntensities[i + 1];
			num += resampledIntensities[i + 2];
			num += resampledIntensities[i + 3];
			num += resampledIntensities[i + 4];
			num += resampledIntensities[i + 5];
			num += resampledIntensities[i + 6];
			num += resampledIntensities[i + 7];
			num += resampledIntensities[i + 8];
			num += resampledIntensities[i + 9];
			i += 10;
		}
		for (; i < length; i++)
		{
			num += resampledIntensities[i];
		}
		return num;
	}

	/// <summary>
	/// find max element.
	/// </summary>
	/// <param name="centroids">
	/// The centroids.
	/// </param>
	/// <returns>
	/// The CentroidStreamPoint object which has the largest intensity
	/// </returns>
	private static CentroidStreamPoint FindMaxElement(List<CentroidStreamPoint> centroids)
	{
		return FindMaxElement(centroids, 0, centroids.Count - 1);
	}

	/// <summary>
	/// Calculates and retrieves the necessary parameters for creating 
	/// the averaged scan from the the given set of raw scans to average.
	/// </summary>
	/// <param name="firstScan">
	/// The first Scan.
	/// </param>
	/// <param name="findSegmentedScan">
	/// The find Segmented Scan.
	/// </param>
	/// <param name="scans">
	/// The scans.
	/// </param>
	/// <returns>
	/// The calculated target spectrum parameters.
	/// </returns>
	private static MassToFrequencyConverter CalculateTargetSpectrumParameters(Scan firstScan, FindSegmentedScan findSegmentedScan, int scans)
	{
		MassToFrequencyConverter massToFrequencyConverter = new MassToFrequencyConverter();
		CentroidStream centroidScan = firstScan.CentroidScan;
		if (centroidScan.Coefficients != null && centroidScan.Coefficients.Length >= 4)
		{
			massToFrequencyConverter.Coefficient1 = centroidScan.Coefficients[2];
			massToFrequencyConverter.Coefficient2 = centroidScan.Coefficients[3];
			if (centroidScan.Coefficients.Length >= 5)
			{
				massToFrequencyConverter.Coefficient3 = centroidScan.Coefficients[4];
			}
		}
		SegmentedScan segmentedScan = firstScan.SegmentedScan;
		int num = segmentedScan.Intensities.Length;
		double num2 = segmentedScan.Positions[0];
		massToFrequencyConverter.HighestMass = segmentedScan.Positions[num - 1];
		massToFrequencyConverter.DeltaFrequency = CalculateDeltaFrequency(segmentedScan, massToFrequencyConverter);
		double num3 = double.MaxValue;
		double num4 = double.MinValue;
		for (int i = 0; i < scans; i++)
		{
			SegmentedScan segmentedScan2 = findSegmentedScan(i);
			num3 = Math.Min(num3, segmentedScan2.Ranges[0].Low);
			num4 = Math.Max(num4, segmentedScan2.Ranges[0].High);
			if (segmentedScan2.PositionCount > 0)
			{
				num = segmentedScan2.SegmentCount;
				double num5 = CalculateDeltaFrequency(segmentedScan2, massToFrequencyConverter);
				if (num5 < massToFrequencyConverter.DeltaFrequency)
				{
					massToFrequencyConverter.DeltaFrequency = num5;
				}
				if (segmentedScan2.Positions[0] < num2)
				{
					num2 = segmentedScan2.Positions[0];
				}
				if (segmentedScan2.Positions[num - 1] > massToFrequencyConverter.HighestMass)
				{
					massToFrequencyConverter.HighestMass = segmentedScan2.Positions[num - 1];
				}
			}
		}
		massToFrequencyConverter.BaseFrequency = massToFrequencyConverter.ConvertMassToFrequency(num2);
		massToFrequencyConverter.SegmentRange = RangeFactory.Create(num3, num4);
		return massToFrequencyConverter;
	}

	/// <summary>
	/// Compare intensities.
	/// </summary>
	/// <param name="left">
	/// The left point.
	/// </param>
	/// <param name="right">
	/// The right point.
	/// </param>
	/// <returns>
	/// 1 if left less than right
	/// -1 if left greater than right
	/// 0 if left and right are equal
	/// </returns>
	private static int CompareIntenisities(CentroidStreamPoint left, CentroidStreamPoint right)
	{
		if (left.Intensity < right.Intensity)
		{
			return 1;
		}
		if (left.Intensity > right.Intensity)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>
	/// Finds the CentroidStreamPoint object which contains the maximum Intensity
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
	/// <returns>
	/// The CentroidStreamPoint object which has the largest intensity
	/// </returns>
	internal static CentroidStreamPoint FindMaxElement(List<CentroidStreamPoint> centroids, int startIndex, int endIndex)
	{
		if (startIndex >= 0 && startIndex <= endIndex && endIndex < centroids.Count)
		{
			CentroidStreamPoint centroidStreamPoint = centroids[startIndex];
			double intensity = centroidStreamPoint.Intensity;
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (centroids[i].Intensity > intensity)
				{
					centroidStreamPoint = centroids[i];
					intensity = centroidStreamPoint.Intensity;
				}
			}
			return centroidStreamPoint;
		}
		return new CentroidStreamPoint();
	}

	/// <summary>
	/// Calculates the delta frequency for the given DataPeaks.
	/// </summary>
	/// <param name="scan">
	/// Segmented Scan
	/// </param>
	/// <param name="massToFrequencyConverter">
	/// Calibration data for target scan
	/// </param>
	/// <returns>
	/// The calculated delta frequency.
	/// </returns>
	private static double CalculateDeltaFrequency(SegmentedScan scan, MassToFrequencyConverter massToFrequencyConverter)
	{
		double num = massToFrequencyConverter.ConvertMassToFrequency(scan.Positions[0]);
		double num2 = ((scan.Intensities.Length > 1) ? massToFrequencyConverter.ConvertMassToFrequency(scan.Positions[1]) : 0.0);
		return num - num2;
	}

	/// <summary>
	/// Create the CentroidStream object with the given size.
	/// </summary>
	/// <param name="size">The number of peaks in this stream.
	/// </param>
	/// <returns>
	/// An empty centroid stream, with arrays allocated to the indicated size.
	/// </returns>
	private static CentroidStream CreateCentroidStream(int size)
	{
		return new CentroidStream
		{
			Baselines = new double[size],
			Charges = new double[size],
			Flags = new PeakOptions[size],
			Intensities = new double[size],
			Masses = new double[size],
			Noises = new double[size],
			Resolutions = new double[size],
			Length = size
		};
	}

	/// <summary>
	/// Create the <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" /> object withe given size, and attach the mass and intensity arrays.
	/// </summary>
	/// <param name="size">The number of mass/intensity pairs.
	/// </param>
	/// <param name="masses">
	/// Mass data for scan 
	/// </param>
	/// <param name="intensities">
	/// Intensity data for scan 
	/// </param>
	/// <param name="segmentRange">Mass range of the scan.</param>
	/// <returns>
	/// An scan with the mass and intensity data attached.
	/// </returns>
	private static SegmentedScan CreateSegmentedScan(int size, double[] masses, double[] intensities, IRangeAccess segmentRange)
	{
		SegmentedScan segmentedScan = new SegmentedScan();
		segmentedScan.Intensities = intensities;
		segmentedScan.Flags = new PeakOptions[size];
		segmentedScan.Positions = masses;
		segmentedScan.Ranges = new ThermoFisher.CommonCore.Data.Business.Range[1]
		{
			new ThermoFisher.CommonCore.Data.Business.Range
			{
				Low = segmentRange.Low,
				High = segmentRange.High
			}
		};
		segmentedScan.SegmentSizes = new int[1] { size };
		return segmentedScan;
	}
}
