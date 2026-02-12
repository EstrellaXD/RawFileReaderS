namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// Persistence mode for data 
/// </summary>
public enum PersistenceMode
{
	/// <summary>
	/// create data that is mapped to an existing file on disk
	/// </summary>
	Persisted,
	/// <summary>
	/// create data that is not mapped to an existing file on disk
	/// </summary>
	NonPersisted
}
