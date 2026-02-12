using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Results of running system suitability tests
/// </summary>
[Serializable]
[DataContract]
public class SystemSuitabilityResults : CommonCoreDataObject, ISystemSuitabilityResultsAccess, ICloneable
{
	/// <summary>
	/// Gets or sets the resolution of the peak (from other peaks)
	/// </summary>
	[DataMember]
	public double MeasuredResolution { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the resolution test passed
	/// </summary>
	[DataMember]
	public bool PassedResolutionChecks { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the symmetry test passed
	/// </summary>
	[DataMember]
	public bool PassedSymmetryChecks { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the peak width test passed
	/// </summary>
	[DataMember]
	public bool PassedPeakWidth { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the peak tailing test passed
	/// </summary>
	[DataMember]
	public bool PassedTailing { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the column overload test passed
	/// </summary>
	[DataMember]
	public bool PassedColumnOverload { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the signal to noise test passed
	/// </summary>
	[DataMember]
	public bool PassedNoise { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the saturation test passed
	/// </summary>
	[DataMember]
	public bool PassedSaturated { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the concave peak test passed
	/// </summary>
	[DataMember]
	public bool PassedConcave { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the baseline clipping test passed
	/// </summary>
	[DataMember]
	public bool PassedBaselineClipped { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the following values have been calculated:
	/// <c>PassedResolutionChecks</c>, <c>MeasuredResolution</c>
	/// </summary>
	[DataMember]
	public bool ResolutionChecksPerformed { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the following values have been calculated:
	/// <c>PassedSymmetryChecks</c>
	/// </summary>
	[DataMember]
	public bool SymmetryChecksPerformed { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the following values have been calculated:
	/// <c>PassedPeakWidth</c>, <c>PassedTailing</c>, <c>PassedColumnOverload</c>, <c>PassedNoise</c>, <c>PassedSaturated</c>,
	/// <c>PassedConcave</c>, <c>PassedBaselineClipped</c>
	/// </summary>
	[DataMember]
	public bool ClassificationChecksPerformed { get; set; }

	/// <summary>
	/// Gets a value which determines if the ResolutionCheck was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus ResolutionCheckStatus => ResultOf(ResolutionChecksPerformed, PassedResolutionChecks);

	/// <summary>
	/// Gets a value which determines if the SymmetryCheck was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus SymmetryCheckStatus => ResultOf(SymmetryChecksPerformed, PassedSymmetryChecks);

	/// <summary>
	/// Gets a value which determines if the PeakWidth test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus PeakWidthStatus => ResultOf(ClassificationChecksPerformed, PassedPeakWidth);

	/// <summary>
	/// Gets a value which determines if the Tailing test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus TailingStatus => ResultOf(ClassificationChecksPerformed, PassedTailing);

	/// <summary>
	/// Gets a value which determines if the ColumnOverload test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus ColumnOverloadStatus => ResultOf(ClassificationChecksPerformed, PassedColumnOverload);

	/// <summary>
	/// Gets a value which determines if the Noise test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus NoiseStatus => ResultOf(ClassificationChecksPerformed, PassedNoise);

	/// <summary>
	/// Gets a value which determines if the Saturated test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus SaturatedStatus => ResultOf(ClassificationChecksPerformed, PassedSaturated);

	/// <summary>
	/// Gets a value which determines if the Concave test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus ConcaveStatus => ResultOf(ClassificationChecksPerformed, PassedConcave);

	/// <summary>
	/// Gets a value which determines if the BaselineClipped test was performed, and the results of the test, when run.
	/// </summary>
	public ResultStatus BaselineClippedStatus => ResultOf(ClassificationChecksPerformed, PassedBaselineClipped);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SystemSuitabilityResults" /> class. 
	/// Default constructor
	/// </summary>
	public SystemSuitabilityResults()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SystemSuitabilityResults" /> class. 
	/// Copy constructor
	/// </summary>
	/// <param name="access">
	/// The access.
	/// </param>
	public SystemSuitabilityResults(ISystemSuitabilityResultsAccess access)
	{
		if (access != null)
		{
			ClassificationChecksPerformed = access.ClassificationChecksPerformed;
			MeasuredResolution = access.MeasuredResolution;
			PassedBaselineClipped = access.PassedBaselineClipped;
			PassedColumnOverload = access.PassedColumnOverload;
			PassedConcave = access.PassedConcave;
			PassedNoise = access.PassedNoise;
			PassedPeakWidth = access.PassedPeakWidth;
			PassedResolutionChecks = access.PassedResolutionChecks;
			PassedSaturated = access.PassedSaturated;
			PassedSymmetryChecks = access.PassedSymmetryChecks;
			PassedTailing = access.PassedTailing;
			ResolutionChecksPerformed = access.ResolutionChecksPerformed;
			SymmetryChecksPerformed = access.SymmetryChecksPerformed;
		}
	}

	/// <summary>
	/// Return tri-state test status, based on if a test has been run, and the test results
	/// </summary>
	/// <param name="testPerformed">
	/// true if a test has been run
	/// </param>
	/// <param name="testPassed">
	/// The results of the test (if run) ignored otherwise
	/// </param>
	/// <returns>
	/// A status of not tested, when the test is not run, or the pass/fail result when the test has been run
	/// </returns>
	private static ResultStatus ResultOf(bool testPerformed, bool testPassed)
	{
		if (testPerformed)
		{
			if (!testPassed)
			{
				return ResultStatus.Failed;
			}
			return ResultStatus.Passed;
		}
		return ResultStatus.NotTested;
	}

	/// <summary>
	/// Make a copy of the system suitability results
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Object" />.
	/// </returns>
	public object Clone()
	{
		return new SystemSuitabilityResults(this);
	}
}
