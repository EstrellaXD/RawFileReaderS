using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Provides a means of opening instrument methods.
/// </summary>
public static class InstrumentMethodReaderFactory
{
	private static readonly ObjectFactory<IInstrumentMethodFileAccess> Reader = CreateReader();

	/// <summary>
	/// create a reader for instrument methods
	/// </summary>
	/// <returns>
	/// A factory to read instrument methods.
	/// </returns>
	private static ObjectFactory<IInstrumentMethodFileAccess> CreateReader()
	{
		ObjectFactory<IInstrumentMethodFileAccess> objectFactory = new ObjectFactory<IInstrumentMethodFileAccess>(".meth", "ThermoFisher.CommonCore.RawFileReader.InstrumentMethodFileReader", "ThermoFisher.CommonCore.RawFileReader.dll", "OpenMethod");
		objectFactory.Initialize();
		return objectFactory;
	}

	/// <summary>
	/// Read an instrument method.
	/// The file contents are returned. The file is not kept open.
	/// Because this reads a file, try catch is suggested around this activity.
	/// Caller must test IsError after opening to obtain any detected error conditions in the file.
	/// </summary>
	/// <param name="fileName">Name of file to read</param>
	/// <returns>Access to the contents of the file.</returns>
	public static IInstrumentMethodFileAccess ReadFile(string fileName)
	{
		return Reader.OpenFile(fileName);
	}
}
