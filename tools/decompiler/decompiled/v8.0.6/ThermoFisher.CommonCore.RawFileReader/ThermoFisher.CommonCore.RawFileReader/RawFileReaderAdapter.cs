using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// This static class contains factories to open raw files
/// </summary>
public static class RawFileReaderAdapter
{
	/// <summary>
	/// Create an IRawDataExtended interface to read data from a raw file
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <returns>Interface to read data from file</returns>
	public static IRawDataExtended FileFactory(string fileName)
	{
		return RawFileAccess.Create(fileName, preferRandomAccess: false, null);
	}

	/// <summary>
	/// Create an IRawDataExtended interface to read data from a raw file.
	/// Use supplied view collection manager for file reading.
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <param name="manager">Data reader</param>
	/// <returns>Interface to read data from file</returns>
	public static IRawDataExtended DelegatedAccessFileFactory(string fileName, IViewCollectionManager manager)
	{
		return RawFileAccess.Create(fileName, preferRandomAccess: true, manager);
	}

	/// <summary>
	/// Open a raw file, which will be accessed from multiple threads.
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <returns>Interface to create objects for each thread to use</returns>
	public static IRawFileThreadManager ThreadedFileFactory(string fileName)
	{
		return new ThreadManager(fileName);
	}

	/// <summary>
	/// Open a raw file, which will be accessed from multiple threads.
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <param name="manager">Randoem acess reader</param>
	/// <returns>Interface to create objects for each thread to use</returns>
	public static IRawFileThreadManager RandomAccessThreadedFileFactory(string fileName, IViewCollectionManager manager)
	{
		return new ThreadManager(fileName, randomAccess: true, manager);
	}

	/// <summary>
	/// Open a raw file, which will be accessed from multiple threads.
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <param name="manager">Data reader</param>
	/// <returns>Interface to create objects for each thread to use</returns>
	public static IRawFileThreadManager DelegatedRandomAccessThreadedFileFactory(string fileName, IViewCollectionManager manager)
	{
		return new ThreadManager(fileName, randomAccess: true, manager);
	}

	/// <summary>
	/// Opens an Xcalibur family file, and returns information from the header.
	/// The file is not kept open.
	/// For example: Reads headers from <c>.raw, .sld, .pmd</c> files.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>Interface to read a file header</returns>
	/// <exception cref="T:System.ArgumentException">Thrown when there are problems with the file name</exception>
	/// <exception cref="T:System.ApplicationException">Only 64 bit applications are supported by this project</exception>
	public static IFileHeader FileHeaderFactory(string fileName)
	{
		return FileHeaderLoader.LoadFromFile(fileName);
	}

	/// <summary>
	/// Creates an object which can be used to decode scan information
	/// from the binary (byte array) form of a scan
	/// </summary>
	/// <param name="scanData">Compressed data for this scan</param>
	/// <param name="index">Defines the data format for this scan</param>
	/// <param name="fileRevision">Raw file version which was used to encode this scan</param>
	/// <returns>An interface with decoder methods</returns>
	public static IMsScanDecoder ScanDecoderFactory(byte[] scanData, IMsScanIndexAccess index, int fileRevision)
	{
		return new MsScanDecoder(scanData, 0L, index, fileRevision);
	}
}
