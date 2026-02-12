namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines the instrument data index for analog type devices
/// </summary>
public interface IAnalogScanIndex : IBaseScanIndex
{
	/// <summary>
	/// Gets the number of channels.
	/// </summary>
	int NumberOfChannels { get; }
}
