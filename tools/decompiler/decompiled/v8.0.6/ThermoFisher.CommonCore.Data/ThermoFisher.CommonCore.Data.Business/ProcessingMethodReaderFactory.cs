using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Provides a means of opening processing methods.
/// </summary>
public static class ProcessingMethodReaderFactory
{
	private static readonly ObjectFactory<IProcessingMethodFileAccess> Reader = CreateReader();

	/// <summary>
	/// create reader.
	/// </summary>
	/// <returns>
	/// The reader
	/// </returns>
	private static ObjectFactory<IProcessingMethodFileAccess> CreateReader()
	{
		ObjectFactory<IProcessingMethodFileAccess> objectFactory = new ObjectFactory<IProcessingMethodFileAccess>(".pmd", "ThermoFisher.CommonCore.RawFileReader.ProcessingMethodFileReader", "ThermoFisher.CommonCore.RawFileReader.dll", "OpenProcessingMethod");
		objectFactory.Initialize();
		return objectFactory;
	}

	/// <summary>
	/// Read a processing method.
	/// The file contents are returned. The file is not kept open.
	/// </summary>
	/// <param name="fileName">Name of file to read</param>
	/// <returns>Access to the contents of the file.</returns>
	public static IProcessingMethodFileAccess ReadFile(string fileName)
	{
		return Reader.OpenFile(fileName);
	}
}
