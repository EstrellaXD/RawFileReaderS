using System;
using System.IO;
using System.Text;
using OpenMcdf;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The file header extension.
/// </summary>
internal static class FileHeaderExtension
{
	private static readonly int FileHeaderStructSize = Utilities.StructSizeLookup.Value[3];

	/// <summary>
	/// Extension method to save file header.
	/// </summary>
	/// <param name="fileHeader">
	/// The file header to save.
	/// </param>
	/// <param name="binaryWriter">
	/// The binary writer used to write data.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public static bool Save(this FileHeader fileHeader, BinaryWriter binaryWriter, DeviceErrors errors)
	{
		try
		{
			binaryWriter.Write(WriterHelper.StructToByteArray(fileHeader.FileHeaderStruct, FileHeaderStructSize));
			binaryWriter.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Calculates the instrument method file checksum. It's duplicating exactly the Xcalibur checksum for compound doc file.
	/// </summary>
	/// <param name="rootStorage">The instrument method file (root storage).</param>
	/// <param name="checksum">The calculated checksum value.</param>
	/// <param name="errorMessage">The error message if error occurs.</param>
	/// <returns>True if the calculation is successfully; otherwise false. </returns>
	/// <exception cref="T:System.ArgumentNullException">rootStorage;@Instrument method file storage cannot be NULL.</exception>
	public static bool CalcChecksum(this CFStorage rootStorage, out uint checksum, out string errorMessage)
	{
		if (rootStorage == null)
		{
			throw new ArgumentNullException("rootStorage", "Instrument method file storage cannot be NULL.");
		}
		checksum = 0u;
		string message = null;
		Adler32 adler32 = new Adler32();
		if (OleInstrumentMethodHelper.TryCatchReadStreamData(rootStorage, "LCQ Header", out var bytes, out errorMessage))
		{
			adler32.CalcFileHeader(bytes);
			uint seed = adler32.Checksum;
			rootStorage.VisitEntries(delegate(CFItem cfItem)
			{
				string name = cfItem.Name;
				if (!string.IsNullOrWhiteSpace(name))
				{
					adler32.Calc(seed, Encoding.Unicode.GetBytes(name));
					seed = adler32.Checksum;
					if (cfItem.IsStorage)
					{
						string text = name;
						CFStorage deviceStorage = rootStorage.GetStorage(text);
						if (deviceStorage == null)
						{
							message = "Unable to obtain the device method storage : " + text;
						}
						else
						{
							deviceStorage.VisitEntries(delegate(CFItem cfItemInner)
							{
								if (cfItemInner.IsStream)
								{
									string name2 = cfItemInner.Name;
									adler32.Calc(seed, Encoding.Unicode.GetBytes(name2));
									seed = adler32.Checksum;
									if (OleInstrumentMethodHelper.TryCatchReadStreamData(deviceStorage, name2, out bytes, out message))
									{
										adler32.Calc(seed, bytes);
										seed = adler32.Checksum;
									}
								}
							}, recursive: false);
						}
					}
					else if (cfItem.IsStream && name != "LCQ Header" && OleInstrumentMethodHelper.TryCatchReadStreamData(rootStorage, name, out bytes, out message))
					{
						adler32.Calc(seed, bytes);
						seed = adler32.Checksum;
					}
				}
			}, recursive: false);
			if (!string.IsNullOrWhiteSpace(message))
			{
				errorMessage = message;
				return false;
			}
			checksum = seed;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks if the who modified has changed.
	/// </summary>
	/// <param name="fileHeader">The file header.</param>
	/// <param name="newFileHeader">The new file header.</param>
	/// <returns>True if who modified has changed; otherwise false.</returns>
	public static bool CheckIsWhoModifiedChanged(this FileHeader fileHeader, IFileHeader newFileHeader)
	{
		if (!(fileHeader.WhoModifiedId != newFileHeader.WhoModifiedId))
		{
			return fileHeader.WhoModifiedLogon != newFileHeader.WhoModifiedLogon;
		}
		return true;
	}
}
