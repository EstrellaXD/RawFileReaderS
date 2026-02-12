using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This static class contains factory to create sequence file writer adapter.  
/// It's used internally to get a sequence file writer object.
/// </summary>
public class SequenceFileWriterAdapter
{
	/// <summary>
	/// Creates the sequence file writer.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="openExisting">True open an existing sequence file with read/write privilege; false to create a new unique sequence file</param>
	/// <returns>Sequence file writer object</returns>
	public static ISequenceFileWriter CreateSequenceFileWriter(string fileName, bool openExisting)
	{
		return new SequenceFileWriter(fileName, openExisting);
	}
}
