using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// These settings detail how a peak is to be identified from the
/// list of possible peaks found by integrating a chromatogram.
/// The peak is expected at a certain time, which may be adjusted using a retention time reference.
/// The peak is then selected from possible peaks within a window around the selected time.
/// </summary>
[Serializable]
[DataContract]
public class PeakLocationSettings : CommonCoreDataObject, IPeakLocationSettingsAccess, ICloneable
{
	private double _signalToNoiseThreshold;

	private bool _adjustExpectedRt;

	private double _userEnteredRt;

	private PeakMethod _locateMethod = PeakMethod.Nearest;

	private double _searchWindow;

	private double _baselineAndNoiseWindow = 2.0;

	private FindSettings _findSettings = new FindSettings();

	/// <summary>
	/// Gets or sets a value indicating whether retention time should be adjusted based on a reference peak.
	/// </summary>
	[DataMember]
	public bool AdjustExpectedRT
	{
		get
		{
			return _adjustExpectedRt;
		}
		set
		{
			_adjustExpectedRt = value;
		}
	}

	/// <summary>
	/// Gets or sets the expected time, as in the method (before any adjustments)
	/// </summary>
	[DataMember]
	public double UserEnteredRT
	{
		get
		{
			return _userEnteredRt;
		}
		set
		{
			_userEnteredRt = value;
		}
	}

	/// <summary>
	/// Gets or sets a value which determine how a single peak is found from the list of
	/// returned peaks from integrating the chromatogram.
	/// For example: Highest peak in time window.
	/// </summary>
	[DataMember]
	public PeakMethod LocateMethod
	{
		get
		{
			return _locateMethod;
		}
		set
		{
			_locateMethod = value;
		}
	}

	/// <summary>
	/// Gets or sets the window, centered around the peak, in minutes.
	/// The located peak must be within a window of expected +/- width.
	/// </summary>
	[DataMember]
	public double SearchWindow
	{
		get
		{
			return _searchWindow;
		}
		set
		{
			_searchWindow = value;
		}
	}

	/// <summary>
	/// Gets or sets a setting which is used to restrict the chromatogram.
	/// Only scans within the range "adjusted expected RT" +/- Window are processed.
	/// For example: a 1 minute window setting implies 2 minutes of data.
	/// </summary>
	[DataMember]
	public double BaselineAndNoiseWindow
	{
		get
		{
			return _baselineAndNoiseWindow;
		}
		set
		{
			_baselineAndNoiseWindow = value;
		}
	}

	/// <summary>
	/// Gets or sets settings for finding a peak based on spectral fit
	/// </summary>
	[DataMember]
	public FindSettings FindSettings
	{
		get
		{
			return _findSettings;
		}
		set
		{
			_findSettings = value;
		}
	}

	/// <summary>
	/// Gets settings for finding a peak based on spectral fit
	/// </summary>
	IFindSettingsAccess IPeakLocationSettingsAccess.FindSettings => _findSettings;

	/// <summary>
	/// Gets or sets a rejection parameter for peaks
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
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakLocationSettings" /> class. 
	/// Default constructor
	/// </summary>
	public PeakLocationSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakLocationSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public PeakLocationSettings(IPeakLocationSettingsAccess access)
	{
		if (access != null)
		{
			_adjustExpectedRt = access.AdjustExpectedRT;
			_baselineAndNoiseWindow = access.BaselineAndNoiseWindow;
			_findSettings = new FindSettings(access.FindSettings);
			_locateMethod = access.LocateMethod;
			_searchWindow = access.SearchWindow;
			_signalToNoiseThreshold = access.SignalToNoiseThreshold;
			_userEnteredRt = access.UserEnteredRT;
		}
	}

	/// <summary>
	/// make a copy of this object
	/// </summary>
	/// <returns>a copy of this object</returns>
	public object Clone()
	{
		return new PeakLocationSettings(this);
	}
}
