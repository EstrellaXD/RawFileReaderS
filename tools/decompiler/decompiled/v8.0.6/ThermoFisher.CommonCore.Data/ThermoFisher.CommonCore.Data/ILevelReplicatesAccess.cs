using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Interface to read replicate information
/// </summary>
public interface ILevelReplicatesAccess : ICalibrationLevelAccess
{
	/// <summary>
	/// Gets the replicates of this calibration level
	/// </summary>
	ReadOnlyCollection<IReplicate> ReplicateCollection { get; }

	/// <summary>
	/// Gets the number of replicates for this level
	/// </summary>
	int Replicates { get; }

	/// <summary>
	/// Gets an array access operator to return Replicate array element.
	/// </summary>
	/// <param name="index">Index into the array</param>
	/// <returns>The requested replicate</returns>
	IReplicate this[int index] { get; }
}
