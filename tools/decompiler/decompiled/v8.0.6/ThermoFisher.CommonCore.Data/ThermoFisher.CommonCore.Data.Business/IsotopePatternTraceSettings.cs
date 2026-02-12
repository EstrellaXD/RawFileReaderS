using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines settings needed to make an XIC based on an isotope pattern.
/// </summary>
public class IsotopePatternTraceSettings
{
	/// <summary>
	/// The default mass tolerance setting.
	/// </summary>
	public Tolerance MassTolerance;

	/// <summary>
	/// The default relative intensity tolerance setting (%).
	/// </summary>
	public double IntensityTolerance;

	/// <summary>
	/// The default minimum intensity setting for a mass spectra ion (counts).
	/// </summary>
	public double MinimumIntensity;

	/// <summary>
	/// List of mass pattern items to include in filter.
	/// </summary>
	public List<MassPattern> MassPatternFilter;

	/// <summary>
	/// Initialize the <see cref="T:ThermoFisher.CommonCore.Data.Business.IsotopePatternTraceSettings" /> class with default values.
	/// </summary>
	public IsotopePatternTraceSettings()
	{
		MassTolerance = new Tolerance
		{
			Value = 5.0,
			Mode = ToleranceMode.Ppm
		};
		IntensityTolerance = 0.3;
		MinimumIntensity = 0.0;
		MassPatternFilter = new List<MassPattern>();
	}
}
