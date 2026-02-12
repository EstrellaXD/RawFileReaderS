using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The ScanWithSimpleData interface.
/// Defines very basic MS data and intensity, plus the event for the scan.
/// </summary>
public interface IScanWithSimpleData
{
	/// <summary>
	/// Gets the scan event.
	/// </summary>
	ScanEventHelper ScanEvent { get; }

	/// <summary>
	/// Gets the data.
	/// </summary>
	ISimpleScanAccess Data { get; }
}
