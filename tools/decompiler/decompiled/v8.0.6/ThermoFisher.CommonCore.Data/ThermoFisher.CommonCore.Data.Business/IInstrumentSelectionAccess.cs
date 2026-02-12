namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a slected instrument
/// </summary>
public interface IInstrumentSelectionAccess
{
	/// <summary>
	/// Gets the Stream number (instance of this instrument type).
	/// Stream numbers start from 1
	/// </summary>
	int InstrumentIndex { get; }

	/// <summary>
	/// Gets the Category of instrument
	/// </summary>
	Device DeviceType { get; }
}
