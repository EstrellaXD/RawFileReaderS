using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The auto sampler information.
/// </summary>
public class AutoSamplerInformation : IAutoSamplerInformation
{
	/// <summary>
	/// Gets or sets the tray index, -1 for "not recorded"
	/// </summary>
	public int TrayIndex { get; set; }

	/// <summary>
	/// Gets or sets the vial index, -1 for "not recorded"
	/// </summary>
	public int VialIndex { get; set; }

	/// <summary>
	/// Gets or sets the number of vials (or wells) per tray.
	/// -1 for "not recorded"
	/// </summary>
	public int VialsPerTray { get; set; }

	/// <summary>
	/// Gets or sets the number of vials (or wells) per tray, across the tray.
	/// -1 for "not recorded"
	/// </summary>
	public int VialsPerTrayX { get; set; }

	/// <summary>
	/// Gets or sets the number of vials (or wells) per tray, down the tray.
	/// -1 for "not recorded"
	/// </summary>
	public int VialsPerTrayY { get; set; }

	/// <summary>
	/// Gets or sets the shape.
	/// If this property returns "Invalid", no other values in this object
	/// contain usable information.
	/// Invalid data can occur for older raw file formats, before auto sampler data was added.
	/// </summary>
	public TrayShape TrayShape { get; set; }

	/// <summary>
	/// Gets the tray shape as a string
	/// </summary>
	public string TrayShapeAsString => TrayShape.ToString();

	/// <summary>
	/// Gets or sets the tray name.
	/// </summary>
	public string TrayName { get; set; }
}
