using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// Represents a "sub range" reader which has (at least) the data for the indicated record range
/// </summary>
internal struct RecordRangeReader
{
	public IMemoryReader Reader { get; set; }

	public int StartRecord { get; set; }

	public int EndRecord { get; set; }
}
