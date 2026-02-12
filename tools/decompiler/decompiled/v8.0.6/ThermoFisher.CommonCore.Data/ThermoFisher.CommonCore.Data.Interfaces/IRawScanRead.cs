using System;
using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a way to get an MS scan, which is
/// permitted to hold a reference to the internal (unmanaged) data
/// of a scan. This can be an advantage when an application
/// needs several parts of a scan. A disadvantage is that is must be disposed.
/// </summary>
public interface IRawScanRead : IDisposable
{
	/// <summary>
	/// Gets the scan index
	/// </summary>
	IMsScanIndexAccess ScanIndex { get; }

	/// <summary>
	/// Gets the scan filter
	/// </summary>
	/// <returns>Interface to read the filter</returns>
	IScanFilter Filter { get; }

	/// <summary>
	/// Gets the scan event
	/// </summary>
	/// <returns>Interface to read the scan event</returns>
	IScanEvent ScanEvent { get; }

	/// <summary>
	/// Gets a value indicating whether this is an FT formatted MS scan.
	/// </summary>
	bool IsFtFormatScan { get; }

	/// <summary>
	/// Reads the segmented scan (centroid or profile data)
	/// </summary>
	/// <returns>Interface to read the scan</returns>
	ISegmentedScanAccess ReadSegmentedScan();

	/// <summary>
	/// This method is similar to
	/// GetSegmentedScanFromScanNumber or GetSimplifiedCentroids in the IRawData interface.
	/// The method returns only the mass and intensity values from
	/// the scan data for a scan, reading form the centroids if FT data is available.
	/// Values for flags etc. are not returned, saving data space and improving efficiency.
	/// The method is designed for improved performance in custom XIC generators.
	/// </summary>
	/// <returns>Mass and intensity values from the scan.</returns>
	ISimpleScanAccess ReadSimplifiedScan();

	/// <summary>
	/// Gets the FT centroids
	/// </summary>
	/// <returns>Interface to read the centroids</returns>
	ICentroidStreamAccess ReadCentroidStream();

	/// <summary>
	/// Gets the noise data for the FT scan
	/// </summary>
	/// <returns>Noise data for this raw scan</returns>
	List<INoisePacket> ReadNoisePackets();
}
