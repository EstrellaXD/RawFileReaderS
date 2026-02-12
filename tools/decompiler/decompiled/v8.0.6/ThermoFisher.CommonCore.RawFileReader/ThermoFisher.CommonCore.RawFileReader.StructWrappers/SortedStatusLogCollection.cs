using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
///     The sorted status log collection.
/// </summary>
internal sealed class SortedStatusLogCollection : IDisposable
{
	private readonly IComparer<StatusLogKey> _comparer;

	private readonly List<LabelValueBlob> _blobEntries;

	private readonly List<StatusLogKey> _sortedKeys;

	private readonly List<float> _rentionTimes;

	private bool _disposed;

	/// <summary>
	/// Gets the count (of sorted items).
	/// This may be lower than the number of records in the file
	/// as duplicate time values are eliminated.
	/// </summary>
	internal int Count => _sortedKeys.Count;

	public ILogDecoder Decoder { get; set; }

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.SortedStatusLogCollection" /> class.
	/// </summary>
	internal SortedStatusLogCollection()
	{
		_comparer = new StatusLogKeyComparer();
		_sortedKeys = new List<StatusLogKey>();
		_blobEntries = new List<LabelValueBlob>();
		_rentionTimes = new List<float>();
	}

	/// <summary>
	/// Add a status blob.
	/// </summary>
	/// <param name="retentionTime">
	/// The retention time.
	/// </param>
	/// <param name="statusBlob">
	/// The status blob to add.
	/// </param>
	/// <param name="finalEntry">Set if this is the last log entry</param>
	internal void AddStatusBlob(float retentionTime, LabelValueBlob statusBlob, bool finalEntry)
	{
		_rentionTimes.Add(retentionTime);
		bool flag = _sortedKeys.Count > 0;
		bool flag2 = true;
		if (finalEntry && flag)
		{
			double retentionTime2 = _sortedKeys[_sortedKeys.Count - 1].RetentionTime;
			if ((double)retentionTime <= retentionTime2)
			{
				flag2 = false;
			}
		}
		else if (flag && Math.Abs(_sortedKeys[_sortedKeys.Count - 1].RetentionTime - (double)retentionTime) < 1E-30)
		{
			flag2 = false;
		}
		if (flag2)
		{
			_sortedKeys.Add(new StatusLogKey(retentionTime, _blobEntries.Count));
		}
		_blobEntries.Add(statusBlob);
	}

	/// <summary>
	/// Find the log entry nearest to a given time.
	/// </summary>
	/// <param name="retentionTime">
	/// The retention time.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.StatusLogEntry" />.
	/// </returns>
	internal StatusLogEntry GetItem(double retentionTime)
	{
		if (_sortedKeys.Count == 0)
		{
			return new StatusLogEntry(retentionTime, new List<LabelValuePair>());
		}
		StatusLogKey item = new StatusLogKey(retentionTime, 0);
		int num = _sortedKeys.BinarySearch(item, _comparer);
		if (num < 0)
		{
			int num2 = ~num;
			if (num2 == _sortedKeys.Count)
			{
				num = _sortedKeys.Count - 1;
			}
			else if (num2 == 0)
			{
				num = 0;
			}
			else
			{
				double num3 = (_sortedKeys[num2 - 1].RetentionTime + _sortedKeys[num2].RetentionTime) / 2.0;
				num = ((retentionTime > num3) ? num2 : (num2 - 1));
			}
		}
		int blobIndex = _sortedKeys[num].BlobIndex;
		List<LabelValuePair> valuePairs = _blobEntries[blobIndex].ReadLabelValuePairs(Decoder);
		return new StatusLogEntry(_sortedKeys[num].RetentionTime, valuePairs);
	}

	/// <summary>
	/// The method gets a list of status log entries based on the index.
	/// </summary>
	/// <param name="index">
	/// The index into the status log entry (i.e. index into a key value pair).
	/// </param>
	/// <returns>
	/// The list of status log entries based on the index.
	/// </returns>
	internal List<StatusLogEntry> GetItemValues(int index)
	{
		List<StatusLogEntry> list = new List<StatusLogEntry>();
		for (int i = 0; i < _blobEntries.Count; i++)
		{
			LabelValuePair labelValuePair = _blobEntries[i]?.GetItemAt(index, Decoder);
			if (labelValuePair != null)
			{
				StatusLogEntry item = new StatusLogEntry(_rentionTimes[i], new List<LabelValuePair> { labelValuePair });
				list.Add(item);
			}
		}
		return list;
	}

	/// <summary>
	/// get a blob entry, with time stamp.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// A tuple, containing time (float) and LabelValueBlob.
	/// </returns>
	internal Tuple<float, LabelValueBlob> GetBlobEntry(int index)
	{
		int count = _blobEntries.Count;
		if (index < 0 || index >= count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return new Tuple<float, LabelValueBlob>(_rentionTimes[index], _blobEntries[index]);
	}

	/// <summary>
	/// get a blob entry, with time stamp.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// A tuple, containing time (float) and LabelValueBlob.
	/// </returns>
	internal Tuple<float, LabelValueBlob> GetSortedBlobEntry(int index)
	{
		int count = _sortedKeys.Count;
		if (index < 0 || index >= count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		StatusLogKey statusLogKey = _sortedKeys[index];
		return new Tuple<float, LabelValueBlob>((float)statusLogKey.RetentionTime, _blobEntries[statusLogKey.BlobIndex]);
	}

	/// <summary>
	/// get a blob entry, with time stamp.
	/// </summary>
	/// <param name="retentionTime">The retention time </param>
	/// <returns>
	/// A tuple, containing time (float) and LabelValueBlob.
	/// </returns>
	internal Tuple<float, LabelValueBlob> GetBlobEntry(double retentionTime)
	{
		if (_sortedKeys.Count == 0)
		{
			return new Tuple<float, LabelValueBlob>((float)retentionTime, null);
		}
		StatusLogKey item = new StatusLogKey(retentionTime, 0);
		int num = _sortedKeys.BinarySearch(item, _comparer);
		if (num < 0)
		{
			int num2 = ~num;
			if (num2 == _sortedKeys.Count)
			{
				num = _sortedKeys.Count - 1;
			}
			else if (num2 == 0)
			{
				num = 0;
			}
			else
			{
				double num3 = (_sortedKeys[num2 - 1].RetentionTime + _sortedKeys[num2].RetentionTime) / 2.0;
				num = ((retentionTime > num3) ? num2 : (num2 - 1));
			}
		}
		return new Tuple<float, LabelValueBlob>(_rentionTimes[num], _blobEntries[num]);
	}

	/// <summary>
	/// get item at index
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.StatusLogEntry" />.
	/// </returns>
	/// <exception cref="T:System.ArgumentException">index out of range
	/// </exception>
	internal StatusLogEntry GetItem(int index)
	{
		int count = _blobEntries.Count;
		if (index < 0 || index >= count)
		{
			throw new ArgumentException("index out of range");
		}
		LabelValueBlob labelValueBlob = _blobEntries[index];
		if (labelValueBlob == null)
		{
			return new StatusLogEntry(-1.0, new List<LabelValuePair>());
		}
		List<LabelValuePair> valuePairs = labelValueBlob.ReadLabelValuePairs(Decoder);
		return new StatusLogEntry(_rentionTimes[index], valuePairs);
	}

	/// <summary>
	///     The sort keys.
	/// </summary>
	internal void SortKeys()
	{
		_sortedKeys.Sort(_comparer);
	}

	/// <summary>
	/// clear the list
	/// </summary>
	internal void Clear()
	{
		_sortedKeys?.Clear();
		if (_blobEntries != null)
		{
			_blobEntries.Clear();
		}
		_rentionTimes?.Clear();
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
		}
	}
}
