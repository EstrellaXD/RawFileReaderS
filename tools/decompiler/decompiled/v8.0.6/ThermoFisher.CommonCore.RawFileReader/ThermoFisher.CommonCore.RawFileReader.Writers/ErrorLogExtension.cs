using System;
using System.IO;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to write error log item to disk file
/// </summary>
internal static class ErrorLogExtension
{
	/// <summary>
	/// Saves the error log item.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="errorLog">The error log</param>
	/// <param name="errors">Store errors information.</param>
	/// <returns>True if error log item is written to disk successfully, False otherwise</returns>
	public static bool SaveErrorLogItem(this BinaryWriter writer, float retentionTime, string errorLog, DeviceErrors errors)
	{
		try
		{
			writer.Write(retentionTime);
			writer.StringWrite(errorLog);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the error log item.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="errorLog">The error log</param>
	/// <param name="errors">Store errors information.</param>
	/// <returns>True if error log item is written to disk successfully, False otherwise</returns>
	public static bool SaveErrorLogItem(this BinaryWriter writer, float retentionTime, byte[] errorLog, DeviceErrors errors)
	{
		try
		{
			writer.Write(retentionTime);
			writer.Write(errorLog);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}
}
