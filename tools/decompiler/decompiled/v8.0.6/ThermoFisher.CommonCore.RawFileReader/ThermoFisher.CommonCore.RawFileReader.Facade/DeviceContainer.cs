using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// The device container allow a mimmally initalized version of the device to exists
/// which can support version checks or other needs.
/// All "large data" is perfomed on first access of the FullDevice.
/// "PartialDevice" can be used in a dispose call, as unititze items from
/// "FullDevice" will not need to be disposed.
/// </summary>
internal class DeviceContainer
{
	/// <summary>
	/// Gets or sets an instance of the device that has been minimally initialized
	/// </summary>
	public IDevice PartialDevice { get; set; }

	/// <summary>
	/// Gets or sets an instance of the device, which contains all data.
	/// </summary>
	public Lazy<IDevice> FullDevice { get; set; }
}
