using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The raw file writer adapter. Used internally to get a raw file writer.
/// Publicly can use factory which calls down to here.
/// </summary>
internal static class RawFileWriterAdapter
{
	/// <summary>
	/// The get raw file writer.
	/// </summary>
	/// <param name="rawFilePath">
	/// The raw file path.
	/// </param>
	/// <param name="permitRenaming">[Optional] it has a default value of true.<para />
	/// True to specify that the raw file writer should create a raw file if the file isn't exist; however, if the file already exists, 
	/// the writer should create a raw file with the timestamp appends to the end of the file name.<para />
	/// False to specify that the raw file writer should not create a file and throw an System.IO exception if the file already exists.
	/// </param>
	/// <param name="enableFileStitch">If false, the raw file writer would not perform file stitching at the end of acquisition.
	/// The raw file and device stream data would be kept within a specific folder, only used for that sample.
	/// If true, the code should still permit the previous mechanism to be used, for backwards compatibility.
	/// </param>
	/// <returns>Raw file writer object. </returns>
	public static IRawFileWriter GetRawFileWriter(string rawFilePath, bool permitRenaming = true, bool enableFileStitch = true)
	{
		return new RawFileWriter(rawFilePath, Guid.NewGuid(), permitRenaming, enableFileStitch);
	}
}
