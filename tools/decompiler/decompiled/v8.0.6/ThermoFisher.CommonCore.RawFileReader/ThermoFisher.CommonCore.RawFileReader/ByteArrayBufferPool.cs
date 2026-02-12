using System.Collections.Generic;
using System.Threading;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Keeps a pool of buffers, which drastically reduces
/// memory allocations and garbage collection
/// This also avoids the .net "initialize" of arrays, when not required.
/// </summary> 
internal class ByteArrayBufferPool : IBufferPool
{
	/// <summary>
	/// The pool limit. Maximum number of buffers held.
	/// </summary>
	private readonly int _poolLimit;

	private readonly LinkedList<byte[]> _pool = new LinkedList<byte[]>();

	private SpinLock _spinLock;

	/// <summary>
	/// The allocations since last call to clear.
	/// </summary>
	private volatile int _allocationsSinceClear;

	/// <summary>
	/// Gets metrics
	/// </summary>
	public int Allocations
	{
		get
		{
			bool lockTaken = false;
			try
			{
				_spinLock.Enter(ref lockTaken);
				return _allocationsSinceClear;
			}
			finally
			{
				if (lockTaken)
				{
					_spinLock.Exit(useMemoryBarrier: false);
				}
			}
		}
	}

	/// <summary>
	/// initialize a new ByteArrayBufferPool with the specified max buffers in the pool.
	/// </summary>
	/// <param name="limit">The maximum number of buffers kept in the pool</param>
	public ByteArrayBufferPool(int limit = 20)
	{
		_poolLimit = limit;
	}

	/// <summary>
	/// Ask for a buffer from the pool (rent a buffer)
	/// </summary>
	/// <param name="size">
	/// Required buffer size
	/// </param>
	/// <returns>
	/// A buffer of at least the desired size. Buffer may be larger than requested
	/// </returns>
	public byte[] Rent(int size)
	{
		byte[] array = null;
		bool lockTaken = false;
		try
		{
			_spinLock.Enter(ref lockTaken);
			LinkedListNode<byte[]> first = _pool.First;
			if (first != null)
			{
				LinkedListNode<byte[]> linkedListNode = first;
				while (true)
				{
					byte[] value = linkedListNode.Value;
					if (value.Length >= size)
					{
						array = value;
						break;
					}
					LinkedListNode<byte[]> next = linkedListNode.Next;
					if (next == null)
					{
						break;
					}
					linkedListNode = next;
				}
				if (array != null)
				{
					_pool.Remove(linkedListNode);
				}
			}
		}
		finally
		{
			if (lockTaken)
			{
				_spinLock.Exit(useMemoryBarrier: false);
			}
		}
		if (array != null)
		{
			return array;
		}
		Interlocked.Increment(ref _allocationsSinceClear);
		return new byte[size];
	}

	/// <summary>
	/// Clear all buffers
	/// </summary>
	public void Clear()
	{
		bool lockTaken = false;
		try
		{
			_spinLock.Enter(ref lockTaken);
			_pool.Clear();
			_allocationsSinceClear = 0;
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
	/// Release a buffer into the pool, when you no longer need it.
	/// </summary>
	/// <param name="data">
	/// Buffer which may be pooled
	/// </param>
	public void Release(byte[] data)
	{
		if (data == null)
		{
			return;
		}
		bool lockTaken = false;
		try
		{
			_spinLock.Enter(ref lockTaken);
			if (_pool.Count < _poolLimit)
			{
				_pool.AddFirst(data);
				return;
			}
			LinkedListNode<byte[]> first = _pool.First;
			if (first == null)
			{
				return;
			}
			byte[] array = first.Value;
			int num = array.Length;
			foreach (byte[] item in _pool)
			{
				if (item.Length < num)
				{
					array = item;
					num = item.Length;
				}
			}
			if (num < data.Length)
			{
				_pool.Remove(array);
				_pool.AddFirst(data);
			}
		}
		finally
		{
			if (lockTaken)
			{
				_spinLock.Exit(useMemoryBarrier: false);
			}
		}
	}
}
