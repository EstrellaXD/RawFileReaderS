using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a set of peaks which are part of a charge envelope
/// </summary>
public interface IChargeEnvelope : IChargeEnvelopeSummary
{
	/// <summary>
	/// Gets or sets the collection of peaks in this charge envelopes
	/// </summary>
	List<int> Peaks { get; set; }

	/// <summary>
	/// Gets or sets the index into the centroids for the top peak
	/// </summary>
	new int TopPeakCentroidId { get; set; }
}
