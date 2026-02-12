using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Readonly access to a peak
/// </summary>
public interface IPeakAccess
{
	/// <summary>
	/// Gets a list of peaks that have been merged
	/// </summary>
	ReadOnlyCollection<IPeakAccess> MergedPeaks { get; }

	/// <summary>
	/// Gets a value which determines how signal to noise has been calculated.
	/// When the returns <see cref="T:ThermoFisher.CommonCore.Data.NoiseClassification" />.Value, a numeric value can
	/// be obtained from <see cref="P:ThermoFisher.CommonCore.Data.IPeakAccess.SignalToNoise" />.
	/// </summary>
	NoiseClassification NoiseResult { get; }

	/// <summary>
	/// Gets the Signal To Noise ratio.
	/// If <see cref="P:ThermoFisher.CommonCore.Data.IPeakAccess.NoiseResult" /> is <see cref="T:ThermoFisher.CommonCore.Data.NoiseClassification" />.Value, then this property returns the signal to noise ratio.
	/// Otherwise this should not be used. Use <see cref="T:ThermoFisher.CommonCore.Data.EnumFormat" />.ToString(<see cref="P:ThermoFisher.CommonCore.Data.IPeakAccess.NoiseResult" />) instead.
	/// </summary>
	double SignalToNoise { get; }

	/// <summary>
	/// Gets the position, height, baseline at left limit
	/// </summary>
	PeakPoint Left { get; }

	/// <summary>
	/// Gets the position, height, baseline  at peak apex
	/// </summary>
	PeakPoint Apex { get; }

	/// <summary>
	/// Gets the position, height, baseline at right limit
	/// </summary>
	PeakPoint Right { get; }

	/// <summary>
	/// Gets the integrated peak area
	/// </summary>
	double Area { get; }

	/// <summary>
	/// Gets the mass of the base peak from the apex scan.
	/// </summary>
	double BasePeakMass { get; }

	/// <summary>
	/// Gets the mass to charge ratio of peak.
	/// </summary>
	double MassToCharge { get; }

	/// <summary>
	/// Gets the expected RT after making any RT adjustments.
	/// </summary>
	double ExpectedRT { get; }

	/// <summary>
	/// Gets the noise measured in detected peak (for signal to noise calculation)
	/// </summary>
	double Noise { get; }

	/// <summary>
	/// Gets a value indicating whether the "Noise" value was calculated by an RMS algorithm.
	/// </summary>
	bool RmsNoise { get; }

	/// <summary>
	/// Gets the apex of the peak corresponds to a particular signal.
	/// This gives the scan number of that signal.
	/// If no scan numbers are sent with the peak detection signal, then
	/// the scan number = "signal index at apex +1".
	/// Note that there is no guarantee that left and right edges will always be exactly on a scan, even
	/// though most peak detectors behave that way, so this is not added as a property of <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakPoint" />
	/// </summary>
	int ScanAtApex { get; }

	/// <summary>
	/// Gets the name for this peak (for example, analyte name)
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the number of scans integrated
	/// </summary>
	int Scans { get; }

	/// <summary>
	/// Gets the left edge type. This describes why the peak started. It is only set by the Genesis Detector.
	/// </summary>
	EdgeType LeftEdge { get; }

	/// <summary>
	/// Gets the right edge type. This describes why the peak ended. It is only set by the Genesis Detector.
	/// </summary>
	EdgeType RightEdge { get; }

	/// <summary>
	/// Gets a value indicating whether the peak is Valid.
	/// Peaks are assumed to have valid data, but may be marked invalid by
	/// an integrator, if failing certain tests.
	/// Invalid peaks should never be returned to a calling application
	/// by an integrator algorithm.
	/// </summary>
	bool Valid { get; }

	/// <summary>
	/// Gets a value indicating whether this <see cref="T:ThermoFisher.CommonCore.Data.Business.Peak" /> is saturated.
	/// </summary>
	/// <value>true when integration/mass range has saturation.</value>
	bool Saturated { get; }

	/// <summary>
	/// Gets a value indicating whether valley detection was used when detecting this peak.
	/// </summary>
	bool ValleyDetect { get; }

	/// <summary>
	/// Gets the direction of peak (Positive or Negative)
	/// </summary>
	PeakDirection Direction { get; }

	/// <summary>
	/// Gets the chi-squared error in fitting the peak.
	/// </summary>
	double Fit { get; }

	/// <summary>
	/// Gets the calculated width, or 'gamma_r' for PPD peaks.
	/// Gets the PeakWidth At Half Height for all other peaks, when not 0.0.
	/// </summary>
	double FittedWidth { get; }

	/// <summary>
	/// Gets the calculated intensity, or 'gamma_A'.
	/// </summary>
	double FittedIntensity { get; }

	/// <summary>
	/// Gets the calculated position, or 'gamma_t0'.
	/// </summary>
	double FittedRT { get; }

	/// <summary>
	/// Gets the calculated 4th parameter for gamma (gamma_M) or EMG functions.
	/// </summary>
	double FittedAsymmetry { get; }

	/// <summary>
	/// Gets the peak shape used in the fitting procedure.
	/// </summary>
	int FittedFunction { get; }

	/// <summary>
	/// Gets the number of data points used in the fit.
	/// </summary>
	int FittedPoints { get; }

	/// <summary>
	/// Gets purity of the peak
	/// </summary>
	double Purity { get; }

	/// <summary>
	/// Gets the low time from peak purity calculation
	/// </summary>
	double PurityLowTime { get; }

	/// <summary>
	/// Gets the high time from peak purity calculation
	/// </summary>
	double PurityHighTime { get; }
}
