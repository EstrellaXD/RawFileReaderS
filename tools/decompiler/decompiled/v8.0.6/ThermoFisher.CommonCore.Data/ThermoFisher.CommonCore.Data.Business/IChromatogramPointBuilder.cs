using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Methods for creating a value for one scan in a chromatogram.
/// </summary>
public interface IChromatogramPointBuilder
{
	/// <summary>
	/// Sum all masses within the ranges
	/// </summary>
	/// <param name="ranges">
	/// List of ranges to sum
	/// </param>
	/// <param name="toleranceOptions">
	/// If the ranges have equal mass values,
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses
	/// </param>
	/// <returns>
	/// Sum of intensities in all ranges
	/// </returns>
	double SumIntensities(IRangeAccess[] ranges, MassOptions toleranceOptions);

	/// <summary>
	/// Return the largest intensity (base value) in the ranges supplied
	/// </summary>
	/// <param name="ranges">
	/// Ranges of positions (masses, wavelengths)
	/// </param>
	/// <param name="toleranceOptions">
	/// If the ranges have equal mass values,
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses
	/// </param>
	/// <returns>
	/// Largest intensity in all ranges
	/// </returns>
	double BaseIntensity(IRangeAccess[] ranges, MassOptions toleranceOptions);
}
