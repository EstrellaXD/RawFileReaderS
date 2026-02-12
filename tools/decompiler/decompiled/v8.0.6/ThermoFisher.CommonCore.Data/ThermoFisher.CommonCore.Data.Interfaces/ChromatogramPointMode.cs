namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The chromatogram point mode.
/// How a chromatogram point is calculated from data in a mass range.
/// </summary>
public enum ChromatogramPointMode
{
	/// <summary>
	/// Sum all intensities in a mass range
	/// </summary>
	Sum,
	/// <summary>
	/// Get the max intensity in a mass range
	/// </summary>
	Max,
	/// <summary>
	/// Get the mass of the largest intensity value in the mass range
	/// </summary>
	Mass,
	/// <summary>
	/// Neutral fragment:
	/// When the low mass value is negative:
	/// This scan's Parent mass is added to both low and high, to make a mass range.
	/// which will represent "parent - a given neutral fragment".
	/// When the low mass value if positive, the mass range is not adjusted, as this has been
	/// already calculated as "filter mass - neutral fragment mass".
	/// Typically: This can be used with a filter of "MS2", to get
	/// a neutral fragment chromatogram, for all MS/MS data which has a given fragment.
	/// </summary>
	Fragment
}
