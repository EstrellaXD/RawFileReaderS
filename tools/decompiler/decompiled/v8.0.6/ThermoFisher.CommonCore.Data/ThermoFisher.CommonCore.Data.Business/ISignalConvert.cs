using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The SignalConvert interface.
/// Defines an object which can be converted to signal
/// </summary>
public interface ISignalConvert
{
	/// <summary>
	/// Gets the number of points in this data
	/// </summary>
	int Length { get; }

	/// <summary>
	/// Convert to signal.
	/// The object implementing this would have the required intensity information,
	/// but limited other data (such as RT values) which can be pulled from "scans".
	/// </summary>
	/// <param name="scans">
	/// The scans.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IChromatogramSignalAccess" />.
	/// </returns>
	ChromatogramSignal ToSignal(IList<ISimpleScanHeader> scans);
}
