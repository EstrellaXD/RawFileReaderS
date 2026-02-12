namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The device acquire status.
/// </summary>
internal class DeviceAcquireStatus
{
	/// <summary>
	/// The _device status.
	/// </summary>
	private VirtualDeviceAcquireStatus _deviceStatus;

	/// <summary>
	/// Sets the device status.
	/// </summary>
	public VirtualDeviceAcquireStatus DeviceStatus
	{
		set
		{
			_deviceStatus = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether is device setup.
	/// </summary>
	public bool IsDeviceSetup => _deviceStatus == VirtualDeviceAcquireStatus.DeviceStatusSetup;

	/// <summary>
	/// Gets a value indicating whether is device ready.
	/// </summary>
	public bool IsDeviceReady => _deviceStatus == VirtualDeviceAcquireStatus.DeviceStatusReady;

	/// <summary>
	/// Gets a value indicating whether can device be setup.
	/// </summary>
	public bool CanDeviceBeSetup
	{
		get
		{
			if (_deviceStatus != VirtualDeviceAcquireStatus.DeviceStatusSetup)
			{
				return _deviceStatus == VirtualDeviceAcquireStatus.DeviceStatusReady;
			}
			return true;
		}
	}

	/// <summary>
	/// Gets a value indicating whether can device acquire data.
	/// </summary>
	public bool CanDeviceAcquireData
	{
		get
		{
			if (_deviceStatus != VirtualDeviceAcquireStatus.DeviceStatusReady)
			{
				return _deviceStatus == VirtualDeviceAcquireStatus.DeviceStatusAcquiring;
			}
			return true;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.DeviceAcquireStatus" /> class.
	/// </summary>
	public DeviceAcquireStatus()
	{
		_deviceStatus = VirtualDeviceAcquireStatus.DeviceStatusNew;
	}
}
