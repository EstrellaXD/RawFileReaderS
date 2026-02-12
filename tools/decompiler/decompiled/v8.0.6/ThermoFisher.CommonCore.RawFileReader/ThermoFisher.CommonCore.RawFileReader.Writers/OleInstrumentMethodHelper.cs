using System;
using System.IO;
using OpenMcdf;
using OpenMcdf.Extensions;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides common methods to deal with IOleStorage and IOleStream.
/// </summary>
internal static class OleInstrumentMethodHelper
{
	/// <summary>
	/// Reads the device stream data from the storage.
	/// </summary>
	/// <param name="storage">The storage (either the root storage or device method).</param>
	/// <param name="streamName">Name of the stream data.</param>
	/// <param name="bytes">Returns the stream data in byte array.</param>
	/// <param name="errorMessage">Returns the error information if error occurs.</param>
	/// <returns>True if data saved successfully; otherwise false. </returns>
	public static bool TryCatchReadStreamData(CFStorage storage, string streamName, out byte[] bytes, out string errorMessage)
	{
		Stream stream = null;
		errorMessage = string.Empty;
		bytes = null;
		try
		{
			stream = storage.GetStream(streamName).AsIOStream();
			bytes = new byte[stream.Length];
			stream.Seek(0L, SeekOrigin.Begin);
			if (stream.Read(bytes, 0, (int)stream.Length) != stream.Length)
			{
				errorMessage = "Cannot Read from " + streamName;
				return false;
			}
			return true;
		}
		catch (Exception exception)
		{
			errorMessage = exception.ToMessageAndCompleteStacktrace();
		}
		finally
		{
			stream.Close();
		}
		return false;
	}

	/// <summary>
	/// Saves the device stream data to the compound storage.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="storage">The storage (either the root storage or device method).</param>
	/// <param name="streamName">Name of the stream data.</param>
	/// <param name="error">Stores the last error information.</param>
	/// <returns>True if data saved successfully; otherwise false. </returns>
	public static bool TryCatchSaveStreamData(Func<Stream, DeviceErrors, bool> method, CFStorage storage, string streamName, DeviceErrors error)
	{
		Stream stream = null;
		try
		{
			stream = ((!DeviceStorage.GetStorageNames(storage, StgType.Stream).Contains(streamName)) ? storage.AddStream(streamName).AsIOStream() : storage.GetStream(streamName).AsIOStream());
			method(stream, error);
		}
		catch (Exception ex)
		{
			error.UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
		}
		finally
		{
			stream?.Close();
		}
		return !error.HasError;
	}

	/// <summary>
	/// Saves the file header to the IOleStream.
	/// </summary>
	/// <param name="fileHeader">The file header object.</param>
	/// <param name="streamer">The IOleStream object.</param>
	/// <param name="errors">Stores the error information if error occurs.</param>
	/// <returns>True if the file header saved successfully; otherwise false. </returns>
	public static bool Save(this FileHeader fileHeader, Stream streamer, DeviceErrors errors)
	{
		try
		{
			byte[] array = WriterHelper.StructToByteArray(fileHeader.FileHeaderStruct, Utilities.StructSizeLookup.Value[3]);
			streamer.Write(array, 0, array.Length);
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Creates the compound document file (instrument method file) with a specified file name.
	/// </summary>
	/// <returns>The root storage object (IOleStorage).</returns>
	public static CompoundFile CreateDocFile()
	{
		return new CompoundFile();
	}
}
