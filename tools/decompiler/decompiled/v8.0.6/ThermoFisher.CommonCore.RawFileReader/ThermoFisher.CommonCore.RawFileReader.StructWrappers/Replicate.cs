using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// A calibration replicate, as read from an Xcalibur PMD file.
/// </summary>
internal class Replicate : IRawObjectBase, IReplicateDataAccess
{
	/// <summary>
	/// The replicate info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct Replicateinfo
	{
		public double Amount;

		public double HeightRatio;

		public double AreaRatio;

		public bool ExcludeFromCalibration;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(Replicateinfo))
	} };

	private Replicateinfo _info;

	/// <summary>
	/// Gets or sets the result file name.
	/// </summary>
	private string ResultFileName { get; set; }

	/// <summary>
	/// Gets the amount of target compound in calibration or QC standard.
	/// </summary>
	double IReplicateDataAccess.Amount => _info.Amount;

	/// <summary>
	/// Gets the Ratio of target peak height to ISTD peak height in result file.
	/// </summary>
	double IReplicateDataAccess.HeightRatio => _info.HeightRatio;

	/// <summary>
	/// Gets the Ratio of target peak area to ISTD peak area in result file.
	/// </summary>
	double IReplicateDataAccess.AreaRatio => _info.AreaRatio;

	/// <summary>
	/// Gets a value indicating whether to exclude this data point from calibration curve.
	/// </summary>
	bool IReplicateDataAccess.ExcludeFromCalibration => _info.ExcludeFromCalibration;

	/// <summary>
	/// Gets the raw file name for the replicate
	/// </summary>
	string IReplicateDataAccess.File => ResultFileName;

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
		_info = Utilities.ReadStructure<Replicateinfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		ResultFileName = viewer.ReadStringExt(ref startPos);
		return startPos - dataOffset;
	}
}
