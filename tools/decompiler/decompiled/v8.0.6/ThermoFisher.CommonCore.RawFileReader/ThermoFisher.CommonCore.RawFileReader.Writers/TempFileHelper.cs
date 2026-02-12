using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to create temporary file for writing instrument data
/// </summary>
internal static class TempFileHelper
{
	private static readonly ConcurrentDictionary<string, string> StreamDataFolderPathDict = new ConcurrentDictionary<string, string>();

	/// <summary>
	/// The _temp file folder
	/// </summary>
	private static readonly Lazy<string> TempFileFolder = new Lazy<string>(TempFileFolderPath);

	/// <summary>
	/// Sets the stream data temporary folder.
	/// </summary>
	/// <param name="file">The full raw file name.</param>
	/// <param name="newDirPath">The new dir path.</param>
	public static void SetStreamDataFolder(string file, string newDirPath)
	{
		StreamDataFolderPathDict[file] = newDirPath;
	}

	/// <summary>
	/// Removes the stream data folder.
	/// </summary>
	/// <param name="file">The full raw file name.</param>
	public static void RemoveStreamDataFolder(string file)
	{
		StreamDataFolderPathDict.TryRemove(file, out var _);
	}

	/// <summary>
	/// The create temp file name for device data.
	/// </summary>
	/// <param name="streamDataFolderKey">Dictionary key for a specific stream folder</param>
	/// <param name="writer">Instrument stream writer</param>
	/// <param name="tempFileName">Output of the temporary file name</param>
	/// <param name="addZeroValueLengthField"> The add zero value length field. </param>
	/// <param name="resetStreamPosition">Reset the stream position back to the start. This requires addZeroValueLengthField is set True</param>
	/// <param name="streamDataFileName">A readable temporary file name</param>
	/// <returns>True if a temporary file was created successfully; false otherwise. </returns>
	public static bool CreateTempFile(string streamDataFolderKey, out BinaryWriter writer, out string tempFileName, string streamDataFileName = "", bool addZeroValueLengthField = false, bool resetStreamPosition = false)
	{
		string value;
		bool flag = StreamDataFolderPathDict.TryGetValue(streamDataFolderKey, out value);
		string path = (flag ? value : TempFileFolder.Value);
		tempFileName = (flag ? Path.Combine(path, streamDataFileName + ".tmp") : Path.Combine(path, "LCQ" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".tmp"));
		FileStream output = new FileStream(tempFileName, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
		writer = new BinaryWriter(output, Encoding.Unicode, leaveOpen: false);
		if (addZeroValueLengthField)
		{
			writer.Write(0);
			writer.Flush();
			if (resetStreamPosition)
			{
				writer.BaseStream.Position = 0L;
			}
		}
		return true;
	}

	/// <summary>
	/// Deletes the temporary file.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="fileExtension">Specified the file extension</param>
	public static void DeleteTempFile(string fileName, string fileExtension = ".tmp")
	{
		if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName) || string.Compare(Path.GetExtension(fileName), fileExtension, StringComparison.OrdinalIgnoreCase) != 0)
		{
			return;
		}
		try
		{
			Utilities.RetryMethod(delegate
			{
				File.Delete(fileName);
				return true;
			}, 2, 100);
		}
		catch
		{
		}
	}

	/// <summary>
	/// Get the temporary the file folder path.
	/// </summary>
	/// <returns>The file folder path for keeping the temporary files</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Temporary file path length is greater than 260 chars :  + temporary data folder path</exception>
	private static string TempFileFolderPath()
	{
		string obj = (Utilities.IsRunningUnitTest.Value ? Environment.CurrentDirectory : (Utilities.IsRunningUnderLinux.Value ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Thermo_Scientific/Temp") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Thermo Scientific\\Temp")));
		ValidatePathLength(obj);
		return obj;
	}

	/// <summary>Validates the length of the path. Max path length is 260. </summary>
	/// <param name="folderPath">The folder path.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Temporary file path length is greater than 260 chars : " + folderPath</exception>
	public static void ValidatePathLength(string folderPath)
	{
		if (folderPath.Length >= 260)
		{
			throw new ArgumentOutOfRangeException("Temporary file path length is greater than 260 chars : " + folderPath);
		}
	}

	/// <summary>Creates the directory if does not exist.</summary>
	/// <param name="dirName">Name of the dir.</param>
	/// <exception cref="T:System.Exception">Not able to create directory: {dirName}, error: {e.Message}</exception>
	public static void CreateDirectoryIfDoesNotExist(string dirName)
	{
		try
		{
			Directory.CreateDirectory(dirName);
		}
		catch (Exception ex)
		{
			throw new Exception("Not able to create directory: " + dirName + ", error: " + ex.Message);
		}
	}
}
