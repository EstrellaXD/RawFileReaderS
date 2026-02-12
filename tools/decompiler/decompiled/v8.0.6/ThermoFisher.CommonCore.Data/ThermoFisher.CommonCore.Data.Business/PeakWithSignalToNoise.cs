using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a peak which has included signal to noise results.
/// </summary>
public class PeakWithSignalToNoise : Peak, ISignalToNoiseResultsAccess
{
	/// <summary>
	/// Simple implementation of ISignalToNoiseResultsAccess + setters
	/// </summary>
	private class SignalToNoiseResult : ISignalToNoiseResultsAccess
	{
		public double PeakRetentionTime { get; set; }

		public double PeakWidthHalfHeight { get; set; }

		public double PeakArea { get; set; }

		public double PeakHeight { get; set; }

		public double PeakBaseline { get; set; }

		public double PeakStartTime { get; set; }

		public double PeakEndTime { get; set; }

		public double NoiseStartTime { get; set; }

		public double NoiseEndTime { get; set; }

		public double NoiseMinIntensity { get; set; }

		public double NoiseMaxIntensity { get; set; }

		public double Noise { get; set; }

		public double NoiseSlope { get; set; }

		public double NoiseOffset { get; set; }

		public double SignalToNoiseRatio { get; set; }

		public NoiseClassification NoiseClassification { get; set; }

		/// <summary>
		/// Construct by copying from interface
		/// </summary>
		/// <param name="access">interface to copy</param>
		public SignalToNoiseResult(ISignalToNoiseResultsAccess access)
		{
			PeakRetentionTime = access.PeakRetentionTime;
			PeakWidthHalfHeight = access.PeakWidthHalfHeight;
			PeakArea = access.PeakArea;
			PeakHeight = access.PeakHeight;
			PeakBaseline = access.PeakBaseline;
			PeakStartTime = access.PeakStartTime;
			PeakEndTime = access.PeakEndTime;
			NoiseStartTime = access.NoiseStartTime;
			NoiseEndTime = access.NoiseEndTime;
			NoiseMinIntensity = access.NoiseMinIntensity;
			NoiseMaxIntensity = access.NoiseMaxIntensity;
			Noise = access.Noise;
			NoiseSlope = access.NoiseSlope;
			NoiseOffset = access.NoiseOffset;
			SignalToNoiseRatio = access.SignalToNoiseRatio;
			NoiseClassification = access.NoiseClassification;
		}
	}

	private readonly ISignalToNoiseResultsAccess _noiseResults;

	/// <inheritdoc />
	public double PeakRetentionTime => _noiseResults.PeakRetentionTime;

	/// <inheritdoc />
	public double PeakWidthHalfHeight => _noiseResults.PeakWidthHalfHeight;

	/// <inheritdoc />
	public double PeakArea => _noiseResults.PeakArea;

	/// <inheritdoc />
	public double PeakHeight => _noiseResults.PeakHeight;

	/// <inheritdoc />
	public double PeakBaseline => _noiseResults.PeakBaseline;

	/// <inheritdoc />
	public double PeakStartTime => _noiseResults.PeakStartTime;

	/// <inheritdoc />
	public double PeakEndTime => _noiseResults.PeakEndTime;

	/// <inheritdoc />
	public double NoiseStartTime => _noiseResults.NoiseStartTime;

	/// <inheritdoc />
	public double NoiseEndTime => _noiseResults.NoiseEndTime;

	/// <inheritdoc />
	public double NoiseMinIntensity => _noiseResults.NoiseMinIntensity;

	/// <inheritdoc />
	public double NoiseMaxIntensity => _noiseResults.NoiseMaxIntensity;

	/// <inheritdoc />
	public double NoiseSlope => _noiseResults.NoiseSlope;

	/// <inheritdoc />
	public double NoiseOffset => _noiseResults.NoiseOffset;

	/// <inheritdoc />
	public double SignalToNoiseRatio => _noiseResults.SignalToNoiseRatio;

	/// <inheritdoc />
	public NoiseClassification NoiseClassification => _noiseResults.NoiseClassification;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakWithSignalToNoise" /> class.
	/// Initializes from a peak and signal to noise results.
	/// This clones data from the passed in interfaces.
	/// </summary>
	/// <param name="peak">
	/// The peak.
	/// </param>
	/// <param name="noiseResults">
	/// The noise results.
	/// </param>
	public PeakWithSignalToNoise(IPeakAccess peak, ISignalToNoiseResultsAccess noiseResults)
	{
		if (noiseResults == null)
		{
			throw new ArgumentNullException("noiseResults");
		}
		CreateFromPeak(peak);
		_noiseResults = new SignalToNoiseResult(noiseResults);
	}
}
