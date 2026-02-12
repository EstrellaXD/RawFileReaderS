using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
///     The Device interface.
/// </summary>
internal interface IDevice : IRealTimeAccess, IDisposable
{
	/// <summary>
	///     Gets the device type.
	/// </summary>
	VirtualDeviceTypes DeviceType { get; }

	/// <summary>
	///     Gets the error log entries.
	/// </summary>
	IErrorLog ErrorLogEntries { get; }

	/// <summary>
	///     Gets the instrument id.
	/// </summary>
	IInstrumentId InstrumentId { get; }

	/// <summary>
	///     Gets the run header.
	/// </summary>
	IRunHeader RunHeader { get; }

	/// <summary>
	///     Gets the status log entries.
	/// </summary>
	IStatusLog StatusLogEntries { get; }

	/// <summary>
	/// Gets the absolute position of the end of this device data.
	/// </summary>
	long OffsetOfEndOfDevice { get; }

	/// <summary>
	/// Gets or sets a value indicating whether this was initialized when the file was in acquisition.
	/// </summary>
	bool InAcquisition { get; set; }

	/// <summary>
	/// Support for lazy init of device.
	/// Device construction does "some work" to validate a device.
	/// Any "heavy data decoding" is delayed until the device is first used.
	/// </summary>
	/// <returns>The device</returns>
	IDevice Initialize();

	/// <summary>
	/// The method gets the scan index for the scan number.
	/// </summary>
	/// <param name="spectrum">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IScanIndex" /> object for the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is not in range.
	/// </exception>
	IScanIndex GetScanIndex(int spectrum);

	/// <summary>
	/// The method gets the retention time for the scan number.
	/// </summary>
	/// <param name="spectrum">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The retention time for the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is not in range.
	/// </exception>
	double GetRetentionTime(int spectrum);

	/// <summary>
	/// Gets the packet.
	/// </summary>
	/// <param name="scanNumber">The scan number.</param>
	/// <param name="includeReferenceAndExceptionData">if set to <c>true</c> [include reference and exception data].</param>
	/// <param name="channelNumber">For UV device only, negative one (-1) for getting all the channel data by the given scan number</param>
	/// <param name="packetScanDataFeatures">Optional data which can be returned with a scan</param>
	/// <returns>The data for the scan</returns>
	IPacket GetPacket(int scanNumber, bool includeReferenceAndExceptionData, int channelNumber = -1, PacketFeatures packetScanDataFeatures = PacketFeatures.NoiseAndBaseline | PacketFeatures.Chagre | PacketFeatures.Resolution);

	/// <summary>
	/// Gets the segment peaks.
	/// </summary>
	/// <param name="scanNum">
	/// The scan number.
	/// </param>
	/// <param name="numSegments">
	/// The number segments.
	/// </param>
	/// <param name="numAllPeaks">
	/// The number all peaks.
	/// </param>
	/// <param name="packet">
	/// The packet.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">
	/// if set to <c>true</c> [include reference and exception data].
	/// </param>
	/// <returns>
	/// The data for the scan
	/// </returns>
	IReadOnlyList<SegmentData> GetSegmentPeaks(int scanNum, out int numSegments, out int numAllPeaks, out IPacket packet, bool includeReferenceAndExceptionData);
}
