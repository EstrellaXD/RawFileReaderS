using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// Information about the auto sampler, as logged in a raw file
/// </summary>
internal class WrappedAutoSamplerInformation : IAutoSamplerInformation
{
	/// <summary>
	/// Gets or sets the tray index, -1 for "not recorded"
	/// </summary>
	public int TrayIndex { get; set; }

	/// <summary>
	/// Gets or sets the tray name.
	/// </summary>
	public string TrayName { get; set; }

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
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedAutoSamplerInformation" /> class. 
	/// Construct auto sampler information, with no initial data.
	/// This path occurs for older raw files, which do not have this block.
	/// The shape is defined as "Invalid" in this case.
	/// </summary>
	public WrappedAutoSamplerInformation()
	{
		TrayShape = TrayShape.Invalid;
		TrayName = string.Empty;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedAutoSamplerInformation" /> class.
	/// </summary>
	/// <param name="autoSamplerConfig">
	/// The auto sampler config.
	/// </param>
	public WrappedAutoSamplerInformation(IAutoSamplerConfig autoSamplerConfig)
	{
		TrayIndex = autoSamplerConfig.TrayIndex;
		VialIndex = autoSamplerConfig.VialIndex;
		VialsPerTray = autoSamplerConfig.VialsPerTray;
		VialsPerTrayX = autoSamplerConfig.VialsPerTrayX;
		VialsPerTrayY = autoSamplerConfig.VialsPerTrayY;
		if (TrayIndex == -1 && VialIndex == -1)
		{
			if (VialsPerTray == 0)
			{
				VialsPerTray = -1;
			}
			if (VialsPerTrayX == 0)
			{
				VialsPerTrayX = -1;
			}
			if (VialsPerTrayY == 0)
			{
				VialsPerTrayY = -1;
			}
		}
		TrayShape = autoSamplerConfig.TrayShape;
		TrayName = autoSamplerConfig.TrayName;
	}
}
