using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
/// The virtual controller information structure.
/// </summary>
internal struct VirtualControllerInfoStruct
{
	internal VirtualDeviceTypes VirtualDeviceType;

	internal int VirtualDeviceIndex;

	internal long Offset;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.VirtualControllerInfoStruct" /> struct by
	/// copying from the old controller (i.e. 32 bit) structure.
	/// </summary>
	/// <param name="old">
	/// The old controller structure.
	/// </param>
	internal VirtualControllerInfoStruct(OldVirtualControllerInfo old)
	{
		VirtualDeviceType = old.VirtualDeviceType;
		VirtualDeviceIndex = old.VirtualDeviceIndex;
		Offset = old.Offset;
	}
}
