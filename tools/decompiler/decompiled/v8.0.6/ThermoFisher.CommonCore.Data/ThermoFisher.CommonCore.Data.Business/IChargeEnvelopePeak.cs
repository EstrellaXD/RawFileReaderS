namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Data about a single peak within a charge envelope.
/// <para>
/// This grouping has a separate structure explained in <see cref="T:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary" />. While presentation to CC consumers
/// may be different, the (at least Orbitrap-)generated data structure allocates one 16-bit value for each centroid.
/// </para>
/// </summary>
public interface IChargeEnvelopePeak
{
	/// <summary>
	/// Returns true if the peak is a monoisotopic peak, false otherwise.
	/// </summary>
	bool IsMonoisotopic { get; }

	/// <summary>
	/// Returns true if the conditions below met, false otherwise.
	/// <para>
	/// The top peak is that peak in a charge envelope that fulfills two requirements in this order:
	/// 1) never being considered to be part of another charge envelope, and
	/// 2) having the highest abundance.
	/// </para>
	/// </summary>
	bool IsClusterTop { get; }

	/// <summary>
	/// Returns true if the peak and its charge envelope have further isotope sibling (according to averagine model, etc).
	/// <para>
	/// Note that a charge envelope must be considered non-isotopically resolved if at least one of its peaks has this flag "false",
	/// and it can be considered an error if some peaks of an envelope have this flag set and other peaks not.
	/// </para>
	/// </summary>
	bool IsIsotopicallyResolved { get; }
}
