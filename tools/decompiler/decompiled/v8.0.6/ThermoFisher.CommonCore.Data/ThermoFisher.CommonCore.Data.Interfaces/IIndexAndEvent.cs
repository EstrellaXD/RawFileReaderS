namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Access a MS scan index and the scan's event
/// </summary>
public interface IIndexAndEvent
{
	/// <summary>
	/// Gets the index for a MS scan
	/// </summary>
	IMsScanIndexAccess ScanIndex { get; }

	/// <summary>
	/// Gets scanning information about a scan
	/// </summary>
	IScanEvent ScanEvent { get; }
}
