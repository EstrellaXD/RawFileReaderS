using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Interface to pass replicate information to curve fitting methods
/// </summary>
public interface ILevelReplicates
{
	/// <summary>
	/// Gets the replicates of this calibration level
	/// </summary>
	ItemCollection<Replicate> ReplicateCollection { get; }

	/// <summary>
	/// Gets the number of replicates for this level
	/// </summary>
	int Replicates { get; }

	/// <summary>
	/// Gets or sets the amount of calibration compound (usually a concentration) for this level
	/// </summary>
	double BaseAmount { get; set; }

	/// <summary>
	/// array access operator to return Replicate array element.
	/// </summary>
	/// <param name="index">Index into the array</param>
	/// <returns>The requested replicate</returns>
	Replicate this[int index] { get; set; }

	/// <summary>
	/// Count all included/excluded replicates.
	/// <para>
	///     The included and excluded counts are incremented by the number of included
	///     and excluded points. These counters are not set to zero,
	///     allowing this method to be called repeatedly, for example to count
	///     replicates for all levels.
	/// </para>
	/// </summary>
	/// <param name="included">(updated) included counter</param>
	/// <param name="excluded">(updated) excluded counter</param>
	void CountReplicates(ref int included, ref int excluded);
}
