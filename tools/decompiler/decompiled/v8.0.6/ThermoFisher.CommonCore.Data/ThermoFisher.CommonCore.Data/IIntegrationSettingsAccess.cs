namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to integration settings
/// </summary>
public interface IIntegrationSettingsAccess
{
	/// <summary>
	/// Gets Settings for Avalon integrator
	/// </summary>
	IAvalonSettingsAccess Avalon { get; }

	/// <summary>
	/// Gets Settings for genesis integrator
	/// </summary>
	IGenesisSettingsAccess Genesis { get; }

	/// <summary>
	/// Gets Settings for Icis integrator
	/// </summary>
	IIcisSettingsAccess Icis { get; }

	/// <summary>
	/// Gets the choice of integrator to use
	/// </summary>
	PeakDetector PeakDetector { get; }
}
