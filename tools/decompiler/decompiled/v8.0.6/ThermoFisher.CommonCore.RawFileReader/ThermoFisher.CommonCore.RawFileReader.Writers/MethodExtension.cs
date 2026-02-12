using System;
using System.IO;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The method extension.
/// </summary>
internal static class MethodExtension
{
	/// <summary>
	/// Saves the instrument method to the provided binary writer.
	/// </summary>
	/// <param name="embeddedMethod">Method data to save</param>
	/// <param name="binaryWriter">Writer to use to save</param>
	/// <param name="errors">Any error information that occurred</param>
	/// <returns>True is successful</returns>
	public static bool Save(this EmbeddedMethod embeddedMethod, BinaryWriter binaryWriter, DeviceErrors errors)
	{
		try
		{
			Method instrumentMethod = embeddedMethod.InstrumentMethod;
			if (instrumentMethod.FileHeader.Save(binaryWriter, errors))
			{
				binaryWriter.Write(WriterHelper.StructToByteArray(instrumentMethod.MethodInfoStruct, Marshal.SizeOf(instrumentMethod.MethodInfoStruct)));
				binaryWriter.StringWrite(instrumentMethod.OriginalStorageName);
				binaryWriter.Write(instrumentMethod.StorageDescriptions.Count);
				foreach (StorageDescription storageDescription in instrumentMethod.StorageDescriptions)
				{
					binaryWriter.StringWrite(storageDescription.Description);
					binaryWriter.StringWrite(storageDescription.StorageName);
				}
				if (embeddedMethod.IsPartialSave)
				{
					string originalStorageName = embeddedMethod.InstrumentMethod.OriginalStorageName;
					if (!string.IsNullOrWhiteSpace(originalStorageName) && File.Exists(originalStorageName))
					{
						using FileStream fileStream = new FileStream(instrumentMethod.OriginalStorageName, FileMode.Open, FileAccess.Read);
						fileStream.CopyTo(binaryWriter.BaseStream, instrumentMethod.MethodSize);
						embeddedMethod.EndOffset = binaryWriter.BaseStream.Position;
					}
				}
				else if (embeddedMethod.EndOffset != -1)
				{
					binaryWriter.Seek((int)embeddedMethod.EndOffset, SeekOrigin.Begin);
				}
				binaryWriter.Flush();
			}
			return true;
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex);
		}
		return false;
	}
}
