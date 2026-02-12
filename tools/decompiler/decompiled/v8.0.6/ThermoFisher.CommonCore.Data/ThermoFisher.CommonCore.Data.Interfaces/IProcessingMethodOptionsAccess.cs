using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// This interface permits reading of the "Options" structure
/// from an Xcalibur processing method.
/// </summary>
public interface IProcessingMethodOptionsAccess
{
	/// <summary>
	/// Gets a value indicating whether the standards are internal or external.
	/// </summary>
	CalStandards CalibrationType { get; }

	/// <summary>
	/// Gets a value indicating whether calibration is performed on concentration or amount
	/// </summary>
	CalibrateAs CalibrateAs { get; }

	/// <summary>
	/// Gets a value determining how void time is calculated.
	/// </summary>
	VoidTime VoidTime { get; }

	/// <summary>
	/// Gets a value determining whether amounts or concentrations are reported.
	/// </summary>
	ReportAs ReportAs { get; }

	/// <summary>
	/// Gets a value determining how chromatography was performed.
	/// </summary>
	ChromatographyType ChromatographyType { get; }

	/// <summary>
	/// Gets a value indicating whether outliers (on a cal curve) should be rejected.
	/// </summary>
	bool RejectOutliers { get; }

	/// <summary>
	/// Gets the added time of void volume, where void time is set to "Value"
	/// </summary>
	double VoidTimeValue { get; }

	/// <summary>
	/// Gets the permitted % deviation from an expected standard amount.
	/// </summary>
	double AllowedDevPercent { get; }

	/// <summary>
	/// Gets the search window for the expected time of a peak.
	/// </summary>
	double SearchWindow { get; }

	/// <summary>
	/// Gets the minimum number of expected scans in a baseline
	/// Genesis: MinScansInBaseline
	/// </summary>
	int MinScansInBaseline { get; }

	/// <summary>
	/// Gets a scale factor for the noise level in chromatographic peaks.
	/// </summary>
	double InitialNoiseScale { get; }

	/// <summary>
	/// Gets the limit on baseline noise
	/// </summary>
	double BaseNoiseLimit { get; }

	/// <summary>
	/// Gets the background width (scans)
	/// </summary>
	int BackgroundWidth { get; }

	/// <summary>
	/// Gets the baseline noise rejection factor
	/// </summary>
	double BaseNoiseRejectionFactor { get; }

	/// <summary>
	/// Gets a value indicating whether the "alternate Percent RDS calculation" should be performed.
	/// </summary>
	bool UseAltPercentRsdCalc { get; }

	/// <summary>
	/// Gets a value indicating whether there was a "manual change" to calibration levels.
	/// </summary>
	bool CalLevelsManuallyChanged { get; }

	/// <summary>
	/// Gets the low intensity cutoff
	/// </summary>
	int LowIntensityCutoff { get; }

	/// <summary>
	/// Read the table of dilution levels
	/// </summary>
	/// <returns>The dilution levels</returns>
	ReadOnlyCollection<IDilutionLevelAccess> GetDilutionLevels();

	/// <summary>
	/// Gets a copy of the dilution target component factors table
	/// </summary>
	/// <returns>The dilution target component factors table</returns>
	ReadOnlyCollection<IDilutionTargetCompFactorAccess> GetDilutionFactors();
}
