namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Stores the trailer extra values for a scan within an instrument
/// </summary>
public class TrailerExtraValues
{
	/// <summary>
	/// Gets or sets the scan Number
	/// </summary>
	public int ScanNumber { get; set; }

	/// <summary>
	/// Gets or sets the array of trailer extra values for a scan within an instrument
	/// </summary>
	public string[] Values { get; set; }
}
