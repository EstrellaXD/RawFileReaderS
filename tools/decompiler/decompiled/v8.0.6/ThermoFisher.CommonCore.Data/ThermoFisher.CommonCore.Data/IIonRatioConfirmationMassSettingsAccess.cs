namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to Ion Ration Confirmation masses
/// </summary>
public interface IIonRatioConfirmationMassSettingsAccess : IIonRatioConfirmationTestAccess
{
	/// <summary>
	/// Gets the smoothing data for the ion ratio peak calculation.
	/// </summary>
	/// <value>The smoothing points.</value>
	ISmoothingSettingsAccess SmoothingData { get; }

	/// <summary>
	/// Gets the integration choice item.  This is the only interaction 
	/// with m_integrationChoice.  This class is only a place holder.  Other
	/// users of this class will fill this data item and use the settings.
	/// </summary>
	/// <value>The integration choice item.</value>
	IIntegrationSettingsAccess IntegrationSettings { get; }
}
