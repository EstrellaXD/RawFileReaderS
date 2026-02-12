using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
/// read from file, and give read access to all settings in "component" in a PMD file.
/// Organizes the many fields into groups (interfaces)
/// </summary>
internal class QuanComponent : IRawObjectBase, IManualNoiseAccess, IIcisSettingsAccess, IGenesisRawSettingsAccess, IPeakLocationSettingsAccess, IFindSettingsAccess, IPeakChromatogramSettingsAccess, IXcaliburComponentAccess, ITargetCompoundSettingsAccess, ISystemSuitabilitySettingsAccess, IPeakPuritySettingsAccess, IInternalStandardSettingsAccess, ICalibrationSettingsAccess, ICalibrationAndQuantificationThresholdLimitsAccess, IDetectionThresholdLimitsAccess, IXcaliburIonRatioTestSettingsAccess
{
	/// <summary>
	/// The xcalibur component type.
	/// </summary>
	private enum XcaliburComponentType
	{
		/// <summary>
		/// All types.
		/// </summary>
		AllTypes = -1,
		/// <summary>
		/// Target compound.
		/// </summary>
		TargetCompound,
		/// <summary>
		/// Internal standard.
		/// </summary>
		InternalStandard,
		/// <summary>
		/// Undefined. Component not defined
		/// </summary>
		Undefined,
		/// <summary>
		/// The retention time reference.
		/// </summary>
		RtReference,
		/// <summary>
		/// Surrogate component.
		/// </summary>
		Surrogate
	}

	/// <summary>
	/// The response index. Determines if peak height or area is
	/// used for the peak's response.
	/// </summary>
	internal enum ResponseIndex
	{
		/// <summary>
		/// Use area.
		/// </summary>
		Area,
		/// <summary>
		/// Use height.
		/// </summary>
		Height
	}

	/// <summary>
	/// The origin index. Defines how calibration curve origin is used.
	/// </summary>
	private enum OriginIndex
	{
		/// <summary>
		/// Ignore the origin.
		/// </summary>
		IgnoreOrigin,
		/// <summary>
		/// Force through origin.
		/// </summary>
		ForceOrigin,
		/// <summary>
		/// Include (extra point) at origin.
		/// </summary>
		IncludeOrigin
	}

	/// <summary>
	/// The calibration curve regression index.
	/// </summary>
	private enum CurveIndex
	{
		/// <summary>
		/// Use first order fit.
		/// </summary>
		FirstOrder,
		/// <summary>
		/// Use second order fit.
		/// </summary>
		SecondOrder,
		/// <summary>
		/// Use first order log-log fit.
		/// </summary>
		FirstOrderLogLog,
		/// <summary>
		/// Use second order log-log fit.
		/// </summary>
		SecondOrderLogLog,
		/// <summary>
		/// Use average response factor fit.
		/// </summary>
		AverageRf,
		/// <summary>
		/// Use point to point fit.
		/// </summary>
		PointToPoint,
		/// <summary>
		/// Use cubic spline fit.
		/// </summary>
		CubicSpline,
		/// <summary>
		/// locally weighted regression
		/// </summary>
		Loess
	}

	/// <summary>
	/// The component info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ComponentInfo
	{
		public double UserEnteredRT;

		public double RTSearchWindow;

		public double SNThreshold;

		public double FitThreshold;

		public bool AdjustExpectedRT;

		public bool UseAsRTReference;

		public PeakMethod PeakDetectionMethod;

		public int SmoothingPoints;

		public XcaliburComponentType ComponentType;

		public bool ValleyDetection;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double BaseSNR;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseAmount;

		public CurveIndex Curve;

		public OriginIndex Origin;

		public Weighting Weighting;

		public ResponseIndex Response;

		public double PercentISTDInTarget;

		public double PercentTargetInISTD;

		public TraceType Trace1Type;

		public TraceType Trace2Type;

		public TraceOperator TraceOperator;

		public double AreaFlagThreshold;

		public double HeightFlagThreshold;

		public double AreaFlagPercentThreshold;

		public double HeightFlagPercentThreshold;

		public int ForwardThreshold;

		public int ReverseThreshold;

		public int MatchThreshold;

		public bool IonRatioConfirmation;

		public int Standard;

		public IonRatioMethod IRCMethods;

		public XcaliburIonRatioWindowType IRCWindowType;

		public double QualifierionCoelution;

		public VirtualDeviceTypes DetectorType;

		public double DetectorDelay;

		public double RSquared;

		public double LimitDetection;

		public double LimitQuantitation;

		public double LinearityLimit;

		public double CarryOverLimit;

		public double SpikeAmount;

		public double SpikeUpperRecovery;

		public double SpikeLowerRecovery;

		public double ResThreshold;

		public double SymmetryTake;

		public double SymmetryThreshold;

		public double MinPeakWidth;

		public double MaxPeakWidth;

		public double DetectWidth;

		public double TailingMeasured;

		public double TailingFailureThreshold;

		public double ColumnMeasure;

		public double ColumnFailureThreshold;

		public double BaselineClipping;

		public double DetectSinalToNoise;

		public bool ResolutionChecksEnabled;

		public bool SymmetryChecksEnabled;

		public bool PeakClassificationChecksEnabled;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType IcisNoiseType;

		public int IcisMinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool IcisConstrainPeak;

		public double IcisPeakHeightPercentage;

		public double IcisTailingFactor;

		public bool EnableDetection;

		public int ScanThreshold;

		public double DesiredPeakCoverage;

		public bool LimitWaveRange;

		public double LowWavelength;

		public double HighWavelength;

		public int MassPrecision;

		public double MassTolerance;

		public OldLcqEnums.ToleranceUnit ToleranceUnits;

		public bool InclRefAndExceptionPeaks;

		public bool ManualRegionDetection;

		public double NoiseLoRange;

		public double NoiseHiRange;

		public bool RmsDetection;
	}

	/// <summary>
	/// The component info version 5.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ComponentInfoVersion5
	{
		public double UserEnteredRT;

		public double RTSearchWindow;

		public double SNThreshold;

		public double FitThreshold;

		public bool AdjustExpectedRT;

		public bool UseAsRTReference;

		public PeakMethod PeakDetectionMethod;

		public int SmoothingPoints;

		public XcaliburComponentType ComponentType;

		public bool ValleyDetection;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double BaseSNR;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseAmount;

		public CurveIndex Curve;

		public OriginIndex Origin;

		public Weighting Weighting;

		public ResponseIndex Response;

		public double PercentISTDInTarget;

		public double PercentTargetInISTD;

		public TraceType Trace1Type;

		public TraceType Trace2Type;

		public TraceOperator TraceOperator;

		public double AreaFlagThreshold;

		public double HeightFlagThreshold;

		public double AreaFlagPercentThreshold;

		public double HeightFlagPercentThreshold;

		public int ForwardThreshold;

		public int ReverseThreshold;

		public int MatchThreshold;

		public bool IonRatioConfirmation;

		public int Standard;

		public IonRatioMethod IRCMethods;

		public XcaliburIonRatioWindowType IRCWindowType;

		public double QualifierionCoelution;

		public VirtualDeviceTypes DetectorType;

		public double DetectorDelay;

		public double RSquared;

		public double LimitDetection;

		public double LimitQuantitation;

		public double LinearityLimit;

		public double CarryOverLimit;

		public double SpikeAmount;

		public double SpikeUpperRecovery;

		public double SpikeLowerRecovery;

		public double ResThreshold;

		public double SymmetryTake;

		public double SymmetryThreshold;

		public double MinPeakWidth;

		public double MaxPeakWidth;

		public double DetectWidth;

		public double TailingMeasured;

		public double TailingFailureThreshold;

		public double ColumnMeasure;

		public double ColumnFailureThreshold;

		public double BaselineClipping;

		public double DetectSinalToNoise;

		public bool ResolutionChecksEnabled;

		public bool SymmetryChecksEnabled;

		public bool PeakClassificationChecksEnabled;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType IcisNoiseType;

		public int IcisMinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool IcisConstrainPeak;

		public double IcisPeakHeightPercentage;

		public double IcisTailingFactor;

		public bool EnableDetection;

		public int ScanThreshold;

		public double DesiredPeakCoverage;

		public bool LimitWaveRange;

		public double LowWavelength;

		public double HighWavelength;

		public int MassPrecision;

		public double MassTolerance;

		public OldLcqEnums.ToleranceUnit ToleranceUnits;

		public bool InclRefAndExceptionPeaks;
	}

	/// <summary>
	/// The component info version 4.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ComponentInfoVersion4
	{
		public double UserEnteredRT;

		public double RTSearchWindow;

		public double SNThreshold;

		public double FitThreshold;

		public bool AdjustExpectedRT;

		public bool UseAsRTReference;

		public PeakMethod PeakDetectionMethod;

		public int SmoothingPoints;

		public XcaliburComponentType ComponentType;

		public bool ValleyDetection;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double BaseSNR;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseAmount;

		public CurveIndex Curve;

		public OriginIndex Origin;

		public Weighting Weighting;

		public ResponseIndex Response;

		public double PercentISTDInTarget;

		public double PercentTargetInISTD;

		public TraceType Trace1Type;

		public TraceType Trace2Type;

		public TraceOperator TraceOperator;

		public double AreaFlagThreshold;

		public double HeightFlagThreshold;

		public double AreaFlagPercentThreshold;

		public double HeightFlagPercentThreshold;

		public int ForwardThreshold;

		public int ReverseThreshold;

		public int MatchThreshold;

		public bool IonRatioConfirmation;

		public int Standard;

		public IonRatioMethod IRCMethods;

		public XcaliburIonRatioWindowType IRCWindowType;

		public double QualifierionCoelution;

		public VirtualDeviceTypes DetectorType;

		public double DetectorDelay;

		public double RSquared;

		public double LimitDetection;

		public double LimitQuantitation;

		public double LinearityLimit;

		public double CarryOverLimit;

		public double SpikeAmount;

		public double SpikeUpperRecovery;

		public double SpikeLowerRecovery;

		public double ResThreshold;

		public double SymmetryTake;

		public double SymmetryThreshold;

		public double MinPeakWidth;

		public double MaxPeakWidth;

		public double DetectWidth;

		public double TailingMeasured;

		public double TailingFailureThreshold;

		public double ColumnMeasure;

		public double ColumnFailureThreshold;

		public double BaselineClipping;

		public double DetectSinalToNoise;

		public bool ResolutionChecksEnabled;

		public bool SymmetryChecksEnabled;

		public bool PeakClassificationChecksEnabled;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType IcisNoiseType;

		public int IcisMinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool IcisConstrainPeak;

		public double IcisPeakHeightPercentage;

		public double IcisTailingFactor;

		public bool EnableDetection;

		public int ScanThreshold;

		public double DesiredPeakCoverage;

		public bool LimitWaveRange;

		public double LowWavelength;

		public double HighWavelength;
	}

	/// <summary>
	/// The component info version 3.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ComponentInfoVersion3
	{
		public double UserEnteredRT;

		public double RTSearchWindow;

		public double SNThreshold;

		public double FitThreshold;

		public bool AdjustExpectedRT;

		public bool UseAsRTReference;

		public PeakMethod PeakDetectionMethod;

		public int SmoothingPoints;

		public XcaliburComponentType ComponentType;

		public bool ValleyDetection;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double BaseSNR;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseAmount;

		public CurveIndex Curve;

		public OriginIndex Origin;

		public Weighting Weighting;

		public ResponseIndex Response;

		public double PercentISTDInTarget;

		public double PercentTargetInISTD;

		public TraceType Trace1Type;

		public TraceType Trace2Type;

		public TraceOperator TraceOperator;

		public double AreaFlagThreshold;

		public double HeightFlagThreshold;

		public double AreaFlagPercentThreshold;

		public double HeightFlagPercentThreshold;

		public int ForwardThreshold;

		public int ReverseThreshold;

		public int MatchThreshold;

		public bool IonRatioConfirmation;

		public int Standard;

		public IonRatioMethod IRCMethods;

		public XcaliburIonRatioWindowType IRCWindowType;

		public double QualifierionCoelution;

		public VirtualDeviceTypes DetectorType;

		public double DetectorDelay;

		public double RSquared;

		public double LimitDetection;

		public double LimitQuantitation;

		public double LinearityLimit;

		public double CarryOverLimit;

		public double SpikeAmount;

		public double SpikeUpperRecovery;

		public double SpikeLowerRecovery;

		public double ResThreshold;

		public double SymmetryTake;

		public double SymmetryThreshold;

		public double MinPeakWidth;

		public double MaxPeakWidth;

		public double DetectWidth;

		public double TailingMeasured;

		public double TailingFailureThreshold;

		public double ColumnMeasure;

		public double ColumnFailureThreshold;

		public double BaselineClipping;

		public double DetectSinalToNoise;

		public bool ResolutionChecksEnabled;

		public bool SymmetryChecksEnabled;

		public bool PeakClassificationChecksEnabled;

		public PeakDetector PeakDetectionAlgorithm;

		public IcisNoiseType IcisNoiseType;

		public int IcisMinPeakWidth;

		public int PeakNoiseFactor;

		public int BaselineWindow;

		public int MultipletResolution;

		public int AreaTailExtension;

		public int AreaNoiseFactor;

		public int AreaScanWindow;

		public bool IcisConstrainPeak;

		public double IcisPeakHeightPercentage;

		public double IcisTailingFactor;
	}

	/// <summary>
	/// The component info version 2.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ComponentInfoVersion2
	{
		public double UserEnteredRT;

		public double RTSearchWindow;

		public double SNThreshold;

		public double FitThreshold;

		public bool AdjustExpectedRT;

		public bool UseAsRTReference;

		public PeakMethod PeakDetectionMethod;

		public int SmoothingPoints;

		public XcaliburComponentType ComponentType;

		public bool ValleyDetection;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double BaseSNR;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseAmount;

		public CurveIndex Curve;

		public OriginIndex Origin;

		public Weighting Weighting;

		public ResponseIndex Response;

		public double PercentISTDInTarget;

		public double PercentTargetInISTD;

		public TraceType Trace1Type;

		public TraceType Trace2Type;

		public TraceOperator TraceOperator;

		public double AreaFlagThreshold;

		public double HeightFlagThreshold;

		public double AreaFlagPercentThreshold;

		public double HeightFlagPercentThreshold;

		public int ForwardThreshold;

		public int ReverseThreshold;

		public int MatchThreshold;

		public bool IonRatioConfirmation;

		public int Standard;

		public IonRatioMethod IRCMethods;

		public XcaliburIonRatioWindowType IRCWindowType;

		public double QualifierionCoelution;

		public VirtualDeviceTypes DetectorType;

		public double DetectorDelay;

		public double RSquared;

		public double LimitDetection;

		public double LimitQuantitation;

		public double LinearityLimit;

		public double CarryOverLimit;

		public double SpikeAmount;

		public double SpikeUpperRecovery;

		public double SpikeLowerRecovery;

		public double ResThreshold;

		public double SymmetryTake;

		public double SymmetryThreshold;

		public double MinPeakWidth;

		public double MaxPeakWidth;

		public double DetectWidth;

		public double TailingMeasured;

		public double TailingFailureThreshold;

		public double ColumnMeasure;

		public double ColumnFailureThreshold;

		public double BaselineClipping;

		public double DetectSinalToNoise;

		public bool ResolutionChecksEnabled;

		public bool SymmetryChecksEnabled;

		public bool PeakClassificationChecksEnabled;
	}

	/// <summary>
	/// The component info version 1.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ComponentInfoVersion1
	{
		public double UserEnteredRT;

		public double RTSearchWindow;

		public double SNThreshold;

		public double FitThreshold;

		public bool AdjustExpectedRT;

		public bool UseAsRTReference;

		public PeakMethod PeakDetectionMethod;

		public int SmoothingPoints;

		public XcaliburComponentType ComponentType;

		public bool ValleyDetection;

		public bool ConstrainPeak;

		public double PeakHeightPercentage;

		public double TailingFactor;

		public double ValleyThreshold;

		public double ValleyDepth;

		public double PeakSNRCutoff;

		public double BaseSNR;

		public double PeakWidth;

		public double DisplayWindowWidth;

		public double BaseAmount;

		public CurveIndex Curve;

		public OriginIndex Origin;

		public Weighting Weighting;

		public ResponseIndex Response;

		public double PercentISTDInTarget;

		public double PercentTargetInISTD;
	}

	/// <summary>
	/// The mass intensity pair.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct MassIntensityPair
	{
		public double Mass;

		public double Intensity;
	}

	private static readonly int FalconEventSize = Marshal.SizeOf(typeof(QualitativePeakDetection.FalconEvent));

	private static readonly int[,] MarshalledSizes;

	private ComponentInfo _info;

	private MassIntensityPair[] _massIntensityPairs;

	private ReadOnlyCollection<IIonRatioConfirmationTestAccess> _ionRatioConfirmationTests;

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
	/// Gets mass tolerance for this component
	/// </summary>
	public IMassOptionsAccess ToleranceSettings { get; private set; }

	/// <summary>
	/// Gets or sets the falcon events.
	/// </summary>
	private QualitativePeakDetection.FalconEvent[] FalconEvents { get; set; }

	/// <summary>
	/// Gets or sets the internal standard units.
	/// </summary>
	private string IstdUnits { get; set; }

	/// <summary>
	/// Gets the retention time reference component.
	/// Adjust the retention time, using this component as a reference
	/// </summary>
	public string AdjustUsing { get; private set; }

	/// <summary>
	/// Gets the name of this component
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets or sets the filter string.
	/// </summary>
	private string FilterString { get; set; }

	/// <summary>
	/// Gets or sets the scan filter.
	/// </summary>
	public IScanFilter ScanFilter { get; set; }

	/// <summary>
	/// Gets a value indicating whether manual noise should be used
	/// </summary>
	bool IManualNoiseAccess.UseManualNoiseRegion => _info.ManualRegionDetection;

	/// <summary>
	/// Gets the manual noise region (time range in minutes)
	/// </summary>
	ThermoFisher.CommonCore.Data.Business.Range IManualNoiseAccess.ManualNoiseRtRange => ThermoFisher.CommonCore.Data.Business.Range.Create(_info.NoiseLoRange, _info.NoiseHiRange);

	/// <summary>
	/// Gets the manual noise region (intensity range)
	/// These values are not used by Xcalibur
	/// </summary>
	ThermoFisher.CommonCore.Data.Business.Range IManualNoiseAccess.ManualNoiseIntensityRange => ThermoFisher.CommonCore.Data.Business.Range.Create(0.0, 100.0);

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
	/// Gets a noise level multiplier.
	/// This determines the peak edge after the location of the possible peak,
	/// allowing the peak to narrow or broaden without affecting the baseline. 
	/// Range: 1 - 500
	/// Default multiplier: 5
	/// </summary>
	int IIcisSettingsAccess.AreaNoiseFactor => _info.AreaNoiseFactor;

	/// <summary>
	/// Gets a noise level multiplier (a minimum S/N ratio).
	/// This determines the potential peak signal threshold. 
	/// Range: 1 - 1000
	/// Default multiplier: 10
	/// </summary>
	int IIcisSettingsAccess.PeakNoiseFactor => _info.PeakNoiseFactor;

	/// <summary>
	/// Gets a value indicating whether to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	bool IIcisSettingsAccess.ConstrainPeakWidth => _info.IcisConstrainPeak;

	/// <summary>
	/// Gets the percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the ConstrainPeak is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	double IIcisSettingsAccess.PeakHeightPercentage => _info.IcisPeakHeightPercentage;

	/// <summary>
	/// Gets the Tailing Factor.
	/// This controls how Genesis integrates the tail of a peak.
	/// This factor is the maximum ratio of the trailing edge to the leading side of a constrained peak.
	/// This applies only when the ConstrainPeak is true.
	/// The valid range is 0.5 through 9.0. 
	/// </summary>
	double IIcisSettingsAccess.TailingFactor => _info.IcisTailingFactor;

	/// <summary>
	/// Gets the minimum number of scans required in a peak. 
	/// Range: 0 to 100. 
	/// Default: 3. 
	/// </summary>
	int IIcisSettingsAccess.MinimumPeakWidth => _info.IcisMinPeakWidth;

	/// <summary>
	///  Gets the minimum separation in scans between the apexes of two potential peaks.
	///  This is a criterion to determine if two peaks are resolved.
	///  Enter a larger number in a noisy environment when the signal is bouncing around.
	///  Range: 1 to 500.
	///  Default: 10 scans. 
	/// </summary>
	int IIcisSettingsAccess.MultipletResolution => _info.MultipletResolution;

	/// <summary>
	/// Gets the number of scans on each side of the peak apex to be allowed. 
	/// Range: 0 to 100.
	/// Default: 0 scans.
	/// 0 specifies that all scans from peak-start to peak-end are to be included in the area integration.
	/// </summary>
	int IIcisSettingsAccess.AreaScanWindow => _info.AreaScanWindow;

	/// <summary>
	/// Gets the number of scans past the peak endpoint to use in averaging the intensity.
	/// Range: 0 to 100. 
	/// Default: 5 scans.
	/// </summary>
	int IIcisSettingsAccess.AreaTailExtension => _info.AreaTailExtension;

	/// <summary>
	/// Gets a value indicating whether noise is calculated using an RMS method
	/// </summary>
	bool IIcisSettingsAccess.CalculateNoiseAsRms => _info.RmsDetection;

	/// <summary>
	/// Gets an value which indicates how the ICIS peak detector determines which signals are noise.
	/// The selected points can  determine a noise level, or be fed into an RMS calculator,
	/// depending on the RMS setting.
	/// </summary>
	IcisNoiseType IIcisSettingsAccess.NoiseMethod => _info.IcisNoiseType;

	/// <summary>
	/// Gets settings for the ICIS peak integrator
	/// </summary>
	public IIcisSettingsAccess IcisSettings { get; private set; }

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
	bool IGenesisRawSettingsAccess.CalculateNoiseAsRms => _info.RmsDetection;

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
	double IGenesisRawSettingsAccess.BackgroundUpdateRate => ((IGenesisRawSettingsAccess)PeakDetection).BackgroundUpdateRate;

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
	double IGenesisRawSettingsAccess.PercentLargestPeak => 0.0;

	/// <summary>
	/// Gets a value indicating whether filtering of peaks is by relative signal height
	/// </summary>
	bool IGenesisRawSettingsAccess.FilterByRelativePeakHeight => false;

	/// <summary>
	/// Gets settings for the genesis peak integrator
	/// </summary>
	public IGenesisRawSettingsAccess GenesisSettings { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the retention time should be adjusted based on a reference peak.
	/// </summary>
	bool IPeakLocationSettingsAccess.AdjustExpectedRT => _info.AdjustExpectedRT;

	/// <summary>
	/// Gets the expected time, as in the method (before any adjustments)
	/// </summary>
	double IPeakLocationSettingsAccess.UserEnteredRT => _info.UserEnteredRT;

	/// <summary>
	/// Gets a value which determine how a single peak is found from the list of
	/// returned peaks from integrating the chromatogram.
	/// For example: Highest peak in time window.
	/// </summary>
	PeakMethod IPeakLocationSettingsAccess.LocateMethod => _info.PeakDetectionMethod;

	/// <summary>
	/// Gets the window, centered around the peak, in minutes.
	/// The located peak must be within a window of expected +/- width.
	/// </summary>
	double IPeakLocationSettingsAccess.SearchWindow => _info.RTSearchWindow / 60.0;

	/// <summary>
	/// Gets the baseline and noise window.
	/// This setting is used to restrict the chromatogram.
	/// Only scans within the range "adjusted expected RT" +/- Window are processed.
	/// For example: a 1 minute window setting implies 2 minutes of data.
	/// </summary>
	double IPeakLocationSettingsAccess.BaselineAndNoiseWindow => Options.SearchWindow;

	/// <summary>
	/// Gets the settings for finding a peak based on spectral fit
	/// </summary>
	IFindSettingsAccess IPeakLocationSettingsAccess.FindSettings => this;

	/// <summary>
	/// Gets the signal to noise rejection parameter for peaks
	/// </summary>
	double IPeakLocationSettingsAccess.SignalToNoiseThreshold => _info.SNThreshold;

	/// <summary>
	/// Gets the forward threshold for find algorithm.
	/// </summary>
	int IFindSettingsAccess.ForwardThreshold => _info.ForwardThreshold;

	/// <summary>
	/// Gets the match threshold for find algorithm
	/// </summary>
	int IFindSettingsAccess.MatchThreshold => _info.MatchThreshold;

	/// <summary>
	/// Gets the reverse threshold for find algorithm
	/// </summary>
	int IFindSettingsAccess.ReverseThreshold => _info.ReverseThreshold;

	/// <summary>
	/// Gets or sets the spec points.
	/// </summary>
	private ReadOnlyCollection<SpectrumPoint> SpecPoints { get; set; }

	/// <summary>
	/// Gets the spec points.
	/// </summary>
	/// <value>The spec points.</value>
	ReadOnlyCollection<SpectrumPoint> IFindSettingsAccess.SpecPoints => SpecPoints;

	/// <summary>
	/// Gets the scan filter.
	/// This determines which scans are included in the chromatogram.
	/// </summary>
	string IPeakChromatogramSettingsAccess.Filter
	{
		get
		{
			ScanFilter.MassPrecision = PeakDetection.MassPrecision;
			return ScanFilter.ToString();
		}
	}

	/// <summary>
	/// Gets the chromatogram settings.
	/// This defines how data for a chromatogram point is constructed from a scan.
	/// </summary>
	IChromatogramTraceSettingsAccess IPeakChromatogramSettingsAccess.ChroSettings => new ChromatogramTraceSettings
	{
		Filter = FilterString,
		CompoundNames = Array.Empty<string>(),
		DelayInMin = _info.DetectorDelay,
		FragmentMass = 0.0,
		IncludeReference = _info.InclRefAndExceptionPeaks,
		Trace = _info.Trace1Type,
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
		DelayInMin = _info.DetectorDelay,
		FragmentMass = 0.0,
		IncludeReference = _info.InclRefAndExceptionPeaks,
		Trace = _info.Trace2Type,
		MassRanges = ConvertRanges(MassRangesInfo2)
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
	/// Gets or sets the mass ranges info.
	/// </summary>
	private MassRangeStruct[] MassRangesInfo { get; set; }

	/// <summary>
	/// Gets or sets the mass ranges info 2.
	/// </summary>
	private MassRangeStruct[] MassRangesInfo2 { get; set; }

	/// <summary>
	/// Gets or sets the calibration units.
	/// </summary>
	private string CalibrationUnits { get; set; }

	/// <summary>
	/// Gets or sets the internal standard reference.
	/// </summary>
	private string IstdReference { get; set; }

	/// <summary>
	/// Gets or sets the calibration levels.
	/// </summary>
	private ReadOnlyCollection<ICalibrationLevelAccess> CalibrationLevels { get; set; }

	/// <summary>
	/// Gets or sets the qc levels.
	/// </summary>
	private ReadOnlyCollection<IQualityControlLevelAccess> QcLevels { get; set; }

	/// <summary>
	/// Gets (custom) keys about this component
	/// This is treated as a comment field.
	/// Not used for any "built-in" calculations
	/// but may be used to annotate reports etc.
	/// </summary>
	public string Keys { get; private set; }

	/// <summary>
	/// Gets the settings for a manual noise region
	/// </summary>
	IManualNoiseAccess IXcaliburComponentAccess.ManualNoiseSettings => this;

	/// <summary>
	/// Gets settings for peak location (expected retention time)
	/// </summary>
	IPeakLocationSettingsAccess IXcaliburComponentAccess.LocationSettings => this;

	/// <summary>
	/// Gets settings for the spectral find algorithm.
	/// </summary>
	IFindSettingsAccess IXcaliburComponentAccess.FindSettings => this;

	/// <summary>
	/// Gets settings for creating the component chromatogram
	/// </summary>
	IPeakChromatogramSettingsAccess IXcaliburComponentAccess.ChromatogramSettings => this;

	/// <summary>
	/// Gets component calibration settings (including level tables)
	/// </summary>
	public ICalibrationSettingsAccess CalibrationSettings { get; private set; }

	/// <summary>
	/// Gets settings for the system suitability algorithm
	/// </summary>
	public ISystemSuitabilitySettingsAccess SystemSuitabilitySettings { get; private set; }

	/// <summary>
	/// Gets settings for the PDA peak purity algorithm
	/// </summary>
	public IPeakPuritySettingsAccess PeakPuritySettings { get; private set; }

	/// <summary>
	/// Gets the table of calibration levels
	/// </summary>
	ReadOnlyCollection<ICalibrationLevelAccess> ITargetCompoundSettingsAccess.CalibrationLevels => CalibrationLevels;

	/// <summary>
	/// Gets the table of QC levels
	/// </summary>
	ReadOnlyCollection<IQualityControlLevelAccess> ITargetCompoundSettingsAccess.QcLevels => QcLevels;

	/// <summary>
	/// Gets the calibration curve fitting method
	/// </summary>
	RegressionMethod ITargetCompoundSettingsAccess.CalibrationCurve => _info.Curve switch
	{
		CurveIndex.FirstOrder => RegressionMethod.FirstOrder, 
		CurveIndex.SecondOrder => RegressionMethod.SecondOrder, 
		CurveIndex.FirstOrderLogLog => RegressionMethod.FirstOrderLogLog, 
		CurveIndex.SecondOrderLogLog => RegressionMethod.SecondOrderLogLog, 
		CurveIndex.AverageRf => RegressionMethod.AverageResponseFactor, 
		CurveIndex.PointToPoint => RegressionMethod.PointToPoint, 
		CurveIndex.CubicSpline => RegressionMethod.CubicSpline, 
		CurveIndex.Loess => RegressionMethod.LocallyWeighted, 
		_ => RegressionMethod.FirstOrder, 
	};

	/// <summary>
	/// Gets the weighting for calibration curve
	/// </summary>
	Weighting ITargetCompoundSettingsAccess.Weighting => _info.Weighting;

	/// <summary>
	/// Gets the calibration curve origin mode
	/// </summary>
	Origin ITargetCompoundSettingsAccess.Origin => _info.Origin switch
	{
		OriginIndex.ForceOrigin => Origin.Force, 
		OriginIndex.IncludeOrigin => Origin.Include, 
		_ => Origin.Excluded, 
	};

	/// <summary>
	/// Gets a value which determines how the response should be measured (using either peak height or peak area).
	/// </summary>
	ResponseRatio ITargetCompoundSettingsAccess.Response
	{
		get
		{
			if (_info.Response != ResponseIndex.Area)
			{
				return ResponseRatio.Height;
			}
			return ResponseRatio.Area;
		}
	}

	/// <summary>
	/// Gets the Unit for calibration
	/// </summary>
	string ITargetCompoundSettingsAccess.Units => CalibrationUnits;

	/// <summary>
	/// Gets the name of the internal standard for this component
	/// </summary>
	string ITargetCompoundSettingsAccess.InternalStandard => IstdReference;

	/// <summary>
	/// Gets the isotopic contribution of the internal standard to the target compound
	/// </summary>
	double ITargetCompoundSettingsAccess.ContributionOfISTDToTarget => _info.PercentISTDInTarget;

	/// <summary>
	/// Gets the isotopic contribution of the target compound to the internal standard
	/// </summary>
	double ITargetCompoundSettingsAccess.ContributionOfTargetToISTD => _info.PercentTargetInISTD;

	/// <summary>
	/// Gets a value indicating whether resolution checks will be performed
	/// </summary>
	bool ISystemSuitabilitySettingsAccess.EnableResolutionChecks => _info.ResolutionChecksEnabled;

	/// <summary>
	/// Gets the Resolution Threshold.
	/// The threshold value determines if a peak's resolution or ok or not.
	/// The default value is 90%.
	/// Resolution is defined as the ratio:
	/// <para>100 × V/P</para>
	/// where:
	/// <para>V = depth of the Valley: the difference in intensity from the chromatogram at the apex of the target peak
	/// to the lowest point in the valley between the target peak and a neighboring peak</para>
	/// <para>P = Peak height: the height of the target peak, above the peak's baseline</para>
	/// </summary>
	double ISystemSuitabilitySettingsAccess.ResolutionThreshold => _info.ResThreshold;

	/// <summary>
	/// Gets a value indicating whether peak symmetry checks are to be performed.
	/// Symmetry is determined at a specified peak height
	/// and is a measure of how even-sided a peak is
	/// about a perpendicular dropped from its apex.
	/// </summary>
	bool ISystemSuitabilitySettingsAccess.EnableSymmetryChecks => _info.SymmetryChecksEnabled;

	/// <summary>
	/// Gets the Peak Height at which symmetry is measured.
	/// The default value is 50%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	double ISystemSuitabilitySettingsAccess.SymmetryPeakHeight => _info.SymmetryTake;

	/// <summary>
	/// Gets the Symmetry Threshold.
	/// The SOP defined Symmetry Threshold is &gt; 70% at 50% peak height.
	/// This represents a realistic practical tolerance for capillary GC data.
	/// You can enter any value within the range 0% to 100%.
	/// The default value is 80% at 50% peak height.
	/// The algorithm determines symmetry at the <c>SymmetryPeakHeight</c>
	/// For the purposes of the test, a peak is considered symmetrical if:
	/// (Lesser of L and R) × 100 / (Greater of L and R) &gt; Symmetry Threshold %
	/// where:
	/// <para>L = the distance from the left side of the peak to
	/// the perpendicular dropped from the peak apex</para>
	/// <para>R = the distance from the right side of the peak to
	/// the perpendicular dropped from the peak apex</para>
	/// Measurements of L and R are taken from the raw file without smoothing.
	/// </summary>
	double ISystemSuitabilitySettingsAccess.SymmetryThreshold => _info.SymmetryThreshold;

	/// <summary>
	/// Gets a value indicating whether peak classification checks are to be run
	/// </summary>
	bool ISystemSuitabilitySettingsAccess.EnablePeakClassificationChecks => _info.PeakClassificationChecksEnabled;

	/// <summary>
	/// Gets the Peak Height at which the suitability calculator tests the width of target peaks.
	/// You can enter any value within the range 0% to 100%. The default value is 50%. 
	/// </summary>
	double ISystemSuitabilitySettingsAccess.PeakWidthPeakHeight => _info.DetectWidth;

	/// <summary>
	/// Gets the minimum peak width, at the specified peak height, for the peak width suitability test.
	/// The default value is 1.8. You can set any value in the range 0 to 30 seconds. 
	/// </summary>
	double ISystemSuitabilitySettingsAccess.MinPeakWidth => _info.MinPeakWidth;

	/// <summary>
	/// Gets the maximum peak width, at the specified peak height, for the peak width suitability test.
	/// The default value is 3.6. You can set any value in the range 0 to 30 seconds. 
	/// </summary>
	double ISystemSuitabilitySettingsAccess.MaxPeakWidth => _info.MaxPeakWidth;

	/// <summary>
	/// Gets the Peak Height at which the algorithm measures the tailing of target peaks.
	/// The default SOP value is 10%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	double ISystemSuitabilitySettingsAccess.TailingPeakHeight => _info.TailingMeasured;

	/// <summary>
	///  Gets the failure threshold for the tailing suitability test.
	///  The default SOP defined failure threshold is %lt 2 at 10% peak height. The valid range is 1 to 50.
	///  Tailing is calculated at the value defined in <see cref="P:ThermoFisher.CommonCore.Data.ISystemSuitabilitySettingsAccess.TailingPeakHeight" />.
	///  For the purposes of the test, a peak is considered to be excessively tailed if:
	///  <code>
	///  R / L &gt; Failure Threshold %
	///  where:
	///  L = the distance from the left side of the peak to the perpendicular dropped from the peak apex
	///  R = the distance from the right side of the peak to the perpendicular dropped from the peak apex
	///  Measurements of L and R are taken from the raw file without smoothing.</code>
	/// </summary>
	double ISystemSuitabilitySettingsAccess.TailingFailureThreshold => _info.TailingFailureThreshold;

	/// <summary>
	/// Gets the Peak Height at which the algorithm measures column overloading.
	/// The default SOP value is 50%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	double ISystemSuitabilitySettingsAccess.ColumnOverloadPeakHeight => _info.ColumnMeasure;

	/// <summary>
	/// Gets the failure threshold value for the column overload suitability test.
	/// The default SOP defined threshold is 1.5 at 50% peak height. The valid range is 1 to 20.
	/// A peak is considered to be overloaded if:
	/// <code>
	/// L / R &gt; Failure Threshold %
	/// where:
	/// L = the distance from the left side of the peak to the perpendicular dropped from the peak apex
	/// R = the distance from the right side of the peak to the perpendicular dropped from the peak apex
	/// Measurements of L and R are taken from the raw file without smoothing. </code>
	/// </summary>
	double ISystemSuitabilitySettingsAccess.ColumnOverloadFailureThreshold => _info.ColumnFailureThreshold;

	/// <summary>
	/// Gets the Number of Peak Widths for Noise Detection testing parameter for
	/// the baseline clipping system suitability test.
	/// The default value is 1.0 and the permitted range is 0.1 to 10.
	/// A peak is considered to be baseline clipped if there is no signal
	/// (zero intensity) on either side of the peak within the specified
	/// number of peak widths. The range is truncated to the quantitation window
	/// if the specified number of peak widths extends beyond the window’s edge.
	/// </summary>
	double ISystemSuitabilitySettingsAccess.PeakWidthsForNoiseDetection => _info.BaselineClipping;

	/// <summary>
	/// Gets the threshold for system suitability testing 
	/// of the signal-to-noise ratio. The default value is 20 and the
	/// permitted range is 1 to 500. The algorithm calculates the signal-to-noise ratio 
	/// within the quantitation window using only baseline signal.
	/// Any extraneous, minor, detected peaks are excluded from the calculation. 
	/// </summary>
	double ISystemSuitabilitySettingsAccess.SignalToNoiseRatio => _info.DetectSinalToNoise;

	/// <summary>
	/// Gets the % of the detected baseline for which we want to compute PeakPurity
	/// </summary>
	double IPeakPuritySettingsAccess.DesiredPeakCoverage => _info.DesiredPeakCoverage;

	/// <summary>
	/// Gets a value indicating whether we want to compute Peak Purity
	/// </summary>
	bool IPeakPuritySettingsAccess.EnableDetection => _info.EnableDetection;

	/// <summary>
	/// Gets a value indicating whether we want to use
	/// the enclosed wavelength range, not the total scan
	/// </summary>
	bool IPeakPuritySettingsAccess.LimitWavelengthRange => _info.LimitWaveRange;

	/// <summary>
	/// Gets the high limit of the scan over which to compute
	/// </summary>
	double IPeakPuritySettingsAccess.MaximumWavelength => _info.HighWavelength;

	/// <summary>
	/// Gets the low limit of the scan over which to compute
	/// </summary>
	double IPeakPuritySettingsAccess.MinimumWavelength => _info.LowWavelength;

	/// <summary>
	/// Gets the max of a scan must be greater than this to be included
	/// </summary>
	int IPeakPuritySettingsAccess.ScanThreshold => _info.ScanThreshold;

	/// <summary>
	/// Gets or sets the options.
	/// </summary>
	internal IProcessingMethodOptionsAccess Options { get; set; }

	/// <summary>
	/// Gets or sets the peak detection.
	/// </summary>
	internal QualitativePeakDetection PeakDetection { get; set; }

	/// <summary>
	/// Gets the amount of internal standard. Not used in any calculation yet.
	/// </summary>
	double IInternalStandardSettingsAccess.ISTDAmount => _info.BaseAmount;

	/// <summary>
	/// Gets the units for the internal standard
	/// </summary>
	string IInternalStandardSettingsAccess.ISTDUnits => IstdUnits;

	/// <summary>
	/// Gets the target compound settings.
	/// </summary>
	/// <value>The target compound settings.</value>
	ITargetCompoundSettingsAccess ICalibrationSettingsAccess.TargetCompoundSettings => this;

	/// <summary>
	/// Gets the internal standard settings.
	/// </summary>
	/// <value>The internal standard settings.</value>
	IInternalStandardSettingsAccess ICalibrationSettingsAccess.InternalStandardSettings => this;

	/// <summary>
	/// Gets a value which determines if this component is a target compound or an internal standard
	/// </summary>
	ComponentType ICalibrationSettingsAccess.ComponentType
	{
		get
		{
			if (_info.ComponentType != XcaliburComponentType.InternalStandard)
			{
				return ComponentType.TargetCompound;
			}
			return ComponentType.ISTD;
		}
	}

	/// <summary>
	/// Gets "Fit Threshold" defined as 
	/// Min fit threshold (0-1.0) for detection by spectral fit.
	/// This value is believed to be not currently used in Xcalibur code (may be for an older fit algorithm)?
	/// Returned for completeness only.
	/// </summary>
	public double FitThreshold => _info.FitThreshold;

	/// <summary>
	/// Gets the calibration and quantification data.
	/// </summary>
	public ICalibrationAndQuantificationThresholdLimitsAccess CalibrationAndQuantificationThresholdLimits { get; private set; }

	/// <summary>
	/// Gets the detection threshold limits.
	/// </summary>
	public IDetectionThresholdLimitsAccess DetectionThresholdLimits { get; private set; }

	/// <summary>
	/// Gets a value indicating whether this is used as a RT Reference for another component.
	/// </summary>
	public bool UseAsRtReference => _info.UseAsRTReference;

	/// <summary>
	/// Gets the number of points to be averaged in peak detection and integration.
	/// </summary>
	public int SmoothingPoints => _info.SmoothingPoints;

	/// <summary>
	/// Gets the suggested view width for displaying the chromatogram (seconds)
	/// </summary>
	public double DisplayWindowWidth => _info.DisplayWindowWidth;

	/// <summary>
	/// Gets a value which determines which peak detector to use with the component
	/// </summary>
	public PeakDetector PeakDetectionAlgorithm => _info.PeakDetectionAlgorithm;

	/// <summary>
	/// Gets the carry over limit threshold.
	/// </summary>
	/// <value>The carry over limit threshold.</value>
	double ICalibrationAndQuantificationThresholdLimitsAccess.CarryoverLimitThreshold => _info.CarryOverLimit;

	/// <summary>
	/// Gets the detection limit threshold.
	/// </summary>
	/// <value>The detection limit threshold.</value>
	double ICalibrationAndQuantificationThresholdLimitsAccess.DetectionLimitThreshold => _info.LimitDetection;

	/// <summary>
	/// Gets the linearity limit threshold.
	/// </summary>
	/// <value>The linearity limit threshold.</value>
	double ICalibrationAndQuantificationThresholdLimitsAccess.LinearityLimitThreshold => _info.LinearityLimit;

	/// <summary>
	/// Gets the quantitation limit threshold.
	/// </summary>
	/// <value>The quantitation limit threshold.</value>
	double ICalibrationAndQuantificationThresholdLimitsAccess.QuantitationLimitThreshold => _info.LimitQuantitation;

	/// <summary>
	/// Gets the R squared threshold.
	/// </summary>
	/// <value>The R squared threshold.</value>
	double ICalibrationAndQuantificationThresholdLimitsAccess.RSquaredThreshold => _info.RSquared;

	/// <summary>
	/// Gets the limit of reporting
	/// A value should only be reported if it is &gt;= the limit of reporting.
	/// This value is used to calculate the ReportingLimitPassed flag.
	/// </summary>
	double ICalibrationAndQuantificationThresholdLimitsAccess.LimitOfReporting => 0.0;

	/// <summary>
	/// Gets the Area limit threshold.
	/// </summary>
	/// <value>The Area limit threshold.</value>
	double IDetectionThresholdLimitsAccess.AreaThresholdLimit => _info.AreaFlagPercentThreshold;

	/// <summary>
	/// Gets the height limit threshold.
	/// </summary>
	/// <value>The height limit threshold.</value>
	double IDetectionThresholdLimitsAccess.HeightThresholdLimit => _info.HeightFlagThreshold;

	/// <summary>
	/// Gets a value indicating whether IRC tests are enabled
	/// </summary>
	bool IXcaliburIonRatioTestSettingsAccess.Enabled => _info.IonRatioConfirmation;

	/// <summary>
	/// Gets the "standard" used 
	/// </summary>
	int IXcaliburIonRatioTestSettingsAccess.Standard => _info.Standard;

	/// <summary>
	/// Gets the Ion Ratio method
	/// </summary>
	IonRatioMethod IXcaliburIonRatioTestSettingsAccess.Method => _info.IRCMethods;

	/// <summary>
	/// Gets the ion ratio window type
	/// </summary>
	XcaliburIonRatioWindowType IXcaliburIonRatioTestSettingsAccess.WindowType => _info.IRCWindowType;

	/// <summary>
	/// Gets the qualifier ion co-elution limits (minutes)
	/// </summary>
	double IXcaliburIonRatioTestSettingsAccess.QualifierIonCoelution => _info.QualifierionCoelution;

	/// <summary>
	/// Gets the table of masses for ion ratio testing
	/// </summary>
	ReadOnlyCollection<IIonRatioConfirmationTestAccess> IXcaliburIonRatioTestSettingsAccess.IonRatioConfirmationTests => _ionRatioConfirmationTests;

	/// <summary>
	/// Gets the settings for Ion Ration Confirmation
	/// </summary>
	public IXcaliburIonRatioTestSettingsAccess IonRatioConfirmation { get; private set; }

	/// <inheritdoc />
	public int InstrumentIndex => 1;

	/// <summary>
	/// load from file
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The offset into view after load
	/// </returns>
	/// <exception cref="T:System.NotSupportedException">Method using Surrogate
	/// </exception>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = Utilities.ReadStructure<ComponentInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		IonRatioConfirmation = this;
		IcisSettings = this;
		GenesisSettings = this;
		CalibrationSettings = this;
		SystemSuitabilitySettings = this;
		PeakPuritySettings = this;
		CalibrationAndQuantificationThresholdLimits = this;
		DetectionThresholdLimits = this;
		if (fileRevision < 25)
		{
			_info.Trace1Type = TraceType.TIC;
			_info.TraceOperator = TraceOperator.None;
			_info.Trace2Type = TraceType.TIC;
			_info.AreaFlagThreshold = 0.0;
			_info.HeightFlagThreshold = 0.0;
			_info.AreaFlagPercentThreshold = 0.0;
			_info.HeightFlagPercentThreshold = 0.0;
			_info.ForwardThreshold = 0;
			_info.ReverseThreshold = 0;
			_info.MatchThreshold = 0;
			_info.IonRatioConfirmation = false;
			_info.Standard = 1;
			_info.QualifierionCoelution = 0.025;
			_info.IRCMethods = IonRatioMethod.Manual;
			_info.IRCWindowType = XcaliburIonRatioWindowType.Relative;
			_info.DetectorDelay = 0.0;
			_info.RSquared = 0.995;
			_info.LimitDetection = 0.0;
			_info.LimitQuantitation = 0.0;
			_info.LinearityLimit = 1E+20;
			_info.CarryOverLimit = 1E+20;
			_info.SpikeAmount = 0.001;
			_info.SpikeUpperRecovery = 0.0;
			_info.SpikeLowerRecovery = 0.0;
			Keys = string.Empty;
		}
		if (fileRevision < 35)
		{
			_info.PeakDetectionAlgorithm = PeakDetector.Genesis;
			_info.IcisNoiseType = IcisNoiseType.Incos;
			_info.IcisMinPeakWidth = 3;
			_info.PeakNoiseFactor = 10;
			_info.BaselineWindow = 40;
			_info.MultipletResolution = 10;
			_info.AreaTailExtension = 5;
			_info.AreaNoiseFactor = 5;
			_info.AreaScanWindow = 0;
			_info.IcisConstrainPeak = false;
			_info.IcisPeakHeightPercentage = 5.0;
			_info.IcisTailingFactor = 1.0;
		}
		if (fileRevision < 38)
		{
			_info.EnableDetection = false;
			_info.ScanThreshold = 50;
			_info.DesiredPeakCoverage = 95.0;
			_info.LowWavelength = 190.0;
			_info.HighWavelength = 800.0;
		}
		if (fileRevision < 43)
		{
			_info.MassPrecision = 1;
			_info.MassTolerance = 500.0;
			_info.ToleranceUnits = OldLcqEnums.ToleranceUnit.Mmu;
			_info.InclRefAndExceptionPeaks = false;
		}
		ToleranceSettings = new MassOptions
		{
			Precision = _info.MassPrecision,
			Tolerance = _info.MassTolerance,
			ToleranceUnits = QualitativePeakDetection.ConvertLcqToleranceUnits(_info.ToleranceUnits)
		};
		_info.IRCMethods = IonRatioMethod.Manual;
		FilterScanEvent filterScanEvent = new FilterScanEvent(viewer.LoadRawFileObjectExt<Filter>(fileRevision, ref startPos), fromScan: false);
		ScanFilter = new WrappedScanFilter(filterScanEvent);
		FilterString = ScanFilter.ToString();
		Name = viewer.ReadStringExt(ref startPos);
		AdjustUsing = viewer.ReadStringExt(ref startPos);
		IstdUnits = viewer.ReadStringExt(ref startPos);
		MassRangesInfo = MassRangeStruct.LoadArray(viewer, ref startPos);
		if (fileRevision < 25)
		{
			_info.Trace1Type = ((MassRangesInfo.Length == 0) ? TraceType.TIC : TraceType.MassRange);
		}
		XcaliburComponentType componentType = _info.ComponentType;
		bool num = componentType == XcaliburComponentType.TargetCompound || componentType == XcaliburComponentType.Surrogate || (fileRevision >= 47 && componentType == XcaliburComponentType.InternalStandard);
		if (fileRevision <= 55)
		{
			_info.NoiseLoRange = 0.0;
			_info.NoiseHiRange = 0.0;
			_info.ManualRegionDetection = false;
			_info.RmsDetection = false;
		}
		if (num)
		{
			startPos = ReadCalibrationAndQcLevels(viewer, fileRevision, startPos);
		}
		else
		{
			CalibrationUnits = string.Empty;
			IstdReference = string.Empty;
			CalibrationLevels = new ReadOnlyCollection<ICalibrationLevelAccess>(Array.Empty<ICalibrationLevelAccess>());
			QcLevels = new ReadOnlyCollection<IQualityControlLevelAccess>(Array.Empty<IQualityControlLevelAccess>());
		}
		_massIntensityPairs = viewer.ReadStructArrayExt<MassIntensityPair>(ref startPos);
		List<SpectrumPoint> list = new List<SpectrumPoint>(_massIntensityPairs.Length);
		MassIntensityPair[] massIntensityPairs = _massIntensityPairs;
		for (int i = 0; i < massIntensityPairs.Length; i++)
		{
			MassIntensityPair massIntensityPair = massIntensityPairs[i];
			list.Add(new SpectrumPoint
			{
				Mass = massIntensityPair.Mass,
				Intensity = massIntensityPair.Intensity
			});
		}
		SpecPoints = new ReadOnlyCollection<SpectrumPoint>(list);
		if (fileRevision >= 25)
		{
			Keys = viewer.ReadStringExt(ref startPos);
			int num2 = viewer.ReadIntExt(ref startPos);
			IIonRatioConfirmationTestAccess[] array = new IIonRatioConfirmationTestAccess[num2];
			for (int j = 0; j < num2; j++)
			{
				array[j] = viewer.LoadRawFileObjectExt<IonRatioTest>(fileRevision, ref startPos);
			}
			_ionRatioConfirmationTests = new ReadOnlyCollection<IIonRatioConfirmationTestAccess>(array);
			int num3 = viewer.ReadIntExt(ref startPos);
			for (int k = 0; k < num3; k++)
			{
				viewer.LoadRawFileObjectExt<IonRatio>(fileRevision, ref startPos);
			}
			MassRangesInfo2 = MassRangeStruct.LoadArray(viewer, ref startPos);
			if (_info.ComponentType == XcaliburComponentType.Surrogate)
			{
				throw new NotSupportedException("Method using Surrogate");
			}
		}
		else
		{
			MassRangesInfo2 = Array.Empty<MassRangeStruct>();
			_ionRatioConfirmationTests = new ReadOnlyCollection<IIonRatioConfirmationTestAccess>(Array.Empty<IIonRatioConfirmationTestAccess>());
		}
		if (fileRevision >= 40)
		{
			int num4 = viewer.ReadIntExt(ref startPos);
			FalconEvents = viewer.ReadSimpleStructureArray<QualitativePeakDetection.FalconEvent>(startPos, num4);
			startPos += num4 * FalconEventSize;
		}
		else
		{
			FalconEvents = QualitativePeakDetection.InitialEvents();
		}
		_info.SmoothingPoints = Math.Max(1, _info.SmoothingPoints);
		return startPos - dataOffset;
	}

	/// <summary>
	/// read calibration and qc levels.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <param name="startPos">
	/// The start position.
	/// </param>
	/// <returns>
	/// The position after reading
	/// </returns>
	private long ReadCalibrationAndQcLevels(IMemoryReader viewer, int fileRevision, long startPos)
	{
		CalibrationUnits = viewer.ReadStringExt(ref startPos);
		viewer.ReadStringExt(ref startPos);
		IstdReference = viewer.ReadStringExt(ref startPos);
		int num = viewer.ReadIntExt(ref startPos);
		List<ICalibrationLevelAccess> list = new List<ICalibrationLevelAccess>(num);
		for (int i = 0; i < num; i++)
		{
			Level item = viewer.LoadRawFileObjectExt<Level>(fileRevision, ref startPos);
			list.Add(item);
		}
		CalibrationLevels = new ReadOnlyCollection<ICalibrationLevelAccess>(list);
		int num2 = viewer.ReadIntExt(ref startPos);
		List<IQualityControlLevelAccess> list2 = new List<IQualityControlLevelAccess>(num);
		for (int j = 0; j < num2; j++)
		{
			Level item2 = viewer.LoadRawFileObjectExt<Level>(fileRevision, ref startPos);
			list2.Add(item2);
		}
		QcLevels = new ReadOnlyCollection<IQualityControlLevelAccess>(list2);
		return startPos;
	}

	/// <summary>
	/// Get a copy of the find spectrum
	/// </summary>
	/// <returns>
	/// The spectrum to find
	/// </returns>
	SpectrumPoint[] IFindSettingsAccess.GetFindSpectrum()
	{
		return SpecPoints.ToArray();
	}

	/// <summary>
	/// Convert mass ranges.
	/// </summary>
	/// <param name="massRangesInfo">
	/// The mass ranges info.
	/// </param>
	/// <returns>
	/// The range array.
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

	static QuanComponent()
	{
		int[,] obj = new int[6, 2]
		{
			{ 55, 0 },
			{ 43, 0 },
			{ 38, 0 },
			{ 35, 0 },
			{ 25, 0 },
			{ 0, 0 }
		};
		obj[0, 1] = Marshal.SizeOf(typeof(ComponentInfo));
		obj[1, 1] = Marshal.SizeOf(typeof(ComponentInfoVersion5));
		obj[2, 1] = Marshal.SizeOf(typeof(ComponentInfoVersion4));
		obj[3, 1] = Marshal.SizeOf(typeof(ComponentInfoVersion3));
		obj[4, 1] = Marshal.SizeOf(typeof(ComponentInfoVersion2));
		obj[5, 1] = Marshal.SizeOf(typeof(ComponentInfoVersion1));
		MarshalledSizes = obj;
	}
}
