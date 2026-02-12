using System;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Contains common instrument information.
/// </summary>
[Serializable]
public abstract class Instrument
{
	/// <summary>
	/// Gets or sets the type of the device.
	/// </summary>
	[XmlIgnore]
	public Device DeviceType { get; protected set; }

	/// <summary>
	/// Gets or sets the base URI.
	/// </summary>
	[XmlAttribute]
	public string BaseUri { get; set; }

	/// <summary>
	/// Gets or sets the name of the instrument
	/// </summary>
	[XmlAttribute]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the Model of instrument
	/// </summary>
	[XmlAttribute]
	public string Model { get; set; }

	/// <summary>
	/// Gets or sets the Serial number of instrument
	/// </summary>
	[XmlAttribute]
	public string SerialNumber { get; set; }

	/// <summary>
	/// Gets or sets the Software version of instrument
	/// </summary>
	[XmlAttribute]
	public string SoftwareVersion { get; set; }

	/// <summary>
	/// Gets or sets the Hardware version of instrument
	/// </summary>
	[XmlAttribute]
	public string HardwareVersion { get; set; }

	/// <summary>
	/// Gets or sets Units of UV or analog data (not used for MS instruments)
	/// </summary>
	[XmlAttribute]
	public DataUnits Units { get; set; }

	/// <summary>
	/// Gets or sets the run header of the instrument.
	/// </summary>
	public RunHeader RunHeader { get; set; }

	/// <summary>
	/// Gets or sets the list of auto filters.
	/// </summary>
	public string[] AutoFilters { get; set; }

	/// <summary>
	/// Gets or sets the list of instrument methods.
	/// </summary>
	[XmlArray(ElementName = "TrailerExtraHeaders")]
	[XmlArrayItem(ElementName = "TrailerExtraHeader")]
	public HeaderItem[] TrailerExtraHeaders { get; set; }

	/// <summary>
	/// Gets or sets the tune data for the instrument
	/// </summary>
	[XmlIgnore]
	public TuneData TuneData { get; set; }

	/// <summary>
	/// Gets or sets the Status log data for the instrument
	/// </summary>
	[XmlIgnore]
	public StatusLog StatusLogData { get; set; }

	/// <summary>
	/// Gets or sets the segment event table for the current instrument
	/// </summary>
	[XmlArray("Segments")]
	[XmlArrayItem("Events", typeof(string[]))]
	[XmlArrayItem(ElementName = "Event")]
	public string[][] SegmentEventTable { get; set; }

	/// <summary>
	/// Gets or sets names for channels (for UV or analog data)
	/// </summary>
	[XmlArray(ElementName = "ChannelLabels")]
	[XmlArrayItem(ElementName = "ChannelLabel")]
	public string[] ChannelLabels { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Instrument" /> class.
	/// </summary>
	protected Instrument()
	{
	}
}
