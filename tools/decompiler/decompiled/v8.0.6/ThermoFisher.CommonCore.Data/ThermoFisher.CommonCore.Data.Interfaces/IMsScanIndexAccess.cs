namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
///  Read only access to scan statistics for MS detector
/// </summary>
public interface IMsScanIndexAccess
{
	/// <summary>
	/// Gets the intensity of highest peak in scan
	/// </summary>
	double BasePeakIntensity { get; }

	/// <summary>
	/// Gets the mass of largest peak in scan
	/// </summary>
	double BasePeakMass { get; }

	/// <summary>
	/// Gets the highest mass in scan
	/// </summary>
	double HighMass { get; }

	/// <summary>
	/// Gets the lowest mass in scan
	/// </summary>
	double LowMass { get; }

	/// <summary>
	/// Gets the Number of points in scan
	/// </summary>
	int PacketCount { get; }

	/// <summary>
	///  Gets the indication of data format used by this scan
	/// </summary>
	int PacketType { get; }

	/// <summary>
	/// Gets the event (scan type) number within segment
	/// </summary>
	int ScanEventNumber { get; }

	/// <summary>
	/// Gets the number of the scan
	/// </summary>
	int ScanNumber { get; }

	/// <summary>
	/// Gets the time segment number for this event
	/// </summary>
	int SegmentNumber { get; }

	/// <summary>
	/// Gets the time at start of scan (minutes)
	/// </summary>
	double StartTime { get; }

	/// <summary>
	///  Gets the total Ion Current for scan
	/// </summary>
	double TIC { get; }

	/// <summary>
	///     Gets the cycle number.
	///     <remarks>
	///         Cycle number used to associate events within a scan event cycle.
	///         For example, on the first cycle of scan events, all the events
	///         would set this to '1'. On the second cycle, all the events would
	///         set this to '2'. This field must be set by devices if supporting
	///         compound names for filtering. However, it may be set in all
	///         acquisitions to help processing algorithms.
	///     </remarks>
	/// </summary>
	int CycleNumber { get; }

	/// <summary>
	/// Gets a value indicating whether this scan contains centroid data (else profile0
	/// </summary>
	bool IsCentroidScan { get; }
}
