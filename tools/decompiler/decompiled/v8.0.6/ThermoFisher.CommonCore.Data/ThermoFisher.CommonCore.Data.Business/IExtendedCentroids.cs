using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines an extended centroid scan (FTMS data), which includes analysis of charge envelopes.
/// </summary>
public interface IExtendedCentroids : ICentroidStreamAccess, ISimpleScanAccess
{
	/// <summary>
	/// Gets a value indicating whether charge envelope data 
	/// was recorded for this scan
	/// </summary>
	bool HasChargeEnvelopes { get; }

	/// <summary>
	/// Gets additional annotations per peak, related to change envelopes
	/// </summary>
	IApdPeakAnnotation[] Annotations { get; }

	/// <summary>
	/// Gets the change envelopes. This include overall information
	/// about the envelope, plus the set of included peaks, 
	/// </summary>
	IChargeEnvelope[] ChargeEnvelopes { get; }
}
