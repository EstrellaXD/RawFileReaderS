using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// class to manage multiple threads accessing the same file, with no locks
/// </summary>
internal class LocklessReader : IRawFileThreadAccessor
{
	/// <summary>
	/// Gets the file loader.
	/// </summary>
	private IRawFileLoader FileLoader { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.LocklessReader" /> class.
	/// </summary>
	/// <param name="fileLoader">The loader.</param>
	public LocklessReader(IRawFileLoader fileLoader)
	{
		FileLoader = fileLoader;
	}

	/// <summary>
	/// This interface method creates a thread safe access to raw data, for use by a single thread.
	/// Each time a new thread (async call etc.) is made for accessing raw data, this method must be used to
	/// create a private object for that thread to use.
	/// This interface does not require that the application performs any locking.
	/// In some implementations this may have internal locking (such as when based on a real time file, which is continually changing in size),
	/// and in some implementations it may be lock-less.
	/// </summary>
	/// <returns>An interface which can be used by a thread to access raw data</returns>
	public IRawDataExtended CreateThreadAccessor()
	{
		return new RawFileAccessBase(FileLoader);
	}
}
