namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Determines how void time is calculated.
/// </summary>
public enum VoidTime
{
	/// <summary>
	/// A specific time is entered for void time
	/// </summary>
	VoidTimeByValue,
	/// <summary>
	/// First qualitative peak defines void time
	/// </summary>
	VoidTimeFirstPeak
}
