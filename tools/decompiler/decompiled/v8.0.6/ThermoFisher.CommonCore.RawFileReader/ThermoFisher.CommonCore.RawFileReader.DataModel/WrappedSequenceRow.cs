using System;
using System.Linq;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped sequence row. Converts internal data to public type.
/// </summary>
internal class WrappedSequenceRow : SampleInformation
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedSequenceRow" /> class.
	/// </summary>
	/// <param name="sequenceRow">The sequence row.</param>
	/// <exception cref="T:System.ArgumentNullException">Thrown when sequenceRow is null</exception>
	public WrappedSequenceRow(ISequenceRow sequenceRow)
	{
		if (sequenceRow == null)
		{
			throw new ArgumentNullException("sequenceRow");
		}
		CopyFrom(sequenceRow);
	}

	/// <summary>
	/// Copies from.
	/// </summary>
	/// <param name="seqRow">The sequence row.</param>
	private void CopyFrom(ISequenceRow seqRow)
	{
		if (seqRow != null)
		{
			base.RowNumber = seqRow.RowNumber;
			base.SampleType = (SampleType)seqRow.SampleType;
			base.Path = seqRow.Path;
			base.RawFileName = seqRow.RawFileName;
			base.SampleName = seqRow.SampleName;
			base.SampleName = seqRow.SampleName;
			base.SampleId = seqRow.SampleId;
			base.Comment = seqRow.Comment;
			base.CalibrationLevel = seqRow.CalLevel;
			base.Barcode = seqRow.Barcode;
			base.BarcodeStatus = (BarcodeStatusType)seqRow.BarcodeStatus;
			base.Vial = seqRow.Vial;
			base.InjectionVolume = seqRow.InjectionVolume;
			base.SampleWeight = seqRow.SampleWeight;
			base.SampleVolume = seqRow.SampleVolume;
			base.IstdAmount = seqRow.InternalStandardAmount;
			base.DilutionFactor = seqRow.ConcentrationDilutionFactor;
			base.CalibrationFile = seqRow.CalibFile;
			base.InstrumentMethodFile = seqRow.Inst;
			base.ProcessingMethodFile = seqRow.Method;
			int count = seqRow.ExtraUserColumns.Length + seqRow.UserTexts.Length;
			int num = 0;
			base.UserText = Enumerable.Repeat(string.Empty, count).ToArray();
			string[] userTexts = seqRow.UserTexts;
			foreach (string text in userTexts)
			{
				base.UserText[num++] = text;
			}
			userTexts = seqRow.ExtraUserColumns;
			foreach (string text2 in userTexts)
			{
				base.UserText[num++] = text2;
			}
		}
	}
}
