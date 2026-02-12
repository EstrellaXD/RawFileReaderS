using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// SharedBuffer is a class that utilize memory mapped file for sharing data between processes.
/// </summary>
internal static class SharedMemHelper
{
	/// <summary>
	/// Creates the shared buffer accessor.
	/// </summary>
	/// <param name="deviceId">device id</param>
	/// <param name="mapName">Name of the map.</param>
	/// <param name="size">The size.</param>
	/// <param name="creatable">if set to <c>true</c> [creatable].</param>
	/// <param name="errors">device errors object</param>
	/// <returns>True created shared memory map object successfully; otherwise False</returns>
	public static IReadWriteAccessor CreateSharedBufferAccessor(Guid deviceId, string mapName, long size, bool creatable, DeviceErrors errors)
	{
		try
		{
			bool num = Utilities.IsAdministrator();
			string streamId = StreamHelper.ConstructStreamId(deviceId, mapName);
			DataFileAccessMode accessMode = ((!num) ? (creatable ? DataFileAccessMode.OpenCreateReadWrite : DataFileAccessMode.OpenReadWrite) : (creatable ? DataFileAccessMode.OpenCreateReadWriteGlobal : DataFileAccessMode.OpenReadWriteGlobal));
			IReadWriteAccessor randomAccessViewer = MemoryMappedRawFileManager.Instance.GetRandomAccessViewer(deviceId, mapName, 0L, size, inAcquisition: true, accessMode, PersistenceMode.NonPersisted);
			ValidateMemMapAccessor(randomAccessViewer, streamId, errors);
			return randomAccessViewer;
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex);
		}
		return null;
	}

	/// <summary>
	/// Validates the memory map accessor.
	/// </summary>
	/// <param name="accessor">The shared memory map accessor.</param>
	/// <param name="streamId">Stream data ID</param>
	/// <param name="errors">Device errors object that store errors information.</param>
	private static void ValidateMemMapAccessor(IReadWriteAccessor accessor, string streamId, DeviceErrors errors)
	{
		if (accessor == null)
		{
			string error;
			if (string.IsNullOrWhiteSpace(error = MemoryMappedRawFileManager.Instance.GetErrors(streamId)))
			{
				error = "Invalid shared memory handler.";
			}
			errors.UpdateError(error);
		}
	}
}
