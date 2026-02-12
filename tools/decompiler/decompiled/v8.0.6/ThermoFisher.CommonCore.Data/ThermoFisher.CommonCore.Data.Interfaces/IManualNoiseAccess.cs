using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Access to settings which defines a manual noise feature (for peak integration)
/// </summary>
public interface IManualNoiseAccess
{
	/// <summary>
	/// Gets a value indicating whether manual noise should be used
	/// </summary>
	bool UseManualNoiseRegion { get; }

	/// <summary>
	/// Gets the manual noise region (time range in minutes)
	/// </summary>
	Range ManualNoiseRtRange { get; }

	/// <summary>
	/// Gets the manual noise region (intensity range)
	/// These values are not used by Xcalibur
	/// </summary>
	Range ManualNoiseIntensityRange { get; }
}
