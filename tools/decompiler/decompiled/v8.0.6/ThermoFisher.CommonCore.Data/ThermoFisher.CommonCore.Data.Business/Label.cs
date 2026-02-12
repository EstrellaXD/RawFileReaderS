using System;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Will be used with Channel labels. Currently not implemented. 
/// </summary>
[Serializable]
public class Label
{
	/// <summary>
	/// Gets or sets the index.
	/// </summary>
	[XmlAttribute]
	public int Index { get; set; }

	/// <summary>
	/// Gets or sets the label text.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Label" /> class.
	/// </summary>
	public Label()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Label" /> class.
	/// </summary>
	/// <param name="source">The source to copy the values from.</param>
	public Label(Label source)
	{
		Text = source.Text;
		Index = source.Index;
	}
}
