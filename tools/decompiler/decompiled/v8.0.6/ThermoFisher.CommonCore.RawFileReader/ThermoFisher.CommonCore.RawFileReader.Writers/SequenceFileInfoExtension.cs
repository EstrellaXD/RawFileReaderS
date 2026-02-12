using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides custom extension methods for sequence file info, such as Save().
/// </summary>
internal static class SequenceFileInfoExtension
{
	/// <summary>
	/// Saves the sequence file information struct to file.
	/// </summary>
	/// <param name="info">The information.</param>
	/// <param name="writer">The writer.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True sequence file info saved to sequence file; false otherwise.</returns>
	public static bool Save(this SequenceFileInfoStruct info, BinaryWriter writer, DeviceErrors errors)
	{
		try
		{
			writer.Write(info.GetSequenceInfo());
			string[] userLabel = info.UserLabel;
			foreach (string value in userLabel)
			{
				writer.StringWrite(value);
			}
			writer.StringWrite(info.TrayConfiguration);
			userLabel = info.UserPrivateLabel;
			foreach (string value2 in userLabel)
			{
				writer.StringWrite(value2);
			}
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}
}
