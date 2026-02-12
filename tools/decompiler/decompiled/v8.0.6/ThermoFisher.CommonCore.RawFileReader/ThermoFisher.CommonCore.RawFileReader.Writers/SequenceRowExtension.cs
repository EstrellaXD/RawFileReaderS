using System;
using System.Collections.Generic;
using System.IO;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The sequence row extension.
/// </summary>
internal static class SequenceRowExtension
{
	private static readonly int SeqRowInfoStructSize = Utilities.StructSizeLookup.Value[4];

	/// <summary>
	/// Extension method to save file header.
	/// </summary>
	/// <param name="sequenceRow">
	/// The sequence Row.
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
	public static bool Save(this SequenceRow sequenceRow, BinaryWriter binaryWriter, DeviceErrors errors)
	{
		errors.AppendInformataion("Start Save Sequence Row");
		try
		{
			binaryWriter.Write(WriterHelper.StructToByteArray(sequenceRow.SequenceRowStruct, SeqRowInfoStructSize));
			binaryWriter.StringWrite(sequenceRow.CalLevel);
			binaryWriter.StringWrite(sequenceRow.SampleName);
			binaryWriter.StringWrite(sequenceRow.SampleId);
			binaryWriter.StringWrite(sequenceRow.Comment);
			string[] userTexts = sequenceRow.UserTexts;
			foreach (string value in userTexts)
			{
				binaryWriter.StringWrite(value);
			}
			binaryWriter.StringWrite(sequenceRow.Inst);
			binaryWriter.StringWrite(sequenceRow.Method);
			binaryWriter.StringWrite(sequenceRow.RawFileName);
			binaryWriter.StringWrite(sequenceRow.Path);
			binaryWriter.StringWrite(sequenceRow.Vial);
			binaryWriter.StringWrite(sequenceRow.CalibFile);
			binaryWriter.StringWrite(sequenceRow.Barcode);
			binaryWriter.Write(sequenceRow.BarcodeStatus);
			userTexts = sequenceRow.ExtraUserColumns;
			foreach (string value2 in userTexts)
			{
				binaryWriter.StringWrite(value2);
			}
			binaryWriter.Flush();
			errors.AppendInformataion("End Save Sequence Row");
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Saves the specified binary writer.
	/// </summary>
	/// <param name="samples">The samples.</param>
	/// <param name="binaryWriter">The binary writer.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True samples saved to file; false otherwise.</returns>
	public static bool Save(this IReadOnlyList<SampleInformation> samples, BinaryWriter binaryWriter, DeviceErrors errors)
	{
		try
		{
			if (samples == null || samples.Count == 0)
			{
				binaryWriter.Write(0);
				return true;
			}
			binaryWriter.Write(samples.Count);
			foreach (SampleInformation sample in samples)
			{
				SeqRowInfoStruct objStruct = new SeqRowInfoStruct
				{
					Revision = 0,
					RowNumber = sample.RowNumber,
					SampleType = (int)sample.SampleType,
					VialName = sample.Vial,
					InjectionVolume = sample.InjectionVolume,
					SampleWeight = sample.SampleWeight,
					SampleVolume = sample.SampleVolume,
					ISTDAmount = sample.IstdAmount,
					DilutionFactor = sample.DilutionFactor
				};
				binaryWriter.Write(WriterHelper.StructToByteArray(objStruct, SeqRowInfoStructSize));
				binaryWriter.StringWrite(sample.CalibrationLevel);
				binaryWriter.StringWrite(sample.SampleName);
				binaryWriter.StringWrite(sample.SampleId);
				binaryWriter.StringWrite(sample.Comment);
				for (int i = 0; i < 5; i++)
				{
					binaryWriter.StringWrite(sample.UserText[i]);
				}
				binaryWriter.StringWrite(sample.InstrumentMethodFile);
				binaryWriter.StringWrite(sample.ProcessingMethodFile);
				binaryWriter.StringWrite(sample.RawFileName);
				binaryWriter.StringWrite(sample.Path);
				binaryWriter.StringWrite(sample.Vial);
				binaryWriter.StringWrite(sample.CalibrationFile);
				binaryWriter.StringWrite(sample.Barcode);
				binaryWriter.Write((int)sample.BarcodeStatus);
				int num = 20;
				for (int j = 5; j < num; j++)
				{
					binaryWriter.StringWrite(sample.UserText[j]);
				}
				binaryWriter.Flush();
			}
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}
}
