using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines which instrument is selected in a file.
/// </summary>
[Serializable]
public class InstrumentSelection : IInstrumentSelectionAccess
{
	/// <summary>
	/// Gets the Stream number (instance of this instrument type).
	/// Stream numbers start from 1
	/// </summary>
	public int InstrumentIndex { get; private set; }

	/// <summary>
	/// Gets the Category of instrument
	/// </summary>
	public Device DeviceType { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.InstrumentSelection" /> class.
	/// </summary>
	/// <param name="instrumentIndex">Index of the instrument.</param>
	/// <param name="deviceType">Type of the device.</param>
	public InstrumentSelection(int instrumentIndex, Device deviceType)
	{
		InstrumentIndex = instrumentIndex;
		DeviceType = deviceType;
	}
}
