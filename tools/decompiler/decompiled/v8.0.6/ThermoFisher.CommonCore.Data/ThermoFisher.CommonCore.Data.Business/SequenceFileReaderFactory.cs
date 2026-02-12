using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Provides a means of opening sequences.
/// </summary>
public static class SequenceFileReaderFactory
{
	private static readonly ObjectFactory<ISequenceFileAccess> Reader = CreateReader();

	/// <summary>
	/// Creates the sequence file reader.
	/// </summary>
	/// <returns>Sequence file reader factory object</returns>
	private static ObjectFactory<ISequenceFileAccess> CreateReader()
	{
		ObjectFactory<ISequenceFileAccess> objectFactory = new ObjectFactory<ISequenceFileAccess>(".sld", "ThermoFisher.CommonCore.RawFileReader.SequenceFileReader", "ThermoFisher.CommonCore.RawFileReader.dll", "OpenSequence");
		objectFactory.Initialize();
		return objectFactory;
	}

	/// <summary>
	/// Read a sequence.
	/// The file contents are returned. The file is not kept open.
	/// </summary>
	/// <param name="fileName">Name of file to read</param>
	/// <returns>Access to the contents of the file.</returns>
	public static ISequenceFileAccess ReadFile(string fileName)
	{
		return Reader.OpenFile(fileName);
	}
}
