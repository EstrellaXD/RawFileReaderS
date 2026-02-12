namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Define a combination of both scan and header
/// </summary>
public interface ISimpleScanWithHeader
{
	/// <summary>
	/// Gets the scan header
	/// </summary>
	ISimpleScanHeader Header { get; }

	/// <summary>
	/// Gets the scan data
	/// </summary>
	ISimpleScanAccess Data { get; }

	/// <summary>
	/// Gets the event, which defines how this scan was created
	/// </summary>
	IScanEvent Event { get; }
}
