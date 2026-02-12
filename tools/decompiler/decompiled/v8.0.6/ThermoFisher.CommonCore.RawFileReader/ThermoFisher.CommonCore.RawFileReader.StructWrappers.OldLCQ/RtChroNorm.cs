using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The real time chromatogram normalization options for legacy LCQ files.
/// </summary>
internal sealed class RtChroNorm : IRawObjectBase
{
	private RtChroNormStruct _realTimeChroNormStructInfo;

	/// <summary>
	/// Gets or sets a value indicating whether the y axis scale is fixed.
	/// </summary>
	public bool FixScale
	{
		get
		{
			return _realTimeChroNormStructInfo.FixScale;
		}
		set
		{
			_realTimeChroNormStructInfo.FixScale = value;
		}
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
		int count = Marshal.SizeOf(typeof(RtChroNormStruct));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		_realTimeChroNormStructInfo = new RtChroNormStruct
		{
			IntensityMin = BitConverter.ToDouble(value, 0),
			IntensityMax = BitConverter.ToDouble(value, 8),
			NormType = (OldLcqEnums.NormalizeType)BitConverter.ToInt32(value, 16),
			FixScale = (BitConverter.ToInt32(value, 20) > 0)
		};
		return startPos - dataOffset;
	}
}
