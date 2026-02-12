using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to centroid stream
/// </summary>
public interface ICentroidStreamAccess : ISimpleScanAccess
{
	/// <summary>
	/// Gets the scan Number
	/// </summary>
	int ScanNumber { get; }

	/// <summary>
	/// Gets the number of centroids
	/// </summary>
	int Length { get; }

	/// <summary>
	/// Gets the coefficients count.
	/// </summary>
	int CoefficientsCount { get; }

	/// <summary>
	/// Gets the calibration Coefficients
	/// </summary>
	double[] Coefficients { get; }

	/// <summary>
	/// Gets resolution of each peak
	/// </summary>
	double[] Resolutions { get; }

	/// <summary>
	/// Gets the list of baseline at each peak
	/// </summary>
	double[] Baselines { get; }

	/// <summary>
	/// Gets the list of noise level near peak
	/// </summary>
	double[] Noises { get; }

	/// <summary>
	/// Gets the list of charge calculated for peak
	/// </summary>
	double[] Charges { get; }

	/// <summary>
	/// Gets the flags for the peaks (such as reference)
	/// </summary>
	PeakOptions[] Flags { get; }

	/// <summary>
	/// Get the data as one object per peaks.
	/// Note: This may copy data into an array on each call, and should therefore
	/// not be called multiple times on the same scan, for performance reasons.
	/// </summary>
	/// <returns>
	/// The array of <see cref="T:ThermoFisher.CommonCore.Data.Business.LabelPeak" />.
	/// </returns>
	IList<ICentroidPeak> GetCentroids();
}
