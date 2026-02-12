namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// If a packet of a theoretical pattern is not found in the measured pattern, this causes
/// a penalty. This enum defines the magnitude of the penalty
/// </summary>
public enum MissingPacketPenaltyMode
{
	/// <summary>
	/// The penalty is one standard deviation
	/// </summary>
	Penalty1StdDevMode,
	/// <summary>
	/// The penalty is four standard deviations
	/// </summary>
	Penalty4StdDevMode,
	/// <summary>
	/// The penalty is sixteen standard deviations
	/// </summary>
	Penalty16StdDevMode,
	/// <summary>
	/// The penalty is the spectral distance to the closest packet
	/// </summary>
	PenaltyClosestMatchMode,
	/// <summary>
	/// The penalty is selected automatically. According to S/N
	/// </summary>
	PenaltyAutomaticMode,
	/// <summary>
	/// The penalty is selected automatically. According to S/N and ion statistic 
	/// </summary>
	PenaltyIonStatistic
}
