using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The instrument configuration struct, from legacy LCQ files.
/// </summary>
internal struct InstrumentConfigStruct
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	internal bool[] ChannelInUse;

	internal OldLcqEnums.InstrumentControl LCControl;

	internal OldLcqEnums.InstrumentControl ASControl;

	internal OldLcqEnums.InstrumentControl DetControl;
}
