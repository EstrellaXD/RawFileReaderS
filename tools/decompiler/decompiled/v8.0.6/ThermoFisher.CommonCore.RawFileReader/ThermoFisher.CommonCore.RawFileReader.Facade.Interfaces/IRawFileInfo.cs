using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The RawFileInfo interface.
/// </summary>
internal interface IRawFileInfo : IRealTimeAccess, IDisposable
{
	/// <summary>
	///     Gets the blob size.
	/// </summary>
	uint BlobSize { get; }

	/// <summary>
	///     Gets the blob start.
	/// </summary>
	long BlobStart { get; }

	/// <summary>
	///     Gets or sets the computer name.
	/// </summary>
	string ComputerName { get; set; }

	/// <summary>
	///     Gets a value indicating whether has experiment method.
	/// </summary>
	bool HasExpMethod { get; }

	/// <summary>
	///     Gets a value indicating whether is in acquisition.
	/// </summary>
	bool IsInAcquisition { get; }

	/// <summary>
	///     Gets or sets the mass spec data offset.
	/// </summary>
	long MsDataOffset { get; set; }

	/// <summary>
	///     Gets the next available controller index.
	/// </summary>
	int NextAvailableControllerIndex { get; }

	/// <summary>
	///     Gets the number of virtual controllers.
	/// </summary>
	int NumberOfVirtualControllers { get; }

	/// <summary>
	/// Gets the time stamp.
	/// </summary>
	DateTime TimeStamp { get; }

	/// <summary>
	///     Gets the user texts.
	/// </summary>
	string[] UserLabels { get; }

	/// <summary>
	///     Gets the virtual controller data.
	/// </summary>
	List<VirtualControllerInfo> VirtualControllers { get; }

	/// <summary>
	/// The number of virtual controllers of type.
	/// </summary>
	/// <param name="type">
	/// The type.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	int NumberOfVirtualControllersOfType(VirtualDeviceTypes type);

	/// <summary>
	/// Updates the virtual controller.
	/// </summary>
	/// <param name="numVirControllers">The number virtual controllers.</param>
	/// <param name="virOffset">The virtual data offset </param>
	/// <param name="offset">The offset.</param>
	/// <param name="virDeviceIndex">Index of the virtual device.</param>
	/// <param name="virDeviceType">Type of the virtual device.</param>
	void UpdateVirtualController(int numVirControllers, long virOffset, long offset, int virDeviceIndex, VirtualDeviceTypes virDeviceType);
}
