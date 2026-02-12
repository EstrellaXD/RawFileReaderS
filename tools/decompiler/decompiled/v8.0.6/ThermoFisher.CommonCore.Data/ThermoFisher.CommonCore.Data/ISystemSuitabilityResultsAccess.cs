namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to Results of running system suitability tests
/// </summary>
public interface ISystemSuitabilityResultsAccess
{
	/// <summary>
	/// Gets the resolution of the peak (from other peaks)
	/// </summary>
	double MeasuredResolution { get; }

	/// <summary>
	/// Gets a value indicating whether the resolution test passed
	/// </summary>
	bool PassedResolutionChecks { get; }

	/// <summary>
	/// Gets a value indicating whether the symmetry test passed
	/// </summary>
	bool PassedSymmetryChecks { get; }

	/// <summary>
	/// Gets a value indicating whether the peak width test passed
	/// </summary>
	bool PassedPeakWidth { get; }

	/// <summary>
	/// Gets a value indicating whether the peak tailing test passed
	/// </summary>
	bool PassedTailing { get; }

	/// <summary>
	/// Gets a value indicating whether the column overload test passed
	/// </summary>
	bool PassedColumnOverload { get; }

	/// <summary>
	/// Gets a value indicating whether the signal to noise test passed
	/// </summary>
	bool PassedNoise { get; }

	/// <summary>
	/// Gets a value indicating whether the saturation test passed
	/// </summary>
	bool PassedSaturated { get; }

	/// <summary>
	/// Gets a value indicating whether the concave peak test passed
	/// </summary>
	bool PassedConcave { get; }

	/// <summary>
	/// Gets a value indicating whether the baseline clipping test passed
	/// </summary>
	bool PassedBaselineClipped { get; }

	/// <summary>
	/// Gets a value indicating whether the following values have been calculated:
	/// <c>PassedResolutionChecks</c>, <c>MeasuredResolution</c>
	/// </summary>
	bool ResolutionChecksPerformed { get; }

	/// <summary>
	/// Gets a value indicating whether the following values have been calculated:
	/// <c>PassedSymmetryChecks</c>
	/// </summary>
	bool SymmetryChecksPerformed { get; }

	/// <summary>
	/// Gets a value indicating whether the following values have been calculated:
	/// <c>PassedPeakWidth</c>, <c>PassedTailing</c>, <c>PassedColumnOverload</c>, <c>PassedNoise</c>, <c>PassedSaturated</c>,
	/// <c>PassedConcave</c>, <c>PassedBaselineClipped</c>
	/// </summary>
	bool ClassificationChecksPerformed { get; }

	/// <summary>
	/// Gets a value indicating whether the ResolutionCheck was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus ResolutionCheckStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the SymmetryCheck was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus SymmetryCheckStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the PeakWidth test was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus PeakWidthStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the Tailing test was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus TailingStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the ColumnOverload test was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus ColumnOverloadStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the Noise test was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus NoiseStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the Saturated test was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus SaturatedStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the Concave test was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus ConcaveStatus { get; }

	/// <summary>
	/// Gets a value indicating whether the BaselineClipped test was performed, and the results of the test, when run.
	/// </summary>
	ResultStatus BaselineClippedStatus { get; }
}
