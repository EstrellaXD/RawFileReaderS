using System;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// The status device.
/// This device only has status logs.
/// Use implementation of status logs within base class (UVDevice)
/// </summary>
internal class StatusDevice : UvDevice
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.StatusDevice" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">raw file loader ID</param>
	/// <param name="deviceInfo">The device information.</param>
	/// <param name="rawFileName">The viewer.</param>
	/// <param name="fileRevision">The file version.</param>
	/// <param name="isInAcquisition">if set to <c>true</c> [is in acquisition].</param>
	/// <param name="oldRev">Set if legacy file format</param>
	public StatusDevice(IViewCollectionManager manager, Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileRevision, bool isInAcquisition, bool oldRev)
		: base(manager, loaderId, deviceInfo, rawFileName, fileRevision, isInAcquisition, oldRev)
	{
	}
}
