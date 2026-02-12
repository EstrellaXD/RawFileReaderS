using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Encapsulates information about PDA Instrument.
/// </summary>
[Serializable]
public class PDAInstrument : Instrument
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PDAInstrument" /> class.
	/// </summary>
	public PDAInstrument()
	{
		base.DeviceType = Device.Pda;
	}
}
