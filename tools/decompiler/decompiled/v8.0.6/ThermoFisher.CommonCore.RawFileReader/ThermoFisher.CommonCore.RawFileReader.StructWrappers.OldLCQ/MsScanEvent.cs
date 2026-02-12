using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The MS scan event, from legacy LCQ files
/// </summary>
internal sealed class MsScanEvent : IRawObjectBase
{
	/// <summary>
	/// Gets the MS scan event struct info.
	/// </summary>
	public MsScanEventStruct MsScanEventStructInfo { get; private set; }

	/// <summary>
	/// Gets the MS dependent data info.
	/// </summary>
	public MsDependentData MsDependentDataInfo { get; private set; }

	/// <summary>
	/// Gets the reactions info.
	/// </summary>
	public Reaction[] ReactionsInfo { get; private set; }

	/// <summary>
	/// Gets the mass ranges info.
	/// </summary>
	public MassRangeStruct[] MassRangesInfo { get; private set; }

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
		int count = Marshal.SizeOf(typeof(MsScanEventStruct));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		MsScanEventStructInfo = new MsScanEventStruct
		{
			Polarity = (ScanFilterEnums.PolarityTypes)BitConverter.ToInt32(value, 0),
			ScanMode = (OldLcqEnums.MsScanMode)BitConverter.ToInt32(value, 4),
			ScanType = (OldLcqEnums.MsScanType)BitConverter.ToInt32(value, 8),
			DepDataFlag = BitConverter.ToInt32(value, 12),
			MicroScans = BitConverter.ToInt32(value, 16),
			StoreDepData = BitConverter.ToInt32(value, 20),
			Duration = BitConverter.ToDouble(value, 24),
			UseSourceCID = (BitConverter.ToInt32(value, 32) > 0),
			TurboScanMode = (BitConverter.ToInt32(value, 36) > 0),
			CIDPercent = BitConverter.ToDouble(value, 40)
		};
		MsDependentDataInfo = viewer.LoadRawFileObjectExt(() => new MsDependentData(), fileRevision, ref startPos);
		ReactionsInfo = viewer.LoadRawFileObjectArray<Reaction>(fileRevision, ref startPos);
		MassRangesInfo = MassRangeStruct.LoadArray(viewer, ref startPos);
		return startPos - dataOffset;
	}
}
