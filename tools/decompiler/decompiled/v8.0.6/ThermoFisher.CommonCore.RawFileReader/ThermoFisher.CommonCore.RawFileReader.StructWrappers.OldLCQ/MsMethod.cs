using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The MS method, from legacy LCQ files
/// </summary>
internal sealed class MsMethod : IRawObjectBase
{
	/// <summary>
	/// Gets the MS method info.
	/// </summary>
	public MsMethodInfoStruct MsMethodInfo { get; private set; }

	/// <summary>
	/// Gets the dump value info.
	/// </summary>
	public LcDumpValueStruct DumpValueInfo { get; private set; }

	/// <summary>
	/// Gets the fraction collector info.
	/// </summary>
	public FractionCollectorStruct FractionCollectorInfo { get; private set; }

	/// <summary>
	/// Gets the ITCL info.
	/// </summary>
	public Itcl ItclInfo { get; private set; }

	/// <summary>
	/// Gets the non ITCL info.
	/// </summary>
	public NonItcl NonItclInfo { get; private set; }

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
		MsMethodInfo = new MsMethodInfoStruct
		{
			Type = (OldLcqEnums.MsType)viewer.ReadIntExt(ref startPos)
		};
		int count = Marshal.SizeOf(typeof(LcDumpValueStruct));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		DumpValueInfo = new LcDumpValueStruct
		{
			BetweenRuns = (OldLcqEnums.DivertBetweenRuns)BitConverter.ToInt32(value, 0),
			UseDumpValve = (BitConverter.ToInt32(value, 4) > 0),
			StartFlowIntoMS = BitConverter.ToDouble(value, 8),
			StopFlowIntoMS = BitConverter.ToDouble(value, 16)
		};
		if (fileRevision >= 11)
		{
			count = Marshal.SizeOf(typeof(FractionCollectorStruct));
			value = viewer.ReadBytesExt(ref startPos, count);
			FractionCollectorInfo = new FractionCollectorStruct
			{
				FcChoice = (OldLcqEnums.FractionCollectorChoice)BitConverter.ToInt32(value, 0),
				TriggerFractionCollector = BitConverter.ToInt32(value, 4),
				IntensityThreshold = BitConverter.ToDouble(value, 8)
			};
		}
		else
		{
			FractionCollectorInfo = default(FractionCollectorStruct);
		}
		if (MsMethodInfo.Type == OldLcqEnums.MsType.IsItcl)
		{
			ItclInfo = viewer.LoadRawFileObjectExt(() => new Itcl(), fileRevision, ref startPos);
		}
		else
		{
			NonItclInfo = viewer.LoadRawFileObjectExt(() => new NonItcl(), fileRevision, ref startPos);
		}
		return startPos - dataOffset;
	}
}
