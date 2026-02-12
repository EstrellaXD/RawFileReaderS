using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The MS segment.
/// </summary>
internal sealed class MsSegment : IRawObjectBase
{
	private MsSegmentStruct _msSegmentStructInfo;

	/// <summary>
	/// Gets the minimum segment time.
	/// minimum segment time (to correct old methods)
	/// On 3/14/96, the min MS Segment time was increased to 0.1 -- compensate while loading
	/// </summary>
	/// <value>
	/// The minimum segment time.
	/// </value>
	private double MinSegmentTime { get; }

	/// <summary>
	/// Gets the MS scan events.
	/// </summary>
	public MsScanEvent[] MsScanEvents { get; private set; }

	/// <summary>
	/// Gets the acquisition time.
	/// </summary>
	public double AcquisitionTime => _msSegmentStructInfo.AcquisitionTime;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.MsSegment" /> class.
	/// </summary>
	public MsSegment()
	{
		MinSegmentTime = 0.1;
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
		int count = Marshal.SizeOf(typeof(MsSegmentStruct));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		_msSegmentStructInfo = new MsSegmentStruct
		{
			AcquisitionTime = BitConverter.ToDouble(value, 0),
			AgcFlag = BitConverter.ToInt32(value, 8)
		};
		MsScanEvents = viewer.LoadRawFileObjectArray<MsScanEvent>(fileRevision, ref startPos);
		viewer.ReadStringExt(ref startPos);
		if (_msSegmentStructInfo.AcquisitionTime < MinSegmentTime)
		{
			_msSegmentStructInfo.AcquisitionTime = MinSegmentTime;
		}
		return startPos - dataOffset;
	}
}
