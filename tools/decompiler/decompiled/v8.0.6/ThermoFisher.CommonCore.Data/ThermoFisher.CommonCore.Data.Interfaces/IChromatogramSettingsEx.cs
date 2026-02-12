namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The Chromatogram Settings Extended interface, for platform 3.0 raw files.
/// </summary>
public interface IChromatogramSettingsEx : IChromatogramSettings
{
	/// <summary>
	/// Gets or sets the compound names.
	/// </summary>
	string[] CompoundNames { get; set; }

	/// <summary>
	/// Gets or sets a custom tool to provide a calculated value from 
	/// the mass, intensity data in a scan (for MS custom chromatogram trace type)
	/// </summary>
	IScanValueProvider CustomValueProvider { get; set; }

	/// <summary>
	/// Custom RT range, used if nonzero
	/// </summary>
	IRangeAccess RtRange { get; }
}
