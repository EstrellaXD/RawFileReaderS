namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Stores one set of tune data values for an instrument
/// </summary>
public class TuneDataValues
{
	/// <summary>
	/// Gets or sets the index number of the tune record
	/// </summary>
	public int ID { get; set; }

	/// <summary>
	/// Gets or sets the array of tune data values for an instrument
	/// </summary>
	public string[] Values { get; set; }
}
