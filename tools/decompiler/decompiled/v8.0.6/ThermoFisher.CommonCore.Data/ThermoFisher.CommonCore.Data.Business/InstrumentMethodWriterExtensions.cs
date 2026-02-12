using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Instrument Method Writer Extension Methods
/// </summary>
public static class InstrumentMethodWriterExtensions
{
	private const string DataSection = "Data";

	/// <summary>
	/// Creates a device section with the given device name and writes the binary data
	/// and text data to it. If the same device name is already exist, it will get
	/// overwritten.
	/// </summary>
	/// <param name="writer">The instrument method writer object.</param>
	/// <param name="binaryData">The instrument method in binary format.</param>
	/// <param name="textData">The instrument method in text string.</param>
	/// <param name="deviceName">The device name</param>
	/// <returns>True if </returns>
	public static string WriteSection(this IInstrumentMethodWriter writer, byte[] binaryData, string textData, string deviceName)
	{
		if (writer == null)
		{
			return "The instrument method writer cannot be null.";
		}
		if (string.IsNullOrEmpty(deviceName))
		{
			return "The device name cannot be null or empty.";
		}
		try
		{
			Dictionary<string, IDeviceMethod> devices = writer.GetDevices();
			IDeviceMethod deviceMethod;
			if (devices.ContainsKey(deviceName))
			{
				deviceMethod = devices[deviceName];
			}
			else
			{
				deviceMethod = DeviceMethodFactory.CreateDeviceMethod();
				devices.Add(deviceName, deviceMethod);
			}
			Dictionary<string, byte[]> streamBytes = deviceMethod.GetStreamBytes();
			deviceMethod.MethodText = (string.IsNullOrEmpty(textData) ? string.Empty : textData);
			streamBytes["Data"] = binaryData;
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
		return string.Empty;
	}
}
