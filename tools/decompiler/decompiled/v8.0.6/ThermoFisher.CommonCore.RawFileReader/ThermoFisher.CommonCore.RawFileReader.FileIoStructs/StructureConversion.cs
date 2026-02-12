using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
///     The class contains structure conversion utilities.
/// </summary>
internal static class StructureConversion
{
	/// <summary>
	/// The method copies the virtual controller information from 32 bit values.
	/// </summary>
	/// <param name="info">
	/// The raw file information structure.
	/// </param>
	/// <returns>
	/// The converted <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.RawFileInfoStruct" />.
	/// </returns>
	internal static RawFileInfoStruct ConvertFrom32Bit(RawFileInfoStruct info)
	{
		for (int i = 0; i < info.NumberOfVirtualControllers; i++)
		{
			info.VirtualControllerInfoStruct[i].Offset = info.VirtualControllerInfoVer3[i].Offset;
			info.VirtualControllerInfoStruct[i].VirtualDeviceIndex = info.VirtualControllerInfoVer3[i].VirtualDeviceIndex;
			info.VirtualControllerInfoStruct[i].VirtualDeviceType = info.VirtualControllerInfoVer3[i].VirtualDeviceType;
		}
		return info;
	}

	/// <summary>
	/// The method copies the run header information from the 32 bit values.
	/// </summary>
	/// <param name="runheader">
	/// The run header.
	/// </param>
	/// <returns>
	/// The converted <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders.RunHeaderStruct" />.
	/// </returns>
	internal static RunHeaderStruct ConvertFrom32Bit(RunHeaderStruct runheader)
	{
		runheader.SpectPos = runheader.SpectPos32Bit;
		runheader.PacketPos = runheader.PacketPos32Bit;
		runheader.StatusLogPos = runheader.StatusLogPos32Bit;
		runheader.ErrorLogPos = runheader.ErrorLogPos32Bit;
		runheader.RunHeaderPos = runheader.RunHeaderPos32Bit;
		runheader.TrailerScanEventsPos = runheader.TrailerScanEventsPos32Bit;
		runheader.TrailerExtraPos = runheader.TrailerExtraPos32Bit;
		runheader.ControllerInfo = new VirtualControllerInfoStruct(runheader.ControllerInfoVer4);
		return runheader;
	}
}
