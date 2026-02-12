using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Provides a means of reading headers from Xcalibur files.
/// </summary>
public static class FileHeaderReaderFactory
{
	private static readonly ObjectFactory<IFileHeader> Reader = CreateReader();

	/// <summary>
	/// Creates the reader.
	/// </summary>
	/// <returns>File header reader factory object</returns>
	private static ObjectFactory<IFileHeader> CreateReader()
	{
		ObjectFactory<IFileHeader> objectFactory = new ObjectFactory<IFileHeader>(".pmd", "ThermoFisher.CommonCore.RawFileReader.RawFileReaderAdapter", "ThermoFisher.CommonCore.RawFileReader.dll", "FileHeaderFactory");
		objectFactory.Initialize();
		return objectFactory;
	}

	/// <summary>
	/// Read a file header.
	/// The file header contents are returned. The file is not kept open.
	/// </summary>
	/// <param name="fileName">Name of file to read</param>
	/// <returns>Access to the contents of the file header.</returns>
	public static IFileHeader ReadFile(string fileName)
	{
		return Reader.OpenFile(fileName);
	}
}
