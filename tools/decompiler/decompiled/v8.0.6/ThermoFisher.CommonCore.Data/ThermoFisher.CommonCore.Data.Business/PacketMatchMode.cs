namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Gives orientation whether or not a packet is inside the expected mass and intensity window (= Expectation rectangle)
/// If it is outside: it tells where the packet lies:
/// North: intensity too big.
/// South: Intensity too small
/// West: Mass too low.
/// East: Mass too big.
/// </summary>
public enum PacketMatchMode
{
	/// <summary>
	/// Unknown or not initialized yet
	/// </summary>
	OutsideUnknown,
	/// <summary>
	/// Packet has lower mass and higher intensity than expected
	/// </summary>
	OutsideNorthWest,
	/// <summary>
	///  Packet has lower mass than expected but correct intensity 
	/// </summary>
	OutsideWest,
	/// <summary>
	/// Packet has lower mass and lower  intensity than expected 
	/// </summary>
	OutsideSouthWest,
	/// <summary>
	///  Packet has correct mass but lower intensity than expected
	/// </summary>
	OutsideSouth,
	/// <summary>
	///  Packet has higher mass and lower intensity than expected
	/// </summary>
	OutsideSouthEast,
	/// <summary>
	/// Packet has correct intensity but mass but higher mass than expected
	/// </summary>
	OutsideEast,
	/// <summary>
	/// Packet has higher mass and higher intensity than expected
	/// </summary>
	OutsideNorthEast,
	/// <summary>
	///  Packet has correct mass and higher intensity than expected
	/// </summary>
	OutsideNorth,
	/// <summary>
	/// Packet is inside expected error limits
	/// </summary>
	Inside
}
