using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The ICIS status log, for legacy LCQ files.
/// </summary>
internal sealed class IcisStatusLog : IIcisStatusLog, IRawObjectBase
{
	/// <summary>
	/// Gets the ICIS status log.
	/// </summary>
	/// <value>
	/// The ICIS status log.
	/// </value>
	public string Status { get; private set; }

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		Status = viewer.ReadString(dataOffset, out var numOfBytesRead);
		return numOfBytesRead;
	}
}
