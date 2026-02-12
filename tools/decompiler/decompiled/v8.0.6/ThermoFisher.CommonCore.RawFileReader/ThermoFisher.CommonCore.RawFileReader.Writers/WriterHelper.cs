using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides some commonly used functions for writing device stream data.
/// </summary>
internal static class WriterHelper
{
	/// <summary>
	/// Use mutex to create a "critical section" region of code that executes in isolation. 
	/// </summary>
	/// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
	/// <param name="method">The method.</param>
	/// <param name="errors">The errors information.</param>
	/// <param name="mutexName">Name of the mutex.</param>
	/// <param name="useGlobalNamespace">True add Global prefix to the file name; otherwise no.</param>
	/// <returns>True if the operation succeed; otherwise False</returns>
	public static T CritSec<T>(Func<DeviceErrors, T> method, DeviceErrors errors, string mutexName, bool useGlobalNamespace)
	{
		T result = default(T);
		Mutex mutex = null;
		try
		{
			mutex = Utilities.CreateNamedMutexAndWait(mutexName, useGlobalNamespace, errors);
			if (mutex != null)
			{
				result = method(errors);
				return result;
			}
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
		}
		finally
		{
			if (mutex != null)
			{
				mutex.ReleaseMutex();
				mutex.Close();
			}
		}
		return result;
	}

	/// <summary>
	/// Use mutex to create a "critical section" region of code that executes in isolation. 
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="errors">The errors.</param>
	/// <param name="mutex">The mutex.</param>
	/// <returns>True if the operation succeed, otherwise False</returns>
	public static bool CritSec(Func<DeviceErrors, bool> method, DeviceErrors errors, Mutex mutex)
	{
		bool result = false;
		try
		{
			if (mutex == null || !mutex.WaitOne(5000))
			{
				errors.UpdateError("Cannot get the mutex in time.");
				return false;
			}
			result = method(errors);
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
		}
		finally
		{
			mutex?.ReleaseMutex();
		}
		return result;
	}

	/// <summary>
	/// A wrapper method of a try catch.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="error">The error.</param>
	/// <returns>True the method execute successfully; otherwise false</returns>
	public static bool TryCatch(Func<DeviceErrors, bool> method, DeviceErrors error)
	{
		bool result = false;
		try
		{
			result = method(error);
		}
		catch (Exception ex)
		{
			error.UpdateError(ex);
		}
		return result;
	}

	/// <summary>
	/// Convert a structure to byte array.
	/// </summary>
	/// <typeparam name="T">Such as file header, sequence row, etc.</typeparam>
	/// <param name="objStruct">The object structure.</param>
	/// <param name="size">The size.</param>
	/// <returns>results in byte array</returns>
	public static byte[] StructToByteArray<T>(T objStruct, int size) where T : struct
	{
		byte[] array = new byte[size];
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		Marshal.StructureToPtr(objStruct, gCHandle.AddrOfPinnedObject(), fDeleteOld: false);
		gCHandle.Free();
		return array;
	}

	/// <summary>
	/// Write string data to the file.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="isWideChar">True if it's wide/unicode char, False otherwise.</param>
	public static void StringWrite(this BinaryWriter writer, string value, bool isWideChar = true)
	{
		int num = 0;
		if (!string.IsNullOrWhiteSpace(value))
		{
			num = value.Length;
		}
		writer.Write(num);
		if (value != null && num > 0)
		{
			if (isWideChar)
			{
				char[] chars = value.ToCharArray();
				writer.Write(chars);
			}
			else
			{
				byte[] bytes = Encoding.ASCII.GetBytes(value);
				writer.Write(bytes);
			}
		}
	}

	/// <summary>
	/// Convert double array to the integer array.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <returns>results in integer array</returns>
	public static int[] ToIntArray(this double[] values)
	{
		ValidateArrayArgument(values, "Double array to Int array");
		int num = values.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (int)values[i];
		}
		return array;
	}

	/// <summary>
	/// Convert the integer array to the byte array.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <returns>result in byte array</returns>
	public static byte[] ToByteArray(this int[] values)
	{
		ValidateArrayArgument(values, "Integer array to byte array");
		byte[] array = new byte[values.Length * 4];
		Buffer.BlockCopy(values, 0, array, 0, array.Length);
		return array;
	}

	/// <summary>
	/// Convert double array to byte array.
	/// </summary>
	/// <param name="values">
	/// The values.
	/// </param>
	/// <returns>Byte array</returns>
	public static byte[] ToByteArray(this double[] values)
	{
		ValidateArrayArgument(values, "Double array to byte array");
		byte[] array = new byte[values.Length * 8];
		Buffer.BlockCopy(values, 0, array, 0, array.Length);
		return array;
	}

	/// <summary>
	/// Updates the errors exception.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>Store the error information to the error object and return back the original exception object</returns>
	public static Exception UpdateErrorsException(this Exception value, DeviceErrors errors)
	{
		errors.UpdateError(value.ToMessageAndCompleteStacktrace(), value.HResult);
		return value;
	}

	/// <summary>
	/// Updates the errors from the shared memory map Buffer Info.
	/// </summary>
	/// <param name="bufferInfo"> The buffer information. </param>
	/// <param name="errors"> Storing the errors information. </param>
	/// <returns>The shared memory map buffer info object</returns>
	public static BufferInfo UpdateErrors(this BufferInfo bufferInfo, DeviceErrors errors)
	{
		if (bufferInfo != null && bufferInfo.HasError)
		{
			errors.UpdateError(bufferInfo.ErrorMessage, bufferInfo.ErrorCode);
		}
		return bufferInfo;
	}

	/// <summary>
	/// Disposes the buffer information object.
	/// </summary>
	/// <param name="bufferInfo">The buffer information.</param>
	public static void DisposeBufferInfo(this BufferInfo bufferInfo)
	{
		bufferInfo?.Dispose();
	}

	/// <summary>
	/// Creates a unique file name based on the time and date along with the file passed in.
	/// </summary>
	/// <param name="filePath">
	/// The file path.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	public static string GetTimeStampFileName(string filePath)
	{
		string text = filePath;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
		string directoryName = Path.GetDirectoryName(filePath);
		string extension = Path.GetExtension(filePath);
		if (!string.IsNullOrEmpty(fileNameWithoutExtension) && !string.IsNullOrEmpty(directoryName) && !string.IsNullOrEmpty(extension))
		{
			while (File.Exists(text))
			{
				Thread.Sleep(1000);
				string path = string.Format("{0}_{1}", fileNameWithoutExtension, DateTime.Now.ToString("yyyyMMddHHmmss"));
				text = Path.Combine(directoryName, path) + extension;
			}
			return text;
		}
		throw new FileNotFoundException($"The file or path can not be found for file: {filePath}");
	}

	/// <summary>
	/// Deletes the temporary file (with .TMP extension) only if no reference
	/// </summary>
	/// <param name="bufferInfo">The buffer information.</param>
	/// <param name="tempFileName">Name of the temporary file.</param>
	/// <param name="deletePermitted">True if permitted to delete temp files</param>
	/// <returns>return back the buffer info object</returns>
	public static BufferInfo DeleteTempFileOnlyIfNoReference(this BufferInfo bufferInfo, string tempFileName, bool deletePermitted = true)
	{
		if (bufferInfo != null)
		{
			bufferInfo.DecrementReferenceCount();
			if (bufferInfo.HasReference())
			{
				return bufferInfo;
			}
		}
		if (deletePermitted)
		{
			TempFileHelper.DeleteTempFile(tempFileName);
		}
		return bufferInfo;
	}

	/// <summary>
	/// Determines whether [is generic header ready].
	/// </summary>
	/// <param name="writtenFlags">The written flags.</param>
	/// <returns>True if all generic headers have been written; false otherwise.</returns>
	public static bool IsWrittenGenericHeaderDone(this int[] writtenFlags)
	{
		if (writtenFlags != null && writtenFlags.Length != 0)
		{
			return writtenFlags.All((int flag) => flag != 0);
		}
		return false;
	}

	/// <summary>
	/// Determines whether the specified segmented scan has scan.
	/// </summary>
	/// <param name="segmentedScan">The segmented scan.</param>
	/// <returns>True if the segmented scan has scan, false otherwise.</returns>
	public static bool HasNoScan(ISegmentedScanAccess segmentedScan)
	{
		if (segmentedScan != null && segmentedScan.SegmentCount != 0 && segmentedScan.SegmentLengths != null)
		{
			return segmentedScan.SegmentLengths.Sum() <= 0;
		}
		return true;
	}

	/// <summary>
	/// Validates the string name.
	/// </summary>
	/// <param name="name">The string name.</param>
	/// <param name="message">Error message</param>
	/// <exception cref="T:System.ArgumentNullException">name;@Error message</exception>
	public static void ValidateName(string name, string message)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name", message);
		}
	}

	/// <summary>
	/// Valid the instrument method file extension.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>Instrument method file name with correct file extension.</returns>
	public static string ValidInstrumentMethodFileExtension(string fileName)
	{
		if (!fileName.ToUpperInvariant().EndsWith(".METH"))
		{
			fileName += ".meth";
		}
		return fileName;
	}

	/// <summary>
	/// Validates the array argument.
	/// </summary>
	/// <typeparam name="T">array of integer and double</typeparam>
	/// <param name="data">The data.</param>
	/// <param name="argName">Name of the argument.</param>
	/// <exception cref="T:System.ArgumentNullException">name of the input argument</exception>
	private static void ValidateArrayArgument<T>(T[] data, string argName)
	{
		if (data == null)
		{
			throw new ArgumentException("Null argument" + argName);
		}
	}
}
