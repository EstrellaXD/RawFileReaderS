using System;
using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// User parameters for the Genesis peak integration algorithm.
/// These settings determine how the chromatogram is integrated.
/// </summary>
[Serializable]
[DataContract]
public class GenesisSettings : CommonCoreDataObject, IGenesisSettingsAccess
{
	/// <summary>
	/// Constrain the width of the peak. Default false
	/// </summary>
	private bool _constrainPeak;

	/// <summary>
	/// Expected peak width (seconds). default 0.0
	/// </summary>
	private double _expectedPeakWidth;

	/// <summary>
	/// Percent of the total peak height (100%) that a signal needs to be above the baseline. Default 5%.
	/// </summary>
	private double _peakHeightPercent = 5.0;

	/// <summary>
	/// edge detection Signal to Noise threshold. default 0.5
	/// </summary>
	private double _signalToNoiseThreshold = 0.5;

	/// <summary>
	/// which controls how Genesis integrates the tail of a peak. default 1.0
	/// </summary>
	private double _tailingFactor = 1.0;

	/// <summary>
	/// perform base to base integration? default false
	/// </summary>
	private bool _valleyDetection;

	/// <summary>
	///  how far down from the peak apex a valley must be. default 200
	/// </summary>
	private double _peakSignalToNoiseRatioCutoff = 200.0;

	/// <summary>
	/// percentage of the valley bottom
	/// that the peak trace can rise above a baseline (before or after the peak). default 10%
	/// </summary>
	private double _risePercent = 10.0;

	/// <summary>
	///  How deep a valley must be. default 1.0
	/// </summary>
	private double _valleyDepth = 1.0;

	/// <summary>
	/// Should noise be calculated as RMS? default true
	/// </summary>
	private bool _calculateNoiseAsRms = true;

	/// <summary>
	/// The higher the baseline noise tolerance value,
	/// the higher the baseline is drawn through the noise data. default 10.0
	/// </summary>
	private double _baselineNoiseTolerance = 10.0;

	/// <summary>
	/// minimum number of scans that Genesis uses to calculate a baseline. default 16.
	/// </summary>
	private int _minScansInBaseline = 16;

	/// <summary>
	/// controls the width of the RMS noise band above and below the peak detection baseline.  default 2.0
	/// </summary>
	private double _baselineNoiseRejectionFactor = 2.0;

	/// <summary>
	/// minutes between background scan recalculations. default 5.0
	/// </summary>
	private double _backgroundUpdateRate = 5.0;

	/// <summary>
	/// The smallest permitted signal to noise ratio. Default 0.5. range in UI 0.0 to 999.
	/// </summary>
	private double _baseSignalToNoiseRatio = 0.5;

	/// <summary>
	/// lowest permitted percentage of the largest peak. Default 10.0.
	/// </summary>
	private double _percentLargestPeak = 10.0;

	/// <summary>
	///  peaks are filtered by relative signal height? default false.
	/// </summary>
	private bool _filterByRelativePeakHeight;

	/// <summary>
	/// Gets or sets a value indicating whether to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	[DataMember]
	public bool ConstrainPeak
	{
		get
		{
			return _constrainPeak;
		}
		set
		{
			_constrainPeak = value;
		}
	}

	/// <summary>
	/// Gets or sets the expected peak width (seconds).
	/// This controls the minimum width that a peak is expected to have (seconds)
	/// if valley detection is enabled. The property is expressed as a window.
	/// With valley detection enabled,
	/// any valley points nearer than  [expected width]/2
	/// to the top of the peak are ignored.
	/// If a valley point is found outside the expected peak width,
	/// Genesis terminates the peak at that point.
	/// Genesis always terminates a peak when the signal reaches the baseline,
	/// independent of the value set for the ExpectedPeakWidth.
	/// </summary>
	[DataMember]
	public double ExpectedPeakWidth
	{
		get
		{
			return _expectedPeakWidth;
		}
		set
		{
			_expectedPeakWidth = value;
		}
	}

	/// <summary>
	/// Gets or sets the percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the <c>ConstrainPeak</c> is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	[DataMember]
	public double PeakHeightPercent
	{
		get
		{
			return _peakHeightPercent;
		}
		set
		{
			_peakHeightPercent = value;
		}
	}

	/// <summary>
	/// Gets or sets the edge detection Signal to Noise threshold.
	/// This displayed as "S/N Threshold" in product UI.
	/// Larger values cause peaks to become narrower.
	/// A peak is considered ended if the following condition is met:
	/// <c>height &lt;= (BaseNoise * SignalToNoiseThreshold))</c>
	/// Where BaseNoise is the calculated noise on the fitted baseline,
	/// and height is the height above baseline.
	/// </summary>
	[DataMember]
	public double SignalToNoiseThreshold
	{
		get
		{
			return _signalToNoiseThreshold;
		}
		set
		{
			_signalToNoiseThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the Tailing Factor, which controls how Genesis integrates the tail of a peak.
	/// This factor is the maximum ratio of the trailing edge to the leading side of a constrained peak.
	/// This applies only when the <see cref="P:ThermoFisher.CommonCore.Data.Business.GenesisSettings.ConstrainPeak" /> is true.
	/// The valid range is 0.5 through 9.0. 
	/// </summary>
	[DataMember]
	public double TailingFactor
	{
		get
		{
			return _tailingFactor;
		}
		set
		{
			_tailingFactor = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to perform base to base integration
	/// </summary>
	[DataMember]
	public bool ValleyDetection
	{
		get
		{
			return _valleyDetection;
		}
		set
		{
			_valleyDetection = value;
		}
	}

	/// <summary>
	/// Gets or sets a value determining how far down from the peak apex a valley must be.
	/// The peak edge is set to values below this defined ratio. 
	/// This test assumes an edge of a peak is found when the baseline adjusted height of the edge is less than
	/// the ratio of the baseline adjusted apex height and the peak S/N cutoff ratio. 
	/// If the S/N at the apex is 500 and the peak S/N cutoff value is 200,
	/// Genesis defines the right and left edges of the peak when the S/N reaches a value less than 200.
	/// Range: 50.0 to 10000.0. 
	/// Technical equation:<c>if height &lt; (1/PeakSignalToNoiseRatioCutoff)*height(apex) =&gt; valley here</c>
	/// </summary>
	[DataMember]
	public double PeakSignalToNoiseRatioCutoff
	{
		get
		{
			return _peakSignalToNoiseRatioCutoff;
		}
		set
		{
			_peakSignalToNoiseRatioCutoff = value;
		}
	}

	/// <summary>
	/// Gets or sets the percentage of the valley bottom
	/// that the peak trace can rise above a baseline (before or after the peak). 
	/// If the trace exceeds RisePercent,
	/// Genesis applies valley detection peak integration criteria. 
	/// This method drops a vertical line from the apex of the valley between unresolved
	/// peaks to the baseline.
	/// The intersection of the vertical line and the baseline defines the end of the first
	/// peak and the beginning of the second peak. 
	/// This test is applied to both the left and right edges of the peak. 
	/// The RisePercent criteria is useful for integrating peaks with long tails.
	/// Useful range: 0.1 to 50
	/// </summary>
	[DataMember]
	public double RisePercent
	{
		get
		{
			return _risePercent;
		}
		set
		{
			_risePercent = value;
		}
	}

	/// <summary>
	/// Gets or sets a value determining how deep a valley must be.
	/// The range is 1.0 to 100.0. for valley detection.
	/// Technical equation:<c>height(here +/- VALLEY_WIDTH) &gt; ValleyDepth*SNR+height(here) =&gt; valley here</c>
	/// </summary>
	[DataMember]
	public double ValleyDepth
	{
		get
		{
			return _valleyDepth;
		}
		set
		{
			_valleyDepth = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to enable RMS noise calculation.
	/// If not set, noise is calculated peak to peak.
	/// It is set by default.
	/// </summary>
	[DataMember]
	public bool CalculateNoiseAsRms
	{
		get
		{
			return _calculateNoiseAsRms;
		}
		set
		{
			_calculateNoiseAsRms = value;
		}
	}

	/// <summary>
	/// Gets or sets the Baseline noise tolerance.
	/// controls how the baseline is drawn in the noise data.
	/// The higher the baseline noise tolerance value,
	/// the higher the baseline is drawn through the noise data.
	/// The valid range is 0.0 to 100.0
	/// </summary>
	[DataMember]
	public double BaselineNoiseTolerance
	{
		get
		{
			return _baselineNoiseTolerance;
		}
		set
		{
			_baselineNoiseTolerance = value;
		}
	}

	/// <summary>
	/// Gets or sets the minimum number of scans that Genesis uses to calculate a baseline.
	/// A larger number includes more data in determining an averaged baseline.
	/// The valid range is 2 to 100.0.
	/// </summary>
	[DataMember]
	public int MinScansInBaseline
	{
		get
		{
			return _minScansInBaseline;
		}
		set
		{
			_minScansInBaseline = value;
		}
	}

	/// <summary>
	/// Gets or sets a factor which controls the width of the RMS noise band above and below the peak detection baseline
	/// and is applied to the raw RMS noise values to raise the effective RMS noise during peak detection.
	/// The left and right peak boundaries are assigned above the noise and, therefore,
	/// closer to the peak apex value in minutes. 
	/// This action effectively raises the peak integration baseline above the RMS noise level. 
	/// Range: 0.1 to 10.0.
	/// Default: 2.0.
	/// </summary>
	[DataMember]
	public double BaselineNoiseRejectionFactor
	{
		get
		{
			return _baselineNoiseRejectionFactor;
		}
		set
		{
			_baselineNoiseRejectionFactor = value;
		}
	}

	/// <summary>
	/// Gets or sets the minutes between background scan recalculations.
	/// Baseline is refitted each time this interval elapses. 
	/// </summary>
	[DataMember]
	public double BackgroundUpdateRate
	{
		get
		{
			return _backgroundUpdateRate;
		}
		set
		{
			_backgroundUpdateRate = value;
		}
	}

	/// <summary>
	/// Gets or sets the smallest permitted signal to noise ratio.
	/// Peaks are rejected if they have a lower signal to noise ratio than this.
	/// </summary>
	[DataMember]
	public double BaseSignalToNoiseRatio
	{
		get
		{
			return _baseSignalToNoiseRatio;
		}
		set
		{
			_baseSignalToNoiseRatio = value;
		}
	}

	/// <summary>
	/// Gets or sets the lowest permitted percentage of the largest peak.
	/// Do not return peaks which are less than this % of the highest peak above baseline.
	/// </summary>
	[DataMember]
	public double PercentLargestPeak
	{
		get
		{
			return _percentLargestPeak;
		}
		set
		{
			_percentLargestPeak = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether peaks are filtered by relative signal height
	/// </summary>
	[DataMember]
	public bool FilterByRelativePeakHeight
	{
		get
		{
			return _filterByRelativePeakHeight;
		}
		set
		{
			_filterByRelativePeakHeight = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.GenesisSettings" /> class. 
	/// Default constructor
	/// </summary>
	public GenesisSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.GenesisSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public GenesisSettings(IGenesisSettingsAccess access)
	{
		if (access != null)
		{
			BackgroundUpdateRate = access.BackgroundUpdateRate;
			BaselineNoiseRejectionFactor = access.BaselineNoiseRejectionFactor;
			BaselineNoiseTolerance = access.BaselineNoiseTolerance;
			BaseSignalToNoiseRatio = access.BaseSignalToNoiseRatio;
			CalculateNoiseAsRms = access.CalculateNoiseAsRms;
			ConstrainPeak = access.ConstrainPeak;
			ExpectedPeakWidth = access.ExpectedPeakWidth;
			FilterByRelativePeakHeight = access.FilterByRelativePeakHeight;
			MinScansInBaseline = access.MinScansInBaseline;
			PeakHeightPercent = access.PeakHeightPercent;
			PeakSignalToNoiseRatioCutoff = access.PeakSignalToNoiseRatioCutoff;
			PercentLargestPeak = access.PercentLargestPeak;
			RisePercent = access.RisePercent;
			SignalToNoiseThreshold = access.SignalToNoiseThreshold;
			TailingFactor = access.TailingFactor;
			ValleyDepth = access.ValleyDepth;
			ValleyDetection = access.ValleyDetection;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.GenesisSettings" /> class.
	/// Maps settings from an Xcalibur processing method.
	/// </summary>
	/// <param name="access">
	/// The access.
	/// </param>
	public GenesisSettings(IGenesisRawSettingsAccess access)
	{
		if (access != null)
		{
			BackgroundUpdateRate = access.BackgroundUpdateRate;
			BaselineNoiseRejectionFactor = access.BaselineNoiseRejectionFactor;
			BaselineNoiseTolerance = access.BaselineNoiseLimit * 100.0;
			SignalToNoiseThreshold = access.BaseSignalToNoiseRatio;
			BaseSignalToNoiseRatio = access.SignalToNoiseThreshold;
			CalculateNoiseAsRms = access.CalculateNoiseAsRms;
			ConstrainPeak = access.ConstrainPeak;
			ExpectedPeakWidth = access.ExpectedPeakWidth;
			FilterByRelativePeakHeight = access.FilterByRelativePeakHeight;
			MinScansInBaseline = access.MinScansInBaseline;
			PeakHeightPercent = access.PeakHeightPercent;
			double peakSignalToNoiseRatioCutoff = access.PeakSignalToNoiseRatioCutoff;
			if (peakSignalToNoiseRatioCutoff > 0.0)
			{
				PeakSignalToNoiseRatioCutoff = 1.0 / peakSignalToNoiseRatioCutoff;
			}
			PercentLargestPeak = access.PercentLargestPeak;
			RisePercent = (int)(1000000.0 * (access.ValleyThreshold - 1.0)) / 10000;
			SignalToNoiseThreshold = access.SignalToNoiseThreshold;
			TailingFactor = access.TailingFactor;
			ValleyDepth = access.ValleyDepth;
			ValleyDetection = access.ValleyDetection;
		}
	}

	/// <summary>
	/// make a copy of this object
	/// </summary>
	/// <returns>A copy of this object</returns>
	public object Clone()
	{
		return new GenesisSettings(this);
	}
}
