using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The (calibration or QC) level.
/// </summary>
internal class Level : IRawObjectBase, ILevelWithSimpleReplicates, IQualityControlLevelAccess, ICalibrationLevelAccess
{
	/// <summary>
	/// The level info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct LevelInfo
	{
		public double BaseAmount;

		public double TestPercent;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(LevelInfo))
	} };

	private LevelInfo _info;

	/// <summary>
	/// Gets replicate data, as saved in a PMD file
	/// </summary>
	public ReadOnlyCollection<IReplicateDataAccess> ReplicateCollection { get; private set; }

	/// <summary>
	/// Gets the name for this calibration level
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the amount of calibration compound (usually a concentration) for this level
	/// </summary>
	public double BaseAmount => _info.BaseAmount;

	/// <summary>
	/// Gets the QC test <c>standard: 100 * (yobserved-ypredicted)/ypreditced</c>
	/// </summary>
	public double TestPercent => _info.TestPercent;

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
		_info = Utilities.ReadStructure<LevelInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		Name = viewer.ReadStringExt(ref startPos);
		int num = viewer.ReadIntExt(ref startPos);
		List<IReplicateDataAccess> list = new List<IReplicateDataAccess>(num);
		for (int i = 0; i < num; i++)
		{
			Replicate item = viewer.LoadRawFileObjectExt(() => new Replicate(), fileRevision, ref startPos);
			list.Add(item);
		}
		ReplicateCollection = new ReadOnlyCollection<IReplicateDataAccess>(list);
		return startPos - dataOffset;
	}
}
