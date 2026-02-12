using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Encapsulates information about UVInstrument.
/// </summary>
[Serializable]
public class UVInstrument : Instrument
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.UVInstrument" /> class.
	/// </summary>
	public UVInstrument()
	{
		base.DeviceType = Device.UV;
	}
}
