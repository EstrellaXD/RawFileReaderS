using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The wrapper class for the virtual controller information.
/// </summary>
internal class VirtualControllerInfo
{
	/// <summary>
	/// Gets the offset.
	/// </summary>
	public long Offset { get; private set; }

	/// <summary>
	/// Gets the virtual device index.
	/// </summary>
	public int VirtualDeviceIndex { get; private set; }

	/// <summary>
	/// Gets the virtual device type.
	/// </summary>
	public VirtualDeviceTypes VirtualDeviceType { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualControllerInfo" /> class by
	/// copying from the structure.
	/// </summary>
	/// <param name="copyFrom">
	/// The structure to copy from.
	/// </param>
	public VirtualControllerInfo(VirtualControllerInfoStruct copyFrom)
	{
		VirtualDeviceType = copyFrom.VirtualDeviceType;
		VirtualDeviceIndex = copyFrom.VirtualDeviceIndex;
		Offset = copyFrom.Offset;
	}
}
