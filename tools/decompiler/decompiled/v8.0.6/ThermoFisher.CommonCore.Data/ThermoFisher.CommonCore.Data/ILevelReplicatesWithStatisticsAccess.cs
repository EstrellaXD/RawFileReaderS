namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// This interface represents replicate information after preforming calibration calculations
/// and determining statistics. These statistics are may be used to annotate calibration curves.
/// </summary>
public interface ILevelReplicatesWithStatisticsAccess : ILevelReplicatesAccess, ICalibrationLevelAccess, IReplicateStatisticsAccess
{
}
