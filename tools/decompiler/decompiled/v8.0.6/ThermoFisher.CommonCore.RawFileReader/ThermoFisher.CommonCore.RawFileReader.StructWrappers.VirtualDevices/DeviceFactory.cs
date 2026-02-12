using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// Provide method to create device objects
/// </summary>
internal static class DeviceFactory
{
	/// <summary>
	/// Gets the device.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">raw file loader ID</param>
	/// <param name="deviceInfo">The device information.</param>
	/// <param name="rawFileName">The viewer.</param>
	/// <param name="fileVersion">The file version.</param>
	/// <param name="isInAcquisition">if set to <c>true</c> [is in acquisition].</param>
	/// <returns>Device object</returns>
	internal static IDevice GetDevice(IViewCollectionManager manager, Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileVersion, bool isInAcquisition)
	{
		bool oldRev = fileVersion < 25;
		return deviceInfo.VirtualDeviceType switch
		{
			VirtualDeviceTypes.MsDevice => new MassSpecDevice(manager, loaderId, deviceInfo, rawFileName, fileVersion, isInAcquisition, oldRev), 
			VirtualDeviceTypes.MsAnalogDevice => new UvDevice(manager, loaderId, deviceInfo, rawFileName, fileVersion, isInAcquisition, oldRev), 
			VirtualDeviceTypes.AnalogDevice => new UvDevice(manager, loaderId, deviceInfo, rawFileName, fileVersion, isInAcquisition, oldRev), 
			VirtualDeviceTypes.PdaDevice => new UvDevice(manager, loaderId, deviceInfo, rawFileName, fileVersion, isInAcquisition, oldRev), 
			VirtualDeviceTypes.UvDevice => new UvDevice(manager, loaderId, deviceInfo, rawFileName, fileVersion, isInAcquisition, oldRev), 
			VirtualDeviceTypes.StatusDevice => new StatusDevice(manager, loaderId, deviceInfo, rawFileName, fileVersion, isInAcquisition, oldRev), 
			_ => null, 
		};
	}
}
