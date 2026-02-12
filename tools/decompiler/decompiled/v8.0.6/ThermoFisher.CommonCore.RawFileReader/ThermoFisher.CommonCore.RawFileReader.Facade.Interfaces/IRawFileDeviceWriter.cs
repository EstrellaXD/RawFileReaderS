using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The RawFileDeviceWriter interface.
/// </summary>
internal interface IRawFileDeviceWriter : IDisposable
{
	/// <summary>
	/// Gets the run header.
	/// </summary>
	RunHeader RunHeader { get; }

	/// <summary>
	/// The save.
	/// </summary>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <param name="packetDataOffset">
	/// The packet data offset.
	/// </param>
	/// <param name="controllerHeaderOffset">
	/// The controller header offset.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	bool Save(BinaryWriter writer, DeviceErrors errors, long packetDataOffset, out long controllerHeaderOffset);

	/// <summary>
	/// Refresh data from memory maps
	/// </summary>
	/// <returns>
	/// True if OK
	/// </returns>
	bool Refresh();
}
