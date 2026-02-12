using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The qualitative peak detection settings from a processing method.
/// </summary>
internal class QualitativePeakDetection : IQualitativePeakDetectionAccess, IIcisSettingsAccess, IGenesisRawSettingsAccess, IPeakChromatogramSettingsAccess, IManualNoiseAccess, IMaximizingMassesAccess, IPeakLimitsAccess, IRawObjectBase
{
	/// <summary>
	/// The peak detection info version 1.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakDetectionInfoVersion1
	{
		public bool EnableDetection;

		public int MassRequired;

		public double PercentMassesFound;

		public double MinPeakSeparation;

		public double PercentLargestPeak;

		public double PercentComponentPeak;

		public PeakPercent PeakPercent;

		public int SmoothingPoints;

		public bool ValleyDetection;

		public double BkgUpdateRate;

		public int BkgWidth;

		public int WindowSize;

		public int FilterWidth;

		public double SNThreshold;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseSNR;
	}

	/// <summary>
	/// The peak detection info version 12.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakDetectionInfoVersion12
	{
		public bool EnableDetection;

		public int MassRequired;

		public double PercentMassesFound;

		public double MinPeakSeparation;

		public double PercentLargestPeak;

		public double PercentComponentPeak;

		public PeakPercent PeakPercent;

		public int SmoothingPoints;

		public bool ValleyDetection;

		public double BkgUpdateRate;

		public int BkgWidth;

		public int WindowSize;

		public int FilterWidth;

		public double SNThreshold;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseSNR;

		public double Delay;

		public TraceType ChroTraceType;
	}

	/// <summary>
	/// The peak detection info version 34.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakDetectionInfoVersion34
	{
		public bool EnableDetection;

		public int MassRequired;

		public double PercentMassesFound;

		public double MinPeakSeparation;

		public double PercentLargestPeak;

		public double PercentComponentPeak;

		public PeakPercent PeakPercent;

		public int SmoothingPoints;

		public bool ValleyDetection;

		public double BkgUpdateRate;

		public int BkgWidth;

		public int WindowSize;

		public int FilterWidth;

		public double SNThreshold;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseSNR;

		public double Delay;

		public TraceType Trace1;

		public TraceType Trace2;

		public TraceOperator TraceOperator;

		public VirtualDeviceTypes DetectorType;

		public bool IsLimitPeaksEnabled;

		public LimitPeaks LimitPeaks;

		public double PeaksNumber;

		public bool IsRelPeakEnabled;

		public double RTStart;

		public double RTEnd;
	}

	/// <summary>
	/// The peak detection info version 42.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakDetectionInfoVersion42
	{
		public bool EnableDetection;

		public int MassRequired;

		public double PercentMassesFound;

		public double MinPeakSeparation;

		public double PercentLargestPeak;

		public double PercentComponentPeak;

		public PeakPercent PeakPercent;

		public int SmoothingPoints;

		public bool ValleyDetection;

		public double BkgUpdateRate;

		public int BkgWidth;

		public int WindowSize;

		public int FilterWidth;

		public double SNThreshold;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseSNR;

		public double Delay;

		public TraceType Trace1;

		public TraceType Trace2;

		public TraceOperator TraceOperator;

		public VirtualDeviceTypes DetectorType;

		public bool IsLimitPeaksEnabled;

		public LimitPeaks LimitPeaks;

		public double PeaksNumber;

		public bool IsRelPeakEnabled;

		public double RTStart;

		public double RTEnd;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType NoiseType;

		public int MinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool ICISConstrainPeak;

		public double ICISPeakHeightPercentage;

		public double ICISTailingFactor;
	}

	/// <summary>
	/// The peak detection info version 43.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakDetectionInfoVersion43
	{
		public bool EnableDetection;

		public int MassRequired;

		public double PercentMassesFound;

		public double MinPeakSeparation;

		public double PercentLargestPeak;

		public double PercentComponentPeak;

		public PeakPercent PeakPercent;

		public int SmoothingPoints;

		public bool ValleyDetection;

		public double BkgUpdateRate;

		public int BkgWidth;

		public int WindowSize;

		public int FilterWidth;

		public double SNThreshold;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseSNR;

		public double Delay;

		public TraceType Trace1;

		public TraceType Trace2;

		public TraceOperator TraceOperator;

		public VirtualDeviceTypes DetectorType;

		public bool IsLimitPeaksEnabled;

		public LimitPeaks LimitPeaks;

		public double PeaksNumber;

		public bool IsRelPeakEnabled;

		public double RTStart;

		public double RTEnd;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType NoiseType;

		public int MinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool ICISConstrainPeak;

		public double ICISPeakHeightPercentage;

		public double ICISTailingFactor;

		public int MassPrecision;

		public double MassTolerance;

		public OldLcqEnums.ToleranceUnit ToleranceUnits;

		public bool InclRefAndExceptionPeaks;
	}

	/// <summary>
	/// The peak detection info version 44.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakDetectionInfoVersion44
	{
		public bool EnableDetection;

		public int MassRequired;

		public double PercentMassesFound;

		public double MinPeakSeparation;

		public double PercentLargestPeak;

		public double PercentComponentPeak;

		public PeakPercent PeakPercent;

		public int SmoothingPoints;

		public bool ValleyDetection;

		public double BkgUpdateRate;

		public int BkgWidth;

		public int WindowSize;

		public int FilterWidth;

		public double SNThreshold;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseSNR;

		public double Delay;

		public TraceType Trace1;

		public TraceType Trace2;

		public TraceOperator TraceOperator;

		public VirtualDeviceTypes DetectorType;

		public bool IsLimitPeaksEnabled;

		public LimitPeaks LimitPeaks;

		public double PeaksNumber;

		public bool IsRelPeakEnabled;

		public double RTStart;

		public double RTEnd;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType NoiseType;

		public int MinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool ICISConstrainPeak;

		public double ICISPeakHeightPercentage;

		public double ICISTailingFactor;

		public int MassPrecision;

		public double MassTolerance;

		public OldLcqEnums.ToleranceUnit ToleranceUnits;

		public bool InclRefAndExceptionPeaks;

		public bool ManualRegionDetection;

		public double NoiseLoRange;

		public double NoiseHiRange;

		public bool RMSDetection;
	}

	/// <summary>
	/// The peak detection info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakDetectionInfo
	{
		public bool EnableDetection;

		public int MassRequired;

		public double PercentMassesFound;

		public double MinPeakSeparation;

		public double PercentLargestPeak;

		public double PercentComponentPeak;

		public PeakPercent PeakPercent;

		public int SmoothingPoints;

		public bool ValleyDetection;

		public double BkgUpdateRate;

		public int BkgWidth;

		public int WindowSize;

		public int FilterWidth;

		public double SNThreshold;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseSNR;

		public double Delay;

		public TraceType Trace1;

		public TraceType Trace2;

		public TraceOperator TraceOperator;

		public VirtualDeviceTypes DetectorType;

		public bool IsLimitPeaksEnabled;

		public LimitPeaks LimitPeaks;

		public double PeaksNumber;

		public bool IsRelPeakEnabled;

		public double RTStart;

		public double RTEnd;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType NoiseType;

		public int MinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool ICISConstrainPeak;

		public double ICISPeakHeightPercentage;

		public double ICISTailingFactor;

		public int MassPrecision;

		public double MassTolerance;

		public OldLcqEnums.ToleranceUnit ToleranceUnits;

		public bool InclRefAndExceptionPeaks;

		public bool ManualRegionDetection;

		public double NoiseLoRange;

		public double NoiseHiRange;

		public bool RMSDetection;

		public double NoiseLoIntensityRange;

		public double NoiseHiIntensityRange;
	}

	/// <summary>
	/// The falcon event. Structure for a single event
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct FalconEvent
	{
		public int OPCode;

		public int Kind;

		public double Time;

		public double Value1;

		public double Value2;
	}

	private PeakDetectionInfo _info;

	private static readonly int[,] MarshalledSizes;

	private const int TpInitialStartThreshold = 21;

	private const int TpInitialEndThreshold = 22;

	private const int TpInitialAreaThreshold = 23;

	private const int TpInitialPpResolution = 25;

	private const int TpInitialNegativePeaks = 26;

	private const int TpInitialBunchFactor = 27;

	private const int TpInitialTension = 28;

	/// <summary>
	/// Gets the component name
	/// </summary>
	public string ComponentName { get; private set; }

	/// <summary>
	/// Gets or sets the mass ranges info.
	/// </summary>
	private MassRangeStruct[] MassRangesInfo { get; set; }

	/// <summary>
	/// Gets the scan filter, as an interface.
	/// This same data is available in string form
	/// in the ChromatogramSettings property
	/// </summary>
	public IScanFilter ScanFilter { get; private set; }

	/// <summary>
	/// Gets or sets the mass ranges 2.
	/// </summary>
	private MassRangeStruct[] MassRanges2 { get; set; }

	/// <summary>
	/// Gets or sets the falcon events.
	/// </summary>
	private FalconEvent[] FalconEvents { get; set; }

	/// <summary>
	/// Gets a value indicating whether peak detection is enabled.
	/// Note: This property is not used in product "Xcalibur"
	/// </summary>
	public bool EnableDetection => _info.EnableDetection;

	/// <summary>
	/// Gets the number of masses required to maximize
	/// </summary>
	int IMaximizingMassesAccess.MassRequired => _info.MassRequired;

	/// <summary>
	/// Gets the percentage of masses which must maximize
	/// </summary>
	double IMaximizingMassesAccess.PercentMassesFound => _info.PercentMassesFound;

	/// <summary>
	/// Gets the minimum peak separation (time) for Mass-Maximizing detection
	/// </summary>
	double IMaximizingMassesAccess.MinimumPeakSeparation => _info.MinPeakSeparation;

	/// <summary>
	/// Gets the number of scans in the max-masses window
	/// </summary>
	int IMaximizingMassesAccess.WindowSize => _info.WindowSize;

	/// <summary>
	/// Gets the box filter width for Mass-Maximizing detection
	/// </summary>
	int IMaximizingMassesAccess.FilterWidth => _info.FilterWidth;

	/// <summary>
	/// Gets the number of scans averaged for a background
	/// </summary>
	int IMaximizingMassesAccess.BackgroundWidth => _info.BkgWidth;

	/// <summary>
	/// Gets the number of smoothing points, for background analysis
	/// This setting is common to all integrators
	/// </summary>
	public int SmoothingPoints => _info.SmoothingPoints;

	/// <summary>
	/// Gets the width of display window for the peak (in seconds)
	/// This is for presentation only
	/// </summary>
	public double DisplayWindowWidth => _info.DisplayWindowWidth;

	/// <summary>
	/// Gets a value indicating whether peak limits are enabled
	/// </summary>
	bool IPeakLimitsAccess.IsLimitPeaksEnabled => _info.IsLimitPeaksEnabled;

	/// <summary>
	/// Gets a value indicating whether to Select top peak by area or height
	/// </summary>
	LimitPeaks IPeakLimitsAccess.LimitPeaks => _info.LimitPeaks;

	/// <summary>
	/// Gets the number of "top peaks" to select
	/// </summary>
	double IPeakLimitsAccess.NumberOfPeaks => _info.PeaksNumber;

	/// <summary>
	/// Gets a value indicating whether "relative peak height threshold" is enabled
	/// </summary>
	bool IPeakLimitsAccess.IsRelativePeakEnabled => _info.IsRelPeakEnabled;

	/// <summary>
	/// Gets the percent of the largest peak, which is used for filtering
	/// peak detection results, when "IsRelativePeakEnabled"
	/// </summary>
	double IPeakLimitsAccess.PercentLargestPeak => _info.PercentLargestPeak;

	/// <summary>
	/// Gets a the "percent of component peak" (limit)
	/// Only valid when PeakPercent is set to PercentOfComponentPeak
	/// </summary>
	double IPeakLimitsAccess.PercentComponentPeak => _info.PercentComponentPeak;

	/// <summary>
	/// Gets a value indicating how peak percentages are specified
	/// (unused in product Xcalibur)
	/// </summary>
	PeakPercent IPeakLimitsAccess.PeakPercent => _info.PeakPercent;

	/// <summary>
	/// Gets the Algorithm to use (Genesis, ICIS etc.)
	/// </summary>
	public PeakDetector PeakDetectionAlgorithm => _info.PeakDetectionAlgorithm;

	/// <summary>
	/// Gets or sets the mass options.
	/// </summary>
	public IMassOptionsAccess MassOptions
	{
		get
		{
			return new MassOptions
			{
				Precision = MassPrecision,
				ToleranceUnits = ToleranceUnits,
				Tolerance = MassTolerance
			};
		}
		set
		{
			MassPrecision = value.Precision;
			ToleranceUnits = value.ToleranceUnits;
			MassTolerance = value.Tolerance;
		}
	}

	/// <summary>
	/// Gets Number of decimals used in defining mass values
	/// </summary>
	public int MassPrecision
	{
		get
		{
			return _info.MassPrecision;
		}
		private set
		{
			_info.MassPrecision = value;
		}
	}

	/// <summary>
	/// Gets tolerance used for mass
	/// </summary>
	public double MassTolerance
	{
		get
		{
			return _info.MassTolerance;
		}
		private set
		{
			_info.MassTolerance = value;
		}
	}

	/// <summary>
	/// Gets units of mass tolerance
	/// </summary>
	public ThermoFisher.CommonCore.Data.ToleranceUnits ToleranceUnits
	{
		get
		{
			return ConvertLcqToleranceUnits(_info.ToleranceUnits);
		}
		private set
		{
			_info.ToleranceUnits = value switch
			{
				ThermoFisher.CommonCore.Data.ToleranceUnits.mmu => OldLcqEnums.ToleranceUnit.Mmu, 
				ThermoFisher.CommonCore.Data.ToleranceUnits.ppm => OldLcqEnums.ToleranceUnit.Ppm, 
				_ => OldLcqEnums.ToleranceUnit.Amu, 
			};
		}
	}

	/// <summary>
	/// Gets the (Avalon) integrator events
	/// </summary>
	public ReadOnlyCollection<IntegratorEvent> IntegratorEvents
	{
		get
		{
			IntegratorEvent[] array = new IntegratorEvent[FalconEvents.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = TypeConverters.ConvertIntegratorEvent(FalconEvents[i]);
			}
			return new ReadOnlyCollection<IntegratorEvent>(array);
		}
	}

	/// <summary>
	/// Gets the number of scans in the baseline window.
	/// Each scan is checked to see if it should be considered a baseline scan.
	/// This is determined by looking at a number of scans (BaselineWindow) before
	/// and after the a data point. If it is the lowest point in the group it will be
	/// marked as a "baseline" point.
	/// Range: 1 - 500
	/// Default: 40
	/// </summary>
	int IIcisSettingsAccess.BaselineWindow => _info.BaselineWindow;

	/// <summary>
	/// Gets the area noise factor.
	/// </summary>
	int IIcisSettingsAccess.AreaNoiseFactor => _info.AreaNoiseFactor;

	/// <summary>
	/// Gets the peak noise factor.
	/// </summary>
	int IIcisSettingsAccess.PeakNoiseFactor => _info.PeakNoiseFactor;

	/// <summary>
	/// Gets a value indicating whether to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	bool IIcisSettingsAccess.ConstrainPeakWidth => _info.ICISConstrainPeak;

	/// <summary>
	/// Gets the percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the ConstrainPeak is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	double IIcisSettingsAccess.PeakHeightPercentage => _info.ICISPeakHeightPercentage;

	/// <summary>
	/// Gets the tailing factor.
	/// </summary>
	double IIcisSettingsAccess.TailingFactor => _info.ICISTailingFactor;

	/// <summary>
	/// Gets the minimum peak width.
	/// </summary>
	int IIcisSettingsAccess.MinimumPeakWidth => _info.MinPeakWidth;

	/// <summary>
	/// Gets the <c>multiplet</c> resolution.
	/// </summary>
	int IIcisSettingsAccess.MultipletResolution => _info.MultipletResolution;

	/// <summary>
	/// Gets the area scan window.
	/// </summary>
	int IIcisSettingsAccess.AreaScanWindow => _info.AreaScanWindow;

	/// <summary>
	/// Gets the area tail extension.
	/// </summary>
	int IIcisSettingsAccess.AreaTailExtension => _info.AreaTailExtension;

	/// <summary>
	/// Gets a value indicating whether noise is calculated using an RMS method
	/// </summary>
	bool IIcisSettingsAccess.CalculateNoiseAsRms => _info.RMSDetection;

	/// <summary>
	/// Gets a value which indicates how the ICIS peak detector determines which signals are noise.
	/// The selected points can  determine a noise level, or be fed into an RMS calculator,
	/// depending on the RMS setting.
	/// </summary>
	IcisNoiseType IIcisSettingsAccess.NoiseMethod => _info.NoiseType;

	/// <summary>
	/// Gets the settings for the ICIS integrator
	/// </summary>
	public IIcisSettingsAccess IcisSettings => this;

	/// <summary>
	/// Gets a value indicating whether a peak's width (the tail) must be constrained.
	/// This flag allows you to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	bool IGenesisRawSettingsAccess.ConstrainPeak => _info.ConstrainPeak;

	/// <summary>
	/// Gets the width of a typical peak in seconds.
	/// This controls the minimum width that a peak is expected to have
	/// if valley detection is enabled.
	/// Integrator converts this to expectedPeakHalfWidth (minutes) by dividing by 120.
	/// With valley detection enabled,
	/// any valley points nearer than the expectedPeakHalfWidth (which is [expected width]/2)
	/// to the top of the peak are ignored.
	/// If a valley point is found outside the expected peak width,
	/// Genesis terminates the peak at that point.
	/// Genesis always terminates a peak when the signal reaches the baseline,
	/// independent of the value set for the expectedPeakHalfWidth.
	/// </summary>
	double IGenesisRawSettingsAccess.ExpectedPeakWidth => _info.PeakWidth;

	/// <summary>
	/// Gets a constraint on peak height.
	/// The percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the <c>ConstrainPeak</c> is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	double IGenesisRawSettingsAccess.PeakHeightPercent => _info.PeakHeightPercentage;

	/// <summary>
	/// Gets the minimum acceptable signal to noise of a peak.
	/// Genesis ignores all chromatogram peaks that have signal-to-noise values
	/// that are less than the S/N Threshold value
	/// </summary>
	double IGenesisRawSettingsAccess.SignalToNoiseThreshold => _info.SNThreshold;

	/// <summary>
	/// Gets the peak tailing factor.
	/// This controls how Genesis integrates the tail of a peak.
	/// This factor is the maximum ratio of the trailing edge to the leading side of a constrained peak.
	/// This applies only when the <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IGenesisRawSettingsAccess.ConstrainPeak" /> is true.
	/// The valid range is 0.5 through 9.0. 
	/// </summary>
	double IGenesisRawSettingsAccess.TailingFactor => _info.TailingFactor;

	/// <summary>
	/// Gets a value indicating whether valley detection is performed.
	/// This parameter must be set to true when performing base to base integration
	/// </summary>
	bool IGenesisRawSettingsAccess.ValleyDetection => _info.ValleyDetection;

	/// <summary>
	/// Gets the Peak Signal ToNoise Ratio Cutoff.
	/// The peak edge is set to values below this defined S/N. 
	/// This test assumes an edge of a peak is found when the baseline adjusted height of the edge is less than
	/// the ratio of the baseline adjusted apex height and the peak S/N cutoff ratio. 
	/// If the S/N at the apex is 500 and the peak S/N cutoff value is 200,
	/// Genesis defines the right and left edges of the peak when the S/N reaches a value less than 200.
	/// Range: 50.0 to 10000.0. 
	/// Technical equation:<c>if height &lt; (1/PeakSignalToNoiseRatioCutoff)*height(apex) =&gt; valley here</c>
	/// </summary>
	double IGenesisRawSettingsAccess.PeakSignalToNoiseRatioCutoff => _info.PeakSNRCutoff;

	/// <summary>
	/// Gets the multiplier of the valley bottom
	/// that the peak trace can rise above a baseline (before or after the peak). 
	/// If the trace exceeds ValleyThreshold,
	/// Genesis applies valley detection peak integration criteria. 
	/// This method drops a vertical line from the apex of the valley between unresolved
	/// peaks to the baseline.
	/// The intersection of the vertical line and the baseline defines the end of the first
	/// peak and the beginning of the second peak. 
	/// This test is applied to both the left and right edges of the peak. 
	/// The ValleyThreshold criteria is useful for integrating peaks with long tails.
	/// Useful range: 1.001 to 1.5
	/// Note: Appears on product UI converted from factor to percentage as "Rise percentage".
	/// For example: 1.1 = 10%
	/// Code tests similar to the following:<code>
	/// if ((currentSignal-baseline) &gt; ((valleyBottom-baseline) * ValleyThreshold))
	/// {
	///     side of peak has bottomed out, and risen above minimum
	/// }
	/// </code>
	/// </summary>
	double IGenesisRawSettingsAccess.ValleyThreshold => _info.ValleyThreshold;

	/// <summary>
	/// Gets or the S/N range is 1.0 to 100.0. for valley detection.
	/// Technical equation:<c>height(here +/- VALLEY_WIDTH) &gt; ValleyDepth*SNR+height(here) =&gt; valley here </c>
	/// </summary>
	double IGenesisRawSettingsAccess.ValleyDepth => _info.ValleyDepth;

	/// <summary>
	/// Gets a value indicating whether to enable RMS noise calculation.
	/// If not set, noise is calculated peak to peak.
	/// It is set by default.
	/// </summary>
	bool IGenesisRawSettingsAccess.CalculateNoiseAsRms => _info.RMSDetection;

	/// <summary>
	/// Gets a noise limit, where the code stops attempting to find a better baseline.
	/// controls how the baseline is drawn in the noise data.
	/// The higher the baseline noise tolerance value,
	/// the higher the baseline is drawn through the noise data.
	/// The valid range is 0.0 to 1.0.
	/// </summary>
	double IGenesisRawSettingsAccess.BaselineNoiseLimit => Options.BaseNoiseLimit;

	/// <summary>
	/// Gets the minimum number of scans that Genesis uses to calculate a baseline.
	/// A larger number includes more data in determining an averaged baseline.
	/// The valid range is 2 to 100.
	/// </summary>
	int IGenesisRawSettingsAccess.MinScansInBaseline => Options.MinScansInBaseline;

	/// <summary>
	/// Gets a factor which controls the width of the RMS noise band above and below the peak detection baseline
	/// and is applied to the raw RMS noise values to raise the effective RMS noise during peak detection.
	/// The left and right peak boundaries are assigned above the noise and, therefore,
	/// closer to the peak apex value in minutes. 
	/// This action effectively raises the peak integration baseline above the RMS noise level. 
	/// Range: 0.1 to 10.0.
	/// Default: 2.0.
	/// </summary>
	double IGenesisRawSettingsAccess.BaselineNoiseRejectionFactor => Options.BaseNoiseRejectionFactor;

	/// <summary>
	/// Gets the number of minutes between background scan recalculations.
	/// Baseline is refitted each time this interval elapses. 
	/// </summary>
	double IGenesisRawSettingsAccess.BackgroundUpdateRate => _info.BkgUpdateRate;

	/// <summary>
	/// Gets a limit for the "baseline signal to noise ratio".
	/// A peak is considered ended if the following condition is met:
	/// <c>height &lt;= (BaseNoise * BaseSignalToNoiseRatio))</c>
	/// Where BaseNoise is the calculated noise on the fitted baseline,
	/// and height is the height above baseline.
	/// </summary>
	double IGenesisRawSettingsAccess.BaseSignalToNoiseRatio => _info.BaseSNR;

	/// <summary>
	/// Gets the minimum acceptable percentage of the largest peak.
	/// Do not return peaks which have a height less than this % of the highest peak above baseline.
	/// </summary>
	double IGenesisRawSettingsAccess.PercentLargestPeak => _info.PercentLargestPeak;

	/// <summary>
	/// Gets a value indicating whether filtering of peaks is by relative signal height
	/// </summary>
	bool IGenesisRawSettingsAccess.FilterByRelativePeakHeight => _info.IsRelPeakEnabled;

	/// <summary>
	/// Gets or sets the options.
	/// </summary>
	internal IProcessingMethodOptionsAccess Options { get; set; }

	/// <summary>
	/// Gets the settings for the Genesis integrator
	/// Note: This property is under review.
	/// May return an alternative interface
	/// </summary>
	public IGenesisRawSettingsAccess GenesisSettings => this;

	/// <summary>
	/// Gets the settings for creating a chromatogram
	/// </summary>
	public IPeakChromatogramSettingsAccess ChromatogramSettings => this;

	/// <summary>
	/// Gets the scan filter.
	/// This determines which scans are included in the chromatogram.
	/// </summary>
	string IPeakChromatogramSettingsAccess.Filter => FilterString;

	/// <summary>
	/// Gets the chromatogram settings.
	/// This defines how data for a chromatogram point is constructed from a scan.
	/// </summary>
	IChromatogramTraceSettingsAccess IPeakChromatogramSettingsAccess.ChroSettings => new ChromatogramTraceSettings
	{
		Filter = FilterString,
		CompoundNames = Array.Empty<string>(),
		DelayInMin = _info.Delay,
		FragmentMass = 0.0,
		IncludeReference = _info.InclRefAndExceptionPeaks,
		Trace = _info.Trace1,
		MassRanges = ConvertRanges(MassRangesInfo)
	};

	/// <summary>
	/// Gets the chromatogram settings
	/// When there is a trace operator set,
	/// This defines how data for a chromatogram point is constructed from a scan for the chromatogram
	/// to be added or subtracted.
	/// </summary>
	IChromatogramTraceSettingsAccess IPeakChromatogramSettingsAccess.ChroSettings2 => new ChromatogramTraceSettings
	{
		Filter = FilterString,
		CompoundNames = Array.Empty<string>(),
		DelayInMin = _info.Delay,
		FragmentMass = 0.0,
		IncludeReference = _info.InclRefAndExceptionPeaks,
		Trace = _info.Trace2,
		MassRanges = ConvertRanges(MassRanges2)
	};

	/// <summary>
	/// Gets the device type.
	/// This defines which data stream within the raw file is used. 
	/// </summary>
	Device IPeakChromatogramSettingsAccess.Instrument => _info.DetectorType.ToDevice();

	/// <summary>
	/// Gets the trace operator.
	/// If the operator is not "None" then a second chromatogram can be added to or subtracted from the first.
	/// </summary>
	TraceOperator IPeakChromatogramSettingsAccess.TraceOperator => _info.TraceOperator;

	/// <summary>
	/// Gets or sets the filter string.
	/// </summary>
	private string FilterString { get; set; }

	/// <summary>
	/// Gets the manual noise range settings
	/// </summary>
	public IManualNoiseAccess ManualNoise => this;

	/// <summary>
	/// Gets a value indicating whether manual noise should be used
	/// </summary>
	bool IManualNoiseAccess.UseManualNoiseRegion => _info.ManualRegionDetection;

	/// <summary>
	/// Gets the manual noise region (time range in minutes)
	/// </summary>
	ThermoFisher.CommonCore.Data.Business.Range IManualNoiseAccess.ManualNoiseRtRange => ThermoFisher.CommonCore.Data.Business.Range.Create(_info.NoiseLoRange, _info.NoiseHiRange);

	/// <summary>
	/// Gets settings for the maximizing masses algorithm
	/// Note: This algorithm is not used by product "Xcalibur"
	/// </summary>
	public IMaximizingMassesAccess MaximizingMasses => this;

	/// <summary>
	/// Gets settings to limit (filter) the list of returned peaks
	/// after integration
	/// </summary>
	public IPeakLimitsAccess LimitPeakSettings => this;

	/// <summary>
	/// Gets the time range, over which qualitative processing is done.
	/// Only peaks detected within this range are processed further
	/// (for example, library searched)
	/// </summary>
	public ThermoFisher.CommonCore.Data.Business.Range RetentionTimeWindow => ThermoFisher.CommonCore.Data.Business.Range.Create(_info.RTStart, _info.RTEnd);

	/// <summary>
	/// Gets the manual noise region (intensity range)
	/// These values are not used by Xcalibur
	/// </summary>
	ThermoFisher.CommonCore.Data.Business.Range IManualNoiseAccess.ManualNoiseIntensityRange => ThermoFisher.CommonCore.Data.Business.Range.Create(_info.NoiseLoIntensityRange, _info.NoiseHiIntensityRange);

	/// <inheritdoc />
	public int InstrumentIndex => 1;

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = Utilities.ReadStructure<PeakDetectionInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		FilterScanEvent filterScanEvent = new FilterScanEvent(viewer.LoadRawFileObjectExt<Filter>(fileRevision, ref startPos), fromScan: false);
		ScanFilter = new WrappedScanFilter(filterScanEvent);
		FilterString = ScanFilter.ToString();
		ComponentName = viewer.ReadStringExt(ref startPos);
		MassRangesInfo = MassRangeStruct.LoadArray(viewer, ref startPos);
		if (fileRevision < 25)
		{
			_info.IsRelPeakEnabled = true;
		}
		MassRanges2 = ((fileRevision >= 25) ? MassRangeStruct.LoadArray(viewer, ref startPos) : Array.Empty<MassRangeStruct>());
		if (fileRevision >= 40)
		{
			FalconEvents = viewer.ReadStructArrayExt<FalconEvent>(ref startPos);
		}
		if (fileRevision < 35)
		{
			_info.PeakDetectionAlgorithm = PeakDetector.Genesis;
			_info.NoiseType = IcisNoiseType.Incos;
			_info.MinPeakWidth = 3;
			_info.PeakNoiseFactor = 10;
			_info.BaselineWindow = 40;
			_info.MultipletResolution = 10;
			_info.AreaTailExtension = 5;
			_info.AreaNoiseFactor = 5;
			_info.AreaScanWindow = 0;
			_info.ICISConstrainPeak = false;
			_info.ICISPeakHeightPercentage = 5.0;
			_info.ICISTailingFactor = 1.0;
		}
		if (fileRevision < 43)
		{
			_info.MassPrecision = 1;
			_info.MassTolerance = 500.0;
			_info.ToleranceUnits = OldLcqEnums.ToleranceUnit.Mmu;
			_info.InclRefAndExceptionPeaks = false;
		}
		if (fileRevision < 51)
		{
			_info.ManualRegionDetection = false;
			_info.NoiseLoRange = 0.0;
			_info.NoiseHiRange = 100.0;
			_info.RMSDetection = false;
		}
		if (fileRevision < 53)
		{
			_info.NoiseHiIntensityRange = 0.0;
			_info.NoiseLoIntensityRange = 100.0;
		}
		if (_info.SmoothingPoints == 0)
		{
			_info.SmoothingPoints = 1;
		}
		if (fileRevision < 12)
		{
			_info.Delay = 0.0;
			_info.Trace1 = ((MassRangesInfo.Length == 0) ? TraceType.TIC : TraceType.MassRange);
		}
		else if (fileRevision < 25 && _info.Trace1 >= TraceType.Fragment)
		{
			_info.DetectorType = VirtualDeviceTypes.MsAnalogDevice;
			_info.Trace1 = 11 + _info.Trace1 - 3;
		}
		return startPos - dataOffset;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.QualitativePeakDetection" /> class.
	/// </summary>
	public QualitativePeakDetection()
	{
		FalconEvents = InitialEvents();
		_info.MassRequired = 1;
		_info.PercentMassesFound = 0.0;
		_info.MinPeakSeparation = 0.3;
		_info.PercentLargestPeak = 10.0;
		_info.PercentComponentPeak = 10.0;
		_info.SmoothingPoints = 3;
		_info.ValleyDetection = false;
		_info.Delay = 0.0;
		_info.NoiseLoRange = 0.0;
		_info.NoiseHiRange = 0.0;
		_info.NoiseLoIntensityRange = 0.0;
		_info.NoiseHiIntensityRange = 0.0;
	}

	/// <summary>
	/// Gets the initial events.
	/// </summary>
	/// <returns>
	/// The events
	/// </returns>
	public static FalconEvent[] InitialEvents()
	{
		return new List<FalconEvent>(10)
		{
			new FalconEvent
			{
				OPCode = 21,
				Kind = 0,
				Time = 0.0,
				Value1 = 10000.0,
				Value2 = 0.0
			},
			new FalconEvent
			{
				OPCode = 22,
				Kind = 0,
				Time = 0.0,
				Value1 = 10000.0,
				Value2 = 0.0
			},
			new FalconEvent
			{
				OPCode = 23,
				Kind = 0,
				Time = 0.0,
				Value1 = 10000.0,
				Value2 = 0.0
			},
			new FalconEvent
			{
				OPCode = 25,
				Kind = 0,
				Time = 0.0,
				Value1 = 1.0,
				Value2 = 0.0
			},
			new FalconEvent
			{
				OPCode = 27,
				Kind = 0,
				Time = 0.0,
				Value1 = 1.0,
				Value2 = 0.0
			},
			new FalconEvent
			{
				OPCode = 26,
				Kind = 0,
				Time = 0.0,
				Value1 = 0.0,
				Value2 = 0.0
			},
			new FalconEvent
			{
				OPCode = 28,
				Kind = 0,
				Time = 0.0,
				Value1 = 1.0,
				Value2 = 0.0
			}
		}.ToArray();
	}

	/// <summary>
	/// convert LCQ tolerance units.
	/// </summary>
	/// <param name="toConvert">
	/// The to convert.
	/// </param>
	/// <returns>
	/// The converted units
	/// </returns>
	public static ThermoFisher.CommonCore.Data.ToleranceUnits ConvertLcqToleranceUnits(OldLcqEnums.ToleranceUnit toConvert)
	{
		return toConvert switch
		{
			OldLcqEnums.ToleranceUnit.Mmu => ThermoFisher.CommonCore.Data.ToleranceUnits.mmu, 
			OldLcqEnums.ToleranceUnit.Ppm => ThermoFisher.CommonCore.Data.ToleranceUnits.ppm, 
			_ => ThermoFisher.CommonCore.Data.ToleranceUnits.amu, 
		};
	}

	/// <summary>
	/// Convert mass ranges.
	/// </summary>
	/// <param name="massRangesInfo">
	/// The mass ranges info.
	/// </param>
	/// <returns>
	/// The converted ranges.
	/// </returns>
	private ThermoFisher.CommonCore.Data.Business.Range[] ConvertRanges(MassRangeStruct[] massRangesInfo)
	{
		int num = massRangesInfo.Length;
		ThermoFisher.CommonCore.Data.Business.Range[] array = new ThermoFisher.CommonCore.Data.Business.Range[num];
		for (int i = 0; i < num; i++)
		{
			MassRangeStruct massRangeStruct = massRangesInfo[i];
			array[i] = ThermoFisher.CommonCore.Data.Business.Range.Create(massRangeStruct.LowMass, massRangeStruct.HighMass);
		}
		return array;
	}

	static QualitativePeakDetection()
	{
		int[,] obj = new int[7, 2]
		{
			{ 53, 0 },
			{ 51, 0 },
			{ 43, 0 },
			{ 35, 0 },
			{ 25, 0 },
			{ 12, 0 },
			{ 0, 0 }
		};
		obj[0, 1] = Marshal.SizeOf(typeof(PeakDetectionInfo));
		obj[1, 1] = Marshal.SizeOf(typeof(PeakDetectionInfoVersion44));
		obj[2, 1] = Marshal.SizeOf(typeof(PeakDetectionInfoVersion43));
		obj[3, 1] = Marshal.SizeOf(typeof(PeakDetectionInfoVersion42));
		obj[4, 1] = Marshal.SizeOf(typeof(PeakDetectionInfoVersion34));
		obj[5, 1] = Marshal.SizeOf(typeof(PeakDetectionInfoVersion12));
		obj[6, 1] = Marshal.SizeOf(typeof(PeakDetectionInfoVersion1));
		MarshalledSizes = obj;
	}
}
