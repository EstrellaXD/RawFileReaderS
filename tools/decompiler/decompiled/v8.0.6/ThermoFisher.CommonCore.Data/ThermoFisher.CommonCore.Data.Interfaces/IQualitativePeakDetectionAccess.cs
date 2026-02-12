using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a reader for importing the peak detection settings from a PMD file.
/// In UI, This may be separately displayed in tabs as chromatogram settings, and settings
/// for various integrators.
/// </summary>
public interface IQualitativePeakDetectionAccess
{
	/// <summary>
	/// Gets the settings for the ICIS integrator
	/// </summary>
	IIcisSettingsAccess IcisSettings { get; }

	/// <summary>
	/// Gets the settings for the Genesis integrator
	/// Note: This property is under review.
	/// May return an alternative interface
	/// </summary>
	IGenesisRawSettingsAccess GenesisSettings { get; }

	/// <summary>
	/// Gets the settings for creating a chromatogram
	/// </summary>
	IPeakChromatogramSettingsAccess ChromatogramSettings { get; }

	/// <summary>
	/// Gets the manual noise range settings
	/// </summary>
	IManualNoiseAccess ManualNoise { get; }

	/// <summary>
	/// Gets settings for the maximizing masses algorithm
	/// Note: This algorithm is not used by product "Xcalibur"
	/// </summary>
	IMaximizingMassesAccess MaximizingMasses { get; }

	/// <summary>
	/// Gets settings to limit (filter) the list of returned peaks
	/// after integration
	/// </summary>
	IPeakLimitsAccess LimitPeakSettings { get; }

	/// <summary>
	/// Gets a value indicating whether peak detection is enabled.
	/// Note: This property is not used in product "Xcalibur"
	/// </summary>
	bool EnableDetection { get; }

	/// <summary>
	/// Gets the number of smoothing points, for background analysis
	/// This setting is common to all integrators
	/// </summary>
	int SmoothingPoints { get; }

	/// <summary>
	/// Gets the width of display window for the peak (in seconds)
	/// This is for presentation only
	/// </summary>
	double DisplayWindowWidth { get; }

	/// <summary>
	/// Gets the time range, over which qualitative processing is done.
	/// Only peaks detected within this range are processed further
	/// (for example, library searched)
	/// </summary>
	Range RetentionTimeWindow { get; }

	/// <summary>
	/// Gets the Algorithm to use (Genesis, ICIS etc.)
	/// </summary>
	PeakDetector PeakDetectionAlgorithm { get; }

	/// <summary>
	/// Gets Number of decimals used in defining mass values
	/// </summary>
	int MassPrecision { get; }

	/// <summary>
	/// Gets tolerance used for mass
	/// </summary>
	double MassTolerance { get; }

	/// <summary>
	/// Gets units of mass tolerance
	/// </summary>
	ToleranceUnits ToleranceUnits { get; }

	/// <summary>
	/// Gets the component name
	/// </summary>
	string ComponentName { get; }

	/// <summary>
	/// Gets the scan filter, as an interface.
	/// This same data is available in string form
	/// in the ChromatogramSettings property
	/// </summary>
	IScanFilter ScanFilter { get; }

	/// <summary>
	/// Gets the (avalon) integrator events
	/// </summary>
	ReadOnlyCollection<IntegratorEvent> IntegratorEvents { get; }
}
