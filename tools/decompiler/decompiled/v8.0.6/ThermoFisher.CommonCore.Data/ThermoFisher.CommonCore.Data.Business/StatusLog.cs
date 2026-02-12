using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Stores the Status log for an instrument
/// </summary>
public class StatusLog
{
	/// <summary>
	/// Gets or sets the header information for the status log
	/// </summary>
	[XmlArray(ElementName = "Headers")]
	[XmlArrayItem(ElementName = "Header")]
	public HeaderItem[] Headers { get; set; }

	/// <summary>
	/// Gets or sets the array of values for the status logs for the current instrument
	/// </summary>
	[XmlArray(ElementName = "StatusLogValues")]
	[XmlArrayItem(ElementName = "StatusLogValue")]
	public StatusLogValues[] StatusLogValues { get; set; }
}
