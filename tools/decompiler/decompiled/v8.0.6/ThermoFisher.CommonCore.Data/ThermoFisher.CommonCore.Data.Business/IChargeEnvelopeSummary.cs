namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// All isotopes just having a different charge will be grouped together in a charge envelope. Due
/// to calculation restrictions it is quite likely that there are further peaks which theoretically
/// be part of the same charge envelope, but where noise level, neighborhood, hardware limitations
/// keep those peaks unassigned.
/// This interfaces has statistics about a charge envelope, but not the contributing peaks.
/// </summary>
public interface IChargeEnvelopeSummary
{
	/// <summary>
	/// This is the monoisotopic mass that a particular peak belongs to. The value is
	/// a calculated one, it is very likely that this cannot be seen at all in the spectrum.
	/// But it is the reference point of all members of one charge envelope.
	/// <para>
	/// Note that the value becomes 0 if the value is not isotopically resolved. See also <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.AverageMass" /> and
	/// <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.IsIsotopicallyResolved" />
	/// </para>
	/// <para>
	///  Keep in mind that depending on instrument calibration this value may not exactly match a calculated mass.
	/// </para>
	/// </summary>
	double MonoisotopicMass { get; }

	/// <summary>
	/// This cross-correlation factor is the maximum of all cross-correlation values
	/// over all averagines. An averagine is a statistical model of the isotope distribution
	/// of the same molecule at a given charge. The observed peaks within the spectrum are fitted
	/// to the model, the overlap is calculated by a cross-correlation that only takes the intensities
	/// into account.
	/// <para>
	/// The averagine model is strongly linked to peptide analysis. As an example, averagine
	/// mass distribution for pesticides are totally different.
	/// </para>
	/// <para>
	/// Cross-correlation factors vary in the range 0 to 1. 0 will be set if <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.MonoisotopicMass" />
	/// is also 0. In this case, the fit was unsuccessful. The value is also 0 if <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.IsIsotopicallyResolved" />
	/// is set.
	/// </para>
	/// </summary>
	double CrossCorrelation { get; }

	/// <summary>
	/// This is the index to the top peak centroid in the centroid list coming with the same scan.
	/// One can use this to get access to the mass of the so called top peak.
	/// </summary>
	/// <remarks>
	/// The top peak is that peak in a charge envelope that fulfills two requirements in this order: 1) never being considered
	/// to be part of another charge envelope, and 2) having the highest abundance.
	/// </remarks>
	int TopPeakCentroidId { get; }

	/// <summary>
	/// Return whether this charge envelope was created using isotopically resolved species.
	/// <para>
	/// For isotopically resolved peaks the <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.MonoisotopicMass" />
	/// is set. If not resolved, only an average mass is returned.
	/// </para>
	/// </summary>
	bool IsIsotopicallyResolved { get; }

	/// <summary>
	/// When peaks are non-isotopically resolved (see <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.IsIsotopicallyResolved" />),
	/// this value contains the average mass of all species in the envelope, otherwise it contains 0.
	/// </summary>
	double AverageMass { get; }
}
