using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to calibration curve
/// </summary>
public interface ICalibrationCurveDataAccess
{
	/// <summary>
	/// Gets the fitted line
	/// </summary>
	ReadOnlyCollection<ICalibrationCurvePointAccess> FittedLinePoints { get; }

	/// <summary>
	/// Gets the included replicates from current sequence data
	/// </summary>
	ReadOnlyCollection<ICalibrationCurvePointAccess> IncludedPoints { get; }

	/// <summary>
	/// Gets the excluded replicates from current sequence data
	/// </summary>
	ReadOnlyCollection<ICalibrationCurvePointAccess> ExcludedPoints { get; }

	/// <summary>
	/// Gets the included replicates from previously acquired data
	/// </summary>
	ReadOnlyCollection<ICalibrationCurvePointAccess> ExternalIncludedPoints { get; }

	/// <summary>
	/// Gets the excluded replicates from previously acquired  data
	/// </summary>
	ReadOnlyCollection<ICalibrationCurvePointAccess> ExternalExcludedPoints { get; }

	/// <summary>
	/// Gets the RSquared value from the regression calculation (-1 if not valid)
	/// </summary>
	double RSquared { get; }

	/// <summary>
	/// Gets the equation text from the regression calculation.
	/// </summary>
	string Equation { get; }

	/// <summary>
	/// Gets the percentage coefficient of variance from the first calibration level.
	/// </summary>
	double PercentCv { get; }

	/// <summary>
	/// Gets the percentage relative standard deviation from the first calibration level.
	/// </summary>
	double PercentRsd { get; }

	/// <summary>
	/// Gets a value indicating whether the fitted line is empty
	/// The curve data needs to be plotted as appropriate for
	/// an internal standard: Centered on the set of points,
	/// </summary>
	bool IsInternalStandard { get; }
}
