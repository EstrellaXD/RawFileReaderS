using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Encapsulates information about AnalogInstrument.
/// </summary>
[Serializable]
public class AnalogInstrument : Instrument
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.AnalogInstrument" /> class.
	/// </summary>
	public AnalogInstrument()
	{
		base.DeviceType = Device.Analog;
	}
}
