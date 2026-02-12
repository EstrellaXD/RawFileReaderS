using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Generates low overhead objects, which multiple threads
/// can use to read from the same raw file in parallel.
/// Access is typically lockless for 64 bit code, with completed files.
/// Lock will occur for files which are in acquisition.
/// </summary>
internal sealed class ThreadManager : IRawFileThreadManager, IRawFileThreadAccessor, IDisposable
{
	private readonly IRawFileThreadAccessor _rawFileThreadAccess;

	private IRawFileLoader _rawFileLoader;

	private IRawDataExtended _fileReaderAdapter;

	private bool _disposed;

	/// <summary>
	/// Prevents a default instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.ThreadManager" /> class from being created.
	/// </summary>
	private ThreadManager()
	{
		_disposed = false;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.ThreadManager" /> class.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="randomAccess"></param>
	/// <param name="manager">tool to read data</param>
	/// <exception cref="T:System.ArgumentException">Error empty null file name.</exception>
	public ThreadManager(string fileName, bool randomAccess = false, IViewCollectionManager manager = null)
		: this()
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentException(Resources.ErrorEmptyNullFileName);
		}
		if (!fileName.ToUpperInvariant().Contains(".RAW"))
		{
			fileName += ".raw";
		}
		_fileReaderAdapter = null;
		_rawFileThreadAccess = null;
		Utilities.Validate64Bit();
		_rawFileLoader = RawFileLoaderFactory.CreateLoader(fileName, randomAccess, manager);
		if (_rawFileLoader.HasError)
		{
			_rawFileThreadAccess = new LocklessReader(_rawFileLoader);
		}
		else if (_rawFileLoader.RawFileInformation.IsInAcquisition)
		{
			_fileReaderAdapter = RawFileAccess.Create(_rawFileLoader);
			_rawFileThreadAccess = new ThreadSafeRawFileAccess(_fileReaderAdapter);
		}
		else
		{
			_rawFileThreadAccess = new LocklessReader(_rawFileLoader);
		}
	}

	/// <summary>
	/// This interface method creates a thread safe access to raw data, for use by a single thread.
	/// Each time a new thread (async call etc.) is made for accessing raw data, this method must be used to
	/// create a private object for that thread to use.
	/// This interface does not require that the application performs any locking.
	/// In some implementations this may have internal locking (such as when based on a real time file, which is continually changing in size),
	/// and in some implementations it may be lockless.
	/// </summary>
	/// <returns>
	/// An interface which can be used by a thread to access raw data
	/// </returns>
	public IRawDataExtended CreateThreadAccessor()
	{
		if (!_disposed)
		{
			return _rawFileThreadAccess.CreateThreadAccessor();
		}
		return null;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			if (_fileReaderAdapter != null)
			{
				_fileReaderAdapter.Dispose();
				_fileReaderAdapter = null;
			}
			if (_rawFileLoader != null)
			{
				_rawFileLoader.Dispose();
				_rawFileLoader = null;
			}
			_disposed = true;
		}
	}
}
