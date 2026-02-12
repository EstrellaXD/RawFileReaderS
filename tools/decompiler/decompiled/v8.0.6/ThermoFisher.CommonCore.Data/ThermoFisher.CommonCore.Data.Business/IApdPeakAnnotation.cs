namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Advanced Peak Detection on (FT) scans allow to better calculate charges of peaks. This is done by finding correlations
/// of peaks. If found, the assumption is considered valid that these peaks belong to the same species of a molecule, but
/// with different charges.
/// <para>
/// This grouping has a separate structure explained in <see cref="T:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary" />. While presentation to CC consumers
/// may be different, the (at least Orbitrap-)generated data structure allocates one 16-bit value for each centroid.
/// </para>
/// <para>
/// The 16-bit value contains three bits and an index number into an array of <see cref="T:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary" />s which also come
/// with the same scan.
/// TODO
/// This is an interface. Implementation details ( such as "structure is 16 bit") are not meaningful here.
/// Describe instead "What information is presented to the consumer".
/// Using IPeakAnnotation you can ???
/// </para>
/// </summary>
public interface IApdPeakAnnotation : IChargeEnvelopePeak
{
	/// <summary>
	/// Returns null if information is not available (as with non-APD annotated Orbitrap spectra or unresolved peaks),
	/// the index to the corresponding <see cref="T:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary" /> otherwise. Values out of range must not exist and should be checked
	/// in advance.
	/// </summary>
	int? ChargeEnvelopeIndex { get; }
}
