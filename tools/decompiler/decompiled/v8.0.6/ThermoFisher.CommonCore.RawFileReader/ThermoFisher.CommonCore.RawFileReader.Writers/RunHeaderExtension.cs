using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to update the run header shared memory buffer
/// </summary>
internal static class RunHeaderExtension
{
	private static readonly int RunHeaderStructSize = Utilities.StructSizeLookup.Value[2];

	/// <summary>
	/// Wrapper to write the expected run time. All devices MUST do this so
	/// that the real-time update can display a sensible Axis.
	/// </summary>
	/// <param name="runheader">Device's run header object</param>
	/// <param name="writer">Shared memory mapped file accessor</param>
	/// <param name="runTime">The expected Run Time.</param>
	/// <param name="errors">Store error information</param>
	/// <returns>True if expected run time is written to disk successfully, False otherwise</returns>
	public static bool SaveExpectedRunime(this RunHeader runheader, IReadWriteAccessor writer, double runTime, DeviceErrors errors)
	{
		double num = 0.1;
		double num2 = 1E+20;
		if (Math.Abs(runTime - -1.0) < double.Epsilon)
		{
			return true;
		}
		if (runTime >= num && runTime <= num2)
		{
			runheader.ExpectedRunTime = runTime;
			writer.WriteDouble(3720L, runTime);
			return true;
		}
		return errors.UpdateError("Invalid run time value : " + runTime);
	}

	/// <summary>
	/// Write the mass resolution for the mass spec device to disk.
	/// Used by the scan processing routines.
	/// </summary>
	/// <param name="runHeader">Device's run header object</param>
	/// <param name="writer">Shared memory mapped file accessor</param>
	/// <param name="halfPeakWidth">Width of the half peak.</param>
	/// <param name="errors">Store errors information</param>
	/// <returns>True if mass resolution is written to disk successfully, False otherwise</returns>
	public static bool SaveMassResolution(this RunHeader runHeader, IReadWriteAccessor writer, double halfPeakWidth, DeviceErrors errors)
	{
		double num = 0.01;
		double num2 = 100.0;
		if (halfPeakWidth >= num && halfPeakWidth <= num2)
		{
			runHeader.MassResolution = halfPeakWidth;
			writer.WriteDouble(3712L, halfPeakWidth);
			return true;
		}
		return errors.UpdateError("Invalid mass resolution value : " + halfPeakWidth);
	}

	/// <summary>
	/// try write integer to memory map.
	/// </summary>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="offset">
	/// The offset into the map.
	/// </param>
	/// <param name="value">
	/// The value to write.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// True if value is written to disk successfully.
	/// </returns>
	private static bool TryWriteInt(this IReadWriteAccessor writer, long offset, int value, DeviceErrors errors)
	{
		try
		{
			writer.WriteInt(offset, value);
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the filter mass precision.
	/// </summary>
	/// <param name="runHeader">
	/// The run Header.
	/// </param>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="precision">
	/// The precision.
	/// </param>
	/// <param name="errors">
	/// Store errors information
	/// </param>
	/// <returns>
	/// True if filer mass precision is written to disk successfully, False otherwise
	/// </returns>
	public static bool SaveFilterMassPrecision(this RunHeader runHeader, IReadWriteAccessor writer, int precision, DeviceErrors errors)
	{
		runHeader.FilterMassPrecision = precision;
		return writer.TryWriteInt(7404L, precision, errors);
	}

	/// <summary>
	/// Writes the number of status log to disk.
	/// </summary>
	/// <param name="writer">Shared memory mapped file accessor.</param>
	/// <param name="numStatusLong">The number of status log entries.</param>
	/// <param name="errors">Store the errors information.</param>
	/// <returns>True if number of status log entries is written to disk successfully, False otherwise</returns>
	public static bool SaveNumStatusLog(IReadWriteAccessor writer, int numStatusLong, DeviceErrors errors)
	{
		return writer.TryWriteInt(16L, numStatusLong, errors);
	}

	/// <summary>
	/// Writes the number of error log to disk.
	/// </summary>
	/// <param name="writer">Shared memory mapped file accessor.</param>
	/// <param name="numErrorLog">The number error log.</param>
	/// <param name="errors">Store errors information.</param>
	/// <returns>True if number of error long entries is written to disk successfully, False otherwise</returns>
	public static bool SaveNumErrorLog(IReadWriteAccessor writer, int numErrorLog, DeviceErrors errors)
	{
		return writer.TryWriteInt(20L, numErrorLog, errors);
	}

	/// <summary>
	/// Saves the last scan number.
	/// </summary>
	/// <param name="writer">Shared memory mapped file accessor.</param>
	/// <param name="lastSpec">The last spec.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True if the last spectrum number is written to disk successfully, false otherwise.</returns>
	public static bool SaveScanNum(IReadWriteAccessor writer, int lastSpec, DeviceErrors errors)
	{
		return writer.TryWriteInt(12L, lastSpec, errors);
	}

	/// <summary>
	/// Writes the total number of trailer extra data to disk.
	/// </summary>
	/// <param name="writer">The Shared memory mapped file accessor for RunHeader.</param>
	/// <param name="numLogs">The number of trailer extra entries.</param>
	/// <param name="errors">Store the errors information.</param>
	/// <returns>True if number of trailer extra entries is written to disk successfully, False otherwise</returns>
	public static bool SaveNumTrailerExtra(IReadWriteAccessor writer, int numLogs, DeviceErrors errors)
	{
		return writer.TryWriteInt(7380L, numLogs, errors);
	}

	/// <summary>
	/// Writes the total number of tune data entries to disk.
	/// </summary>
	/// <param name="writer">The Shared memory mapped file accessor for RunHeader.</param>
	/// <param name="numLogs">The number of data entries.</param>
	/// <param name="errors">Store the errors information.</param>
	/// <returns>True if number of tune data entries is written to disk successfully, false otherwise</returns>
	public static bool SaveNumTuneData(IReadWriteAccessor writer, int numLogs, DeviceErrors errors)
	{
		return writer.TryWriteInt(7384L, numLogs, errors);
	}

	/// <summary>
	/// Writes the total number of trailer scan events to disk.
	/// </summary>
	/// <param name="writer">The Shared memory mapped file accessor for RunHeader.</param>
	/// <param name="numScanEvents">The number of trailer scan event entries.</param>
	/// <param name="errors">Store the errors information.</param>
	/// <returns>True if number of trailer scan event entries is written to disk successfully, false otherwise</returns>
	public static bool SaveNumTrailerScanEvents(IReadWriteAccessor writer, int numScanEvents, DeviceErrors errors)
	{
		return writer.TryWriteInt(7376L, numScanEvents, errors);
	}

	/// <summary>
	/// Write the state of the IsInAcquisition flag to indicate that the data acquiring is in progress or completed.
	/// </summary>
	/// <param name="writer">The Shared memory mapped file accessor for RunHeader.</param>
	/// <param name="isInAcq">True is still acquiring, otherwise False .</param>
	/// <param name="errors">Store the errors information.</param>
	/// <returns>True if the state of the data acquiring is written to disk successfully, false otherwise</returns>
	public static bool SaveIsInAcquisition(IReadWriteAccessor writer, bool isInAcq, DeviceErrors errors)
	{
		return writer.TryWriteInt(584L, isInAcq ? 1 : 0, errors);
	}

	/// <summary>
	/// Saves the device run header fields - comment1 and comment2.
	/// </summary>
	/// <param name="runHeader">The device run header.</param>
	/// <param name="writer">The writer.</param>
	/// <param name="comment1">The comment1.</param>
	/// <param name="comment2">The comment2.</param>
	/// <param name="errors">Store the errors information.</param>
	/// <returns>True if these fields are written to disk successfully, false otherwise.</returns>
	public static bool SaveComment1AndComment2(this RunHeader runHeader, IReadWriteAccessor writer, string comment1, string comment2, DeviceErrors errors)
	{
		try
		{
			int num = 80;
			byte[] array = new byte[num + 128];
			runHeader.Comment1 = comment1.Substring(0, Math.Min(comment1.Length, 39));
			runHeader.Comment2 = comment2.Substring(0, Math.Min(comment2.Length, 63));
			Buffer.BlockCopy(runHeader.Comment1.ToCharArray(), 0, array, 0, runHeader.Comment1.Length * 2);
			Buffer.BlockCopy(runHeader.Comment2.ToCharArray(), 0, array, num, runHeader.Comment2.Length * 2);
			writer.WriteBytes(352L, array);
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Updates the run header shared memory mapped buffer.
	/// </summary>
	/// <param name="runHeader">
	/// The run Header.
	/// </param>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>True the run header save to the shared memory map object, otherwise False.</returns>
	public static bool SaveRunHeader(this RunHeader runHeader, IReadWriteAccessor writer, DeviceErrors errors)
	{
		try
		{
			writer.WriteStruct(0L, runHeader.RunHeaderStruct);
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the device run header to the raw file using the  provided binary writer.
	/// </summary>
	/// <param name="runHeader">
	/// The run header.
	/// </param>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public static bool SaveToRawFile(this RunHeader runHeader, BinaryWriter writer, DeviceErrors errors)
	{
		try
		{
			errors.AppendInformataion("Start RunHeader SaveToRawFile");
			writer.Write(WriterHelper.StructToByteArray(runHeader.RunHeaderStruct, RunHeaderStructSize));
			writer.Flush();
			errors.AppendInformataion("End RunHeader SaveToRawFile");
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}
}
