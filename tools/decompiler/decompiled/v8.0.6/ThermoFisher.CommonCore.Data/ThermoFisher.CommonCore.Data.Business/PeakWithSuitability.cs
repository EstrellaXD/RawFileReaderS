namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a peak which has included system suitability results.
/// </summary>
public class PeakWithSuitability : Peak, ISystemSuitabilityResultsAccess
{
	/// <summary>
	/// Gets or sets the suitability results.
	/// </summary>
	private readonly SystemSuitabilityResults _suitabilityResults;

	/// <summary>
	/// Gets the resolution of the peak (from other peaks)
	/// </summary>
	public double MeasuredResolution => _suitabilityResults.MeasuredResolution;

	/// <summary>
	/// Gets a value indicating whether the resolution test passed
	/// </summary>
	public bool PassedResolutionChecks => _suitabilityResults.PassedResolutionChecks;

	/// <summary>
	/// Gets a value indicating whether the symmetry test passed
	/// </summary>
	public bool PassedSymmetryChecks => _suitabilityResults.PassedSymmetryChecks;

	/// <summary>
	/// Gets a value indicating whether the peak width test passed
	/// </summary>
	public bool PassedPeakWidth => _suitabilityResults.PassedPeakWidth;

	/// <summary>
	/// Gets a value indicating whether the peak tailing test passed
	/// </summary>
	public bool PassedTailing => _suitabilityResults.PassedTailing;

	/// <summary>
	/// Gets a value indicating whether the column overload test passed
	/// </summary>
	public bool PassedColumnOverload => _suitabilityResults.PassedColumnOverload;

	/// <summary>
	/// Gets a value indicating whether the signal to noise test passed
	/// </summary>
	public bool PassedNoise => _suitabilityResults.PassedNoise;

	/// <summary>
	/// Gets a value indicating whether the saturation test passed
	/// </summary>
	public bool PassedSaturated => _suitabilityResults.PassedSaturated;

	/// <summary>
	/// Gets a value indicating whether the concave peak test passed
	/// </summary>
	public bool PassedConcave => _suitabilityResults.PassedConcave;

	/// <summary>
	/// Gets a value indicating whether the baseline clipping test passed
	/// </summary>
	public bool PassedBaselineClipped => _suitabilityResults.PassedBaselineClipped;

	/// <summary>
	/// Gets a value indicating whether the following values have been calculated:
	/// <c>PassedResolutionChecks</c>, <c>MeasuredResolution</c>
	/// </summary>
	public bool ResolutionChecksPerformed => _suitabilityResults.ResolutionChecksPerformed;

	/// <summary>
	/// Gets a value indicating whether the following values have been calculated:
	/// <c>PassedSymmetryChecks</c>
	/// </summary>
	public bool SymmetryChecksPerformed => _suitabilityResults.SymmetryChecksPerformed;

	/// <summary>
	/// Gets a value indicating whether the following values have been calculated:
	/// <c>PassedPeakWidth</c>, <c>PassedTailing</c>, <c>PassedColumnOverload</c>, <c>PassedNoise</c>, <c>PassedSaturated</c>,
	/// <c>PassedConcave</c>, <c>PassedBaselineClipped</c>
	/// </summary>
	public bool ClassificationChecksPerformed => _suitabilityResults.ClassificationChecksPerformed;

	/// <summary>
	/// Gets a value indicating whether the ResolutionCheck was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus ResolutionCheckStatus => _suitabilityResults.ResolutionCheckStatus;

	/// <summary>
	/// Gets a value indicating whether the SymmetryCheck was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus SymmetryCheckStatus => _suitabilityResults.SymmetryCheckStatus;

	/// <summary>
	/// Gets a value indicating whether the PeakWidth test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus PeakWidthStatus => _suitabilityResults.PeakWidthStatus;

	/// <summary>
	/// Gets a value indicating whether the Tailing test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus TailingStatus => _suitabilityResults.TailingStatus;

	/// <summary>
	/// Gets a value indicating whether the ColumnOverload test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus ColumnOverloadStatus => _suitabilityResults.ColumnOverloadStatus;

	/// <summary>
	/// Gets a value indicating whether the Noise test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus NoiseStatus => _suitabilityResults.NoiseStatus;

	/// <summary>
	/// Gets a value indicating whether the Saturated test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus SaturatedStatus => _suitabilityResults.SaturatedStatus;

	/// <summary>
	/// Gets a value indicating whether the Concave test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus ConcaveStatus => _suitabilityResults.ConcaveStatus;

	/// <summary>
	/// Gets a value indicating whether the BaselineClipped test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus BaselineClippedStatus => _suitabilityResults.BaselineClippedStatus;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakWithSuitability" /> class.
	/// Initializes from a peak and system suitability results.
	/// This clones data from the passed in interfaces.
	/// </summary>
	/// <param name="peak">
	/// The peak.
	/// </param>
	/// <param name="suitabilityResults">
	/// The suitability results.
	/// </param>
	public PeakWithSuitability(IPeakAccess peak, ISystemSuitabilityResultsAccess suitabilityResults)
	{
		CreateFromPeak(peak);
		_suitabilityResults = new SystemSuitabilityResults(suitabilityResults);
	}
}
