using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This static factory class provides methods to create an instrument method writer for creating/updating an instrument method file.
/// </summary>
public static class InstrumentMethodWriterFactory
{
	private static readonly ObjectFactory<IInstrumentMethodWriter> InstMethodWriterFactory = CreateInstrumentMethodWriterFactory();

	/// <summary>
	/// Creates the instrument method writer with an input file name.<para />
	/// The method should open a file if it exists and loads the data into internal structure and then close the file;
	/// otherwise, a new in-memory method file should be created (since it's in-memory, the data isn't persisted to a file yet). <para />
	/// After editing the device method, caller should use either the "Save" or "SaveAs" method to persist the data to a file.
	/// </summary>
	/// <param name="fileName">Name of the instrument method file.</param>
	/// <returns>Instrument method writer object.</returns>
	/// <exception cref="T:System.ArgumentNullException">Null or empty file name argument.</exception>
	public static IInstrumentMethodWriter CreateInstrumentMethodWriter(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentNullException("fileName", "File name cannot be empty.");
		}
		return InstMethodWriterFactory.SpecifiedMethod(new object[1] { fileName });
	}

	/// <summary>
	/// Because there is no input file name, this method will create an "Unnamed" in-memory instrument method
	/// file (since it's in-memory, the data isn't persisted to a file yet).<para />
	/// After editing the device method, caller should use the "SaveAs" method with a valid file name to save the data to a file.
	/// </summary>
	/// <returns>Instrument method writer object.</returns>
	public static IInstrumentMethodWriter CreateInstrumentMethodWriter()
	{
		return InstMethodWriterFactory.SpecifiedMethod(new object[1] { string.Empty });
	}

	/// <summary>
	/// Creates the instrument method writer factory.
	/// </summary>
	/// <returns>Instrument method writer factory object.</returns>
	private static ObjectFactory<IInstrumentMethodWriter> CreateInstrumentMethodWriterFactory()
	{
		return new ObjectFactory<IInstrumentMethodWriter>("ThermoFisher.CommonCore.RawFileReader.Writers.InstrumentMethodWriterAdapter", "ThermoFisher.CommonCore.RawFileReader.dll", "CreateInstrumentMethodWriter", new Type[1] { typeof(string) }, initialize: true);
	}
}
