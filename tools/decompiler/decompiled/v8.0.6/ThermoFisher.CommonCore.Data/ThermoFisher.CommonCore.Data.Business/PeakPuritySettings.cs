using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Settings PDA peak purity calculations
/// </summary>
[Serializable]
[DataContract]
public class PeakPuritySettings : CommonCoreDataObject, IPeakPuritySettingsAccess, ICloneable
{
	private double _desiredPeakCoverage;

	private bool _enableDetection;

	private bool _limitWavelengthRange;

	private double _maximumWavelength;

	private double _minimumWavelength;

	private int _scanThreshold;

	/// <summary>
	/// Gets or sets the % of the detected baseline.
	/// </summary>
	[DataMember]
	public double DesiredPeakCoverage
	{
		get
		{
			return _desiredPeakCoverage;
		}
		set
		{
			_desiredPeakCoverage = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to compute Peak Purity.
	/// </summary>
	[DataMember]
	public bool EnableDetection
	{
		get
		{
			return _enableDetection;
		}
		set
		{
			_enableDetection = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to use the enclosed wavelength range, not the total scan
	/// </summary>
	[DataMember]
	public bool LimitWavelengthRange
	{
		get
		{
			return _limitWavelengthRange;
		}
		set
		{
			_limitWavelengthRange = value;
		}
	}

	/// <summary>
	/// Gets or sets the high limit of the scan over which to compute
	/// </summary>
	[DataMember]
	public double MaximumWavelength
	{
		get
		{
			return _maximumWavelength;
		}
		set
		{
			_maximumWavelength = value;
		}
	}

	/// <summary>
	/// Gets or sets the low limit of the scan over which to compute
	/// </summary>
	[DataMember]
	public double MinimumWavelength
	{
		get
		{
			return _minimumWavelength;
		}
		set
		{
			_minimumWavelength = value;
		}
	}

	/// <summary>
	/// Gets or sets the max of a scan must be greater than this to be included
	/// </summary>
	[DataMember]
	public int ScanThreshold
	{
		get
		{
			return _scanThreshold;
		}
		set
		{
			_scanThreshold = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakPuritySettings" /> class. 
	/// Default constructor
	/// </summary>
	public PeakPuritySettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakPuritySettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public PeakPuritySettings(IPeakPuritySettingsAccess access)
	{
		if (access != null)
		{
			DesiredPeakCoverage = access.DesiredPeakCoverage;
			EnableDetection = access.EnableDetection;
			LimitWavelengthRange = access.LimitWavelengthRange;
			MaximumWavelength = access.MaximumWavelength;
			MinimumWavelength = access.MinimumWavelength;
			ScanThreshold = access.ScanThreshold;
		}
	}

	/// <summary>
	/// Make a copy of this object
	/// </summary>
	/// <returns>a copy of this object</returns>
	public object Clone()
	{
		return new PeakPuritySettings(this);
	}
}
