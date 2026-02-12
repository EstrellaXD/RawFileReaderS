namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to Peak Location Settings
/// </summary>
public interface IPeakLocationSettingsAccess
{
	/// <summary>
	/// Gets a value indicating whether the retention time should be adjusted based on a reference peak.
	/// </summary>
	bool AdjustExpectedRT { get; }

	/// <summary>
	/// Gets the expected time, as in the method (before any adjustments)
	/// </summary>
	double UserEnteredRT { get; }

	/// <summary>
	/// Gets a value which determine how a single peak is found from the list of
	/// returned peaks from integrating the chromatogram.
	/// For example: Highest peak in time window.
	/// </summary>
	PeakMethod LocateMethod { get; }

	/// <summary>
	/// Gets the window, centered around the peak, in minutes.
	/// The located peak must be within a window of expected +/- width.
	/// </summary>
	double SearchWindow { get; }

	/// <summary>
	/// Gets the baseline and noise window.
	/// This setting is used to restrict the chromatogram.
	/// Only scans within the range "adjusted expected RT" +/- Window are processed.
	/// For example: a 1 minute window setting implies 2 minutes of data.
	/// </summary>
	double BaselineAndNoiseWindow { get; }

	/// <summary>
	/// Gets the settings for finding a peak based on spectral fit
	/// </summary>
	IFindSettingsAccess FindSettings { get; }

	/// <summary>
	/// Gets the signal to noise rejection parameter for peaks
	/// </summary>
	double SignalToNoiseThreshold { get; }
}
