using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Ion Ratio Confirmation settings
/// </summary>
[Serializable]
[DataContract]
public class IonRatioConfirmationSettings : CommonCoreDataObject, IIonRatioConfirmationSettingsAccess, ICloneable
{
	private double _qualifierIonCoelution;

	private bool _enable;

	private IonRatioWindowType _windowsType;

	private ItemCollection<IonRatioConfirmationMassSettings> _qualifierIons = new ItemCollection<IonRatioConfirmationMassSettings>();

	/// <summary>
	/// Gets or sets Qualifier Ion Coelution
	/// The time the retention time can vary from the expected retention time for the ion to still be considered confirmed.
	/// Units: minutes
	/// Bounds: 0.000 - 0.100  
	/// </summary>
	[DataMember]
	public double QualifierIonCoelution
	{
		get
		{
			return _qualifierIonCoelution;
		}
		set
		{
			_qualifierIonCoelution = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this Ion Ratio Confirmation is enabled.
	/// </summary>
	/// <value><c>true</c> if enable; otherwise, <c>false</c>.</value>
	[DataMember]
	public bool Enable
	{
		get
		{
			return _enable;
		}
		set
		{
			_enable = value;
		}
	}

	/// <summary>
	/// Gets or sets the type of the windows.
	/// </summary>
	/// <value>The type of the windows.</value>
	[DataMember]
	public IonRatioWindowType WindowsType
	{
		get
		{
			return _windowsType;
		}
		set
		{
			_windowsType = value;
		}
	}

	/// <summary>
	/// Gets or sets the qualifier ions.
	/// </summary>
	/// <value>The qualifier ions.</value>
	[DataMember]
	public ItemCollection<IonRatioConfirmationMassSettings> QualifierIons
	{
		get
		{
			return _qualifierIons;
		}
		set
		{
			_qualifierIons = value;
		}
	}

	/// <summary>
	/// Gets the qualifier ions.
	/// </summary>
	[XmlIgnore]
	ReadOnlyCollection<IIonRatioConfirmationMassSettingsAccess> IIonRatioConfirmationSettingsAccess.QualifierIons
	{
		get
		{
			Collection<IIonRatioConfirmationMassSettingsAccess> collection = new Collection<IIonRatioConfirmationMassSettingsAccess>();
			if (QualifierIons != null)
			{
				foreach (IonRatioConfirmationMassSettings qualifierIon in QualifierIons)
				{
					collection.Add(qualifierIon);
				}
			}
			return new ReadOnlyCollection<IIonRatioConfirmationMassSettingsAccess>(collection);
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
		IonRatioConfirmationSettings ionRatioConfirmationSettings = (IonRatioConfirmationSettings)MemberwiseClone();
		ionRatioConfirmationSettings._qualifierIons = new ItemCollection<IonRatioConfirmationMassSettings>();
		foreach (IonRatioConfirmationMassSettings qualifierIon in _qualifierIons)
		{
			ionRatioConfirmationSettings._qualifierIons.Add((IonRatioConfirmationMassSettings)qualifierIon.Clone());
		}
		return ionRatioConfirmationSettings;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioConfirmationSettings" /> class. 
	/// Default constructor
	/// </summary>
	public IonRatioConfirmationSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioConfirmationSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public IonRatioConfirmationSettings(IIonRatioConfirmationSettingsAccess access)
	{
		if (access == null)
		{
			return;
		}
		Enable = access.Enable;
		QualifierIonCoelution = access.QualifierIonCoelution;
		WindowsType = access.WindowsType;
		_qualifierIons = new ItemCollection<IonRatioConfirmationMassSettings>();
		foreach (IIonRatioConfirmationMassSettingsAccess qualifierIon in access.QualifierIons)
		{
			_qualifierIons.Add(new IonRatioConfirmationMassSettings(qualifierIon));
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioConfirmationSettings" /> class.
	/// </summary>
	/// <param name="component">
	/// The component.
	/// </param>
	public IonRatioConfirmationSettings(IXcaliburComponentAccess component)
	{
		IXcaliburIonRatioTestSettingsAccess ionRatioConfirmation = component.IonRatioConfirmation;
		IIntegrationSettingsAccess integrationSettings = new IntegrationSettings();
		ISmoothingSettingsAccess smoothingSettings = new SmoothingSettings
		{
			SmoothingPoints = component.SmoothingPoints
		};
		Enable = ionRatioConfirmation.Enabled;
		_qualifierIons = new ItemCollection<IonRatioConfirmationMassSettings>();
		foreach (IIonRatioConfirmationTestAccess ionRatioConfirmationTest in ionRatioConfirmation.IonRatioConfirmationTests)
		{
			_qualifierIons.Add(new IonRatioConfirmationMassSettings(ionRatioConfirmationTest, integrationSettings, smoothingSettings));
		}
		QualifierIonCoelution = ionRatioConfirmation.QualifierIonCoelution;
		WindowsType = ((ionRatioConfirmation.WindowType != XcaliburIonRatioWindowType.Absolute) ? IonRatioWindowType.Relative : IonRatioWindowType.Absolute);
	}
}
