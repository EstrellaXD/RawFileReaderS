using System;
using System.IO;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This static factory class provides methods to create and open existing sequence file.
/// </summary>
public static class SequenceFileWriterFactory
{
	private static readonly ObjectFactory<ISequenceFileWriter> SeqFileFactory = CreateSequenceFileWriterFactory();

	/// <summary>
	/// Creates the sequence file writer.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="openExisting">True open an existing sequence file with read/write privilege; false to create a new unique sequence file</param>
	/// <returns>Sequence file writer object</returns>
	public static ISequenceFileWriter CreateSequenceFileWriter(string fileName, bool openExisting)
	{
		if (openExisting)
		{
			CheckFileExists(fileName);
		}
		return SeqFileFactory.SpecifiedMethod(new object[2] { fileName, openExisting });
	}

	/// <summary>
	/// Creates the UV type of device factory.
	/// </summary>
	/// <returns>UV type of device factory object</returns>
	private static ObjectFactory<ISequenceFileWriter> CreateSequenceFileWriterFactory()
	{
		return new ObjectFactory<ISequenceFileWriter>("ThermoFisher.CommonCore.RawFileReader.Writers.SequenceFileWriterAdapter", "ThermoFisher.CommonCore.RawFileReader.dll", "CreateSequenceFileWriter", new Type[2]
		{
			typeof(string),
			typeof(bool)
		}, initialize: true);
	}

	/// <summary>
	/// Checks the file exists.
	/// </summary>
	/// <param name="fileName">Name of the in-acquisition raw file.</param>
	/// <exception cref="T:System.ArgumentException">Device writer can't attach the raw file that doesn't exist.</exception>
	private static void CheckFileExists(string fileName)
	{
		if (!File.Exists(fileName))
		{
			throw new ArgumentException("File doesn't exist : " + fileName);
		}
	}
}
