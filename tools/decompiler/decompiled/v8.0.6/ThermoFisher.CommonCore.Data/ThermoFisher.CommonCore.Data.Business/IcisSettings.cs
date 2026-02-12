using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// User parameters for the ICIS peak integration algorithm.
/// These settings determine how the chromatogram is integrated.
/// </summary>
[Serializable]
[DataContract]
public class IcisSettings : CommonCoreDataObject, IIcisSettingsAccess, ICloneable
{
	/// <summary>
	/// The _baseline window.
	/// </summary>
	private int _baselineWindow = 40;

	/// <summary>
	/// The _area noise factor.
	/// </summary>
	private int _areaNoiseFactor = 5;

	/// <summary>
	/// The _peak noise factor.
	/// </summary>
	private int _peakNoiseFactor = 10;

	/// <summary>
	/// The _constrain peak width.
	/// </summary>
	private bool _constrainPeakWidth;

	/// <summary>
	/// The _peak height percentage.
	/// </summary>
	private double _peakHeightPercentage = 5.0;

	/// <summary>
	/// The _tailing factor.
	/// </summary>
	private double _tailingFactor = 1.0;

	private int _minimumPeakWidth = 3;

	private int _multipletResolution = 10;

	private int _areaScanWindow;

	private int _areaTailExtension = 5;

	private bool _calculateNoiseAsRms;

	private IcisNoiseType _noiseMethod;

	/// <summary>
	/// Gets or sets the number of scans.
	/// Each scan is checked to see if it should be considered a baseline scan.
	/// This is determined by looking at a number of scans (BaselineWindow) before
	/// and after the a data point. If it is the lowest point in the group it will be
	/// marked as a "baseline" point.
	/// Range: 1 - 500
	/// Default: 40
	/// </summary>
	[DataMember]
	public int BaselineWindow
	{
		get
		{
			return _baselineWindow;
		}
		set
		{
			_baselineWindow = value;
		}
	}

	/// <summary>
	/// Gets or sets the Noise level multiplier.
	/// This determines the peak edge after the location of the possible peak,
	/// allowing the peak to narrow or broaden without affecting the baseline. 
	/// Range: 1 - 500
	/// Default multiplier: 5
	/// </summary>
	[DataMember]
	public int AreaNoiseFactor
	{
		get
		{
			return _areaNoiseFactor;
		}
		set
		{
			_areaNoiseFactor = value;
		}
	}

	/// <summary>
	/// Gets or sets the noise level multiplier (a minimum S/N ratio).
	/// This determines the potential peak signal threshold. 
	/// Range: 1 - 1000
	/// Default multiplier: 10
	/// </summary>
	[DataMember]
	public int PeakNoiseFactor
	{
		get
		{
			return _peakNoiseFactor;
		}
		set
		{
			_peakNoiseFactor = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to constrain the peak width of a detected peak (remove tailing)
	/// width is then restricted by specifying a peak height threshold and a tailing factor.
	/// </summary>
	[DataMember]
	public bool ConstrainPeakWidth
	{
		get
		{
			return _constrainPeakWidth;
		}
		set
		{
			_constrainPeakWidth = value;
		}
	}

	/// <summary>
	/// Gets or sets the percent of the total peak height (100%) that a signal needs to be above the baseline
	/// before integration is turned on or off.
	/// This applies only when the ConstrainPeak is true.
	/// The valid range is 0.0 to 100.0%.
	/// </summary>
	[DataMember]
	public double PeakHeightPercentage
	{
		get
		{
			return _peakHeightPercentage;
		}
		set
		{
			_peakHeightPercentage = value;
		}
	}

	/// <summary>
	/// Gets or sets the tailing factor.
	/// This controls how Genesis integrates the tail of a peak.
	/// This factor is the maximum ratio of the trailing edge to the leading side of a constrained peak.
	/// This applies only when the ConstrainPeak is true.
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
	/// Gets or sets the minimum number of scans required in a peak. 
	/// Range: 0 to 100. 
	/// Default: 3. 
	/// </summary>
	[DataMember]
	public int MinimumPeakWidth
	{
		get
		{
			return _minimumPeakWidth;
		}
		set
		{
			_minimumPeakWidth = value;
		}
	}

	/// <summary>
	///  Gets or sets the minimum separation in scans between the apexes of two potential peaks.
	///  This is a criterion to determine if two peaks are resolved.
	///  Enter a larger number in a noisy environment when the signal is bouncing around.
	///  Range: 1 to 500.
	///  Default: 10 scans. 
	/// </summary>
	[DataMember]
	public int MultipletResolution
	{
		get
		{
			return _multipletResolution;
		}
		set
		{
			_multipletResolution = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of scans on each side of the peak apex to be allowed. 
	/// Range: 0 to 100.
	/// Default: 0 scans.
	/// 0 specifies that all scans from peak-start to peak-end are to be included in the area integration.
	/// </summary>
	[DataMember]
	public int AreaScanWindow
	{
		get
		{
			return _areaScanWindow;
		}
		set
		{
			_areaScanWindow = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of scans past the peak endpoint to use in averaging the intensity.
	/// Range: 0 to 100. 
	/// Default: 5 scans.
	/// </summary>
	[DataMember]
	public int AreaTailExtension
	{
		get
		{
			return _areaTailExtension;
		}
		set
		{
			_areaTailExtension = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether noise is calculated using an RMS method
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
	/// Gets or sets a value which determines how the ICIS peak detector  determines which signals are noise.
	/// The selected points can  determine a noise level, or be fed into an RMS calculator,
	/// depending on the RMS setting.
	/// </summary>
	[DataMember]
	public IcisNoiseType NoiseMethod
	{
		get
		{
			return _noiseMethod;
		}
		set
		{
			_noiseMethod = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IcisSettings" /> class. 
	/// Default constructor
	/// </summary>
	public IcisSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IcisSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public IcisSettings(IIcisSettingsAccess access)
	{
		if (access != null)
		{
			AreaNoiseFactor = access.AreaNoiseFactor;
			AreaScanWindow = access.AreaScanWindow;
			AreaTailExtension = access.AreaTailExtension;
			BaselineWindow = access.BaselineWindow;
			CalculateNoiseAsRms = access.CalculateNoiseAsRms;
			ConstrainPeakWidth = access.ConstrainPeakWidth;
			MinimumPeakWidth = access.MinimumPeakWidth;
			MultipletResolution = access.MultipletResolution;
			NoiseMethod = access.NoiseMethod;
			PeakHeightPercentage = access.PeakHeightPercentage;
			PeakNoiseFactor = access.PeakNoiseFactor;
			TailingFactor = access.TailingFactor;
		}
	}

	/// <summary>
	/// make a copy of this object
	/// </summary>
	/// <returns>a copy of this object</returns>
	public object Clone()
	{
		return new IcisSettings(this);
	}
}
