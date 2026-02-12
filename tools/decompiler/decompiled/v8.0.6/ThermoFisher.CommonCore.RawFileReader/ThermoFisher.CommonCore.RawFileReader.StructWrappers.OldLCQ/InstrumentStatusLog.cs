using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The instrument status log, from legacy LCQ files.
/// </summary>
internal class InstrumentStatusLog : IRawObjectBase
{
	private readonly int _numItems;

	/// <summary>
	/// Gets the instrument status struct info.
	/// </summary>
	public InstStatusStruct[] InstStatusStructInfo { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.InstrumentStatusLog" /> class.
	/// </summary>
	/// <param name="numItems">
	/// The number of items.
	/// </param>
	public InstrumentStatusLog(int numItems)
	{
		_numItems = numItems;
		InstStatusStructInfo = new InstStatusStruct[numItems];
	}

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
		long startPos = dataOffset;
		if (_numItems <= 0)
		{
			InstStatusStructInfo = Array.Empty<InstStatusStruct>();
			return 0L;
		}
		for (int i = 0; i < _numItems; i++)
		{
			InstStatusStructInfo[i] = viewer.ReadStructureExt<InstStatusStruct>(ref startPos);
		}
		return startPos - dataOffset;
	}
}
