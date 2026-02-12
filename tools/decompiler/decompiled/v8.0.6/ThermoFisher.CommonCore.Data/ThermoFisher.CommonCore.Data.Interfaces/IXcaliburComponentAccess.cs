using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read component data, as imported from an 
/// Xcalibur PMD file
/// </summary>
public interface IXcaliburComponentAccess
{
	/// <summary>
	/// Gets the settings for a manual noise region
	/// </summary>
	IManualNoiseAccess ManualNoiseSettings { get; }

	/// <summary>
	/// Gets settings for the ICIS peak integrator
	/// </summary>
	IIcisSettingsAccess IcisSettings { get; }

	/// <summary>
	/// Gets settings for the genesis peak integrator
	/// </summary>
	IGenesisRawSettingsAccess GenesisSettings { get; }

	/// <summary>
	/// Gets settings for peak location (expected retention time)
	/// </summary>
	IPeakLocationSettingsAccess LocationSettings { get; }

	/// <summary>
	/// Gets settings for the spectral find algorithm.
	/// </summary>
	IFindSettingsAccess FindSettings { get; }

	/// <summary>
	/// Gets settings for creating the component chromatogram
	/// </summary>
	IPeakChromatogramSettingsAccess ChromatogramSettings { get; }

	/// <summary>
	/// Gets the filter for this component as an interface.
	/// Note that a filter is also a property of IPeakChromatogramSettingsAccess,
	/// available a string, formatted to the mass precision of the component.
	/// </summary>
	IScanFilter ScanFilter { get; }

	/// <summary>
	/// Gets component calibration settings (including level tables)
	/// </summary>
	ICalibrationSettingsAccess CalibrationSettings { get; }

	/// <summary>
	/// Gets settings for the system suitability algorithm
	/// </summary>
	ISystemSuitabilitySettingsAccess SystemSuitabilitySettings { get; }

	/// <summary>
	/// Gets settings for the PDA peak purity algorithm
	/// </summary>
	IPeakPuritySettingsAccess PeakPuritySettings { get; }

	/// <summary>
	/// Gets the (avalon) integrator events
	/// </summary>
	ReadOnlyCollection<IntegratorEvent> IntegratorEvents { get; }

	/// <summary>
	/// Gets the settings for Ion Ration Confirmation
	/// </summary>
	IXcaliburIonRatioTestSettingsAccess IonRatioConfirmation { get; }

	/// <summary>
	/// Gets mass tolerance for this component
	/// </summary>
	IMassOptionsAccess ToleranceSettings { get; }

	/// <summary>
	/// Gets the name of this component
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets (custom) keys about this component
	/// This is treated as a comment field.
	/// Not used for any "built-in" calculations
	/// but may be used to annotate reports etc.
	/// </summary>
	string Keys { get; }

	/// <summary>
	/// Gets a value indicating whether this is used as a RT Reference for another component.
	/// </summary>
	bool UseAsRtReference { get; }

	/// <summary>
	/// Gets the retention time reference component.
	/// Adjust the retention time, using this component as a reference
	/// </summary>
	string AdjustUsing { get; }

	/// <summary>
	/// Gets the number of points to be averaged in peak detection and integration.
	/// </summary>
	int SmoothingPoints { get; }

	/// <summary>
	/// Gets the suggested view width for displaying the chromatogram (minutes)
	/// </summary>
	double DisplayWindowWidth { get; }

	/// <summary>
	/// Gets a value which determines which peak detector to use with the component
	/// </summary>
	PeakDetector PeakDetectionAlgorithm { get; }

	/// <summary>
	/// Gets "Fit Threshold" defined as 
	/// Min fit threshold (0-1.0) for detection by spectral fit.
	/// This value is believed to be not currently used in Xcalibur code (may be for an older fit algorithm)?
	/// Returned for completeness only.
	/// </summary>
	double FitThreshold { get; }

	/// <summary>
	/// Gets the calibration and quantification data.
	/// </summary>
	ICalibrationAndQuantificationThresholdLimitsAccess CalibrationAndQuantificationThresholdLimits { get; }

	/// <summary>
	/// Gets the detection threshold limits.
	/// </summary>
	IDetectionThresholdLimitsAccess DetectionThresholdLimits { get; }
}
