using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This static class contains factories to create device writers.
/// UV Device, PDA Device, MS Device and Status Device writers.
/// Marked internal, as this factory is only available
/// to internal company or partner code, via public key.
/// </summary>
internal static class DeviceWriterAdapter
{
	/// <summary>
	/// Creates the UV type device writer to write UV data.
	/// </summary>
	/// <param name="fileName">Connect to a "in-acquisition" raw file</param>
	/// <param name="deviceType">Type of the device to be created.</param>
	/// <returns>A UV type of device write to write UV, Analog and MS Analog data to the disk file</returns>
	public static IUvDeviceWriter CreateUvDeviceWriter(string fileName, Device deviceType)
	{
		return new UvDeviceWriter(deviceType, -1, fileName, inAcquisition: true);
	}

	/// <summary>
	/// Creates the UV type device writer to write UV data, including methods which have binary packed data
	/// </summary>
	/// <param name="fileName">Connect to a "in-acquisition" raw file</param>
	/// <param name="deviceType">Type of the device to be created.</param>
	/// <param name="domain">Determines the format of this channel (such as Xcalibur or chromeleon)</param>
	/// <returns>A UV type of device write to write UV, Analog and MS Analog data to the disk file</returns>
	public static IUvDeviceBinaryWriter CreateUvDeviceBinaryWriter(string fileName, Device deviceType, RawDataDomain domain)
	{
		return new UvDeviceWriter(deviceType, -1, fileName, inAcquisition: true, domain);
	}

	/// <summary>
	/// Creates the PDA type device writer to write UV data, including methods which have binary packed data
	/// </summary>
	/// <param name="fileName">Connect to a "in-acquisition" raw file</param>
	/// <param name="domain">Determines if this channel came from Xcalibur or Chromeleon</param>
	/// <returns>A PDA type of device write to write PDA data to the disk file</returns>
	public static IPdaDeviceBinaryWriter CreatePdaDeviceBinaryWriter(string fileName, RawDataDomain domain = RawDataDomain.MassSpectrometry)
	{
		return new PdaDeviceWriter(Device.Pda, -1, fileName, inAcquisition: true, domain);
	}

	/// <summary>
	/// Creates Analog device writer to write analog data.
	/// </summary>
	/// <param name="fileName">Connect to a "in-acquisition" raw file</param>
	/// <param name="deviceType">Type of the device to be created.</param>
	/// <returns>A device writer to write analog data to the disk file</returns>
	public static IAnalogDeviceWriter CreateAnalogDeviceWriter(string fileName, Device deviceType)
	{
		return new UvDeviceWriter(deviceType, -1, fileName, inAcquisition: true);
	}

	/// <summary>
	/// Creates Analog device writer to write analog data.
	/// </summary>
	/// <param name="fileName">Connect to a "in-acquisition" raw file</param>
	/// <param name="deviceType">Type of the device to be created.</param>
	/// <returns>A device writer to write analog data to the disk file</returns>
	public static IAnalogDeviceBinaryWriter CreateAnalogDeviceBinaryWriter(string fileName, Device deviceType)
	{
		return new UvDeviceWriter(deviceType, -1, fileName, inAcquisition: true);
	}

	/// <summary>
	/// Creates the PDA device writer.
	/// </summary>
	/// <param name="fileName">Connect to a "in-acquisition" raw file</param>
	/// <returns>A device writer to write PDA data to the disk file</returns>
	public static IPdaDeviceWriter CreatePdaDeviceWriter(string fileName)
	{
		return new PdaDeviceWriter(Device.Pda, -1, fileName, inAcquisition: true);
	}

	/// <summary>
	/// Creates the mass spec device writer.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="highestTypeUsed">If &gt; 1: Defines the highest packet type that this device may use.
	/// By default this is 1, as that was the value used in original "version 66" raw files.
	/// All apps linked to any version of this dll can decode such files.
	/// New devices may have new data types, which can only be decoded by (matching) newer versions of this code.</param>
	/// <param name="dataDomain">Optional "raw data domain" which will default to legacy file format (as used by Xcalibur).</param>
	/// <returns>A device writer to write mass spec data to the disk file</returns>
	public static IMassSpecDeviceWriter CreateMassSpecDeviceWriter(string fileName, SpectrumPacketType highestTypeUsed = SpectrumPacketType.LowResolutionSpectrum, RawDataDomain dataDomain = RawDataDomain.Legacy)
	{
		return new MassSpecDeviceWriter(Device.MS, -1, fileName, inAcquisition: true, 66, highestTypeUsed, dataDomain);
	}

	/// <summary>
	/// Creates the mass spec device binary writer. This gets the same data as "IMassSpecDeviceWriter" but
	/// data is alrady packed into byte arrays for direct recording. Files created with this writer are always created in the 
	/// MassSpectrometry data domain, as this feature is new as of .NET 5 code (Luna system).
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="highestTypeUsed">If &gt; 1: Defines the highest packet type that this device may use.
	/// By default this is 1, as that was the value used in original "version 66" raw files.
	/// All apps linked to any version of this dll can decode such files.
	/// New devices may have new data types, which can only be decoded by (matching) newer versions of this code.</param>
	/// <returns>A device writer to write mass spec data to the disk file</returns>
	public static IMassSpecDeviceBinaryWriter CreateMassSpecDeviceBinaryWriter(string fileName, SpectrumPacketType highestTypeUsed = SpectrumPacketType.LowResolutionSpectrum)
	{
		return new MassSpecDeviceWriter(Device.MS, -1, fileName, inAcquisition: true, 66, highestTypeUsed, RawDataDomain.MassSpectrometry);
	}

	/// <summary>
	/// Creates an "other" device writer.
	/// These devices may also be called "status devices"
	/// or "diagnostic devices" as they have logs only, and 
	/// cannot record scan data.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>A device writer to write mass spec data to the disk file</returns>
	public static IOtherDeviceWriter CreateOtherDeviceWriter(string fileName)
	{
		return new UvDeviceWriter(Device.Other, -1, fileName, inAcquisition: true);
	}

	/// <summary>
	/// Creates an "other" device binary writer.
	/// These devices may also be called "status devices"
	/// or "diagnostic devices" as they have logs only, and 
	/// cannot record scan data.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>A device writer to write mass spec data to the disk file</returns>
	public static IOtherDeviceBinaryWriter CreateOtherDeviceBinaryWriter(string fileName)
	{
		return new UvDeviceWriter(Device.Other, -1, fileName, inAcquisition: true);
	}
}
