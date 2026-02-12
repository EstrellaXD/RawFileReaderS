using System;
using System.IO;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The generic data item extension.
/// </summary>
internal static class GenericDataItemExtension
{
	/// <summary>
	/// Saves the generic data item.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="data">The data.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True if generic data item is written to disk successfully, False otherwise</returns>
	public static bool SaveGenericDataItem(this BinaryWriter writer, float retentionTime, byte[] data, DeviceErrors errors)
	{
		try
		{
			writer.Write(retentionTime);
			writer.Write(data);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the generic data item.
	/// </summary>
	/// <param name="writer">The writer.</param>        
	/// <param name="data">The data.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True if generic data item is written to disk successfully, False otherwise</returns>
	public static bool SaveGenericDataItem(this BinaryWriter writer, byte[] data, DeviceErrors errors)
	{
		try
		{
			writer.Write(data);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}
}
