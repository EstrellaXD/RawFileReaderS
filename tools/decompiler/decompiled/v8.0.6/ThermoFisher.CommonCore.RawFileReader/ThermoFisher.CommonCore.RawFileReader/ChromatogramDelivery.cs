using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Basic format of chromatogram delivery:
/// Just keeps a copy of the data
/// </summary>
internal class ChromatogramDelivery : IChromatogramDelivery
{
	/// <summary>
	/// Gets or sets the "request" which determines what kind of chromatogram is needed.
	/// </summary>
	public IChromatogramRequest Request { get; set; }

	/// <summary>
	/// Gets the chromatogram data
	/// </summary>
	public ChromatogramSignal DeliveredSignal { get; private set; }

	/// <summary>
	/// Implements the "Process" interface, as just saving a reference to the data.
	/// </summary>
	/// <param name="signal">The chromatogram which has been generated</param>
	public void Process(ChromatogramSignal signal)
	{
		DeliveredSignal = signal;
	}
}
