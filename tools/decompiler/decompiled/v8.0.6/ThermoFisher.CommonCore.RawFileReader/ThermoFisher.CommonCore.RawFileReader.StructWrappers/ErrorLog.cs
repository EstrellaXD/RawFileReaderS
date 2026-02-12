using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The error log.
/// </summary>
internal class ErrorLog : List<ErrorLogItem>, IErrorLog, IRealTimeAccess, IDisposable, IRawObjectBase
{
	private readonly Guid _loaderId;

	private readonly IRunHeader _runHeader;

	private bool _disposed;

	private IReadWriteAccessor _acqDataViewer;

	private int _logEntriesLoaded;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	public int FileRevision { get; }

	/// <summary>
	/// Gets the header file map name.
	/// </summary>
	public string HeaderFileMapName { get; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	public string DataFileMapName { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.ErrorLog" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	public ErrorLog(IViewCollectionManager manager, Guid loaderId)
	{
		Manager = manager;
		_acqDataViewer = null;
		_loaderId = loaderId;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.ErrorLog" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	/// <param name="runHeader">
	/// The run header.
	/// </param>
	public ErrorLog(IViewCollectionManager manager, Guid loaderId, IRunHeader runHeader)
		: this(manager, loaderId)
	{
		_runHeader = runHeader;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.ErrorLog" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <param name="runHeader">
	/// The run header.
	/// </param>
	public ErrorLog(IViewCollectionManager manager, Guid loaderId, int fileRevision, IRunHeader runHeader)
		: this(manager, loaderId, runHeader)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = string.Empty;
		DataFileMapName = runHeader.ErrorLogFilename;
	}

	/// <summary>
	/// The get item.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.ErrorLogItem" />.
	/// </returns>
	public ErrorLogItem GetItem(int index)
	{
		return base[index];
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">The viewer (memory map into file).</param>
	/// <param name="dataOffset">The data offset (into the memory map).</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		int numErrorLog = _runHeader.NumErrorLog;
		_logEntriesLoaded = numErrorLog;
		Clear();
		viewer.ReadIntExt(ref startPos);
		if (numErrorLog == 0)
		{
			return startPos - dataOffset;
		}
		int reachReloadLimit = 1046528;
		long byteCounter = 0L;
		int index = 0;
		bool preferLargeReads = viewer.PreferLargeReads;
		do
		{
			if (!preferLargeReads)
			{
				GetErrorLogsData(viewer, startPos + byteCounter);
			}
			else
			{
				Utilities.LoadDataFromInternalMemoryArrayReader(GetErrorLogsData, viewer, startPos + byteCounter, 1048576);
			}
		}
		while (index < numErrorLog);
		startPos = (viewer.PreferLargeReads ? (startPos + byteCounter) : byteCounter);
		Sort();
		return startPos - dataOffset;
		long GetErrorLogsData(IMemoryReader reader, long offset)
		{
			for (; index < numErrorLog; index++)
			{
				float num = reader.ReadFloatExt(ref offset);
				string text = reader.ReadStringExt(ref offset);
				Add(new ErrorLogItem(num, text));
				if (preferLargeReads && offset >= reachReloadLimit)
				{
					index++;
					break;
				}
			}
			byteCounter = (preferLargeReads ? (byteCounter + offset) : offset);
			return byteCounter;
		}
	}

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>True refresh succeed, false otherwise. </returns>
	public bool RefreshViewOfFile()
	{
		if (_logEntriesLoaded >= _runHeader.NumErrorLog)
		{
			return true;
		}
		bool result = false;
		try
		{
			string streamId = StreamHelper.ConstructStreamId(_loaderId, DataFileMapName);
			_acqDataViewer = _acqDataViewer.GetMemoryMappedViewer(_loaderId, DataFileMapName, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
			if (_acqDataViewer != null)
			{
				Load(_acqDataViewer, 0L, FileRevision);
				return true;
			}
			result = MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(streamId);
		}
		catch (Exception)
		{
		}
		return result;
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_acqDataViewer.ReleaseAndCloseMemoryMappedFile(Manager);
		}
	}
}
