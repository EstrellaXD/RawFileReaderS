using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
///     The singleton class tracks the memory mapped files that have been open.
/// </summary>
internal class MemoryMappedRawFileManager : IViewCollectionManager
{
	private static readonly Lazy<MemoryMappedRawFileManager> LazyInstance = new Lazy<MemoryMappedRawFileManager>(() => new MemoryMappedRawFileManager());

	private readonly ConcurrentDictionary<string, MemoryMappedRawFile> _memoryMappedFiles;

	private readonly ConcurrentDictionary<string, string> _lastErrors;

	/// <summary>
	/// Gets the instance.
	/// </summary>
	public static IViewCollectionManager Instance => LazyInstance.Value;

	/// <summary>
	/// Gets the extended attributes.
	/// Custom data extension that gives additional information to the reader plugin about 
	/// extra data/info for the plugin code to use at runtime.
	/// </summary>
	public Dictionary<string, string> ExtensionAttributes { get; private set; }

	/// <summary>
	/// Prevents a default instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemoryMappedRawFileManager" /> class from being created.
	/// </summary>
	private MemoryMappedRawFileManager()
	{
		_memoryMappedFiles = new ConcurrentDictionary<string, MemoryMappedRawFile>();
		_lastErrors = new ConcurrentDictionary<string, string>();
		ExtensionAttributes = new Dictionary<string, string>();
	}

	/// <summary>
	/// The method disposes the memory mapped file and stops tracking it.
	/// </summary>
	/// <param name="streamId">
	/// The stream id - serves as the key.
	/// </param>
	/// <param name="forceToClose">True close the specified memory mapped file even if it's reference to the map; false skip it if the reference count is more than zero.</param>
	public void Close(string streamId, bool forceToClose = false)
	{
		if (!string.IsNullOrWhiteSpace(streamId))
		{
			MemoryMappedRawFile value2;
			if (!_memoryMappedFiles.TryGetValue(streamId, out var value))
			{
				RemoveLastErrors(streamId);
			}
			else if ((value.GetRefCount() <= 0 || forceToClose) && _memoryMappedFiles.TryRemove(streamId, out value2))
			{
				value2.Dispose();
				RemoveLastErrors(streamId);
			}
		}
	}

	/// <summary>
	/// Gets the random access viewer.
	/// </summary>
	/// <param name="id">The identifier.</param>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [refresh memory map file].</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <returns>Memory mapped file accessor.</returns>
	public IReadWriteAccessor GetRandomAccessViewer(Guid id, string fileName, bool inAcquisition, DataFileAccessMode accessMode = DataFileAccessMode.OpenCreateRead, PersistenceMode type = PersistenceMode.Persisted)
	{
		string streamId = StreamHelper.ConstructStreamId(id, fileName);
		if (inAcquisition)
		{
			Instance.Close(streamId, forceToClose: true);
		}
		return GetMemoryMappedFile(id, fileName, accessMode, type, 0L)?.GetRandomAccessViewer(MemoryMappedFileAccess.Read);
	}

	/// <summary>
	/// Gets the random access viewer.
	/// </summary>
	/// <param name="loaderId">The loader identifier.</param>
	/// <param name="mapName">Name of the share memory mapped file.</param>
	/// <param name="offset">The offset.</param>
	/// <param name="size">The size.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [refresh memory map file].</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <returns>Memory mapped file accessor.</returns>
	public IReadWriteAccessor GetRandomAccessViewer(Guid loaderId, string mapName, long offset, long size, bool inAcquisition, DataFileAccessMode accessMode = DataFileAccessMode.OpenCreateRead, PersistenceMode type = PersistenceMode.Persisted)
	{
		string streamId = ((loaderId == Guid.Empty) ? mapName : StreamHelper.ConstructStreamId(loaderId, mapName));
		if (inAcquisition)
		{
			Instance.Close(streamId, forceToClose: true);
		}
		MemoryMappedRawFile memoryMappedFile = GetMemoryMappedFile(loaderId, mapName, accessMode, type, size);
		if (memoryMappedFile == null)
		{
			return null;
		}
		if ((accessMode & DataFileAccessMode.Write) == DataFileAccessMode.Write)
		{
			return memoryMappedFile.GetRandomAccessViewer(offset, size, MemoryMappedFileAccess.ReadWrite);
		}
		return memoryMappedFile.GetRandomAccessViewer(offset, size, MemoryMappedFileAccess.Read);
	}

	/// <summary>
	/// Determines whether the specified file path is open.
	/// </summary>
	/// <param name="streamId">The file path.</param>
	/// <returns>true if open</returns>
	public bool IsOpen(string streamId)
	{
		if (string.IsNullOrWhiteSpace(streamId))
		{
			return false;
		}
		return _memoryMappedFiles[streamId]?.IsOpenSucceed ?? false;
	}

	/// <summary>
	/// Gets the errors.
	/// </summary>
	/// <param name="streamId">Name of the file.</param>
	/// <returns>Error Message</returns>
	public string GetErrors(string streamId)
	{
		if (string.IsNullOrWhiteSpace(streamId))
		{
			return string.Empty;
		}
		if (_lastErrors.TryGetValue(streamId, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	/// <summary>
	/// Remove last errors. Cleans up the last errors for this stream id.
	/// </summary>
	/// <param name="streamId">
	/// The stream id.
	/// </param>
	private void RemoveLastErrors(string streamId)
	{
		_lastErrors.TryRemove(streamId, out var _);
	}

	/// <summary>
	/// Gets the memory mapped file.
	/// </summary>
	/// <param name="loaderId">The loader identifier.</param>
	/// <param name="mapName">Name of the map.</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <param name="size">The size.</param>
	/// <returns>The memory mapped file object</returns>
	private MemoryMappedRawFile GetMemoryMappedFile(Guid loaderId, string mapName, DataFileAccessMode accessMode, PersistenceMode type, long size = 0L)
	{
		string key = ((loaderId == Guid.Empty) ? mapName : StreamHelper.ConstructStreamId(loaderId, mapName));
		_memoryMappedFiles.TryGetValue(key, out var value);
		if (value != null)
		{
			value.IncrementRefCount();
			if (_memoryMappedFiles.TryUpdate(key, value, value))
			{
				return value;
			}
		}
		MemoryMappedRawFile memoryMappedRawFile = new MemoryMappedRawFile(loaderId, mapName, accessMode, type, size);
		if (memoryMappedRawFile.IsOpenSucceed)
		{
			if (!memoryMappedRawFile.Errors.IsNullOrEmpty())
			{
				_lastErrors[key] = memoryMappedRawFile.Errors;
			}
			MemoryMappedRawFile memoryMappedRawFile2 = _memoryMappedFiles.AddOrUpdate(key, memoryMappedRawFile, delegate(string text, MemoryMappedRawFile existingValue)
			{
				existingValue.IncrementRefCount();
				return existingValue;
			});
			if (memoryMappedRawFile2 != memoryMappedRawFile)
			{
				memoryMappedRawFile.Dispose();
			}
			return memoryMappedRawFile2;
		}
		_lastErrors[key] = memoryMappedRawFile.Errors;
		return null;
	}
}
