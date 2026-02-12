using System;
using System.IO;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The audit trail extension.
/// </summary>
internal static class AuditTrailExtension
{
	/// <summary>
	/// The save.
	/// </summary>
	/// <param name="auditTrail">
	/// The audit Trail.
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
	public static bool Save(this AuditTrail auditTrail, BinaryWriter binaryWriter, DeviceErrors errors)
	{
		try
		{
			int num = auditTrail.AuditDataInfo.Length;
			binaryWriter.Write(num);
			for (int i = 0; i < num; i++)
			{
				binaryWriter.Write(WriterHelper.StructToByteArray(auditTrail.AuditDataInfo[i].AuditDataStruct, Marshal.SizeOf(auditTrail.AuditDataInfo[i].AuditDataStruct)));
				binaryWriter.StringWrite(auditTrail.AuditDataInfo[i].Comment);
			}
			binaryWriter.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the audit trailer to IOleStream.
	/// </summary>
	/// <param name="auditTrail">The audit trail.</param>
	/// <param name="streamer">The IOleStream object.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True if audit trail saved to file; otherwise false.</returns>
	public static bool Save(this AuditTrail auditTrail, Stream streamer, DeviceErrors errors)
	{
		for (int i = 0; i < 4; i++)
		{
			streamer.WriteByte(0);
		}
		return true;
	}
}
