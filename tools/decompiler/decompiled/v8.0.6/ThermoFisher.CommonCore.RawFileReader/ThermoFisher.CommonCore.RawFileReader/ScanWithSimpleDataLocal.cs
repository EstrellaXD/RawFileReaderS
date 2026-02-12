using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Defines an implementation of "IScanWithSimpleData",
/// </summary>
internal class ScanWithSimpleDataLocal : IScanWithSimpleData
{
	/// <summary>
	/// Gets or sets the scan event.
	/// </summary>
	public ScanEventHelper ScanEvent { get; set; }

	/// <summary>
	/// Gets or sets the data.
	/// </summary>
	public ISimpleScanAccess Data { get; set; }
}
