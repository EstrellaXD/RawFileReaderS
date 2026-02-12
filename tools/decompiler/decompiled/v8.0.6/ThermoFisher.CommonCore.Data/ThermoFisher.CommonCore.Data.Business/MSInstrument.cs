using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Represents the MassSpec instrument. 
/// </summary>
[Serializable]
public class MSInstrument : Instrument
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MSInstrument" /> class.
	/// </summary>
	public MSInstrument()
	{
		base.DeviceType = Device.MS;
	}
}
