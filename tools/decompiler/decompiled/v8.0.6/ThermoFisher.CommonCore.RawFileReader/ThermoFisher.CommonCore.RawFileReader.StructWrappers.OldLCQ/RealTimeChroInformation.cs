using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The real time chromatogram information, from legacy LCQ files
/// </summary>
internal sealed class RealTimeChroInformation : IRawObjectBase
{
	/// <summary>
	/// Gets the real time chromatogram info struct info.
	/// </summary>
	public RealTimeChroInfoStruct RealTimeChroInfoStructInfo { get; private set; }

	/// <summary>
	/// Gets the x axis parameters.
	/// </summary>
	public AxisParm XAxisParm { get; private set; }

	/// <summary>
	/// Gets the y axis parameters.
	/// </summary>
	public AxisParm YAxisParm { get; private set; }

	/// <summary>
	/// Gets the real time chromatogram norm info.
	/// </summary>
	public RtChroNorm RtChroNormInfo { get; private set; }

	/// <summary>
	/// Gets the real time chromatogram label info.
	/// </summary>
	public RtChroLabel RtChroLabelInfo { get; private set; }

	/// <summary>
	/// Gets the real time chromatogram other info.
	/// </summary>
	public RtChroOther RtChroOtherInfo { get; private set; }

	/// <summary>
	/// Gets the real time chromatogram style info.
	/// </summary>
	public RtChroStyle RtChroStyleInfo { get; private set; }

	/// <summary>
	/// Gets the chromatogram trace info.
	/// </summary>
	public ChroTrace[] ChroTraceInfo { get; private set; }

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
		int count = Marshal.SizeOf(typeof(RealTimeChroInfoStruct));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		RealTimeChroInfoStructInfo = new RealTimeChroInfoStruct
		{
			SplitTimeRange = (BitConverter.ToInt32(value, 0) > 0),
			Splits = BitConverter.ToInt32(value, 4)
		};
		XAxisParm = viewer.LoadRawFileObjectExt(() => new AxisParm(), fileRevision, ref startPos);
		YAxisParm = viewer.LoadRawFileObjectExt(() => new AxisParm(), fileRevision, ref startPos);
		RtChroNormInfo = viewer.LoadRawFileObjectExt(() => new RtChroNorm(), fileRevision, ref startPos);
		RtChroLabelInfo = viewer.LoadRawFileObjectExt(() => new RtChroLabel(), fileRevision, ref startPos);
		RtChroOtherInfo = viewer.LoadRawFileObjectExt(() => new RtChroOther(), fileRevision, ref startPos);
		RtChroStyleInfo = viewer.LoadRawFileObjectExt(() => new RtChroStyle(), fileRevision, ref startPos);
		ChroTraceInfo = viewer.LoadRawFileObjectArray<ChroTrace>(fileRevision, ref startPos);
		return startPos - dataOffset;
	}
}
