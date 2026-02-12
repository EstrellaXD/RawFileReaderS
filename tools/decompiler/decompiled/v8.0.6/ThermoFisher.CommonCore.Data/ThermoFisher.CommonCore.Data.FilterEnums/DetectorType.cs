namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Specifies inclusion or exclusion of the detector value.
/// </summary>
public enum DetectorType
{
	/// <summary>
	/// The detector value valid.
	/// </summary>
	Valid,
	/// <summary>
	/// Any detector value (for filtering).
	/// </summary>
	Any,
	/// <summary>
	/// The detector value is not valid.
	/// </summary>
	NotValid
}
