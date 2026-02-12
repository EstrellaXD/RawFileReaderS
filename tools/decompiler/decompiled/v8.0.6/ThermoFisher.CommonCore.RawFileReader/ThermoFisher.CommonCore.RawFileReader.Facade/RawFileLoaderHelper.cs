using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// The raw file loader helper.
/// </summary>
internal static class RawFileLoaderHelper
{
	/// <summary>
	/// Convert instruments index to controller index.
	/// </summary>
	/// <param name="instrumentType">Type of the instrument.</param>
	/// <param name="instrumentIndex">Index of the instrument.</param>
	/// <param name="virtualControllerInfos">The list of virtual controller information.</param>
	/// <returns>The index into the table of controllers</returns>
	internal static int InstrumentIndexToControllerIndex(Device instrumentType, int instrumentIndex, IReadOnlyList<VirtualControllerInfo> virtualControllerInfos)
	{
		if (!virtualControllerInfos.IsAny())
		{
			return -1;
		}
		int count = virtualControllerInfos.Count;
		if (instrumentType <= Device.None || instrumentType > Device.Other || instrumentIndex <= 0 || instrumentIndex > 64 || instrumentIndex > count)
		{
			return -1;
		}
		VirtualDeviceTypes virtualDeviceTypes = instrumentType.ToVirtualDeviceType();
		int num = 0;
		bool flag = false;
		foreach (VirtualControllerInfo virtualControllerInfo in virtualControllerInfos)
		{
			if (virtualControllerInfo.VirtualDeviceType == virtualDeviceTypes && --instrumentIndex == 0)
			{
				flag = true;
				break;
			}
			num++;
		}
		if (!flag)
		{
			return -1;
		}
		return num;
	}

	/// <summary>
	/// get virtual controller info.
	/// </summary>
	/// <param name="loader"></param>
	/// <param name="instrumentType">
	/// The instrument type.
	/// </param>
	/// <param name="instrumentIndex">
	/// The instrument index.
	/// </param>
	/// <param name="virtualControllerInfos">
	/// The virtual controller information.
	/// </param>
	/// <param name="detectors">
	/// The detectors.
	/// </param>
	/// <param name="selectedDevice">
	/// The selected device.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualControllerInfo" />.
	/// </returns>
	internal static VirtualControllerInfo GetVirtualControllerInfo(this IRawFileLoader loader, Device instrumentType, int instrumentIndex, IReadOnlyList<VirtualControllerInfo> virtualControllerInfos, DeviceContainer[] detectors, out IDevice selectedDevice)
	{
		VirtualControllerInfo virtualControllerInfo = null;
		selectedDevice = null;
		try
		{
			if (detectors.Length == 0)
			{
				return null;
			}
			int num = InstrumentIndexToControllerIndex(instrumentType, instrumentIndex, virtualControllerInfos);
			if (num > -1)
			{
				virtualControllerInfo = virtualControllerInfos[num];
				if (virtualControllerInfo != null)
				{
					selectedDevice = detectors[num]?.FullDevice.Value;
				}
			}
		}
		catch (Exception ex)
		{
			loader.AppendError(ex);
		}
		finally
		{
			_ = selectedDevice;
		}
		return virtualControllerInfo;
	}

	/// <summary>
	/// Gets the device run header.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">The loader ID</param>
	/// <param name="selectedDevice">The device key.</param>
	/// <returns>The run header</returns>
	/// <exception cref="T:System.Exception">Thrown on null device or null run header</exception>
	internal static IRunHeader GetDeviceRunHeader(IViewCollectionManager manager, Guid loaderId, IDevice selectedDevice)
	{
		if (selectedDevice?.RunHeader == null)
		{
			throw new Exception(Resources.ErrorNullRunHeader);
		}
		if (selectedDevice.DeviceType == VirtualDeviceTypes.MsDevice)
		{
			return selectedDevice.RunHeader;
		}
		ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.RunHeader runHeader = new ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.RunHeader(manager, loaderId, selectedDevice.RunHeader);
		RunHeaderStruct runHeaderStruct = runHeader.RunHeaderStruct;
		if (selectedDevice.DeviceType == VirtualDeviceTypes.StatusDevice)
		{
			if (runHeaderStruct.LastSpectrum < runHeaderStruct.FirstSpectrum)
			{
				runHeaderStruct.FirstSpectrum = -1;
				runHeaderStruct.LastSpectrum = -1;
			}
			if (runHeaderStruct.HighMass < runHeaderStruct.LowMass)
			{
				runHeaderStruct.LowMass = 0.0;
				runHeaderStruct.HighMass = 0.0;
			}
			if (runHeaderStruct.EndTime < runHeaderStruct.StartTime)
			{
				runHeaderStruct.StartTime = -1.0;
				runHeaderStruct.EndTime = -1.0;
			}
		}
		runHeaderStruct.NumTuneData = -1;
		runHeaderStruct.NumTrailerExtra = -1;
		runHeader.CopyRunHeaderStruct(ref runHeaderStruct);
		return runHeader;
	}
}
