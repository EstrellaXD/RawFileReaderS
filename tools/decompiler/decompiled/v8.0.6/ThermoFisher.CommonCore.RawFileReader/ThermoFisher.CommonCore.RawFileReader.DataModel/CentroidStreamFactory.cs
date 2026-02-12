using System;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped centroid stream.
/// </summary>
internal static class CentroidStreamFactory
{
	/// <summary>
	/// Creates a CentroidStream from label peaks.
	/// </summary>
	/// <param name="labelPeaks">
	/// The label peaks.
	/// </param>
	/// <exception cref="T:System.ArgumentNullException">
	/// Thrown if labelPeaks is null
	/// </exception>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.CentroidStream" />.
	/// </returns>
	public static CentroidStream CreateCentroidStream(ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak[] labelPeaks)
	{
		if (labelPeaks == null)
		{
			throw new ArgumentNullException("labelPeaks");
		}
		CentroidStream centroidStream = new CentroidStream();
		CopyFrom(centroidStream, labelPeaks);
		return centroidStream;
	}

	/// <summary>
	/// Copy data from an array of LabelPeak
	/// </summary>
	/// <param name="stream">stream to fill</param>
	/// <param name="labelPeaks">
	/// The label peaks to copy.
	/// </param>
	private static void CopyFrom(CentroidStream stream, ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak[] labelPeaks)
	{
		int num = (stream.Length = labelPeaks.Length);
		int num3 = num;
		if (num3 != 0)
		{
			double[] array = (stream.Masses = new double[num3]);
			double[] array3 = array;
			array = (stream.Intensities = new double[num3]);
			double[] array5 = array;
			array = (stream.Resolutions = new double[num3]);
			double[] array7 = array;
			array = (stream.Baselines = new double[num3]);
			double[] array9 = array;
			array = (stream.Noises = new double[num3]);
			double[] array11 = array;
			array = (stream.Charges = new double[num3]);
			double[] array13 = array;
			PeakOptions[] array14 = (stream.Flags = new PeakOptions[num3]);
			PeakOptions[] array16 = array14;
			int num4 = 0;
			for (num = 0; num < labelPeaks.Length; num++)
			{
				ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak value = labelPeaks[num];
				array3[num4] = value.Mass;
				array5[num4] = value.Intensity;
				array7[num4] = value.Resolution;
				array9[num4] = value.Baseline;
				array11[num4] = value.Noise;
				array13[num4] = (int)value.Charge;
				array16[num4] = value.ToPeakOptions();
				num4++;
			}
			stream.Coefficients = Array.Empty<double>();
		}
	}
}
