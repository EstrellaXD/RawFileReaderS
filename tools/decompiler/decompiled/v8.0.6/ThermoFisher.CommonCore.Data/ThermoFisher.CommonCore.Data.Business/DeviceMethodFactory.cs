using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This static factory class provides method to create a device method for an instrument method.
/// </summary>
public static class DeviceMethodFactory
{
	private static readonly ObjectFactory<IDeviceMethod> InstDeviceMethodFactory = CreateDeviceMethodFactory();

	/// <summary>
	/// Creates the device method.
	/// </summary>
	/// <param name="deviceData">Optional: Data for the device</param>
	/// <returns>The IDeviceMethod instance.</returns>
	/// <exception cref="T:System.ArgumentNullException">name;Device method name cannot be empty.</exception>
	public static IDeviceMethod CreateDeviceMethod(IInstrumentMethodDataAccess deviceData = null)
	{
		IDeviceMethod deviceMethod = InstDeviceMethodFactory.SpecifiedMethod(new object[0]);
		if (deviceData == null)
		{
			deviceMethod.MethodText = string.Empty;
		}
		else
		{
			deviceMethod.MethodText = deviceData.MethodText;
			IReadOnlyDictionary<string, byte[]> streamBytes = deviceData.StreamBytes;
			Dictionary<string, byte[]> streamBytes2 = deviceMethod.GetStreamBytes();
			foreach (KeyValuePair<string, byte[]> item in streamBytes)
			{
				streamBytes2.Add(item.Key, item.Value);
			}
		}
		return deviceMethod;
	}

	/// <summary>
	/// Creates the device method factory.
	/// </summary>
	/// <returns>Device method writer factory object.</returns>
	private static ObjectFactory<IDeviceMethod> CreateDeviceMethodFactory()
	{
		return new ObjectFactory<IDeviceMethod>("ThermoFisher.CommonCore.RawFileReader.Writers.DeviceMethodAdapter", "ThermoFisher.CommonCore.RawFileReader.dll", "CreateDeviceMethod", new Type[0], initialize: true);
	}
}
