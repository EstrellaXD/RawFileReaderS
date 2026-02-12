namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines an object which can create accessors for multiple threads to access the same raw data.
/// </summary>
public interface IRawFileThreadAccessor
{
	/// <summary>
	/// This interface method creates a thread safe access to raw data, for use by a single thread.
	/// Each time a new thread (async call etc.) is made for accessing raw data, this method must be used to
	/// create a private object for that thread to use.
	/// This interface does not require that the application performs any locking.
	/// In some implementations this may have internal locking (such as when based on a real time file, which is continually changing in size),
	/// and in some implementations it may be lockless.
	/// </summary>
	/// <returns>An interface which can be used by a thread to access raw data</returns>
	IRawDataExtended CreateThreadAccessor();
}
