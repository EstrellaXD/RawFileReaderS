using System;
using System.IO;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The auto sampler config extension.
/// </summary>
internal static class AutoSamplerConfigExtension
{
	/// <summary>
	/// The save.
	/// </summary>
	/// <param name="autoSampleConfig">
	/// The auto sample config.
	/// </param>
	/// <param name="binaryWriter">
	/// The binary writer.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public static bool Save(this AutoSamplerConfig autoSampleConfig, BinaryWriter binaryWriter, DeviceErrors errors)
	{
		errors.AppendInformataion("Start Save Autosampler Config");
		try
		{
			binaryWriter.Write(WriterHelper.StructToByteArray(autoSampleConfig.AutoSamplerConfigStruct, Marshal.SizeOf(autoSampleConfig.AutoSamplerConfigStruct)));
			binaryWriter.StringWrite(autoSampleConfig.TrayName);
			binaryWriter.Flush();
			errors.AppendInformataion("End Save Autosampler Config");
			return true;
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex);
		}
		return false;
	}
}
