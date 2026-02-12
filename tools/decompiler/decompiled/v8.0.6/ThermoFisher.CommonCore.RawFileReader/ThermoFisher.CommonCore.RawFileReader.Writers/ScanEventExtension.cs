using System;
using System.IO;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides save extension methods for writing scan event object to disk.
/// </summary>
internal static class ScanEventExtension
{
	private static readonly int MassSpecReactionStructSize = Utilities.StructSizeLookup.Value[10];

	/// <summary>
	/// Saves a scan event.
	/// </summary>
	/// <param name="scanEvent">The scan event.</param>
	/// <param name="writer">The writer.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True if a scan event saved to disk; false otherwise.</returns>
	/// <exception cref="T:System.ArgumentNullException">mass Spec Scan Events</exception>
	/// <exception cref="T:System.ArgumentNullException">mass Spec Scan Events</exception>
	public static bool SaveScanEvent(this IScanEvent scanEvent, BinaryWriter writer, DeviceErrors errors)
	{
		if (scanEvent == null)
		{
			throw new ArgumentNullException("scanEvent").UpdateErrorsException(errors);
		}
		byte[] buffer = ((scanEvent is ScanEvent scanEvent2) ? scanEvent2.GetBytesFromScanEventInfo() : CreateBytesFromEvent(scanEvent));
		try
		{
			writer.Write(buffer);
			int massCount = scanEvent.MassCount;
			writer.Write(massCount);
			for (int i = 0; i < massCount; i++)
			{
				IReaction reaction = scanEvent.GetReaction(i);
				uint num = ((reaction.ActivationType >= ActivationType.LastActivation) ? 510u : ((uint)((int)reaction.ActivationType << 1)));
				num = ((!reaction.MultipleActivation) ? (num & 0xFFFFEFFFu) : (num | 0x1000));
				if (reaction.CollisionEnergyValid)
				{
					num |= 1;
				}
				MsReactionStruct objStruct = new MsReactionStruct
				{
					PrecursorMass = reaction.PrecursorMass,
					IsolationWidth = reaction.IsolationWidth,
					CollisionEnergy = reaction.CollisionEnergy,
					CollisionEnergyValid = num,
					RangeIsValid = reaction.PrecursorRangeIsValid,
					FirstPrecursorMass = reaction.FirstPrecursorMass,
					LastPrecursorMass = reaction.LastPrecursorMass,
					IsolationWidthOffset = reaction.IsolationWidthOffset
				};
				writer.Write(WriterHelper.StructToByteArray(objStruct, MassSpecReactionStructSize));
			}
			WriteMassRanges(scanEvent.GetMassRange, writer, scanEvent.MassRangeCount);
			WriteDoubles(scanEvent.GetMassCalibrator, writer, scanEvent.MassCalibratorCount);
			WriteDoubles(scanEvent.GetSourceFragmentationInfo, writer, scanEvent.SourceFragmentationInfoCount);
			WriteMassRanges(scanEvent.GetSourceFragmentationMassRange, writer, scanEvent.SourceFragmentationMassRangeCount);
			writer.StringWrite(scanEvent.Name);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Create byte array from event.
	/// Formats a "ScanEventInfoStruct" from the supplied data, and returns as a byte array.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	/// <returns>
	/// The byte array
	/// </returns>
	private static byte[] CreateBytesFromEvent(IScanEvent scanEvent)
	{
		return WriterHelper.StructToByteArray(ScanEvent.CreateEventInfo(scanEvent), ScanEvent.ScanEventInfoStructSize);
	}

	/// <summary>
	/// write a table of mass ranges.
	/// </summary>
	/// <param name="getRange">
	/// The method to get a range.
	/// </param>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="numRanges">
	/// The number of ranges.
	/// </param>
	private static void WriteMassRanges(Func<int, IRangeAccess> getRange, BinaryWriter writer, int numRanges)
	{
		writer.Write(numRanges);
		for (int i = 0; i < numRanges; i++)
		{
			IRangeAccess rangeAccess = getRange(i);
			writer.Write(rangeAccess.Low);
			writer.Write(rangeAccess.High);
		}
	}

	/// <summary>
	/// write a table of doubles.
	/// </summary>
	/// <param name="getValue">
	/// The method to get a value to write.
	/// </param>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="numValues">
	/// The number of values.
	/// </param>
	private static void WriteDoubles(Func<int, double> getValue, BinaryWriter writer, int numValues)
	{
		writer.Write(numValues);
		for (int i = 0; i < numValues; i++)
		{
			double value = getValue(i);
			writer.Write(value);
		}
	}
}
