using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The real time spec information, from legacy LCQ files.
/// </summary>
internal sealed class RealTimeSpecInformation : IRawObjectBase
{
	/// <summary>
	/// Gets the real time spec info struct info.
	/// </summary>
	public RealTimeSpecInfoStruct RealTimeSpecInfoStructInfo { get; private set; }

	/// <summary>
	/// Gets the x axis parameters.
	/// </summary>
	/// <value>
	/// The x axis parameters.
	/// </value>
	public AxisParm XAxisParm { get; private set; }

	/// <summary>
	/// Gets the y axis parameters.
	/// </summary>
	/// <value>
	/// The y axis parameters.
	/// </value>
	public AxisParm YAxisParm { get; private set; }

	/// <summary>
	/// Gets the z axis parameters.
	/// label offset flag and amount are ignored (they don't apply)
	/// </summary>
	/// <value>
	/// The z axis parameters.
	/// </value>
	public AxisParm ZAxisParm { get; private set; }

	/// <summary>
	/// Gets the filter info.
	/// </summary>
	public Filter FilterInfo { get; private set; }

	/// <summary>
	/// Gets the real time spectrum normalization info.
	/// </summary>
	public RtSpecNorm RtSpecNormInfo { get; private set; }

	/// <summary>
	/// Gets the real time spectrum color info.
	/// </summary>
	public RtSpecColor RtSpecColorInfo { get; private set; }

	/// <summary>
	/// Gets the real time spectrum label info.
	/// </summary>
	public RtSpecLabel RtSpecLabelInfo { get; private set; }

	/// <summary>
	/// Gets the real time spectrum other info.
	/// </summary>
	public RtSpecOther RtSpecOtherInfo { get; private set; }

	/// <summary>
	/// Gets the real time spectrum ranges info.
	/// </summary>
	public RtSpecRanges RtSpecRangesInfo { get; private set; }

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
		int count = Marshal.SizeOf(typeof(RealTimeSpecInfoStruct));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		RealTimeSpecInfoStructInfo = new RealTimeSpecInfoStruct
		{
			SpectrumStyle = (OldLcqEnums.SpectrumStyle)BitConverter.ToInt32(value, 0),
			Split = (BitConverter.ToInt32(value, 4) > 0),
			Splits = BitConverter.ToInt32(value, 8)
		};
		XAxisParm = viewer.LoadRawFileObjectExt(() => new AxisParm(), fileRevision, ref startPos);
		YAxisParm = viewer.LoadRawFileObjectExt(() => new AxisParm(), fileRevision, ref startPos);
		ZAxisParm = viewer.LoadRawFileObjectExt(() => new AxisParm(), fileRevision, ref startPos);
		FilterInfo = viewer.LoadRawFileObjectExt(() => new Filter(), fileRevision, ref startPos);
		RtSpecNormInfo = viewer.LoadRawFileObjectExt(() => new RtSpecNorm(), fileRevision, ref startPos);
		RtSpecColorInfo = viewer.LoadRawFileObjectExt(() => new RtSpecColor(), fileRevision, ref startPos);
		RtSpecLabelInfo = viewer.LoadRawFileObjectExt(() => new RtSpecLabel(), fileRevision, ref startPos);
		RtSpecOtherInfo = viewer.LoadRawFileObjectExt(() => new RtSpecOther(), fileRevision, ref startPos);
		RtSpecRangesInfo = viewer.LoadRawFileObjectExt(() => new RtSpecRanges(), fileRevision, ref startPos);
		return startPos - dataOffset;
	}
}
