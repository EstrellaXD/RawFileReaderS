using System;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The additional (optional) features which can be returned with a scan.
/// </summary>
[Flags]
internal enum PacketFeatures
{
	/// <summary>
	/// Nothing but mass intensity and peak flags
	/// </summary>
	None = 0,
	/// <summary>
	/// Noise and baseline values
	/// </summary>
	NoiseAndBaseline = 1,
	/// <summary>
	/// Return Charge Data
	/// </summary>
	Chagre = 2,
	/// <summary>
	/// Return Resolution data
	/// </summary>
	Resolution = 4,
	/// <summary>
	/// Profile data may be needed
	/// </summary>
	Profile = 8,
	/// <summary>
	/// Debug data may be needed
	/// </summary>
	Debug = 0x10,
	/// <summary>
	/// Return all data
	/// </summary>
	All = 0xFF
}
