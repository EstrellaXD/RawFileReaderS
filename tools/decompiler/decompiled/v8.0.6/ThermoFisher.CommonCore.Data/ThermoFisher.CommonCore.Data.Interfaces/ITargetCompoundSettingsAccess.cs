using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to CalibrationTargetCompoundSettings
/// </summary>
public interface ITargetCompoundSettingsAccess
{
	/// <summary>
	/// Gets the table of calibration levels
	/// </summary>
	ReadOnlyCollection<ICalibrationLevelAccess> CalibrationLevels { get; }

	/// <summary>
	/// Gets the table of QC levels
	/// </summary>
	ReadOnlyCollection<IQualityControlLevelAccess> QcLevels { get; }

	/// <summary>
	/// Gets the calibration curve fitting method
	/// </summary>
	RegressionMethod CalibrationCurve { get; }

	/// <summary>
	/// Gets the weighting for calibration curve
	/// </summary>
	Weighting Weighting { get; }

	/// <summary>
	/// Gets the calibration curve origin mode
	/// </summary>
	Origin Origin { get; }

	/// <summary>
	/// Gets a value which determines how the response should be measured (using either peak height or peak area).
	/// </summary>
	ResponseRatio Response { get; }

	/// <summary>
	/// Gets the Unit for calibration
	/// </summary>
	string Units { get; }

	/// <summary>
	/// Gets the name of the internal standard for this component
	/// </summary>
	string InternalStandard { get; }

	/// <summary>
	/// Gets the isotopic contribution of the internal standard to the target compound
	/// </summary>
	double ContributionOfISTDToTarget { get; }

	/// <summary>
	/// Gets the isotopic contribution of the target compound to the internal standard
	/// </summary>
	double ContributionOfTargetToISTD { get; }
}
