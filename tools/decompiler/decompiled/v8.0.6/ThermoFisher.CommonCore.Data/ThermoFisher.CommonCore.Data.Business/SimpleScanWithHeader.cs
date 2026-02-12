using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A simple scan with header.
/// </summary>
public class SimpleScanWithHeader
{
	/// <summary>
	/// Gets or sets the scan.
	/// </summary>
	public IScanWithSimpleData Scan { get; set; }

	/// <summary>
	/// Gets or sets the header.
	/// </summary>
	public ISimpleScanHeader Header { get; set; }
}
