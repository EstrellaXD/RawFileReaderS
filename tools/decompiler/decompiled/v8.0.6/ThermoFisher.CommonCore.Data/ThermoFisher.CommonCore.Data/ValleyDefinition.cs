namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Specifies how the valley between resolved peaks is measured (in what units)
/// </summary>
public enum ValleyDefinition
{
	/// <summary>
	/// Peaks are measured at full with half maximum.
	/// This implies that two close peaks will both hit half maximum at the valley,
	/// and is equivalent to 100% valley height (no valley), or just starting to be resolved.
	/// </summary>
	FWHM,
	/// <summary>
	/// Valley between two resolved peaks is 10% of peak height.
	/// Used for most accurate mass instruments.
	/// </summary>
	TenPercent
}
