namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Enumeration of possible Virtual Device Acquire Status
/// </summary>
internal enum VirtualDeviceAcquireStatus
{
	/// <summary>Device Status New</summary>
	DeviceStatusNew,
	/// <summary>Device Status Setup</summary>
	DeviceStatusSetup,
	/// <summary>Device Status Ready</summary>
	DeviceStatusReady,
	/// <summary>Device Status Acquiring</summary>
	DeviceStatusAcquiring
}
