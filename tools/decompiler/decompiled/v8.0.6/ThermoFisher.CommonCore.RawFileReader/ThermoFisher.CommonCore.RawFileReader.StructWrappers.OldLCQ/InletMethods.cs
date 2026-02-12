using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The inlet methods, for legacy LCQ files
/// </summary>
internal sealed class InletMethods : IRawObjectBase
{
	/// <summary>
	/// Gets the inlet methods info.
	/// </summary>
	public InstMethodInletStruct InletMethodsInfo { get; private set; }

	/// <summary>
	/// Gets the GC method info.
	/// </summary>
	public GcMethod GcMethodInfo { get; private set; }

	/// <summary>
	/// Gets the LC method info.
	/// </summary>
	public LcMethod LcMethodInfo { get; private set; }

	/// <summary>
	/// Gets the auto sampler info.
	/// </summary>
	public AutoSampler AutoSamplerInfo { get; private set; }

	/// <summary>
	/// Gets the probe info info.
	/// </summary>
	public ProbeInfo ProbeInfoInfo { get; private set; }

	/// <summary>
	/// Gets the syringe info.
	/// </summary>
	public Syringe SyringeInfo { get; private set; }

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
		GcMethodInfo = viewer.LoadRawFileObjectExt(() => new GcMethod(), fileRevision, ref startPos);
		LcMethodInfo = viewer.LoadRawFileObjectExt(() => new LcMethod(), fileRevision, ref startPos);
		AutoSamplerInfo = viewer.LoadRawFileObjectExt(() => new AutoSampler(), fileRevision, ref startPos);
		ProbeInfoInfo = viewer.LoadRawFileObjectExt(() => new ProbeInfo(), fileRevision, ref startPos);
		int count = Marshal.SizeOf(typeof(InstMethodInletStruct));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		InletMethodsInfo = new InstMethodInletStruct
		{
			Tsp = new PairValuesStruct
			{
				Pair1 = BitConverter.ToInt32(value, 0),
				Pair2 = BitConverter.ToInt32(value, 4)
			},
			Esi = new PairValuesStruct
			{
				Pair1 = BitConverter.ToInt32(value, 8),
				Pair2 = BitConverter.ToInt32(value, 12)
			},
			Fab = new PairValuesStruct
			{
				Pair1 = BitConverter.ToInt32(value, 16),
				Pair2 = BitConverter.ToInt32(value, 20)
			}
		};
		SyringeInfo = viewer.LoadRawFileObjectExt(() => new Syringe(), fileRevision, ref startPos);
		return startPos - dataOffset;
	}
}
