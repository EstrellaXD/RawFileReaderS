using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Settings for smoothing algorithm, useful for chromatograms etc.
/// </summary>
[Serializable]
[DataContract]
public class SmoothingSettings : CommonCoreDataObject, ICloneable, ISmoothingSettingsAccess
{
	private int _smoothingPoints = 1;

	private int _smoothRepeat = 1;

	private SmoothMethod _smoothMethod = SmoothMethod.Gaussian;

	/// <summary>
	/// Gets or sets the number of points for smoothing the chromatogram
	/// </summary>
	[DataMember]
	public int SmoothingPoints
	{
		get
		{
			return _smoothingPoints;
		}
		set
		{
			_smoothingPoints = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of times to repeat smoothing
	/// </summary>
	[DataMember]
	public int SmoothRepeat
	{
		get
		{
			return _smoothRepeat;
		}
		set
		{
			_smoothRepeat = value;
		}
	}

	/// <summary>
	/// Gets or sets the envelope shape used by smoothing algorithm
	/// </summary>
	[DataMember]
	public SmoothMethod SmoothMethod
	{
		get
		{
			return _smoothMethod;
		}
		set
		{
			_smoothMethod = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SmoothingSettings" /> class. 
	/// default constructor
	/// </summary>
	public SmoothingSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SmoothingSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public SmoothingSettings(ISmoothingSettingsAccess access)
	{
		if (access != null)
		{
			SmoothingPoints = access.SmoothingPoints;
			SmoothMethod = access.SmoothMethod;
			SmoothRepeat = access.SmoothRepeat;
		}
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	public object Clone()
	{
		return MemberwiseClone();
	}
}
