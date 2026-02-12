using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Parameters for the system suitability algorithm
/// </summary>
[Serializable]
[DataContract]
public class SystemSuitabilitySettings : CommonCoreDataObject, ISystemSuitabilitySettingsAccess, ICloneable
{
	private bool _enableResolutionChecks;

	private double _resolutionThreshold = 90.0;

	private bool _enableSymmetryChecks;

	private double _symmetryPeakHeight = 50.0;

	private double _symmetryThreshold = 80.0;

	private bool _enablePeakClassificationChecks;

	private double _peakWidthPeakHeight = 50.0;

	private double _minPeakWidth = 1.8;

	private double _maxPeakWidth = 3.6;

	private double _tailingPeakHeight = 10.0;

	private double _tailingFailureThreshold = 2.0;

	private double _columnOverloadPeakHeight = 50.0;

	private double _columnOverloadFailureThreshold = 1.5;

	private double _peakWidthsForNoiseDetection = 1.0;

	private double _signalToNoiseRatio = 20.0;

	/// <summary>
	/// Gets or sets a value indicating whether resolution checks will be performed
	/// </summary>
	[DataMember]
	public bool EnableResolutionChecks
	{
		get
		{
			return _enableResolutionChecks;
		}
		set
		{
			_enableResolutionChecks = value;
		}
	}

	/// <summary>
	/// Gets or sets a threshold value which determines if a peak's resolution or ok or not.
	/// The default value is 90%.
	/// Resolution is defined as the ratio:
	/// <para>100 × V/P</para>
	/// where:
	/// <para>V = depth of the Valley: the difference in intensity from the chromatogram at the apex of the target peak
	/// to the lowest point in the valley between the target peak and a neighboring peak</para>
	/// <para>P = Peak height: the height of the target peak, above the peak's baseline</para>
	/// </summary>
	[DataMember]
	public double ResolutionThreshold
	{
		get
		{
			return _resolutionThreshold;
		}
		set
		{
			_resolutionThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether peak symmetry checks are to be performed.
	/// Symmetry is determined at a specified peak height
	/// and is a measure of how even-sided a peak is
	/// about a perpendicular dropped from its apex.
	/// </summary>
	[DataMember]
	public bool EnableSymmetryChecks
	{
		get
		{
			return _enableSymmetryChecks;
		}
		set
		{
			_enableSymmetryChecks = value;
		}
	}

	/// <summary>
	/// Gets or sets the Peak Height at which symmetry is measured.
	/// The default value is 50%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	[DataMember]
	public double SymmetryPeakHeight
	{
		get
		{
			return _symmetryPeakHeight;
		}
		set
		{
			_symmetryPeakHeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the Symmetry Threshold
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
	[DataMember]
	public double SymmetryThreshold
	{
		get
		{
			return _symmetryThreshold;
		}
		set
		{
			_symmetryThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether peak classification checks are to be run
	/// </summary>
	[DataMember]
	public bool EnablePeakClassificationChecks
	{
		get
		{
			return _enablePeakClassificationChecks;
		}
		set
		{
			_enablePeakClassificationChecks = value;
		}
	}

	/// <summary>
	/// Gets or sets the Peak Height at which suitability tests the width of target peaks.
	/// You can enter any value within the range 0% to 100%. The default value is 50%. 
	/// </summary>
	[DataMember]
	public double PeakWidthPeakHeight
	{
		get
		{
			return _peakWidthPeakHeight;
		}
		set
		{
			_peakWidthPeakHeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the minimum peak width, at the specified peak height, for the peak width suitability test.
	/// The default value is 1.8. You can set any value in the range 0 to 30 seconds. 
	/// </summary>
	[DataMember]
	public double MinPeakWidth
	{
		get
		{
			return _minPeakWidth;
		}
		set
		{
			_minPeakWidth = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum peak width, at the specified peak height, for the peak width suitability test.
	/// The default value is 3.6. You can set any value in the range 0 to 30 seconds. 
	/// </summary>
	[DataMember]
	public double MaxPeakWidth
	{
		get
		{
			return _maxPeakWidth;
		}
		set
		{
			_maxPeakWidth = value;
		}
	}

	/// <summary>
	/// Gets or sets the Peak Height at which the algorithm measures the tailing of target peaks.
	/// The default SOP value is 10%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	[DataMember]
	public double TailingPeakHeight
	{
		get
		{
			return _tailingPeakHeight;
		}
		set
		{
			_tailingPeakHeight = value;
		}
	}

	/// <summary>
	///  Gets or sets the failure threshold for the tailing suitability test.
	///  The default SOP defined failure threshold is %lt 2 at 10% peak height. The valid range is 1 to 50.
	///  Tailing is calculated at the value defined in <see cref="P:ThermoFisher.CommonCore.Data.Business.SystemSuitabilitySettings.TailingPeakHeight" />.
	///  For the purposes of the test, a peak is considered to be excessively tailed if:
	///  <code>
	///  R / L &gt; Failure Threshold %
	///  where:
	///  L = the distance from the left side of the peak to the perpendicular dropped from the peak apex
	///  R = the distance from the right side of the peak to the perpendicular dropped from the peak apex
	///  Measurements of L and R are taken from the raw file without smoothing.</code>
	/// </summary>
	[DataMember]
	public double TailingFailureThreshold
	{
		get
		{
			return _tailingFailureThreshold;
		}
		set
		{
			_tailingFailureThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the Peak Height at which the algorithm measures column overloading.
	/// The default SOP value is 50%. You can enter any value within the range 0% to 100%. 
	/// </summary>
	[DataMember]
	public double ColumnOverloadPeakHeight
	{
		get
		{
			return _columnOverloadPeakHeight;
		}
		set
		{
			_columnOverloadPeakHeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the failure threshold value for the column overload suitability test.
	/// The default SOP defined threshold is 1.5 at 50% peak height. The valid range is 1 to 20.
	/// A peak is considered to be overloaded if:
	/// <code>
	/// L / R &gt; Failure Threshold %
	/// where:
	/// L = the distance from the left side of the peak to the perpendicular dropped from the peak apex
	/// R = the distance from the right side of the peak to the perpendicular dropped from the peak apex
	/// Measurements of L and R are taken from the raw file without smoothing. </code>
	/// </summary>
	[DataMember]
	public double ColumnOverloadFailureThreshold
	{
		get
		{
			return _columnOverloadFailureThreshold;
		}
		set
		{
			_columnOverloadFailureThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the Number of Peak Widths for Noise Detection testing parameter for
	/// the baseline clipping system suitability test.
	/// The default value is 1.0 and the permitted range is 0.1 to 10.
	/// A peak is considered to be baseline clipped if there is no signal
	/// (zero intensity) on either side of the peak within the specified
	/// number of peak widths. The range is truncated to the quantitation window
	/// if the specified number of peak widths extends beyond the window’s edge.
	/// </summary>
	[DataMember]
	public double PeakWidthsForNoiseDetection
	{
		get
		{
			return _peakWidthsForNoiseDetection;
		}
		set
		{
			_peakWidthsForNoiseDetection = value;
		}
	}

	/// <summary>
	/// Gets or sets the threshold for system suitability testing 
	/// of the signal-to-noise ratio. The default value is 20 and the
	/// permitted range is 1 to 500. The algorithm calculates the signal-to-noise ratio 
	/// within the quantitation window using only baseline signal.
	/// Any extraneous, minor, detected peaks are excluded from the calculation. 
	/// </summary>
	[DataMember]
	public double SignalToNoiseRatio
	{
		get
		{
			return _signalToNoiseRatio;
		}
		set
		{
			_signalToNoiseRatio = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SystemSuitabilitySettings" /> class. 
	/// default constructor
	/// </summary>
	public SystemSuitabilitySettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SystemSuitabilitySettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public SystemSuitabilitySettings(ISystemSuitabilitySettingsAccess access)
	{
		if (access != null)
		{
			ColumnOverloadFailureThreshold = access.ColumnOverloadFailureThreshold;
			ColumnOverloadPeakHeight = access.ColumnOverloadPeakHeight;
			EnablePeakClassificationChecks = access.EnablePeakClassificationChecks;
			EnableResolutionChecks = access.EnableResolutionChecks;
			EnableSymmetryChecks = access.EnableSymmetryChecks;
			MaxPeakWidth = access.MaxPeakWidth;
			MinPeakWidth = access.MinPeakWidth;
			PeakWidthPeakHeight = access.PeakWidthPeakHeight;
			PeakWidthsForNoiseDetection = access.PeakWidthsForNoiseDetection;
			ResolutionThreshold = access.ResolutionThreshold;
			SignalToNoiseRatio = access.SignalToNoiseRatio;
			SymmetryPeakHeight = access.SymmetryPeakHeight;
			SymmetryThreshold = access.SymmetryThreshold;
			TailingFailureThreshold = access.TailingFailureThreshold;
			TailingPeakHeight = access.TailingPeakHeight;
		}
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new SystemSuitabilitySettings(this);
	}
}
