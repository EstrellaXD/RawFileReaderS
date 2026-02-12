using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to provide algorithms to subtract scans.
/// </summary>
public interface IScanSubtract
{
	/// <summary>
	/// Creates a difference of two scans.
	/// </summary>
	/// <param name="foregroundScan">The scan containing signal.</param>
	/// <param name="backgroundScan">The scan containing background</param>
	/// <returns>The difference "foregroundScan-backgroundScan"</returns>
	Scan Subtract(Scan foregroundScan, Scan backgroundScan);
}
