namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The auto sampler tray shape.
/// </summary>
public enum TrayShape
{
	/// <summary>
	/// Vials or wells are arranged in a rectangle on the tray
	/// </summary>
	Rectangular,
	/// <summary>
	/// Vials are arranged in a circle.
	/// </summary>
	Circular,
	/// <summary>
	/// Vials are staggered on odd numbered positions on the tray.
	/// </summary>
	StaggeredOdd,
	/// <summary>
	/// Vials are staggered on even numbered positions on the tray.
	/// </summary>
	StaggeredEven,
	/// <summary>
	/// The layout is unknown.
	/// </summary>
	Unknown,
	/// <summary>
	/// The layout information is invalid. No other tray layout data should be displayed.
	/// </summary>
	Invalid
}
