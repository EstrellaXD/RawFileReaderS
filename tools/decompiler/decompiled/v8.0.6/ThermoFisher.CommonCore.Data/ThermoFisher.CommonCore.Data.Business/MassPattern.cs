namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A mass pattern item used in the <see cref="T:ThermoFisher.CommonCore.Data.Business.PatternValueProvider" />.
/// </summary>
public class MassPattern
{
	/// <summary>
	/// The mass offset.
	/// </summary>
	public double MassOffset { get; set; }

	/// <summary>
	/// The relative intensity value.
	/// </summary>
	public double Intensity { get; set; }

	/// <summary>
	/// The mass tolerance setting.
	/// </summary>
	public Tolerance MassTolerance { get; set; }

	/// <summary>
	/// The relative intensity tolerance setting (in %).
	/// </summary>
	public double IntensityTolerance { get; set; }

	/// <summary>
	/// The minimum intensity setting (in counts).
	/// </summary>
	public double MinimumIntensity { get; set; }

	/// <summary>
	/// Indicates if this mass pattern item is required.
	/// </summary>
	public bool IsRequired { get; set; }

	/// <summary>
	/// Indicates if this mass pattern item is the reference item.
	/// </summary>
	public bool IsReference { get; set; }
}
