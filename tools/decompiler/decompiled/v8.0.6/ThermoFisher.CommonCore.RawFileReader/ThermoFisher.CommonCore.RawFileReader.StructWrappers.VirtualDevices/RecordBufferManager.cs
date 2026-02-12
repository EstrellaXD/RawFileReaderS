using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// sliding window record buffer manager
/// </summary>
internal class RecordBufferManager : IDisposable
{
	private static readonly RecordRangeReader _emptyReader = new RecordRangeReader
	{
		StartRecord = -1,
		EndRecord = -1
	};

	private readonly RecordRangeReader[] _readers = new RecordRangeReader[3] { _emptyReader, _emptyReader, _emptyReader };

	private SpinLock _spinLock;

	private readonly int _recordsPerBatch;

	private readonly IRecordRangeProvider _managedDevice;

	private readonly IReadWriteAccessor _accessor;

	private readonly int _lastRecord;

	private readonly IMemoryReader _allDataReader;

	private readonly Queue<int> _recentBufferStarts = new Queue<int>(5);

	private Task<RecordRangeReader> _lookahead;

	/// <summary>
	/// Gets a value indicating whether there is only one buffer for the entire data.
	/// This may be set for smaller files with only "a few MB" of data.
	/// </summary>
	internal bool SingleBuffer { get; }

	/// <summary>
	/// Gets a value indicating whether are many buffers for the entire data.
	/// </summary>
	internal bool MultiBuffer { get; }

	/// <summary>
	/// Gets or sets a value indicating whether record numbers start from zero.
	/// For example: MS scans start from 1.
	/// </summary>
	internal bool ZeroBased { get; }

	/// <summary>
	/// Gets the number of bytes which can be read from this data area.
	/// This may not be the length of the containing file or other object.
	/// </summary>
	internal long Available { get; }

	private int MinRecord => (!ZeroBased) ? 1 : 0;

	internal int RecordsPerBatch => _recordsPerBatch;

	/// <summary>
	/// Initialize a new buffer manager for a specific record collection.
	/// </summary>
	/// <param name="accessor">Where the records are located</param>
	/// <param name="length">total bytes in all records </param>
	/// <param name="firstRecord">First record number</param>
	/// <param name="lastRecord">last record number</param>
	/// <param name="managed">The object managed by this tool, which can provide readers for batches of records</param>
	/// <param name="zeroBased">Set if the first valid record number is 0</param>
	/// <param name="singleBufferPermitted">set if the data may be held as a single buffer</param>
	public RecordBufferManager(IReadWriteAccessor accessor, long length, int firstRecord, int lastRecord, IRecordRangeProvider managed, bool zeroBased = false, bool singleBufferPermitted = false)
	{
		_managedDevice = managed;
		_accessor = accessor;
		_lastRecord = lastRecord;
		ZeroBased = zeroBased;
		Available = accessor.Length - accessor.InitialOffset;
		if (accessor.PreferLargeReads && _lastRecord >= firstRecord && firstRecord >= MinRecord)
		{
			int num = 4 * accessor.SuggestedChunkSize;
			if (singleBufferPermitted && length <= num)
			{
				_allDataReader = managed.CreateSubRangeReader(firstRecord, lastRecord);
				SingleBuffer = true;
			}
			else
			{
				_recordsPerBatch = FindChunkLength(length, firstRecord, lastRecord);
				MultiBuffer = _recordsPerBatch > 0;
			}
		}
	}

	/// <summary>
	/// Find the number of records per buffer
	/// </summary>
	/// <param name="allRecordsLength">byte length of all records</param>
	/// <param name="first">first record number in file</param>
	/// <param name="last">last record number in file</param>
	/// <returns>The number of records to hold in each buffer</returns>
	private int FindChunkLength(long allRecordsLength, int first, int last)
	{
		int suggestedChunkSize = _accessor.SuggestedChunkSize;
		if (allRecordsLength > 0 && suggestedChunkSize > 0 && first >= MinRecord && last > first)
		{
			int num = last - first + 1;
			long num2 = allRecordsLength / num;
			if (num2 > 0)
			{
				int num3 = (int)Math.Min(suggestedChunkSize / num2, 50000L);
				if (num3 >= 10)
				{
					return num3;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Get a reader which can return data for a given record
	/// </summary>
	/// <param name="record">requested record</param>
	/// <returns>reader which can find data for this record</returns>
	public IMemoryReader FindReader(int record)
	{
		if (SingleBuffer)
		{
			return _allDataReader;
		}
		if (MultiBuffer)
		{
			RecordRangeReader memoryReader = GetMemoryReader(record);
			if (memoryReader.EndRecord > -1)
			{
				return memoryReader.Reader;
			}
			lock (this)
			{
				memoryReader = GetMemoryReader(record);
				if (memoryReader.EndRecord > -1)
				{
					return memoryReader.Reader;
				}
				RecordRangeReader rangeReader = _emptyReader;
				if (_lookahead != null)
				{
					_lookahead.Wait();
					RecordRangeReader result = _lookahead.Result;
					if (record >= result.StartRecord && record <= result.EndRecord)
					{
						rangeReader = _lookahead.Result;
					}
					_lookahead.Dispose();
					_lookahead = null;
				}
				if (rangeReader.StartRecord == -1)
				{
					rangeReader = CreateReaderForRecord(record);
				}
				_recentBufferStarts.Enqueue(rangeReader.StartRecord);
				if (_recentBufferStarts.Count >= 3)
				{
					bool flag = true;
					bool flag2 = true;
					int num = 0;
					foreach (int recentBufferStart in _recentBufferStarts)
					{
						if (flag2)
						{
							flag2 = false;
						}
						else if (recentBufferStart != num + _recordsPerBatch)
						{
							flag = false;
							break;
						}
						num = recentBufferStart;
					}
					if (flag && rangeReader.EndRecord < _lastRecord)
					{
						_lookahead = Task.Run(() => CreateReaderForRecord(rangeReader.EndRecord + 1));
					}
					_recentBufferStarts.Dequeue();
				}
				ReplaceOldReader(rangeReader);
				return rangeReader.Reader;
			}
		}
		return _accessor;
	}

	/// <summary>
	/// Replace the oldest reader, defined by lowest end record number
	/// </summary>
	/// <param name="newReader">A reader for a new range of records</param>
	private void ReplaceOldReader(RecordRangeReader newReader)
	{
		bool lockTaken = false;
		try
		{
			_spinLock.Enter(ref lockTaken);
			int num = _lastRecord + 1;
			int num2 = 0;
			for (int i = 0; i < _readers.Length; i++)
			{
				RecordRangeReader recordRangeReader = _readers[i];
				if (recordRangeReader.EndRecord < num)
				{
					num = recordRangeReader.EndRecord;
					num2 = i;
				}
			}
			_readers[num2] = newReader;
		}
		finally
		{
			if (lockTaken)
			{
				_spinLock.Exit(useMemoryBarrier: false);
			}
		}
	}

	/// <summary>
	/// Create a reader for the region of records including the given record
	/// </summary>
	/// <param name="record">requested record</param>
	/// <returns>A reader for a range of records</returns>
	private RecordRangeReader CreateReaderForRecord(int record)
	{
		if (record < MinRecord)
		{
			throw new ArgumentOutOfRangeException("record");
		}
		if (_recordsPerBatch > 0)
		{
			if (ZeroBased)
			{
				int num = record / _recordsPerBatch;
				int num2 = _recordsPerBatch * num;
				int num3 = Math.Min(num2 + _recordsPerBatch - 1, _lastRecord);
				IMemoryReader reader = _managedDevice.CreateSubRangeReader(num2, num3);
				return new RecordRangeReader
				{
					StartRecord = num2,
					EndRecord = num3,
					Reader = reader
				};
			}
			int num4 = (record - 1) / _recordsPerBatch;
			int num5 = _recordsPerBatch * num4 + 1;
			int num6 = Math.Min(num5 + _recordsPerBatch - 1, _lastRecord);
			IMemoryReader reader2 = _managedDevice.CreateSubRangeReader(num5, num6);
			return new RecordRangeReader
			{
				StartRecord = num5,
				EndRecord = num6,
				Reader = reader2
			};
		}
		return _emptyReader;
	}

	/// <summary>
	/// Scan available readers, to see if we have one for a given record
	/// </summary>
	/// <param name="record">record requested</param>
	/// <returns>a reader for this record, or the empty reader if not found</returns>
	private RecordRangeReader GetMemoryReader(int record)
	{
		RecordRangeReader emptyReader = _emptyReader;
		bool lockTaken = false;
		try
		{
			_spinLock.Enter(ref lockTaken);
			RecordRangeReader[] readers = _readers;
			for (int i = 0; i < readers.Length; i++)
			{
				RecordRangeReader result = readers[i];
				if (record >= result.StartRecord && record <= result.EndRecord)
				{
					return result;
				}
			}
			return emptyReader;
		}
		finally
		{
			if (lockTaken)
			{
				_spinLock.Exit(useMemoryBarrier: false);
			}
		}
	}

	public void Dispose()
	{
		if (_lookahead != null)
		{
			_lookahead.Wait();
			_lookahead.Dispose();
			_lookahead = null;
		}
		_readers.Initialize();
	}

	/// <summary>
	/// empty the cached buffers
	/// This just sets the array of readers references to a static empty reader
	/// so that when code stops using the real readers they can be garbage collected
	/// </summary>
	public void Clean()
	{
		lock (this)
		{
			for (int i = 0; i < _readers.Length; i++)
			{
				_readers[i] = _emptyReader;
			}
		}
	}
}
