using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Describes which detector stream is being accessed from a specific sample's raw data repository.
/// For example: "Non reference peak data from the first MS", or "the 2nd PDA device".
/// </summary>
public interface IConfiguredDetector : IInstrumentSelectionAccess
{
	/// <summary>
	/// Gets the configured setting of "ReferenceAndException".
	/// This controls the default handling of reference peaks in spectrum reading methods, from the MS detector.
	/// </summary>
	bool UseReferenceAndExceptionData { get; }
}
