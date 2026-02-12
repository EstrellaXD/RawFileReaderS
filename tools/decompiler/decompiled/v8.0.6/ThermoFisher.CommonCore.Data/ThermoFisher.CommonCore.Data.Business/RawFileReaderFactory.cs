using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to read raw files, using 64 bit technology.
/// Call the "ReadFile" method to open a raw file.
/// The returned interface from ReadFile is "IRawDataPlus".
/// The IRawDataPlus interface implements the IRawData interface,
/// so the returned object can also be passed to code expecting IRawData.
/// To access data for the same file from multiple threads, see <see cref="M:ThermoFisher.CommonCore.Data.Business.RawFileReaderFactory.CreateThreadManager(System.String)" />
/// </summary>
public static class RawFileReaderFactory
{
	private static readonly ObjectFactory<IRawDataExtended> Reader = CreateReader();

	private static readonly ObjectFactory<IRawFileThreadManager> Manager = CreateManager();

	/// <summary>
	/// create a thread manager.
	/// </summary>
	/// <returns>
	/// The thread manager
	/// </returns>
	private static ObjectFactory<IRawFileThreadManager> CreateManager()
	{
		return new ObjectFactory<IRawFileThreadManager>(".raw", "ThermoFisher.CommonCore.RawFileReader.RawFileReaderAdapter", "ThermoFisher.CommonCore.RawFileReader.dll", "ThreadedFileFactory", initialize: true);
	}

	/// <summary>
	/// create reader.
	/// </summary>
	/// <returns>
	/// The factory
	/// </returns>
	private static ObjectFactory<IRawDataExtended> CreateReader()
	{
		return new ObjectFactory<IRawDataExtended>(".raw", "ThermoFisher.CommonCore.RawFileReader.RawFileReaderAdapter", "ThermoFisher.CommonCore.RawFileReader.dll", "FileFactory", initialize: true);
	}

	/// <summary>
	/// Open a raw file for reading.
	/// </summary>
	/// <param name="fileName">Name of file to read</param>
	/// <returns>Access to the contents of the file.</returns>
	public static IRawDataPlus ReadFile(string fileName)
	{
		return Reader.OpenFile(fileName);
	}

	/// <summary>
	/// Open a raw file for reading, creating a manager tool, such that
	/// multiple threads can access the same open file.
	/// </summary>
	/// <param name="fileName">Name of file to read</param>
	/// <returns>Access to the contents of the file.</returns>
	public static IRawFileThreadManager CreateThreadManager(string fileName)
	{
		return Manager.OpenFile(fileName);
	}
}
