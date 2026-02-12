using System;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// The file header loader. Reads an Xcalibur file header.
/// </summary>
internal static class FileHeaderLoader
{
	/// <summary>
	/// Create a File Header interface to read a file header.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>Access to the file header</returns>
	/// <exception cref="T:System.ArgumentException">Thrown when there are problems with the file name</exception>
	/// <exception cref="T:System.ApplicationException">Only 64 bit applications are supported by this project</exception>
	public static IFileHeader LoadFromFile(string fileName)
	{
		if (!Utilities.ValidateFileName(fileName, out var errors))
		{
			throw new ArgumentException(errors);
		}
		Utilities.Validate64Bit();
		long startPos = 0L;
		Guid id = Guid.NewGuid();
		IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
		IReadWriteAccessor randomAccessViewer = instance.GetRandomAccessViewer(id, fileName, inAcquisition: false, DataFileAccessMode.OpenCreateReadLoaderId);
		if (randomAccessViewer == null)
		{
			errors = $"{instance.GetErrors(StreamHelper.ConstructStreamId(id, fileName))} {fileName}";
			return FileHeader.FromHeader(new FileHeaderStruct
			{
				FileType = 0,
				FileDescription = errors
			});
		}
		try
		{
			return randomAccessViewer.LoadRawFileObjectExt<FileHeader>(0, ref startPos);
		}
		catch (Exception ex)
		{
			return FileHeader.FromHeader(new FileHeaderStruct
			{
				FileType = 0,
				FileDescription = "Does not support this version of the file type." + Environment.NewLine + ex.Message
			});
		}
		finally
		{
			randomAccessViewer.ReleaseAndCloseMemoryMappedFile(MemoryMappedRawFileManager.Instance);
		}
	}
}
