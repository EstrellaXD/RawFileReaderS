namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The AutoSamplerInformation interface.
/// </summary>
public interface IAutoSamplerInformation
{
	/// <summary>
	/// Gets or sets the tray index, -1 for "not recorded"
	/// </summary>
	int TrayIndex { get; set; }

	/// <summary>
	/// Gets or sets the vial index, -1 for "not recorded"
	/// </summary>
	int VialIndex { get; set; }

	/// <summary>
	/// Gets or sets the number of vials (or wells) per tray.
	/// -1 for "not recorded"
	/// </summary>
	int VialsPerTray { get; set; }

	/// <summary>
	/// Gets or sets the number of vials (or wells) per tray, across the tray.
	/// -1 for "not recorded"
	/// </summary>
	int VialsPerTrayX { get; set; }

	/// <summary>
	/// Gets or sets the number of vials (or wells) per tray, down the tray.
	/// -1 for "not recorded"
	/// </summary>
	int VialsPerTrayY { get; set; }

	/// <summary>
	/// Gets or sets the shape.
	/// If this property returns "Invalid", no other values in this object
	/// contain usable information.
	/// Invalid data can occur for older raw file formats, before auto sampler data was added.
	/// </summary>
	TrayShape TrayShape { get; set; }

	/// <summary>
	/// Gets the tray shape as a string
	/// </summary>
	string TrayShapeAsString { get; }

	/// <summary>
	/// Gets or sets the tray name.
	/// </summary>
	string TrayName { get; set; }
}
