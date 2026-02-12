namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines data which explains how charges are calculated.
/// ChargeEnvelopes contains summary information about each detected envelope.
/// CentroidAnnotations can be used to determine which charge envelope (if any) a peak belongs to.
/// </summary>
public interface ICentroidAnnotations
{
	/// <summary>
	/// Gets or sets a flag indicating whether there are charge envelopes.
	/// </summary>
	bool HasChargeEnvelopes { get; }

	/// <summary>
	/// Gets a value indicating whether this is valid "charge envelope" data, when decoded from a raw file.
	/// Raw files have diagnostic sections of potentially unknown formats, so it is possible that, even though some data exists, it
	/// cannot be decoded as charge envelopes.
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	/// Gets summary information the "charge envelopes". Purpose needs to be detailed?
	/// </summary>
	IChargeEnvelopeSummary[] ChargeEnvelopes { get; }

	/// <summary>
	/// Gets the "Centroid Annotations". Purpose needs to be detailed?
	/// </summary>
	IApdPeakAnnotation[] CentroidAnnotations { get; }
}
