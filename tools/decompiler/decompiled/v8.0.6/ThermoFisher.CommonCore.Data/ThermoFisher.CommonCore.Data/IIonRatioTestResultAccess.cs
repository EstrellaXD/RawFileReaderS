using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// This interfaces encapsulates access to the results of an ion ratio test for one ion.
/// </summary>
public interface IIonRatioTestResultAccess
{
	/// <summary>
	/// Gets a value indicating whether the co-elution test has passed for this ion
	/// </summary>
	bool PassedIonCoelutionTest { get; }

	/// <summary>
	/// Gets the results of the co-elution test
	/// targetCompoundPeak.Apex.RetentionTime - ion.Apex.RetentionTime;
	/// </summary>
	double MeasuredCoelution { get; }

	/// <summary>
	/// Gets the measured ion ratio, as a percentage
	/// <code>(qualifierIonResponse * 100) / targetCoumpoundResponce</code>
	/// </summary>
	double MeasuredRatio { get; }

	/// <summary>
	/// Gets a window in absolute % used to bound this test
	/// </summary>
	double AbsWindow { get; }

	/// <summary>
	/// Gets a value indicating whether the ratio test passed for this ion
	/// </summary>
	bool PassedIonRatioTest { get; }

	/// <summary>
	/// Gets the mass which was tested
	/// </summary>
	double Mass { get; }

	/// <summary>
	/// Gets the peak which was found in the IRC chromatogram
	/// </summary>
	Peak DetectedPeak { get; }
}
