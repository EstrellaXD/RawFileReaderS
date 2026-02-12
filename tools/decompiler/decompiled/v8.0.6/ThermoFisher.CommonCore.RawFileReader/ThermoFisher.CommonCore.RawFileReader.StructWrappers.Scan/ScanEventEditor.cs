namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// The scan event editor.
/// </summary>
internal class ScanEventEditor : ScanEvent
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEventEditor" /> class.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	public ScanEventEditor(ScanEvent scanEvent)
		: base(scanEvent)
	{
		scanEvent.CopyArrays(this);
	}
}
