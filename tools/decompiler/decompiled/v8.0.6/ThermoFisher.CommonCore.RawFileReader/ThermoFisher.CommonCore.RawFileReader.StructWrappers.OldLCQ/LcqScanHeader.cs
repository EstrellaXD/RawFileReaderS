using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The LCQ scan header.
/// </summary>
internal sealed class LcqScanHeader : IRawObjectBase
{
	private readonly int _numItems;

	/// <summary>
	/// Gets the trailer struct info.
	/// </summary>
	public TrailerStruct[] TrailerStructInfo { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.LcqScanHeader" /> class.
	/// </summary>
	/// <param name="numItems">The number items.</param>
	public LcqScanHeader(int numItems)
	{
		_numItems = numItems;
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
			TrailerStructInfo = Array.Empty<TrailerStruct>();
			return 0L;
		}
		TrailerStructInfo = new TrailerStruct[_numItems];
		for (int i = 0; i < _numItems; i++)
		{
			TrailerStructInfo[i] = viewer.ReadStructureExt<TrailerStruct>(ref startPos);
		}
		return startPos - dataOffset;
	}
}
