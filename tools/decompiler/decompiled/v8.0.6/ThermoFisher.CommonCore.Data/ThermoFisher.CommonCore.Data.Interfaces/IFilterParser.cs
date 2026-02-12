namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a method of parsing a scan filter.
/// This could be used for UI validation of scan filter text, without an
/// open raw file.
/// </summary>
public interface IFilterParser
{
	/// <summary>
	/// Gets or sets the precision expected for collision energy
	/// </summary>
	int EnergyPrecision { get; set; }

	/// <summary>
	/// Gets or sets the precision expected for mass values
	/// </summary>
	int MassPrecision { get; set; }

	/// <summary>
	/// Parse a string, returning the scan filter codes.
	/// </summary>
	/// <param name="text">String to parse</param>
	/// <returns>Filters in the supplied string, or null if the string cannot be parsed.
	/// The string can be considered incorrect if there are too many digits in a numeric value
	/// or too many digits after the decimal point, for a mass or energy.
	/// </returns>
	IScanFilterPlus GetFilterFromString(string text);
}
