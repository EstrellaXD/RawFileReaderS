using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Helper class for parallel chromatogram generation.
/// This object pulls data from constructor data into the required
/// properties to meet the interface.
/// This object can be used over 1 billion times when generating chromatograms
/// from a sequence of raw files (especially the scan event helper),
/// so all data is pulled into properties.
/// This object can then be used as a "class" rather than "interface" 
/// permitting the compiler to inline.
/// </summary>
public class ScanAndIndex : ISimpleScanWithHeader
{
	/// <summary>
	/// Gets the header, wrapper from supplied interface.
	/// </summary>
	public ISimpleScanHeader Header { get; }

	/// <summary>
	/// Gets the index.
	/// </summary>
	public int Index { get; private set; }

	/// <summary>
	/// Gets the scan event helper.
	/// </summary>
	public ScanEventHelper ScanEvent { get; }

	/// <summary>
	/// Gets the data.
	/// </summary>
	public ISimpleScanAccess Data { get; }

	/// <summary>
	/// Gets the scan event 
	/// </summary>
	public IScanEvent Event { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanAndIndex" /> class.
	/// </summary>
	/// <param name="scan">
	/// The scan.
	/// </param>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	public ScanAndIndex(SimpleScanWithHeader scan, int scanIndex)
	{
		Header = scan.Header;
		IScanWithSimpleData scan2 = scan.Scan;
		if (scan2 != null)
		{
			ScanEvent = scan2.ScanEvent;
			Data = scan2.Data;
			Event = ScanEvent.ScanEvent;
		}
		Index = scanIndex;
	}
}
