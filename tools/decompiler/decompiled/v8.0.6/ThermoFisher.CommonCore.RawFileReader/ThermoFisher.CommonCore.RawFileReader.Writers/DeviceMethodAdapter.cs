using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// It's used internally to create a device method builder object for instrument method.
/// </summary>
internal class DeviceMethodAdapter
{
	/// <summary>
	/// Creates the device method object.
	/// </summary>
	/// <returns>IDeviceMethod object.</returns>
	public static IDeviceMethod CreateDeviceMethod()
	{
		return new DeviceMethod();
	}
}
