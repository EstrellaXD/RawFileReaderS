using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Encapsulates information about MSAnalogInstrument.
/// </summary>
[Serializable]
public class MSAnalogInstrument : Instrument
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MSAnalogInstrument" /> class.
	/// </summary>
	public MSAnalogInstrument()
	{
		base.DeviceType = Device.MSAnalog;
	}
}
