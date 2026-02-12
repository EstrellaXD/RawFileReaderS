namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Events are either at the start of peak detection (Initial) or part way through the data (Timed)
/// </summary>
public enum EventKind
{
	/// <summary>
	/// Event value before any peak detection begins.
	/// </summary>
	Initial,
	/// <summary>
	/// An event which occurs at a specific time in the chromatogram.
	/// </summary>
	Timed
}
