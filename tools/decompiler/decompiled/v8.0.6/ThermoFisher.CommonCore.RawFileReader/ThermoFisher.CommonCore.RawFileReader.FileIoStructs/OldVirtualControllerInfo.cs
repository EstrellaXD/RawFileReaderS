using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
/// The old virtual controller information structure.
/// Pre-version-64 structure, used a 32 bit file pointer (offset).
/// Causes problems if raw file is larger than 2GB.
/// </summary>
internal struct OldVirtualControllerInfo
{
	internal VirtualDeviceTypes VirtualDeviceType;

	internal int VirtualDeviceIndex;

	internal int Offset;
}
