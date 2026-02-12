using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
///     The trailer scan events.
/// </summary>
internal sealed class TrailerScanEvents : IRawObjectBase, IRealTimeAccess, IDisposable
{
	private readonly Guid _loaderId;

	private readonly IRunHeader _runHeader;

	private List<(int index, long startOffset, long endOffset)> _uniqueScanEventIndices = new List<(int, long, long)>();

	private (long, long)[] _eventAddressTable = Array.Empty<(long, long)>();

	private int[] _indexToUniqueEvents;

	private List<ScanEvent> _uniqueEvents;

	private bool _disposed;

	private IReadWriteAccessor _acqTrailScanEventViewer;

	private bool _realTimeMode;

	private SortedSet<ScanEvent> _scanEventsSet;

	private long _nextRecord;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the scan index count.
	/// </summary>
	internal int ScanIndexCount => _indexToUniqueEvents.Length;

	/// <summary>Gets the index to unique scan events.</summary>
	internal int[] IndexToUniqueScanEvents => _indexToUniqueEvents;

	/// <summary>Gets the unique scan event indices.</summary>
	internal List<(int index, long startOffset, long endOffset)> UniqueScanEventIndices => _uniqueScanEventIndices;

	internal (long, long)[] EventAddressTable => _eventAddressTable;

	/// <summary>
	/// Gets the unique events count.
	/// </summary>
	internal int UniqueEventsCount => _uniqueEvents.Count;

	/// <summary>
	/// Gets the unique events.
	/// </summary>
	internal IReadOnlyCollection<ScanEvent> UniqueEvents => _uniqueEvents;

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	public int FileRevision { get; }

	/// <summary>
	/// Gets the header file map name. <para />
	/// It's only meaningful in Generic data.
	/// </summary>
	public string HeaderFileMapName { get; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	public string DataFileMapName { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.TrailerScanEvents" /> class. 
	/// </summary>
	/// <param name="loaderId">
	/// The loader Id.
	/// </param>
	private TrailerScanEvents(Guid loaderId)
	{
		_loaderId = loaderId;
		_acqTrailScanEventViewer = null;
		_indexToUniqueEvents = Array.Empty<int>();
		_uniqueEvents = new List<ScanEvent>(0);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.TrailerScanEvents" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">Loader ID</param>
	/// <param name="runHeader">The run header.</param>
	public TrailerScanEvents(IViewCollectionManager manager, Guid loaderId, IRunHeader runHeader)
		: this(loaderId)
	{
		Manager = manager;
		_runHeader = runHeader;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.TrailerScanEvents" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">The loader ID</param>
	/// <param name="runHeader">The run header.</param>
	/// <param name="fileRevision">The file revision.</param>
	public TrailerScanEvents(IViewCollectionManager manager, Guid loaderId, IRunHeader runHeader, int fileRevision)
		: this(manager, loaderId, runHeader)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = string.Empty;
		DataFileMapName = runHeader.TrailerScanEventsFilename;
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		if (_runHeader == null || _runHeader.NumTrailerScanEvents == 0)
		{
			_indexToUniqueEvents = Array.Empty<int>();
			_uniqueEvents = new List<ScanEvent>(0);
			return 0L;
		}
		viewer.ReadIntExt(ref startPos);
		int numTrailerScanEvents = _runHeader.NumTrailerScanEvents;
		_eventAddressTable = new(long, long)[numTrailerScanEvents];
		int appendFrom;
		SortedSet<ScanEvent> scanEventsSet;
		long myStartPos;
		if (_realTimeMode && _scanEventsSet != null && _indexToUniqueEvents != null && _indexToUniqueEvents.Length != 0 && _uniqueEvents != null)
		{
			appendFrom = _indexToUniqueEvents.Length;
			Array.Resize(ref _indexToUniqueEvents, numTrailerScanEvents);
			scanEventsSet = _scanEventsSet;
			myStartPos = _nextRecord;
		}
		else
		{
			appendFrom = 0;
			_indexToUniqueEvents = new int[numTrailerScanEvents];
			_uniqueEvents = new List<ScanEvent>(numTrailerScanEvents);
			scanEventsSet = new SortedSet<ScanEvent>();
			if (_realTimeMode)
			{
				_scanEventsSet = scanEventsSet;
			}
			myStartPos = startPos;
		}
		if (ExportDeviceMetadata.ImportTrailScanEventIndicesInfo(viewer, out var uniqueScanEventIndicesBuffer, _indexToUniqueEvents))
		{
			UniqueScanEventInitializer(viewer, startPos, fileRevision, uniqueScanEventIndicesBuffer);
		}
		else if (numTrailerScanEvents - appendFrom < 20000)
		{
			int current = appendFrom;
			long offsetCounter = 0L;
			while (appendFrom < numTrailerScanEvents)
			{
				ScanEvent[] batchEvents = CreateEventBatch(viewer, fileRevision, ref myStartPos, ref appendFrom, numTrailerScanEvents, 2000, ref offsetCounter);
				current = UniqueScanEventInitializer(batchEvents, scanEventsSet, current, _uniqueEvents.Count, _uniqueScanEventIndices);
			}
		}
		else
		{
			BlockingCollection<ScanEvent[]> inputQueue = new BlockingCollection<ScanEvent[]>(20);
			Task task = Task.Run(delegate
			{
				int current2 = appendFrom;
				foreach (ScanEvent[] item in inputQueue.GetConsumingEnumerable())
				{
					current2 = UniqueScanEventInitializer(item, scanEventsSet, current2, _uniqueEvents.Count, _uniqueScanEventIndices);
				}
			});
			CreateInputQueue(inputQueue, viewer, fileRevision, myStartPos, appendFrom, numTrailerScanEvents);
			Task.WaitAll(task);
		}
		_nextRecord = myStartPos;
		return startPos - dataOffset;
	}

	/// <summary>Initialize the table of unique scan events.</summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="startPos">The start position.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="uniqueScanEventIndicesBuffer">the saved unique event indexes tables in byte array</param>
	private void UniqueScanEventInitializer(IMemoryReader viewer, long startPos, int fileRevision, byte[] uniqueScanEventIndicesBuffer)
	{
		int num = uniqueScanEventIndicesBuffer.Length / 20;
		ScanEvent[] array = new ScanEvent[num];
		BlockingCollection<(IMemoryReader, int, IReadOnlyCollection<(long offset, int index)>, ScanEvent[])> inputQueue = new BlockingCollection<(IMemoryReader, int, IReadOnlyCollection<(long, int)>, ScanEvent[])>(20);
		Task task = Task.Run(delegate
		{
			foreach (var item in inputQueue.GetConsumingEnumerable())
			{
				var (reader, fileRev, source, events) = item;
				Parallel.ForEach<(long, int)>(source, delegate((long offset, int index) item)
				{
					var (startPos2, num2) = item;
					events[num2] = reader.LoadRawFileObjectExt(() => new ScanEvent(_runHeader), fileRev, ref startPos2);
				});
			}
		});
		CreateInputQueue(inputQueue, viewer, startPos, fileRevision, num, 16777216, 20, array, uniqueScanEventIndicesBuffer);
		Task.WaitAll(task);
		_uniqueEvents = array.ToList();
	}

	private void CreateInputQueue(BlockingCollection<(IMemoryReader, int, IReadOnlyCollection<(long offset, int index)>, ScanEvent[])> inputQueue, IMemoryReader viewer, long startPos, int fileRevision, int totalUniqueScanEventCount, int memoryBlockSize, int uniqueEventRecordSize, ScanEvent[] uniqueEvents, byte[] uniqueScanEventIndicesBuffer)
	{
		int i = 0;
		long myStartPos = startPos;
		long num = 0L;
		long num2 = 0L;
		int num3 = 0;
		List<(long, int)> list = new List<(long, int)>(29000);
		_uniqueScanEventIndices = new List<(int, long, long)>(totalUniqueScanEventCount);
		for (; i < totalUniqueScanEventCount; i++)
		{
			list.Clear();
			long num4 = 0L;
			for (; i < totalUniqueScanEventCount; i++)
			{
				int item = BitConverter.ToInt32(uniqueScanEventIndicesBuffer, num3);
				num3 += 4;
				int num5 = BitConverter.ToInt32(uniqueScanEventIndicesBuffer, num3);
				num3 += 8;
				int num6 = BitConverter.ToInt32(uniqueScanEventIndicesBuffer, num3);
				num3 += 8;
				if (num6 - num >= memoryBlockSize)
				{
					i--;
					num3 -= uniqueEventRecordSize;
					num2 = num5;
					break;
				}
				list.Add((num5 - num, i));
				num4 = num6;
				_uniqueScanEventIndices.Add((item, num5, num6));
			}
			bool moreDataAvailable;
			MemoryArrayReader item2 = CreateSubView(viewer, myStartPos, (int)(num4 - num), out moreDataAvailable);
			inputQueue.Add((item2, fileRevision, (IReadOnlyCollection<(long, int)>)(object)list.ToArray(), uniqueEvents));
			num = num2;
			myStartPos = startPos + num;
		}
		inputQueue.CompleteAdding();
	}

	private int UniqueScanEventInitializer(ScanEvent[] batchEvents, SortedSet<ScanEvent> scanEventsSet, int current, int pos, List<(int index, long startOffset, long endOffset)> uniqueQueueScanEventPos)
	{
		foreach (ScanEvent scanEvent in batchEvents)
		{
			scanEvent.UniqueScanEventIndex = pos;
			if (scanEventsSet.Add(scanEvent))
			{
				uniqueQueueScanEventPos.Add((current, _eventAddressTable[current].Item1, _eventAddressTable[current].Item2));
				_uniqueEvents.Add(scanEvent);
				_indexToUniqueEvents[current] = pos;
				pos = _uniqueEvents.Count;
			}
			else
			{
				_indexToUniqueEvents[current] = scanEvent.UniqueScanEventIndex;
			}
			current++;
		}
		return current;
	}

	/// <summary>
	/// Decode scan events and add to a queue to be sorted
	/// </summary>
	/// <param name="inputQueue"></param>
	/// <param name="viewer"></param>
	/// <param name="fileRevision"></param>
	/// <param name="myStartPos"></param>
	/// <param name="appendFrom"></param>
	/// <param name="numTse"></param>
	private void CreateInputQueue(BlockingCollection<ScanEvent[]> inputQueue, IMemoryReader viewer, int fileRevision, long myStartPos, int appendFrom, int numTse)
	{
		long offsetCounter = 0L;
		while (appendFrom < numTse)
		{
			ScanEvent[] item = CreateEventBatch(viewer, fileRevision, ref myStartPos, ref appendFrom, numTse, 2000, ref offsetCounter);
			inputQueue.Add(item);
		}
		inputQueue.CompleteAdding();
	}

	/// <summary>Creates the event batch.</summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="myStartPos">My start position.</param>
	/// <param name="appendFrom">The append from.</param>
	/// <param name="numTse">The number tse.</param>
	/// <param name="batch">The batch.</param>
	/// <param name="offsetCounter"></param>
	/// <returns>
	///   <br />
	/// </returns>
	private ScanEvent[] CreateEventBatch(IMemoryReader viewer, int fileRevision, ref long myStartPos, ref int appendFrom, int numTse, int batch, ref long offsetCounter)
	{
		int num = appendFrom + batch;
		if (num > numTse)
		{
			num = numTse;
		}
		ScanEvent[] array = new ScanEvent[num - appendFrom];
		bool moreDataAvailable;
		MemoryArrayReader memoryArrayReader = CreateSubView(viewer, myStartPos, 1048576, out moreDataAvailable);
		long startPos = 0L;
		int num2 = 0;
		while (appendFrom < num)
		{
			if (moreDataAvailable && startPos + 10240 > memoryArrayReader.Length)
			{
				myStartPos += startPos;
				offsetCounter += startPos;
				memoryArrayReader = CreateSubView(viewer, myStartPos, 1048576, out moreDataAvailable);
				startPos = 0L;
			}
			long item = offsetCounter + startPos;
			array[num2] = memoryArrayReader.LoadRawFileObjectExt(() => new ScanEvent(_runHeader), fileRevision, ref startPos);
			_eventAddressTable[appendFrom] = (item, offsetCounter + startPos);
			appendFrom++;
			num2++;
		}
		offsetCounter += startPos;
		myStartPos += startPos;
		return array;
	}

	/// <summary>
	/// Read a chunk of bytes into a memory array, so
	/// that subsequent reads of small data items can be performed against memory
	/// reducing OS calls to "read from files" or other storage.
	/// </summary>
	/// <param name="viewer">The viewer into the main file</param>
	/// <param name="myStartPos">Offset into the main file for this view</param>
	/// <param name="memoryBuffer">Requested buffer size</param>
	/// <param name="moreDataAvailable">set to true if there is more data in the main view after this buffer</param>
	/// <returns>A block of memory</returns>
	private static MemoryArrayReader CreateSubView(IMemoryReader viewer, long myStartPos, int memoryBuffer, out bool moreDataAvailable)
	{
		long num = viewer.Length - myStartPos;
		moreDataAvailable = num > memoryBuffer * 5 / 4;
		if (moreDataAvailable)
		{
			return new MemoryArrayReader(viewer.ReadBytes(myStartPos, memoryBuffer), 0L);
		}
		return new MemoryArrayReader(viewer.ReadBytes(myStartPos, (int)num), 0L);
	}

	/// <summary>
	/// The get event.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" />.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// Thrown if the index is out of range.
	/// </exception>
	internal ScanEvent GetEvent(int index)
	{
		return _uniqueEvents[GetUniqueEventsIndex(index)];
	}

	/// <summary>
	/// Gets the index of the unique events.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>The index</returns>
	internal int GetUniqueEventsIndex(int index)
	{
		if (index < 0 || index >= _indexToUniqueEvents.Length)
		{
			return -1;
		}
		return _indexToUniqueEvents[index];
	}

	/// <summary>
	/// load the events
	/// </summary>
	/// <param name="scanEvents">
	/// The scan events.
	/// </param>
	internal void Load(ScanEvent[] scanEvents)
	{
		if (_runHeader == null || _runHeader.NumTrailerScanEvents == 0)
		{
			_indexToUniqueEvents = Array.Empty<int>();
			_uniqueEvents = new List<ScanEvent>(0);
			return;
		}
		int numTrailerScanEvents = _runHeader.NumTrailerScanEvents;
		_indexToUniqueEvents = new int[numTrailerScanEvents];
		_uniqueEvents = new List<ScanEvent>(numTrailerScanEvents);
		SortedSet<ScanEvent> sortedSet = new SortedSet<ScanEvent>();
		for (int i = 0; i < numTrailerScanEvents; i++)
		{
			int count = _uniqueEvents.Count;
			ScanEvent scanEvent = scanEvents[i];
			scanEvent.UniqueScanEventIndex = count;
			if (sortedSet.Add(scanEvent))
			{
				_uniqueEvents.Add(scanEvent);
				_indexToUniqueEvents[i] = count;
			}
			else
			{
				_indexToUniqueEvents[i] = scanEvent.UniqueScanEventIndex;
			}
		}
	}

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool RefreshViewOfFile()
	{
		_realTimeMode = true;
		try
		{
			_acqTrailScanEventViewer = _acqTrailScanEventViewer.GetMemoryMappedViewer(_loaderId, DataFileMapName, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
			if (_acqTrailScanEventViewer != null)
			{
				Load(_acqTrailScanEventViewer, 0L, FileRevision);
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
	/// dispose of object.
	/// </summary>
	/// <param name="disposing">
	/// true if disposing.
	/// </param>
	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_acqTrailScanEventViewer.ReleaseAndCloseMemoryMappedFile(Manager);
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
