using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides extension methods to write raw file info to the shared memory mapped buffer
/// </summary>
internal static class RawFileInfoExtension
{
	private static readonly int RawFileInfoStructSize = Utilities.StructSizeLookup.Value[1];

	/// <summary>
	/// Adds the controller information.
	/// we are being created by the virtual controller so we need to register ourselves with the global
	/// storage so other instantiations of this file will be able to read this controller information
	/// </summary>
	/// <param name="deviceId">Device Identifier</param>
	/// <param name="deviceType">Type of the device.</param>
	/// <param name="deviceIndex">Index of the device.</param>
	/// <param name="mapName">Shared memory mapped file name</param>
	/// <param name="errors">Store errors information</param>
	/// <returns>True if controller information is written to disk successfully, False otherwise</returns>
	public static bool AddControllerInfo(Guid deviceId, VirtualDeviceTypes deviceType, int deviceIndex, string mapName, DeviceErrors errors)
	{
		string mutexName = "RFIMutex" + mapName;
		int index = deviceIndex;
		VirtualControllerInfoStruct vc = new VirtualControllerInfoStruct
		{
			VirtualDeviceType = deviceType,
			VirtualDeviceIndex = deviceIndex,
			Offset = 0L
		};
		if (WriterHelper.CritSec(delegate(DeviceErrors err)
		{
			using IReadWriteAccessor readWriteAccessor = SharedMemHelper.CreateSharedBufferAccessor(deviceId, mapName, (!Utilities.IsRunningUnderLinux.Value) ? RawFileInfoStructSize : 0, creatable: false, err);
			if (readWriteAccessor == null)
			{
				return false;
			}
			readWriteAccessor.IncrementInt(28L);
			int num = Utilities.StructSizeLookup.Value[25];
			int num2 = 816 + num * index;
			readWriteAccessor.WriteStruct(num2, vc, num);
			return true;
		}, errors, mutexName, useGlobalNamespace: true))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Gets the index of the available controller.
	/// </summary>
	/// <param name="deviceId">The device identifier.</param>
	/// <param name="mapName">Name of the map.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True able to obtain a controller index; otherwise False.</returns>
	public static int GetAvailableControllerIndex(Guid deviceId, string mapName, DeviceErrors errors)
	{
		string mutexName = "RFIMutex" + mapName;
		int num = WriterHelper.CritSec(delegate(DeviceErrors err)
		{
			using IReadWriteAccessor readWriteAccessor = SharedMemHelper.CreateSharedBufferAccessor(deviceId, mapName, (!Utilities.IsRunningUnderLinux.Value) ? RawFileInfoStructSize : 0, creatable: false, err);
			if (readWriteAccessor == null)
			{
				err.UpdateError("Unable to access the RawFileInfo shared memory object.");
				return -1;
			}
			if (readWriteAccessor.ReadStructure<RawFileInfoStruct>(0L, RawFileInfoStructSize).NextAvailableControllerIndex < 64)
			{
				return readWriteAccessor.IncrementInt(32L) - 1;
			}
			err.UpdateError("Exceeded the maximum number of devices (64) which can be used for writing.");
			return -1;
		}, errors, mutexName, useGlobalNamespace: true);
		if (errors.HasError || num < 0)
		{
			num = -1;
		}
		return num;
	}
}
