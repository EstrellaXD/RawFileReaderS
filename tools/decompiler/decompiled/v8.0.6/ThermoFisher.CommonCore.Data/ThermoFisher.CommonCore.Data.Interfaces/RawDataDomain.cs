namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines the style of data.
/// Devices originally designed for various systems can have different sets of attributes,
/// which may be applicable to different customers or markets.
/// A number of interfaces are common to all data protocols, but some are specialized
/// based on a particular device family.
/// </summary>
public enum RawDataDomain
{
	/// <summary>
	/// Legacy (Xcalibur) protocol, (all Xcalibur files). Some legacy features may only be
	/// available for this format. For example: the action "Extract an instrument method from a raw file" 
	/// is only defined for this format of data.
	/// </summary>
	Legacy,
	/// <summary>
	/// Luna Device protocol based on the Mass Spectrometry data formats which are derived
	/// from the legacy Xcalibur product. In genearal, algorithms designed for
	/// Xcalibur may need fewer changes to operate on this format.
	/// </summary>
	MassSpectrometry,
	/// <summary>
	/// Luna Device protocol based on the Chromatography data formats from
	/// the legacy Chromeleon product. In genearal, algorithms designed for
	/// Chromeleon may need fewer changes to operate on this format.
	/// </summary>
	Chromatography
}
