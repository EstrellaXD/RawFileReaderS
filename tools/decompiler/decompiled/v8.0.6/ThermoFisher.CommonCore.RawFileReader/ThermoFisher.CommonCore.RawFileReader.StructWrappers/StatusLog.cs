using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The status log is a list of label value blobs indexed by retention time.
/// </summary>
internal sealed class StatusLog : IStatusLog, IRealTimeAccess, IDisposable, IRawObjectBase
{
	/// <summary>
	/// An internal representation of log entry.
	/// </summary>
	private class LocalLogEntry : IStatusLogEntry
	{
		/// <summary>
		/// Gets or sets the time.
		/// </summary>
		public float Time { get; set; }

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		public object[] Values { get; set; }
	}

	private readonly SortedStatusLogCollection _sortedStatusLogCollection;

	private readonly Guid _loaderId;

	private readonly ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader _runHeader;

	private bool _disposed;

	private IReadWriteAccessor _acqDataViewer;

	private IReadWriteAccessor _acqHeaderViewer;

	private bool _descriptorsLoaded;

	private int _loadedRecords;

	private ILogDecoder _decoder;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the count.
	/// </summary>
	public int Count => _sortedStatusLogCollection.Count;

	/// <summary>
	/// Gets or sets the data descriptors.
	/// </summary>
	public DataDescriptors DataDescriptors { get; internal set; }

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
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.StatusLog" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	public StatusLog(IViewCollectionManager manager, Guid loaderId)
	{
		Manager = manager;
		_sortedStatusLogCollection = new SortedStatusLogCollection();
		_acqDataViewer = null;
		_acqHeaderViewer = null;
		_loaderId = loaderId;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.StatusLog" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	/// <param name="runHeader">
	/// The run header.
	/// </param>
	public StatusLog(IViewCollectionManager manager, Guid loaderId, ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader runHeader)
		: this(manager, loaderId)
	{
		_runHeader = runHeader;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.StatusLog" /> class.
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
	public StatusLog(IViewCollectionManager manager, Guid loaderId, int fileRevision, ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader runHeader)
		: this(manager, loaderId, runHeader)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = runHeader.StatusLogHeaderFilename;
		DataFileMapName = runHeader.StatusLogFilename;
	}

	/// <summary>
	/// The method performs a binary search to find the status entry that is closest to the given retention time.
	/// </summary>
	/// <param name="retentionTime">
	/// The retention time.
	/// </param>
	/// <returns>
	/// The status entry containing the <see cref="T:System.Collections.Generic.List`1" /> of label value pairs for the retention time.
	/// If there are no entries in the log, it will an empty list.
	/// </returns>
	public StatusLogEntry GetItem(double retentionTime)
	{
		return _sortedStatusLogCollection.GetItem(retentionTime);
	}

	/// <summary>
	/// The method gets all the log entries' value pair at the specified index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The all the log entries' value pair at the specified index.
	/// </returns>
	public List<StatusLogEntry> GetItemValues(int index)
	{
		return _sortedStatusLogCollection.GetItemValues(index);
	}

	/// <summary>
	/// get (raw) status log entry by index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IStatusLogEntry" />.
	/// </returns>
	public IStatusLogEntry GetStatusLogEntryByIndex(int index)
	{
		Tuple<float, LabelValueBlob> blobEntry = _sortedStatusLogCollection.GetBlobEntry(index);
		return new LocalLogEntry
		{
			Time = blobEntry.Item1,
			Values = blobEntry.Item2.GetAllValues(_decoder)
		};
	}

	/// <summary>
	/// get (raw) status log entry by index into the sorted (data validated) collection
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IStatusLogEntry" />.
	/// </returns>
	public IStatusLogEntry GetSortedStatusLogEntryByIndex(int index)
	{
		Tuple<float, LabelValueBlob> sortedBlobEntry = _sortedStatusLogCollection.GetSortedBlobEntry(index);
		return new LocalLogEntry
		{
			Time = sortedBlobEntry.Item1,
			Values = sortedBlobEntry.Item2.GetAllValues(_decoder)
		};
	}

	/// <summary>
	/// Gets the (raw) status log entry by retention time.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <returns>The log at the given time.</returns>
	public IStatusLogEntry GetStatusLogEntryByRetentionTime(double retentionTime)
	{
		Tuple<float, LabelValueBlob> blobEntry = _sortedStatusLogCollection.GetBlobEntry(retentionTime);
		return new LocalLogEntry
		{
			Time = blobEntry.Item1,
			Values = ((blobEntry.Item2 == null) ? Array.Empty<object>() : blobEntry.Item2.GetAllValues(_decoder))
		};
	}

	/// <summary>
	/// Gets the labels and index positions of the status log items which may be plotted.
	/// That is, the numeric items.
	/// Labels names are returned by "Key" and the index into the log is "Value".
	/// </summary>
	/// <returns>
	/// The items which can be plotted.
	/// </returns>
	public IEnumerable<KeyValuePair<string, int>> StatusLogPlottableData()
	{
		if (DataDescriptors == null || DataDescriptors.Count == 0)
		{
			yield break;
		}
		int numFields = DataDescriptors.Count;
		for (int index = 0; index < numFields; index++)
		{
			DataDescriptor dataDescriptor = DataDescriptors[index];
			DataTypes dataType = dataDescriptor.DataType;
			if (dataType == DataTypes.Char || (uint)(dataType - 5) <= 6u)
			{
				yield return new KeyValuePair<string, int>(dataDescriptor.Label, index);
			}
		}
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
		long num = dataOffset;
		num += LoadDataDescriptors(viewer, num, fileRevision);
		if (DataDescriptors.Count == 0)
		{
			return num - dataOffset;
		}
		num += LoadStatusLogEntries(viewer, num, fileRevision);
		return num - dataOffset;
	}

	/// <summary>
	/// Loads the data descriptors.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>True load data succeed, false otherwise</returns>
	private long LoadDataDescriptors(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		DataDescriptors = viewer.LoadRawFileObjectExt(() => new DataDescriptors(0), fileRevision, ref startPos);
		_descriptorsLoaded = true;
		return startPos - dataOffset;
	}

	/// <summary>
	/// Gets the status log for a given index into the set of logs.
	/// This returns the log and it's time stamp.
	/// </summary>
	/// <param name="index">Index into table of logs (from 0 to RunHeader.Number of StatusLog - 1)</param>
	/// <returns>The log values for the given index</returns>
	public StatusLogEntry GetStatusRecordByIndex(int index)
	{
		int numStatusLog = _runHeader.NumStatusLog;
		if (index < 0 || index >= numStatusLog)
		{
			throw new ArgumentException("index out of range");
		}
		return _sortedStatusLogCollection.GetItem(index);
	}

	/// <summary>
	/// The method calls <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemoryMappedRawFileManager" /> to get an <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IReadWriteAccessor" />
	/// object for the memory mapped view stream that represents the status log's "blob". It will then
	/// divide the blob into individual status log entries without actually reading in the status log items.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>True load status log entries succeed, false otherwise</returns>
	internal long LoadStatusLogEntries(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long num = dataOffset;
		int numStatusLogs = _runHeader.NumStatusLog;
		_loadedRecords = numStatusLogs;
		_sortedStatusLogCollection.Clear();
		if (numStatusLogs <= 0)
		{
			return 0L;
		}
		uint logEntrySize = DataDescriptors.TotalDataSize;
		long num2 = (logEntrySize + 4) * numStatusLogs;
		num = (viewer.PreferLargeReads ? Utilities.LoadDataFromInternalMemoryArrayReader(GetStatusLogsData, viewer, num, (int)num2) : GetStatusLogsData(viewer, num));
		_sortedStatusLogCollection.SortKeys();
		return num - dataOffset;
		long GetStatusLogsData(IMemoryReader reader, long offset)
		{
			_decoder = new LogDecoder(reader, DataDescriptors);
			_sortedStatusLogCollection.Decoder = _decoder;
			for (int i = 0; i < numStatusLogs; i++)
			{
				float retentionTime = reader.ReadFloatExt(ref offset);
				LabelValueBlob statusBlob = new LabelValueBlob(offset);
				_sortedStatusLogCollection.AddStatusBlob(retentionTime, statusBlob, i == numStatusLogs - 1);
				offset += logEntrySize;
			}
			return offset;
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
		bool flag = _loadedRecords == 0 && !_descriptorsLoaded;
		bool flag2 = _runHeader.NumStatusLog > _loadedRecords;
		if (!(flag2 || flag))
		{
			return true;
		}
		try
		{
			if (flag)
			{
				_acqHeaderViewer = _acqHeaderViewer.GetMemoryMappedViewer(_loaderId, HeaderFileMapName, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
				if (_acqHeaderViewer != null)
				{
					LoadDataDescriptors(_acqHeaderViewer, 0L, FileRevision);
				}
				else if (MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(_loaderId, HeaderFileMapName)))
				{
					return true;
				}
			}
			int num;
			if (_descriptorsLoaded)
			{
				DataDescriptors dataDescriptors = DataDescriptors;
				num = ((dataDescriptors != null && dataDescriptors.Count > 0) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool flag3 = (byte)num != 0;
			if (!(flag3 && flag2))
			{
				if (!flag3)
				{
					_sortedStatusLogCollection.Clear();
				}
				return true;
			}
			_acqDataViewer?.ReleaseAndCloseMemoryMappedFile(Manager);
			_acqDataViewer = _acqDataViewer.GetMemoryMappedViewer(_loaderId, DataFileMapName, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
			if (_acqDataViewer != null)
			{
				LoadStatusLogEntries(_acqDataViewer, 0L, FileRevision);
				return true;
			}
			if (MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(_loaderId, DataFileMapName)))
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	/// <summary>
	/// Releases unmanaged and - optionally - managed resources.
	/// </summary>
	/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_acqHeaderViewer?.ReleaseAndCloseMemoryMappedFile(Manager);
				_acqDataViewer?.ReleaseAndCloseMemoryMappedFile(Manager);
				_sortedStatusLogCollection?.Dispose();
			}
			_disposed = true;
		}
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
