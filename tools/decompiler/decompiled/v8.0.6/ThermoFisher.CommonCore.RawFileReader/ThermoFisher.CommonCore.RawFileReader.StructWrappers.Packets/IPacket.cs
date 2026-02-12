using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// An intenal interface for "packets" which define various formats of scan data.
/// At a minimum: All kinds of scan must be able to return "SegmentedData"
/// </summary>
internal interface IPacket
{
	/// <summary>
	/// Gets the segmented peaks. This is data for each "mass range" within a scan
	/// </summary>
	List<SegmentData> SegmentPeaks { get; }

	/// <summary>
	/// Gets the scan index.
	/// This is optional and intenally null for MS types.
	/// UV/PDA data creates a new index here with modified values to support an "average absobance" result.
	/// </summary>
	IScanIndex Index => null;
}
