using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// A helper class to provide common methods to release/close and get memory mapped file accessor, etc.
/// </summary>
public static class MemoryMappedFileHelper
{
	/// <summary>
	/// Releases and close the memory mapped file and viewer
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="viewer">The viewer.</param>
	/// <param name="forceToCloseMmf">if set to <c>true</c> [force to close MMF].</param>
	public static void ReleaseAndCloseMemoryMappedFile(this IDisposableReader viewer, IViewCollectionManager manager, bool forceToCloseMmf = false)
	{
		if (viewer != null)
		{
			string streamId = viewer.StreamId;
			viewer.Dispose();
			manager.Close(streamId, forceToCloseMmf);
		}
	}

	/// <summary>
	/// Gets the memory mapped viewer.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="loaderId">The loader identifier.</param>
	/// <param name="mapName">Name of the map.</param>
	/// <param name="offset">The offset.</param>
	/// <param name="size">The size.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [in acquisition].</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <returns>Memory mapped file accessor</returns>
	public static IReadWriteAccessor GetMemoryMappedViewer(this IReadWriteAccessor viewer, Guid loaderId, string mapName, long offset, long size, bool inAcquisition, DataFileAccessMode accessMode = DataFileAccessMode.OpenCreateRead, PersistenceMode type = PersistenceMode.Persisted)
	{
		if (!inAcquisition)
		{
			return MemoryMappedRawFileManager.Instance.GetRandomAccessViewer(loaderId, mapName, offset, size, inAcquisition: false, accessMode, type);
		}
		viewer?.Dispose();
		return MemoryMappedRawFileManager.Instance.GetRandomAccessViewer(loaderId, mapName, offset, size, inAcquisition: true, accessMode, type);
	}

	/// <summary>
	/// Gets the memory mapped viewer.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="loaderId">The loader identifier.</param>
	/// <param name="mapName">Name of the map.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [in acquisition].</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <returns>Memory mapped file accessor</returns>
	public static IReadWriteAccessor GetMemoryMappedViewer(this IReadWriteAccessor viewer, Guid loaderId, string mapName, bool inAcquisition, DataFileAccessMode accessMode = DataFileAccessMode.OpenCreateRead, PersistenceMode type = PersistenceMode.Persisted)
	{
		if (!inAcquisition)
		{
			return MemoryMappedRawFileManager.Instance.GetRandomAccessViewer(loaderId, mapName, inAcquisition: false, accessMode, type);
		}
		viewer?.Dispose();
		bool flag = Path.GetExtension(mapName).ToLowerInvariant().IndexOf(".tmp") != -1;
		return MemoryMappedRawFileManager.Instance.GetRandomAccessViewer(loaderId, mapName, inAcquisition: true, accessMode, (inAcquisition && !flag) ? PersistenceMode.NonPersisted : type);
	}

	/// <summary>
	/// Determines whether [is failed to map a zero length file] [the specified stream identifier].
	/// </summary>
	/// <param name="streamId">The stream identifier.</param>
	/// <param name="errors">The error messages</param>
	/// <returns>True tried to map a zero length file; false otherwise.</returns>
	public static bool IsFailedToMapAZeroLengthFile(string streamId, out string errors)
	{
		errors = MemoryMappedRawFileManager.Instance.GetErrors(streamId);
		return errors.Contains(Resources.ErrorMapAZeroLenghtFile);
	}

	/// <summary>
	/// Determines whether [is failed to map a zero length file] [the specified stream identifier].
	/// </summary>
	/// <param name="streamId">The stream identifier.</param>
	/// <returns>True tried to map a zero length file; false otherwise.</returns>
	public static bool IsFailedToMapAZeroLengthFile(string streamId)
	{
		string errors;
		return IsFailedToMapAZeroLengthFile(streamId, out errors);
	}
}
