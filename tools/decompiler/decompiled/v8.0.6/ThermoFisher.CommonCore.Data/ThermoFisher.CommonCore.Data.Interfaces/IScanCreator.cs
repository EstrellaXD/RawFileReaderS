using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ScanCreator interface.
/// </summary>
public interface IScanCreator
{
	/// <summary>
	/// create segmented scan.
	/// </summary>
	/// <param name="index">
	/// The index into the supplied table of scans (on initialize)
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" />.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">when index is outside of the scan numbers array
	/// </exception>
	SegmentedScan CreateSegmentedScan(int index);

	/// <summary>
	/// Create a centroid stream.
	/// </summary>
	/// <param name="index">
	/// The index into the supplied table of scans (on construction)
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.CentroidStream" />.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">when index is outside of the scan numbers array
	/// </exception>
	CentroidStream CreateCentroidStream(int index);

	/// <summary>
	/// Initialize the scan creator.
	/// This provides the set of scans which will be requested.
	/// This must only be called once.
	/// </summary>
	/// <param name="scanNumbers">
	/// The scan numbers.
	/// </param>
	/// <param name="cacheLimit">
	/// The cache limit.
	/// </param>
	void Initialize(List<int> scanNumbers, int cacheLimit);
}
