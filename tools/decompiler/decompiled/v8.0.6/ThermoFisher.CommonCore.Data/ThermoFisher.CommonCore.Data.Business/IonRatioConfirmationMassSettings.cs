using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Parameters to the Ion Ratio confirmation algorithm, for a single mass. 
/// </summary>
[Serializable]
[DataContract]
public class IonRatioConfirmationMassSettings : CommonCoreDataObject, ICloneable, IIonRatioConfirmationMassSettingsAccess, IIonRatioConfirmationTestAccess
{
	private double _mz;

	private double _targetRatio;

	private double _windowPercent;

	private IntegrationSettings _integrationChoice;

	private SmoothingSettings _smoothingData;

	/// <summary>
	/// Gets or sets the smoothing data for the ion ratio peak calculation.
	/// This is the only interaction with m_smoothingPoints.  This class is only
	/// a place holder.  Other  users of this class will fill this data item and
	/// use the settings.
	/// </summary>
	/// <value>The smoothing points.</value>
	[DataMember]
	public SmoothingSettings SmoothingData
	{
		get
		{
			return _smoothingData;
		}
		set
		{
			_smoothingData = value;
		}
	}

	/// <summary>
	/// Gets the integration choice item.  This is the only interaction 
	/// with m_integrationChoice. This class is only a place holder.  Other
	/// users of this class will fill this data item and use the settings.
	/// </summary>
	/// <value>The integration choice item.</value>
	public IIntegrationSettingsAccess IntegrationSettings => IntegrationChoiceItem;

	/// <summary>
	/// Gets or sets the integration choice item.  This is the only interaction 
	/// with m_integrationChoice. This class is only a place holder. Other
	/// users of this class will fill this data item and use the settings.
	/// </summary>
	/// <value>The integration choice item.</value>
	[DataMember]
	public IntegrationSettings IntegrationChoiceItem
	{
		get
		{
			return _integrationChoice;
		}
		set
		{
			_integrationChoice = value;
		}
	}

	/// <summary>
	/// Gets the smoothing data for the ion ratio peak calculation.
	/// This is the only interaction with m_smoothingPoints.  This class is only
	/// a place holder.  Other  users of this class will fill this data item and
	/// use the settings.
	/// </summary>
	/// <value>The smoothing points.</value>
	ISmoothingSettingsAccess IIonRatioConfirmationMassSettingsAccess.SmoothingData => SmoothingData;

	/// <summary>
	/// Gets or sets mass to be tested
	/// </summary>
	[DataMember]
	public double MZ
	{
		get
		{
			return _mz;
		}
		set
		{
			_mz = value;
		}
	}

	/// <summary>
	/// Gets or sets the Expected ratio 
	/// The ratio of the qualifier ion response to the component ion response. 
	/// Range: 0 - 200%
	/// </summary>
	[DataMember]
	public double TargetRatio
	{
		get
		{
			return _targetRatio;
		}
		set
		{
			_targetRatio = value;
		}
	}

	/// <summary>
	/// Gets or sets a Window determine how accurate the match must be
	/// The ratio must be +/- this percentage.
	/// </summary>
	[DataMember]
	public double WindowPercent
	{
		get
		{
			return _windowPercent;
		}
		set
		{
			_windowPercent = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioConfirmationMassSettings" /> class. 
	/// Default constructor
	/// </summary>
	public IonRatioConfirmationMassSettings()
	{
		Construct(initialize: false);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioConfirmationMassSettings" /> class. 
	/// Constructor, which should be used when not serializing, with the parameter set "true"
	/// </summary>
	/// <param name="initialize">
	/// When true: default events are added to the Avalon integrator.
	/// This is not needed when the object is about to be used by a serializer, as in that
	/// case the initial settings will already be in the serialized object
	/// </param>
	public IonRatioConfirmationMassSettings(bool initialize)
	{
		Construct(initialize);
	}

	/// <summary>
	/// The construct.
	/// </summary>
	/// <param name="initialize">
	/// The initialize.
	/// </param>
	private void Construct(bool initialize)
	{
		_integrationChoice = new IntegrationSettings(initialize);
		_smoothingData = new SmoothingSettings();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioConfirmationMassSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public IonRatioConfirmationMassSettings(IIonRatioConfirmationMassSettingsAccess access)
	{
		if (access == null)
		{
			Construct(initialize: true);
			return;
		}
		_integrationChoice = new IntegrationSettings(access.IntegrationSettings);
		MZ = access.MZ;
		SmoothingData = new SmoothingSettings(access.SmoothingData);
		TargetRatio = access.TargetRatio;
		WindowPercent = access.WindowPercent;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioConfirmationMassSettings" /> class.
	/// </summary>
	/// <param name="access">
	/// The access to mass test.
	/// </param>
	/// <param name="integrationSettings">Peak integration settings</param>
	/// <param name="smoothingSettings">Smoothing settings</param>
	public IonRatioConfirmationMassSettings(IIonRatioConfirmationTestAccess access, IIntegrationSettingsAccess integrationSettings, ISmoothingSettingsAccess smoothingSettings)
	{
		MZ = access.MZ;
		TargetRatio = access.TargetRatio;
		WindowPercent = access.WindowPercent;
		IntegrationChoiceItem = new IntegrationSettings(integrationSettings);
		SmoothingData = new SmoothingSettings(smoothingSettings);
	}

	/// <summary>
	/// Copy all settings from this object to another
	/// </summary>
	/// <param name="settings">
	/// Destination of copy
	/// </param>
	public void CopyTo(IonRatioConfirmationMassSettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		settings._mz = _mz;
		settings._targetRatio = _targetRatio;
		settings._windowPercent = _windowPercent;
		settings._smoothingData = (SmoothingSettings)_smoothingData.Clone();
		settings._integrationChoice = (IntegrationSettings)_integrationChoice.Clone();
	}

	/// <summary>
	/// Copy all settings to this object from another
	/// </summary>
	/// <param name="massListSettings">
	/// Source of copy
	/// </param>
	public void CopyFrom(IIonRatioConfirmationMassSettingsAccess massListSettings)
	{
		IntegrationChoiceItem = new IntegrationSettings(massListSettings.IntegrationSettings);
		MZ = massListSettings.MZ;
		TargetRatio = massListSettings.TargetRatio;
		WindowPercent = massListSettings.WindowPercent;
		SmoothingData = new SmoothingSettings(massListSettings.SmoothingData);
	}

	/// <summary>
	/// Copies the base values.
	/// Creates a deep copy.
	/// </summary>
	/// <returns>
	/// Cloned object
	/// </returns>
	public object Clone()
	{
		IonRatioConfirmationMassSettings obj = (IonRatioConfirmationMassSettings)MemberwiseClone();
		obj._smoothingData = (SmoothingSettings)_smoothingData.Clone();
		obj._integrationChoice = (IntegrationSettings)_integrationChoice.Clone();
		return obj;
	}
}
