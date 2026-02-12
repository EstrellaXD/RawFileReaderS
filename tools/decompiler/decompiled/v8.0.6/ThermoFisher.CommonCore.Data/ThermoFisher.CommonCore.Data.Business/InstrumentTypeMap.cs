using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A Metadata class represents key/value pair of Index and InstrumentType of an instrument in the raw file. 
/// </summary>
[Serializable]
public class InstrumentTypeMap
{
	/// <summary>
	/// Gets or sets the index of the Instrument.
	/// </summary>
	public int Index { get; set; }

	/// <summary>
	/// Gets or sets the type of the instrument.
	/// </summary>
	public Device InstrumentType { get; set; }
}
