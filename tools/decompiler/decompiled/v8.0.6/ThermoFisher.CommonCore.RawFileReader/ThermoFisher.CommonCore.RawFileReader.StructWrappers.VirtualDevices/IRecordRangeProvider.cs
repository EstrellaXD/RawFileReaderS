using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// Defines an interface only used to inject functionality to this class.
/// Calling code must be able to create a reader which can provide any record within a given range
/// </summary>
internal interface IRecordRangeProvider
{
	IMemoryReader CreateSubRangeReader(int firstRecord, int lastRecord);
}
