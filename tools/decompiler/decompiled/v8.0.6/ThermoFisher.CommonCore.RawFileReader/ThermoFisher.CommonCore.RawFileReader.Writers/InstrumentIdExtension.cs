using System;
using System.IO;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Save the instrument data information to the disk file
/// </summary>
internal static class InstrumentIdExtension
{
	/// <summary>
	/// Saves the instrument information.
	/// </summary>
	/// <param name="instId">The instrument identifier.</param>
	/// <param name="writer">The writer.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True instrument ID saved to the disk, false otherwise.</returns>
	public static bool Save(this IInstrumentDataAccess instId, BinaryWriter writer, DeviceErrors errors)
	{
		try
		{
			writer.Write(instId.IsValid ? 1 : 0);
			writer.Write((int)instId.Units.ToAbsorbanceUnits());
			string[] channelLabels = instId.ChannelLabels;
			writer.Write(channelLabels.Length);
			string[] array = channelLabels;
			foreach (string value in array)
			{
				writer.StringWrite(value);
			}
			writer.StringWrite(instId.Name);
			writer.StringWrite(instId.Model);
			writer.StringWrite(instId.SerialNumber);
			writer.StringWrite(instId.SoftwareVersion);
			writer.StringWrite(instId.HardwareVersion);
			writer.StringWrite(instId.Flags);
			writer.StringWrite(instId.AxisLabelX);
			writer.StringWrite(instId.AxisLabelY);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the instrument information.
	/// </summary>
	/// <param name="instId">The instrument identifier.</param>
	/// <param name="writer">The writer.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True instrument ID saved to the disk, false otherwise.</returns>
	public static bool Save(this byte[] instId, BinaryWriter writer, DeviceErrors errors)
	{
		try
		{
			writer.Write(instId);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}
}
