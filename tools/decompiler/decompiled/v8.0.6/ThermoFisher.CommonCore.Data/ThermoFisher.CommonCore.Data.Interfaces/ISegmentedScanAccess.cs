using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface for Access to the data in a segmented scan
/// </summary>
public interface ISegmentedScanAccess
{
	/// <summary>
	/// Gets the number of segments
	/// </summary>
	int SegmentCount { get; }

	/// <summary>
	/// Gets the number of data points in each segment
	/// </summary>
	ReadOnlyCollection<int> SegmentLengths { get; }

	/// <summary>
	/// Gets Intensities for each peak
	/// </summary>
	double[] Intensities { get; }

	/// <summary>
	/// Gets Masses or wavelengths for each peak
	/// </summary>
	double[] Positions { get; }

	/// <summary>
	/// Gets Flagging information (such as saturated) for each peak
	/// </summary>
	PeakOptions[] Flags { get; }

	/// <summary>
	/// Gets the Mass ranges for each scan segment
	/// </summary>
	ReadOnlyCollection<IRangeAccess> MassRanges { get; }
}
