using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ChromatogramDelivery interface.
/// This permits a caller to request a chromatogram,
/// and have a method called, when the chromatogram is ready.
/// </summary>
public interface IChromatogramDelivery
{
	/// <summary>
	/// Gets the request. Parameters for the chromatogram.
	/// </summary>
	IChromatogramRequest Request { get; }

	/// <summary>
	/// The method to call when the chromatogram is generated.
	/// </summary>
	/// <param name="signal">
	/// The generated chromatogram.
	/// </param>
	void Process(ChromatogramSignal signal);
}
