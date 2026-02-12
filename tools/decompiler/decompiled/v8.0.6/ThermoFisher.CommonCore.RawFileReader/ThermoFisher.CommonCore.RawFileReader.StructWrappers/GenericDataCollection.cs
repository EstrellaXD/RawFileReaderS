using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
///     The generic data collection. The generic data collection has two parts. The data descriptors and the generic
///     data. The class creates a memory mapped stream view of the data.
/// </summary>
internal sealed class GenericDataCollection : IRawObjectBase, IDisposable, IRealTimeAccess, IRecordRangeProvider
{
	private readonly int _numberOfElements;

	private readonly long _newMmfOffset;

	private readonly Guid _loaderId;

	private bool _disposed;

	private IReadWriteAccessor _acqHeaderViewer;

	private IReadWriteAccessor _acqDataViewer;

	private LabelValueBlob[] _blobs = Array.Empty<LabelValueBlob>();

	private IReadWriteAccessor _genDataViewer;

	private bool _dataDescriptorsLoaded;

	private bool _usesRecordBuffer;

	private RecordBufferManager _bufferManager;

	private bool _supportCashedLogs;

	private int _numberOfEntries;

	private int _logEntrySize;

	public IViewCollectionManager Manager { get; }

	public bool ValidateReads { get; private set; }

	/// <summary>
	///     Gets the data descriptors.
	/// </summary>
	public DataDescriptors DataDescriptors { get; private set; }

	/// <summary>
	///     Gets the total size of the data collection. In the case where the data descriptors and the
	///     data collections are written in contiguous blocks, the container will have to update its viewer
	///     position because the data entries may not have been read.
	/// </summary>
	public long TotalSize { get; private set; }

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

	public ILogDecoder Decoder { get; private set; }

	/// <summary>
	/// Gets the number of elements function.
	/// </summary>
	public Func<int> NumElementsFunc { get; }

	public int Count => _numberOfEntries;

	public LabelValueBlob this[int index]
	{
		get
		{
			if (_supportCashedLogs)
			{
				return _blobs[index];
			}
			return new LabelValueBlob(index * _logEntrySize);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericDataCollection" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	private GenericDataCollection(IViewCollectionManager manager, Guid loaderId)
	{
		Manager = manager;
		_acqHeaderViewer = null;
		_acqDataViewer = null;
		_loaderId = loaderId;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericDataCollection" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	/// <param name="dataDescriptors">
	/// The data descriptors.
	/// </param>
	public GenericDataCollection(IViewCollectionManager manager, Guid loaderId, DataDescriptors dataDescriptors)
		: this(manager, loaderId)
	{
		DataDescriptors = dataDescriptors;
		_numberOfElements = dataDescriptors.Count;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericDataCollection" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">Loader ID</param>
	/// <param name="headerMapFileName">Name of the header map file.</param>
	/// <param name="dataMapFileName">Name of the data map file.</param>
	/// <param name="numDataFuc">The number data.</param>
	/// <param name="fileRevision">The file revision.</param>
	public GenericDataCollection(IViewCollectionManager manager, Guid loaderId, string headerMapFileName, string dataMapFileName, Func<int> numDataFuc, int fileRevision)
		: this(manager, loaderId)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = headerMapFileName;
		DataFileMapName = dataMapFileName;
		NumElementsFunc = numDataFuc;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericDataCollection" /> class.
	/// <remarks>
	/// Due to the fact that some generic data collection's descriptors and data are not stored in contiguous
	/// blocks, we cannot automatically set the memory mapped stream's current position (e.g. the MS device's
	/// Trailer data's descriptor is written before the tune data and the trailer data themselves are written after
	/// the trailer scan events).
	/// </remarks>
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">The loader ID</param>
	/// <param name="numberOfElements">The number of elements.</param>
	/// <param name="newMmfOffset">The new MMF offset.</param>
	public GenericDataCollection(IViewCollectionManager manager, Guid loaderId, int numberOfElements, long newMmfOffset = -1L)
		: this(manager, loaderId)
	{
		_numberOfElements = numberOfElements;
		_newMmfOffset = newMmfOffset;
	}

	/// <summary>
	/// The method loads the generic collection from file by getting the descriptors and setting
	///     up the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValueBlob" /> structures.
	/// </summary>
	/// <param name="viewer">
	/// The view stream.
	/// </param>
	/// <param name="dataOffset">
	/// The starting position of the collection - this is used to create a local memory mapped stream.
	/// </param>
	/// <param name="fileRevision">
	/// The number of elements.
	/// </param>
	/// <returns>
	/// The number of bytes loaded.
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long num = dataOffset;
		num += LoadDataDescriptors(viewer, num, fileRevision);
		if (DataDescriptors.Count == 0)
		{
			return num - dataOffset;
		}
		if (_newMmfOffset > -1)
		{
			LoadGenericDataEntries(viewer, _newMmfOffset, _numberOfElements);
		}
		else
		{
			LoadGenericDataEntries(viewer, num + viewer.InitialOffset, _numberOfElements);
		}
		return num - dataOffset;
	}

	/// <summary>
	/// load data descriptors.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes loaded.
	/// </returns>
	private long LoadDataDescriptors(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		DataDescriptors = viewer.LoadRawFileObjectExt(() => new DataDescriptors(0), fileRevision, ref startPos);
		_dataDescriptorsLoaded = true;
		return startPos - dataOffset;
	}

	/// <summary>
	/// The method calls <see cref="T:ThermoFisher.CommonCore.RawFileReader.Readers.MemoryMappedRawFileManager" /> to get an <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IReadWriteAccessor" />
	///     object for the memory mapped view stream that represents the status log's "blob". It will then
	///     divide the blob into individual status log entries without actually reading in the status log items.
	/// </summary>
	/// <param name="viewer">View into memory map
	/// </param>
	/// <param name="dataOffset">
	/// The starting dataOffset of the collection - this is used to create a local memory mapped stream.
	/// </param>
	/// <param name="numberOfEntries">
	/// The number of log entries.
	/// </param>
	/// <exception cref="T:System.Exception">
	/// Thrown if there is a problem getting the binary collectionStreamViewer for the memory mapped view stream.
	/// </exception>
	/// <returns>
	/// The position after all the records
	/// </returns>
	public long LoadGenericDataEntries(IMemoryReader viewer, long dataOffset, int numberOfEntries)
	{
		_numberOfEntries = numberOfEntries;
		long num = 0L;
		_logEntrySize = (int)DataDescriptors.TotalDataSize;
		TotalSize = _logEntrySize * numberOfEntries;
		_supportCashedLogs = TotalSize < 10485760;
		try
		{
			if (numberOfEntries == 0)
			{
				return num;
			}
			if (viewer is MemoryArrayAccessor reader)
			{
				Decoder = new LogDecoder(reader, DataDescriptors);
			}
			else
			{
				_genDataViewer = Manager.GetRandomAccessViewer(Guid.Empty, viewer.StreamId, dataOffset, TotalSize, inAcquisition: false, DataFileAccessMode.OpenCreateRead | DataFileAccessMode.PermitMissingData);
				IReaderIssues readerIssues = _genDataViewer.ReaderIssues();
				ValidateReads = readerIssues.FileSizeExceeded;
				IReadWriteAccessor genDataViewer = _genDataViewer;
				if (genDataViewer != null && genDataViewer.PreferLargeReads)
				{
					_usesRecordBuffer = true;
					_bufferManager = new RecordBufferManager(_genDataViewer, _logEntrySize * numberOfEntries, 0, numberOfEntries - 1, this, zeroBased: true);
				}
				Decoder = (_usesRecordBuffer ? new LogDecoder(_bufferManager, _logEntrySize, DataDescriptors) : new LogDecoder(_genDataViewer, DataDescriptors));
			}
		}
		catch (Exception ex)
		{
			throw new Exception($"Cannot not get MMF view stream for '{viewer.StreamId}'. Offset = {dataOffset} Size={TotalSize}.\n{ex.Message}");
		}
		if (_supportCashedLogs)
		{
			_blobs = new LabelValueBlob[numberOfEntries];
			for (int i = 0; i < numberOfEntries; i++)
			{
				CachedLabelValueBlob cachedLabelValueBlob = new CachedLabelValueBlob(num);
				_blobs[i] = cachedLabelValueBlob;
				num += _logEntrySize;
			}
		}
		else
		{
			num += _logEntrySize * _numberOfEntries;
		}
		return num;
	}

	/// <summary>
	/// remove all data.
	/// </summary>
	private void RemoveAll()
	{
		_blobs = Array.Empty<LabelValueBlob>();
		_genDataViewer?.ReleaseAndCloseMemoryMappedFile(Manager);
		_genDataViewer = null;
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_acqHeaderViewer?.ReleaseAndCloseMemoryMappedFile(Manager);
			RemoveAll();
			_acqDataViewer?.ReleaseAndCloseMemoryMappedFile(Manager);
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
		try
		{
			if (!_dataDescriptorsLoaded)
			{
				_acqHeaderViewer = _acqHeaderViewer.GetMemoryMappedViewer(_loaderId, HeaderFileMapName, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
				if (_acqHeaderViewer == null)
				{
					return MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(_loaderId, HeaderFileMapName));
				}
				LoadDataDescriptors(_acqHeaderViewer, 0L, FileRevision);
			}
			RemoveAll();
			if (DataDescriptors.Count > 0)
			{
				_acqDataViewer = _acqDataViewer.GetMemoryMappedViewer(_loaderId, DataFileMapName, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
				if (_acqDataViewer != null)
				{
					LoadGenericDataEntries(_acqDataViewer, 0L, NumElementsFunc());
					return true;
				}
				return MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(_loaderId, DataFileMapName));
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public IMemoryReader CreateSubRangeReader(int firstRecord, int lastRecord)
	{
		int totalDataSize = (int)DataDescriptors.TotalDataSize;
		int num = firstRecord * totalDataSize;
		return new MemoryArrayReader(_genDataViewer.ReadBytes(num, (lastRecord - firstRecord + 1) * totalDataSize), num);
	}
}
