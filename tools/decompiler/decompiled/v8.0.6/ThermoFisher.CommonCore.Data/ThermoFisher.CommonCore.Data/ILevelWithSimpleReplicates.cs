using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Import format for Xcalibur PMD
/// </summary>
public interface ILevelWithSimpleReplicates : IQualityControlLevelAccess, ICalibrationLevelAccess
{
	/// <summary>
	/// Gets replicate data, as saved in a PMD file
	/// </summary>
	ReadOnlyCollection<IReplicateDataAccess> ReplicateCollection { get; }
}
