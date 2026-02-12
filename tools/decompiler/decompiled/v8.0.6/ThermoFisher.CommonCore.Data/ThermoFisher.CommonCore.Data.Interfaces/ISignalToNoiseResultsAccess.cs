namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines signal to noise calculation measurements
/// which may be shown on a peak label or tabulated.
/// </summary>
public interface ISignalToNoiseResultsAccess
{
	/// <summary>
	/// Gets the noise classification, which must be
	/// checked to see if a valid noise value is available.
	/// </summary>
	NoiseClassification NoiseClassification { get; }

	/// <summary>
	/// Gets the peak retention time.
	/// </summary>
	double PeakRetentionTime { get; }

	/// <summary>
	/// Gets the peak width at half height above baseline
	/// </summary>
	double PeakWidthHalfHeight { get; }

	/// <summary>
	/// Gets the peak area.
	/// </summary>
	double PeakArea { get; }

	/// <summary>
	/// Gets the height of the signal (i.e. peak height above baseline).
	/// </summary>
	double PeakHeight { get; }

	/// <summary>
	/// Gets the peak baseline value determined by the noise slope and intercept values.
	/// </summary>
	double PeakBaseline { get; }

	/// <summary>
	/// Gets the start time of the peak (i.e. peak left extent)
	/// </summary>
	double PeakStartTime { get; }

	/// <summary>
	/// Gets the end time of the peak (i.e. peak right extent)
	/// </summary>
	double PeakEndTime { get; }

	/// <summary>
	/// Gets the start time of the noise determination range.
	/// </summary>
	double NoiseStartTime { get; }

	/// <summary>
	/// Gets the end time of the noise determination range.
	/// </summary>
	double NoiseEndTime { get; }

	/// <summary>
	/// Gets the minimum intensity of noise points.
	/// </summary>
	double NoiseMinIntensity { get; }

	/// <summary>
	/// Gets the Maximum intensity of noise points.
	/// </summary>
	double NoiseMaxIntensity { get; }

	/// <summary>
	/// Gets the noise part of the signal to noise estimation.
	/// </summary>
	double Noise { get; }

	/// <summary>
	/// Gets the slope of the fitted line.
	/// </summary>
	double NoiseSlope { get; }

	/// <summary>
	/// Gets the offset of the fitted line.
	/// </summary>
	double NoiseOffset { get; }

	/// <summary>
	/// Gets the ratio of the signal to noise estimation (<see cref="P:ThermoFisher.CommonCore.Data.Interfaces.ISignalToNoiseResultsAccess.PeakHeight" /> divided by <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.ISignalToNoiseResultsAccess.Noise" />).
	/// </summary>
	double SignalToNoiseRatio { get; }
}
