using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to provide algorithms to add scans.
/// </summary>
public interface IScanAdd
{
	/// <summary>
	/// Creates a sum of two scans.
	/// </summary>
	/// <param name="firstScan">The first scan.</param>
	/// <param name="secondScan">The second scan</param>
	/// <returns>The sum"first+second"</returns>
	Scan Add(Scan firstScan, Scan secondScan);
}
