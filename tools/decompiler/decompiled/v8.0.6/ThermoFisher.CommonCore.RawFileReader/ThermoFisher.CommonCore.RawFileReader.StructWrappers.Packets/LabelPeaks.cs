using System;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// The class contains label peaks and noise info.
/// </summary>
internal class LabelPeaks
{
	private LabelPeak[] _labelPeaks;

	private NoiseInfoPacketStruct[] _noiseData;

	/// <summary>
	/// Gets the Centroid peaks.
	/// </summary>
	public LabelPeak[] Peaks
	{
		get
		{
			if (!IsNoiseUpdated)
			{
				UpdateNoise();
			}
			return _labelPeaks;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether noise has been updated.
	/// </summary>
	private bool IsNoiseUpdated { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeaks" /> class.
	/// </summary>
	public LabelPeaks()
	{
		IsNoiseUpdated = false;
		_labelPeaks = Array.Empty<LabelPeak>();
		_noiseData = Array.Empty<NoiseInfoPacketStruct>();
	}

	/// <summary>
	/// Set the label peaks.
	/// </summary>
	/// <param name="peaks">
	/// The _labelPeaks.
	/// </param>
	internal void SetLabelPeaks(LabelPeak[] peaks)
	{
		_labelPeaks = peaks;
		IsNoiseUpdated = false;
	}

	/// <summary>
	/// The set noise info packets.
	/// </summary>
	/// <param name="noiseInfoPackets">
	/// The noise info packets.
	/// </param>
	internal void SetNoiseInfoPackets(NoiseInfoPacketStruct[] noiseInfoPackets)
	{
		_noiseData = noiseInfoPackets;
		IsNoiseUpdated = false;
	}

	/// <summary>
	/// Interpolate noise or baseline value.
	/// </summary>
	/// <param name="currentValue">
	/// The current value.
	/// </param>
	/// <param name="previousValue">
	/// The previous value.
	/// </param>
	/// <param name="currentMass">
	/// The current mass.
	/// </param>
	/// <param name="previousMass">
	/// The previous mass.
	/// </param>
	/// <param name="slope">
	/// The slope.
	/// </param>
	/// <returns>
	/// The interpolated value.
	/// </returns>
	private static double InterpolateValues(double currentValue, double previousValue, double currentMass, double previousMass, out double slope)
	{
		slope = (currentValue - previousValue) / (currentMass - previousMass);
		return previousValue - slope * previousMass;
	}

	/// <summary>
	/// The update noise.
	/// </summary>
	private void UpdateNoise()
	{
		if (_labelPeaks.Length == 0)
		{
			IsNoiseUpdated = true;
			return;
		}
		if (_noiseData.Length == 0)
		{
			IsNoiseUpdated = true;
			return;
		}
		NoiseInfoPacketStruct noiseInfoPacketStruct = _noiseData[0];
		float noise = noiseInfoPacketStruct.Noise;
		float baseline = noiseInfoPacketStruct.Baseline;
		float mass = noiseInfoPacketStruct.Mass;
		int num = _labelPeaks.Length;
		int i;
		for (i = 0; i < num; i++)
		{
			LabelPeak labelPeak = _labelPeaks[i];
			if (labelPeak.Mass > (double)mass)
			{
				break;
			}
			labelPeak.Noise = noise;
			labelPeak.Baseline = baseline;
			_labelPeaks[i] = labelPeak;
		}
		NoiseInfoPacketStruct noiseInfoPacketStruct2 = noiseInfoPacketStruct;
		for (int j = 1; j < _noiseData.Length; j++)
		{
			NoiseInfoPacketStruct noiseInfoPacketStruct3 = _noiseData[j];
			double slope;
			double num2 = InterpolateValues(noiseInfoPacketStruct3.Noise, noiseInfoPacketStruct2.Noise, noiseInfoPacketStruct3.Mass, noiseInfoPacketStruct2.Mass, out slope);
			double slope2;
			double num3 = InterpolateValues(noiseInfoPacketStruct3.Baseline, noiseInfoPacketStruct2.Baseline, noiseInfoPacketStruct3.Mass, noiseInfoPacketStruct2.Mass, out slope2);
			double num4 = noiseInfoPacketStruct3.Mass;
			while (i < num)
			{
				LabelPeak labelPeak3;
				LabelPeak labelPeak2 = (labelPeak3 = _labelPeaks[i]);
				double mass2;
				if (!((mass2 = labelPeak2.Mass) <= num4))
				{
					break;
				}
				labelPeak3.Noise = (float)(slope * mass2 + num2);
				labelPeak3.Baseline = (float)(slope2 * mass2 + num3);
				_labelPeaks[i++] = labelPeak3;
			}
			noiseInfoPacketStruct2 = noiseInfoPacketStruct3;
		}
		NoiseInfoPacketStruct noiseInfoPacketStruct4 = _noiseData[_noiseData.Length - 1];
		while (i < num)
		{
			LabelPeak labelPeak4 = _labelPeaks[i];
			labelPeak4.Noise = noiseInfoPacketStruct4.Noise;
			labelPeak4.Baseline = noiseInfoPacketStruct4.Baseline;
			_labelPeaks[i++] = labelPeak4;
		}
		IsNoiseUpdated = true;
	}
}
