using System;
using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Setting for all peak integrators, plus selection of integrator to use
/// </summary>
[Serializable]
[DataContract]
public class IntegrationSettings : CommonCoreDataObject, IIntegrationSettingsAccess, ICloneable
{
	private AvalonSettings _avalon = new AvalonSettings();

	private GenesisSettings _genesis = new GenesisSettings();

	private IcisSettings _icis = new IcisSettings();

	private PeakDetector _peakDetector;

	/// <summary>
	/// Gets or sets settings for Avalon integrator
	/// </summary>
	[DataMember]
	public AvalonSettings Avalon
	{
		get
		{
			return _avalon;
		}
		set
		{
			_avalon = value;
		}
	}

	/// <summary>
	/// Gets or sets settings for genesis integrator
	/// </summary>
	[DataMember]
	public GenesisSettings Genesis
	{
		get
		{
			return _genesis;
		}
		set
		{
			_genesis = value;
		}
	}

	/// <summary>
	/// Gets or sets settings for ICIS integrator
	/// </summary>
	[DataMember]
	public IcisSettings Icis
	{
		get
		{
			return _icis;
		}
		set
		{
			_icis = value;
		}
	}

	/// <summary>
	/// Gets or sets choice of integrator to use
	/// </summary>
	[DataMember]
	public PeakDetector PeakDetector
	{
		get
		{
			return _peakDetector;
		}
		set
		{
			_peakDetector = value;
		}
	}

	/// <summary>
	/// Gets Avalon.
	/// </summary>
	IAvalonSettingsAccess IIntegrationSettingsAccess.Avalon => _avalon;

	/// <summary>
	/// Gets Genesis.
	/// </summary>
	IGenesisSettingsAccess IIntegrationSettingsAccess.Genesis => _genesis;

	/// <summary>
	/// Gets ICIS settings.
	/// </summary>
	IIcisSettingsAccess IIntegrationSettingsAccess.Icis => _icis;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IntegrationSettings" /> class. 
	/// Default constructor
	/// </summary>
	public IntegrationSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IntegrationSettings" /> class. 
	/// Constructor, which should be used when not serializing, with the parameter set "true"
	/// </summary>
	/// <param name="initialize">
	/// When true: default events are added to the Avalon integrator.
	/// This is not needed when the object is about to be used by a serializer, as in that
	/// case the initial settings will already be in the serialized object
	/// </param>
	public IntegrationSettings(bool initialize)
	{
		if (initialize)
		{
			_avalon.ResetToDefaults();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IntegrationSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public IntegrationSettings(IIntegrationSettingsAccess access)
	{
		if (access != null)
		{
			Avalon = new AvalonSettings(access.Avalon);
			Genesis = new GenesisSettings(access.Genesis);
			Icis = new IcisSettings(access.Icis);
			PeakDetector = access.PeakDetector;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IntegrationSettings" /> class.
	/// </summary>
	/// <param name="access">
	/// The access.
	/// </param>
	public IntegrationSettings(IXcaliburComponentAccess access)
	{
		if (access != null)
		{
			Avalon = new AvalonSettings(access.IntegratorEvents);
			Genesis = new GenesisSettings(access.GenesisSettings);
			Icis = new IcisSettings(access.IcisSettings);
			PeakDetector = access.PeakDetectionAlgorithm;
		}
	}

	/// <summary>
	/// Make a deep copy of this object
	/// </summary>
	/// <returns>
	/// deep copy of peak detection settings
	/// </returns>
	public object Clone()
	{
		return new IntegrationSettings(this);
	}
}
