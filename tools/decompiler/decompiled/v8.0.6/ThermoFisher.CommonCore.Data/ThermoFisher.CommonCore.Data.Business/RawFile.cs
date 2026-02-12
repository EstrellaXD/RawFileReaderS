using System;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Encapsulates raw file data.
/// </summary>
[Serializable]
[XmlRoot(Namespace = "http://thermofisher.com/OpenXml")]
public class RawFile
{
	/// <summary>
	/// Gets or sets the date when this data was created
	/// </summary>
	[XmlAttribute]
	public DateTime CreationDate { get; set; }

	/// <summary>
	/// Gets or sets the name of acquired file (excluding path)
	/// </summary>
	[XmlAttribute]
	public string FileName { get; set; }

	/// <summary>
	/// Gets or sets the name of person creating data
	/// </summary>
	[XmlAttribute]
	public string CreatorId { get; set; }

	/// <summary>
	/// Gets or sets the xml schema version for this file.
	/// </summary>
	[XmlAttribute]
	public string SchemaVersion { get; set; }

	/// <summary>
	/// Gets or sets all instrument names.
	/// </summary>
	public string[] AllInstrumentNamesFromInstrumentMethod { get; set; }

	/// <summary>
	/// Gets or sets the information about the sample.
	/// </summary>
	public SampleInformation SampleInformation { get; set; }

	/// <summary>
	/// Gets or sets the MS instruments.
	/// </summary>
	public MSInstrument[] MSInstruments { get; set; }

	/// <summary>
	/// Gets or sets the UV instruments.
	/// </summary>
	public UVInstrument[] UVInstruments { get; set; }

	/// <summary>
	/// Gets or sets the PDA instruments.
	/// </summary>
	public PDAInstrument[] PDAInstruments { get; set; }

	/// <summary>
	/// Gets or sets the PDA instruments.
	/// </summary>
	public MSAnalogInstrument[] MSAnalogInstruments { get; set; }

	/// <summary>
	/// Gets or sets the PDA instruments.
	/// </summary>
	public AnalogInstrument[] AnalogInstruments { get; set; }

	/// <summary>
	/// Gets or sets the instrument types map. This metadata is used to support GetInstrumentTypes method.
	/// </summary>
	public InstrumentTypeMap[] InstrumentTypes { get; set; }

	/// <summary>
	/// Gets or sets the list of instrument methods.
	/// </summary>
	[XmlArray(ElementName = "InstrumentMethods")]
	[XmlArrayItem(ElementName = "InstrumentMethod")]
	public string[] InstrumentMethods { get; set; }

	/// <summary>
	/// Gets The number of instruments (data streams) in this file.
	/// </summary>
	public int InstrumentCount
	{
		get
		{
			int num = 0;
			if (!MSInstruments.IsNullOrEmpty())
			{
				num += MSInstruments.Length;
			}
			if (!PDAInstruments.IsNullOrEmpty())
			{
				num += PDAInstruments.Length;
			}
			if (!UVInstruments.IsNullOrEmpty())
			{
				num += UVInstruments.Length;
			}
			if (!AnalogInstruments.IsNullOrEmpty())
			{
				num += AnalogInstruments.Length;
			}
			if (!MSAnalogInstruments.IsNullOrEmpty())
			{
				num += MSAnalogInstruments.Length;
			}
			return num;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.RawFile" /> class.
	/// </summary>
	public RawFile()
	{
	}

	/// <summary>
	/// get the number of instruments (data streams) of a certain classification.
	/// For example: the number of UV devices which logged data into this file
	/// </summary>
	/// <param name="type">
	/// The device type to count
	/// </param>
	/// <returns>
	/// The number of devices of this type
	/// </returns>
	public int GetInstrumentCountOfType(Device type)
	{
		switch (type)
		{
		case Device.MS:
			if (!MSInstruments.IsNullOrEmpty())
			{
				return MSInstruments.Length;
			}
			break;
		case Device.MSAnalog:
			if (!MSAnalogInstruments.IsNullOrEmpty())
			{
				return MSAnalogInstruments.Length;
			}
			break;
		case Device.Analog:
			if (!AnalogInstruments.IsNullOrEmpty())
			{
				return AnalogInstruments.Length;
			}
			break;
		case Device.UV:
			if (!UVInstruments.IsNullOrEmpty())
			{
				return UVInstruments.Length;
			}
			break;
		case Device.Pda:
			if (!PDAInstruments.IsNullOrEmpty())
			{
				return PDAInstruments.Length;
			}
			break;
		}
		return 0;
	}
}
