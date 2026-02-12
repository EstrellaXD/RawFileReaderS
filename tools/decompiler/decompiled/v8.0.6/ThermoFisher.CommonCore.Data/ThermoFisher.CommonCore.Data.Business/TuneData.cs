using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Stores the header and tune data record values for an instrument
/// </summary>
public class TuneData
{
	/// <summary>
	/// Gets or sets the array of headers for Tune data
	/// </summary>
	[XmlArray(ElementName = "Headers")]
	[XmlArrayItem(ElementName = "Header")]
	public HeaderItem[] Headers { get; set; }

	/// <summary>
	/// Gets or sets the tune data values for an instrument
	/// </summary>
	[XmlArray(ElementName = "TuneDataValues")]
	[XmlArrayItem(ElementName = "TuneDataValue")]
	public TuneDataValues[] TuneDataValues { get; set; }
}
