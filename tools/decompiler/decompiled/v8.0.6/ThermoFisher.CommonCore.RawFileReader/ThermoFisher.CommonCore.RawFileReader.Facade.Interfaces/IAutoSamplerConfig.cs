using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The AutoSamplerConfig interface.
/// </summary>
internal interface IAutoSamplerConfig
{
	/// <summary>
	/// Gets the tray index.
	/// </summary>
	int TrayIndex { get; }

	/// <summary>
	/// Gets the tray name.
	/// </summary>
	string TrayName { get; }

	/// <summary>
	/// Gets the tray shape.
	/// </summary>
	TrayShape TrayShape { get; }

	/// <summary>
	/// Gets the vial index.
	/// </summary>
	int VialIndex { get; }

	/// <summary>
	/// Gets the vials per tray.
	/// </summary>
	int VialsPerTray { get; }

	/// <summary>
	/// Gets the vials per tray x.
	/// </summary>
	int VialsPerTrayX { get; }

	/// <summary>
	/// Gets the vials per tray y.
	/// </summary>
	int VialsPerTrayY { get; }
}
