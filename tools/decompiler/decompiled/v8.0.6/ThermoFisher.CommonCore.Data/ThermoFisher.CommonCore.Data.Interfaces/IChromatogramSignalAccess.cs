using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read-only access to a chromatogram
/// </summary>
public interface IChromatogramSignalAccess
{
	/// <summary>
	/// Gets the times.
	/// </summary>
	/// <value>The signal times.</value>
	IList<double> Times { get; }

	/// <summary>
	/// Gets the intensities.
	/// </summary>
	/// <value>The signal intensities.</value>
	IList<double> Intensities { get; }

	/// <summary>
	/// Gets the signal scans.
	/// </summary>
	/// <value>The signal scans.</value>
	IList<int> Scans { get; }

	/// <summary>
	/// Gets the base peak masses.
	/// </summary>
	/// <value>The base peak masses.</value>
	IList<double> BasePeakMasses { get; }

	/// <summary>
	/// Gets the time at the end of the signal
	/// </summary>
	double EndTime { get; }

	/// <summary>
	/// Gets the time at the start of the signal
	/// </summary>
	double StartTime { get; }

	/// <summary>
	/// Gets the number of points in the signal
	/// </summary>
	int Length { get; }

	/// <summary>
	/// Gets a value indicating whether there is any base peak data in this signal
	/// </summary>
	bool HasBasePeakData { get; }

	/// <summary>
	/// Test if this is valid data (arrays are same length)
	/// </summary>
	/// <returns>True if this is valid data</returns>
	bool Valid();
}
