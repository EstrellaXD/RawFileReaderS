namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Access to Dilution Level data from PMD file (obsolete? subset of a calibration level)
/// </summary>
public interface IDilutionLevelAccess
{
	/// <summary>
	/// Gets Anticipated amount of target compound in calibration of QC standard.
	/// </summary>
	double BaseAmount { get; }

	/// <summary>
	/// Gets QC test standard: <c>100 * (yobserved-ypredicted)/ypreditced</c>
	/// </summary>
	double TestPercent { get; }

	/// <summary>
	/// Gets the level name
	/// </summary>
	string LevelName { get; }
}
