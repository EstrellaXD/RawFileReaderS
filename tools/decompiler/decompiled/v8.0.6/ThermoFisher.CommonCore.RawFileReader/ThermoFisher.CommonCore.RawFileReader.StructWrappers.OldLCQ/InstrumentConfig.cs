using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// Legacy instrument configuration data (LCQ data system)
/// </summary>
internal sealed class InstrumentConfig : IRawObjectBase
{
	private InstrumentConfigStruct _instConfig;

	/// <summary>
	/// Gets the inlet type -- either "ESI" or "APCI"
	/// </summary>
	public string Inlet { get; private set; }

	/// <summary>
	/// Gets the MS Model
	/// </summary>
	public string MsModelName { get; private set; }

	/// <summary>
	/// Gets the UV detector model
	/// </summary>
	public string UvDetectorModel { get; private set; }

	/// <summary>
	/// Gets the DAD (Diode array detector) model.
	/// </summary>
	public string Dad { get; private set; }

	/// <summary>
	/// Gets the LC pump model.
	/// </summary>
	public string LcPumpModel { get; private set; }

	/// <summary>
	/// Gets the auto sampler model.
	/// </summary>
	public string Autosampler { get; private set; }

	/// <summary>
	/// Gets the AD converter model.
	/// </summary>
	public string Ad { get; private set; }

	/// <summary>
	/// Gets the MS model number
	/// </summary>
	public string MsModel { get; private set; }

	/// <summary>
	/// Gets the MS serial number.
	/// </summary>
	public string MsSerialNum { get; private set; }

	/// <summary>
	/// Gets the LC pump model number.
	/// </summary>
	public string LcModel { get; private set; }

	/// <summary>
	/// Gets the LC pump serial number.
	/// </summary>
	public string LcSerialNum { get; private set; }

	/// <summary>
	/// Gets the Detector model number (unused now)
	/// </summary>
	public string DetModel { get; private set; }

	/// <summary>
	/// Gets the Detector serial number (unused now)
	/// </summary>
	public string DetSerialNum { get; private set; }

	/// <summary>
	/// Gets the Auto sampler model number
	/// </summary>
	public string AsModel { get; private set; }

	/// <summary>
	/// Gets the Auto sampler serial number
	/// </summary>
	public string AsSerialNum { get; private set; }

	/// <summary>
	/// Gets the External detector channels 1-4
	/// </summary>
	public string[] ExtDet { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.InstrumentConfig" /> class.
	/// </summary>
	public InstrumentConfig()
	{
		_instConfig = new InstrumentConfigStruct
		{
			ChannelInUse = new bool[4]
		};
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
		if (fileRevision < 8)
		{
			long num = Marshal.SizeOf(typeof(InstrumentConfigStruct1));
			byte[] value = viewer.ReadBytesExt(ref startPos, (int)num);
			_instConfig.ChannelInUse[0] = BitConverter.ToInt32(value, 0) > 0;
			_instConfig.ChannelInUse[1] = BitConverter.ToInt32(value, 4) > 0;
			_instConfig.ChannelInUse[2] = BitConverter.ToInt32(value, 8) > 0;
			_instConfig.ChannelInUse[3] = BitConverter.ToInt32(value, 12) > 0;
		}
		else
		{
			long num = Marshal.SizeOf(typeof(InstrumentConfigStruct));
			byte[] value2 = viewer.ReadBytesExt(ref startPos, (int)num);
			_instConfig.ChannelInUse[0] = BitConverter.ToInt32(value2, 0) > 0;
			_instConfig.ChannelInUse[1] = BitConverter.ToInt32(value2, 4) > 0;
			_instConfig.ChannelInUse[2] = BitConverter.ToInt32(value2, 8) > 0;
			_instConfig.ChannelInUse[3] = BitConverter.ToInt32(value2, 12) > 0;
			_instConfig.LCControl = (OldLcqEnums.InstrumentControl)BitConverter.ToInt32(value2, 16);
			_instConfig.ASControl = (OldLcqEnums.InstrumentControl)BitConverter.ToInt32(value2, 20);
			_instConfig.DetControl = (OldLcqEnums.InstrumentControl)BitConverter.ToInt32(value2, 24);
		}
		Inlet = viewer.ReadStringExt(ref startPos);
		MsModelName = viewer.ReadStringExt(ref startPos);
		UvDetectorModel = viewer.ReadStringExt(ref startPos);
		Dad = viewer.ReadStringExt(ref startPos);
		LcPumpModel = viewer.ReadStringExt(ref startPos);
		Autosampler = viewer.ReadStringExt(ref startPos);
		Ad = viewer.ReadStringExt(ref startPos);
		MsModel = viewer.ReadStringExt(ref startPos);
		MsSerialNum = viewer.ReadStringExt(ref startPos);
		LcModel = viewer.ReadStringExt(ref startPos);
		LcSerialNum = viewer.ReadStringExt(ref startPos);
		DetModel = viewer.ReadStringExt(ref startPos);
		DetSerialNum = viewer.ReadStringExt(ref startPos);
		AsModel = viewer.ReadStringExt(ref startPos);
		AsSerialNum = viewer.ReadStringExt(ref startPos);
		ExtDet = new string[4];
		ExtDet[0] = viewer.ReadStringExt(ref startPos);
		ExtDet[1] = viewer.ReadStringExt(ref startPos);
		ExtDet[2] = viewer.ReadStringExt(ref startPos);
		ExtDet[3] = viewer.ReadStringExt(ref startPos);
		if (fileRevision < 8)
		{
			_instConfig.DetControl = OldLcqEnums.InstrumentControl.ContactClosure;
			_instConfig.LCControl = ((!(LcPumpModel == "HP 1050 Pump and Oven Module")) ? OldLcqEnums.InstrumentControl.ContactClosure : OldLcqEnums.InstrumentControl.Direct);
			_instConfig.ASControl = ((!(Autosampler == "HP 1050 Autosampler Module")) ? OldLcqEnums.InstrumentControl.ContactClosure : OldLcqEnums.InstrumentControl.Direct);
		}
		return startPos - dataOffset;
	}

	/// <summary>
	/// Test if the ext detector is in use.
	/// </summary>
	/// <param name="n">
	/// The index.
	/// </param>
	/// <returns>
	/// true if in use
	/// </returns>
	public bool ExtDetInUse(int n)
	{
		return _instConfig.ChannelInUse[n];
	}
}
