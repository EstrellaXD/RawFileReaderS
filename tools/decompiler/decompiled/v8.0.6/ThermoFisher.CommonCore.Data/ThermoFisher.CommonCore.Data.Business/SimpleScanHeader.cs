using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Basic implementation of a simple scan header, with scan number and time.
/// </summary>
public class SimpleScanHeader : ISimpleScanHeader
{
	/// <summary>
	/// Gets or sets the retention time.
	/// </summary>
	public double RetentionTime { get; set; }

	/// <summary>
	/// Gets or sets the scan number.
	/// </summary>
	public int ScanNumber { get; set; }
}
