using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Additional data about a scan.
/// This data may include Transients (raw detector data) and other instrument specific data blocks.
/// </summary>
public interface IExtendedScanData
{
	/// <summary>
	/// Gets the header for the extended data. The format of this is instrument specific.
	/// </summary>
	long Header { get; }

	/// <summary>
	/// Gets the transient data for this scan, in an instrument specific (unknown) format. This may be very large.
	/// Fo performance reasons: Applications should not attempt to access this, unless needed by an algorithm.
	/// Note that this is not commonly included in raw files.
	/// </summary>
	ReadOnlyCollection<ITransientSegment> Transients { get; }

	/// <summary>
	/// Gets additional data blocks for a scan.
	/// </summary>
	ReadOnlyCollection<IDataSegment> DataSegments { get; }
}
