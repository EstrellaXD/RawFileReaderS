namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// This enum defines ways to extend the matching of "scanned mass ranges"
/// when attempting to group similar data dependent scans.
/// This is new in the .NET 8 version of common core.
/// "None" is the legacy behavior, (backwards compatible)
/// Percent options increase the "mass range" matching tolerance to be wider than the parent mass 
/// matching tolerance by the given %.
/// For example:
/// Data dependent Parent mass 100. Tolerance for match = +/- 0.2.
/// To include parent mass in next level MS scan, scan range may be expanded (say by 10%) to end 
/// at 110.
/// Suppose there is another parent, mass 100.2, just in range to match 100.
/// Mass range 10% higher gives 110.22.
/// If the mass limit is checked at "0.2" tolerance, then the "110" limit for the
/// earlier scan types doesn't match as "110.22" is greater than 110 + 0.2.
/// Suggested setting to solve this case "Percent20".
/// The mass limits cannot be ignored as the MS may be deliberately programmed to look at specific ranges.
/// </summary>
public enum ExtendToleranceBy
{
	/// <summary>
	/// No extension
	/// </summary>
	None,
	/// <summary>
	/// Extend by 10%
	/// </summary>
	Percent10,
	/// <summary>
	/// Extend by 20%
	/// </summary>
	Percent20,
	/// <summary>
	/// Extend by 30%
	/// </summary>
	Percent30,
	/// <summary>
	/// Extend by 40%
	/// </summary>
	Percent40,
	/// <summary>
	/// Extend by 50%
	/// </summary>
	Percent50,
	/// <summary>
	/// Extend by 60%
	/// </summary>
	Percent60,
	/// <summary>
	/// Extend by 70%
	/// </summary>
	Percent70,
	/// <summary>
	/// Extend by 80%
	/// </summary>
	Percent80,
	/// <summary>
	/// Extend by 90%
	/// </summary>
	Percent90,
	/// <summary>
	/// Extend by 100% (double the tolerance)
	/// </summary>
	Percent100,
	/// <summary>
	/// Increase the tolerance by factor of 3
	/// </summary>
	Factor3,
	/// <summary>
	/// Increase the tolerance by factor of 4
	/// </summary>
	Factor4,
	/// <summary>
	/// Increase the tolerance by factor of 5
	/// </summary>
	Factor5
}
