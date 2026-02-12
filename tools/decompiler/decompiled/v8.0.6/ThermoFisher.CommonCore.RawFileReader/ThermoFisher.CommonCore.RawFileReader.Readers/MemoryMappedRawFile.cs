using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Threading;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// The memory mapped file for viewing a raw file.
/// </summary>
internal sealed class MemoryMappedRawFile : IDisposable
{
	private readonly string _streamId;

	private MemoryMappedFile _memoryMappedFile;

	private FileInfo _mmfInfo;

	private int _refCount = 1;

	private bool _isDisposed;

	private static Dictionary<string, int> _tempPhysicalFiles = new Dictionary<string, int>();

	/// <summary>
	/// Gets a value indicating whether this instance is open succeed.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is open succeed; otherwise, <c>false</c>.
	/// </value>
	public bool IsOpenSucceed { get; private set; }

	/// <summary>
	/// Gets a value indicating whether this <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemoryMappedRawFile" /> is errors.
	/// </summary>
	/// <value>
	///   <c>true</c> if errors; otherwise, <c>false</c>.
	/// </value>
	public string Errors { get; private set; }

	/// <summary>
	/// Gets the currently mapped memory map file name.
	/// </summary>
	public string MemMappedFileName { get; }

	/// <summary>
	/// Gets the stream id.
	/// </summary>
	internal string StreamId => _streamId;

	/// <summary>
	/// Prevents a default instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemoryMappedRawFile" /> class from being created.
	/// </summary>
	private MemoryMappedRawFile()
	{
		_mmfInfo = null;
		IsOpenSucceed = false;
		_streamId = (Errors = string.Empty);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemoryMappedRawFile" /> class.
	/// </summary>
	/// <param name="loaderId">The loader identifier.</param>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <param name="size">The size.</param>
	public MemoryMappedRawFile(Guid loaderId, string fileName, DataFileAccessMode accessMode, PersistenceMode type, long size = 0L)
		: this()
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			Errors = Resources.ErrorEmptyNullFileName;
			return;
		}
		string arg = ((loaderId == Guid.Empty) ? string.Empty : loaderId.ToString("N"));
		string text = (((accessMode & DataFileAccessMode.Id) > (DataFileAccessMode)0) ? $"{arg}-{Utilities.RemoveSlashes(fileName)}" : Utilities.RemoveSlashes(fileName));
		MemMappedFileName = "Global\\" + text;
		try
		{
			if (Utilities.IsRunningMono.Value)
			{
				throw new FileNotFoundException();
			}
			if (!OperatingSystem.IsWindows())
			{
				if (type == PersistenceMode.Persisted)
				{
					throw new FileNotFoundException();
				}
				bool flag = File.Exists(text);
				if ((accessMode & DataFileAccessMode.Create) != DataFileAccessMode.Create && !flag)
				{
					throw new Exception("temp memory mapped physical file does not exist");
				}
				MemMappedFileName = text;
				MemoryMappedFileAccess access = (((accessMode & DataFileAccessMode.Write) != DataFileAccessMode.Write) ? MemoryMappedFileAccess.Read : MemoryMappedFileAccess.ReadWrite);
				_memoryMappedFile = MemoryMappedFile.CreateFromFile(text, FileMode.OpenOrCreate, null, size, access);
				if (_tempPhysicalFiles.ContainsKey(text))
				{
					_tempPhysicalFiles[text] += 1;
				}
				else
				{
					_tempPhysicalFiles.Add(text, 1);
				}
			}
			else
			{
				MemMappedFileName = OpenExistMemoryMappedFile(MemMappedFileName, text, accessMode, ref _memoryMappedFile);
				if (type == PersistenceMode.Persisted && File.Exists(fileName))
				{
					GetMappedFileInformation(fileName, out _mmfInfo);
				}
			}
		}
		catch (FileNotFoundException)
		{
			try
			{
				_memoryMappedFile = null;
				if ((accessMode & DataFileAccessMode.Create) == DataFileAccessMode.Create)
				{
					switch (type)
					{
					case PersistenceMode.Persisted:
						MemMappedFileName = CreateMemoryMappedFileFromFile(accessMode, fileName, MemMappedFileName, text, ref _memoryMappedFile);
						break;
					case PersistenceMode.NonPersisted:
						MemMappedFileName = CreateNonPersistedMemoryMappedFile(accessMode, MemMappedFileName, text, size, ref _memoryMappedFile);
						break;
					default:
						Errors = string.Format("Unable to create a memory mapped file {0}! {1}", fileName, "Invalid Memory Mapped File Type");
						break;
					}
				}
				else
				{
					Errors = "The memory mapped file doesn't exist.";
				}
			}
			catch (Exception ex2)
			{
				Errors = ex2.Message;
			}
		}
		catch (Exception ex3)
		{
			Errors = ex3.Message;
		}
		_streamId = StreamHelper.ConstructStreamId(loaderId, fileName);
		if (_memoryMappedFile != null && string.IsNullOrWhiteSpace(Errors))
		{
			IsOpenSucceed = true;
		}
	}

	/// <summary>
	/// increment the ref count.
	/// </summary>
	internal void IncrementRefCount()
	{
		Interlocked.Increment(ref _refCount);
	}

	/// <summary>
	/// decrement the ref count.
	/// </summary>
	internal void DecrementRefCount()
	{
		Interlocked.Decrement(ref _refCount);
	}

	/// <summary>
	/// get the ref count.
	/// </summary>
	/// <returns>
	/// The reference count
	/// </returns>
	internal int GetRefCount()
	{
		return _refCount;
	}

	/// <summary>
	/// The dispose method closes the memory mapped file.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}
		_isDisposed = true;
		IsOpenSucceed = false;
		Errors = "Done Disposing of a memory mapped file";
		_memoryMappedFile.Dispose();
		_memoryMappedFile = null;
		if (!Utilities.IsRunningUnderLinux.Value)
		{
			return;
		}
		try
		{
			if (_tempPhysicalFiles.ContainsKey(MemMappedFileName))
			{
				_tempPhysicalFiles[MemMappedFileName] -= 1;
				if (_tempPhysicalFiles[MemMappedFileName] <= 0)
				{
					_tempPhysicalFiles.Remove(MemMappedFileName);
					File.Delete(MemMappedFileName);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	/// <summary>
	/// Gets the random access viewer.
	/// Create a view that starts at 0 offset (begin) and ends approximately at the end of the memory-mapped file.
	/// </summary>
	/// <param name="fileAccess">
	/// The file Access.
	/// </param>
	/// <returns>
	/// Access to the mapped data
	/// </returns>
	public MemMapAccessor GetRandomAccessViewer(MemoryMappedFileAccess fileAccess)
	{
		return GetRandomAccessViewer(0L, 0L, fileAccess);
	}

	/// <summary>
	/// Gets the random access viewer.
	/// </summary>
	/// <param name="offset">The offset. The byte at which to start the view.</param>
	/// <param name="size">The size. The size of the view. Specify 0 (zero) to create a view that starts at offset and ends approximately at the end of the memory-mapped file.</param>
	/// <param name="fileAccess">The file access</param>
	/// <returns>Access to the mapped data</returns>
	public MemMapAccessor GetRandomAccessViewer(long offset, long size, MemoryMappedFileAccess fileAccess)
	{
		if (!IsOpenSucceed || !string.IsNullOrWhiteSpace(Errors) || _memoryMappedFile == null)
		{
			return null;
		}
		MemoryMappedViewAccessor viewAccessor = _memoryMappedFile.CreateViewAccessor(offset, size, fileAccess);
		long sizeOfView = _mmfInfo?.Length ?? 0;
		return new MemMapAccessor(this, _streamId, viewAccessor, offset, sizeOfView);
	}

	/// <summary>
	/// Gets the mapped file information.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="mmfInfo">store the mapped disk file information.</param>
	private static void GetMappedFileInformation(string fileName, out FileInfo mmfInfo)
	{
		mmfInfo = null;
		if (File.Exists(fileName))
		{
			mmfInfo = new FileInfo(fileName);
		}
	}

	/// <summary>
	/// Opens the exist memory mapped file.
	/// </summary>
	/// <param name="globalMapName">Name of the map.</param>
	/// <param name="localMapName">Name of the local map.</param>
	/// <param name="accessMode">memory mapped file rights</param>
	/// <param name="mmf">The MMF.</param>
	/// <returns>map name</returns>
	[SupportedOSPlatform("windows")]
	private static string OpenExistMemoryMappedFile(string globalMapName, string localMapName, DataFileAccessMode accessMode, ref MemoryMappedFile mmf)
	{
		MemoryMappedFileRights desiredAccessRights = (((accessMode & DataFileAccessMode.Write) == DataFileAccessMode.Write) ? MemoryMappedFileRights.ReadWrite : MemoryMappedFileRights.Read);
		try
		{
			mmf = MemoryMappedFile.OpenExisting(globalMapName, desiredAccessRights, HandleInheritability.None);
			return globalMapName;
		}
		catch (FileNotFoundException)
		{
			mmf = MemoryMappedFile.OpenExisting(localMapName, desiredAccessRights, HandleInheritability.None);
			return localMapName;
		}
	}

	/// <summary>
	/// Creates the memory mapped file from file.
	/// </summary>
	/// <param name="accessMode">memory map access mode</param>
	/// <param name="fileName">The file path.</param>
	/// <param name="globalMapName">Name of the MMF with Global prefix.</param>
	/// <param name="localMapName">Map name without the Global prefix</param>
	/// <param name="memoryMappedFile">A reference of a memory mapped file object</param>
	/// <returns>Memory mapped file name</returns>
	private string CreateMemoryMappedFileFromFile(DataFileAccessMode accessMode, string fileName, string globalMapName, string localMapName, ref MemoryMappedFile memoryMappedFile)
	{
		if (!File.Exists(fileName))
		{
			Errors += $"{Resources.ErrorInvalidFileName} : {fileName}";
			return null;
		}
		GetMappedFileInformation(fileName, out _mmfInfo);
		int numRetries = 5;
		int msToWaitBeforeRetry = 100;
		string[] array = new string[1] { string.Empty };
		if (_mmfInfo.Length == 0L)
		{
			throw new Exception(Resources.ErrorMapAZeroLenghtFile);
		}
		memoryMappedFile = Utilities.RetryMethod(delegate(string[] n)
		{
			FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			long length = fileStream.Length;
			if (length <= 0)
			{
				return (MemoryMappedFile)null;
			}
			MemoryMappedFile result;
			if ((accessMode & DataFileAccessMode.Global) == DataFileAccessMode.Global)
			{
				result = MemoryMappedFile.CreateFromFile(fileStream, Utilities.IsRunningUnderLinux.Value ? null : globalMapName, length, MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: false);
				n[0] = globalMapName;
			}
			else
			{
				result = MemoryMappedFile.CreateFromFile(fileStream, Utilities.IsRunningUnderLinux.Value ? null : localMapName, length, MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: false);
				n[0] = localMapName;
			}
			return result;
		}, array, numRetries, msToWaitBeforeRetry);
		return array[0];
	}

	/// <summary>
	/// Creates a non persisted memory mapped file.
	/// </summary>
	/// <param name="accessMode">Memory mapped file access mode</param>
	/// <param name="mapName">Name of the map file with the Global prefix.</param>
	/// <param name="localMapName">Name of the map file without the Global prefix</param>
	/// <param name="size">The size.</param>
	/// <param name="memoryMappedFile">Memory mapped file object.</param>
	/// <returns>Map name</returns>
	private string CreateNonPersistedMemoryMappedFile(DataFileAccessMode accessMode, string mapName, string localMapName, long size, ref MemoryMappedFile memoryMappedFile)
	{
		if (size == 0L)
		{
			throw new ArgumentException($"Cannot create a non persisted memory mapped file with zero size : {mapName}");
		}
		if (string.IsNullOrEmpty(mapName))
		{
			throw new ArgumentException("The memory mapped file name is blank");
		}
		bool flag = (accessMode & DataFileAccessMode.Global) == DataFileAccessMode.Global;
		try
		{
			if (flag)
			{
				memoryMappedFile = MemoryMappedFile.CreateNew(Utilities.IsRunningUnderLinux.Value ? null : mapName, size, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, HandleInheritability.Inheritable);
				return mapName;
			}
			memoryMappedFile = MemoryMappedFile.CreateNew(Utilities.IsRunningUnderLinux.Value ? null : localMapName, size, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, HandleInheritability.Inheritable);
			return localMapName;
		}
		catch (Exception)
		{
			memoryMappedFile = null;
			return string.Empty;
		}
	}
}
