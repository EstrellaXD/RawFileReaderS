using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The sequence row.
/// </summary>
internal class SequenceRow : ISequenceRow, IRawObjectBase
{
	/// <summary>
	/// The number of extra user columns.
	/// added at rev 58...
	/// </summary>
	public const int NumberOfExtraUserColumns = 15;

	/// <summary>
	/// The number of user texts.
	/// </summary>
	public const int NumberOfUserTexts = 5;

	private SeqRowInfoStruct _rowInformation;

	/// <summary>
	/// Gets or sets the barcode.
	/// </summary>
	public string Barcode { get; set; }

	/// <summary>
	/// Gets or sets the barcode status.
	/// </summary>
	public int BarcodeStatus { get; set; }

	/// <summary>
	///     Gets or sets the calibration level
	/// </summary>
	public string CalLevel { get; set; }

	/// <summary>
	/// Gets or sets the calibration file.
	/// </summary>
	public string CalibFile { get; set; }

	/// <summary>
	///     Gets or sets the comment
	/// </summary>
	public string Comment { get; set; }

	/// <summary>
	///     Gets or sets the concentration or dilution factor
	/// </summary>
	public double ConcentrationDilutionFactor
	{
		get
		{
			return _rowInformation.DilutionFactor;
		}
		set
		{
			_rowInformation.DilutionFactor = value;
		}
	}

	/// <summary>
	/// Gets the extra user columns.
	/// </summary>
	public string[] ExtraUserColumns { get; }

	/// <summary>
	///     Gets or sets the injection volume
	/// </summary>
	public double InjectionVolume
	{
		get
		{
			return _rowInformation.InjectionVolume;
		}
		set
		{
			_rowInformation.InjectionVolume = value;
		}
	}

	/// <summary>
	/// Gets or sets the instrument.
	/// </summary>
	public string Inst { get; set; }

	/// <summary>
	///     Gets or sets the internal standard amount
	/// </summary>
	public double InternalStandardAmount
	{
		get
		{
			return _rowInformation.ISTDAmount;
		}
		set
		{
			_rowInformation.ISTDAmount = value;
		}
	}

	/// <summary>
	/// Gets or sets the method.
	/// </summary>
	public string Method { get; set; }

	/// <summary>
	/// Gets or sets the path.
	/// </summary>
	public string Path { get; set; }

	/// <summary>
	/// Gets or sets the raw file name.
	/// </summary>
	public string RawFileName { get; set; }

	/// <summary>
	///     Gets the format revision of this object
	/// </summary>
	public int Revision => _rowInformation.Revision;

	/// <summary>
	///     Gets or sets the sequence row number
	/// </summary>
	public int RowNumber
	{
		get
		{
			return _rowInformation.RowNumber;
		}
		set
		{
			_rowInformation.RowNumber = value;
		}
	}

	/// <summary>
	///     Gets or sets the sample id
	/// </summary>
	public string SampleId { get; set; }

	/// <summary>
	///     Gets or sets the sample name
	/// </summary>
	public string SampleName { get; set; }

	/// <summary>
	///     Gets or sets the (application specific) sample type
	/// </summary>
	public int SampleType
	{
		get
		{
			return _rowInformation.SampleType;
		}
		set
		{
			_rowInformation.SampleType = value;
		}
	}

	/// <summary>
	///     Gets or sets the sample volume
	/// </summary>
	public double SampleVolume
	{
		get
		{
			return _rowInformation.SampleVolume;
		}
		set
		{
			_rowInformation.SampleVolume = value;
		}
	}

	/// <summary>
	///     Gets or sets the sample weight
	/// </summary>
	public double SampleWeight
	{
		get
		{
			return _rowInformation.SampleWeight;
		}
		set
		{
			_rowInformation.SampleWeight = value;
		}
	}

	/// <summary>
	/// Gets the user texts.
	/// </summary>
	public string[] UserTexts { get; }

	/// <summary>
	///     Gets or sets the short vial string (obsolete?)
	/// </summary>
	public string Vial { get; set; }

	/// <summary>
	/// Gets the sequence row struct, only needed internally for writing raw file.
	/// </summary>
	internal SeqRowInfoStruct SequenceRowStruct => _rowInformation;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.SequenceRow" /> class.
	/// </summary>
	public SequenceRow()
	{
		UserTexts = new string[5];
		ExtraUserColumns = new string[15];
	}

	/// <summary>
	/// The load.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int64" />.
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_rowInformation = viewer.ReadStructureExt<SeqRowInfoStruct>(ref startPos);
		CalLevel = viewer.ReadWideCharsExt(ref startPos);
		SampleName = viewer.ReadWideCharsExt(ref startPos);
		SampleId = viewer.ReadWideCharsExt(ref startPos);
		Comment = viewer.ReadWideCharsExt(ref startPos);
		for (int i = 0; i < 5; i++)
		{
			UserTexts[i] = viewer.ReadWideCharsExt(ref startPos);
		}
		Inst = viewer.ReadWideCharsExt(ref startPos);
		Method = viewer.ReadWideCharsExt(ref startPos);
		RawFileName = viewer.ReadWideCharsExt(ref startPos);
		Path = viewer.ReadWideCharsExt(ref startPos);
		if (fileRevision >= 25)
		{
			Vial = viewer.ReadWideCharsExt(ref startPos);
			CalibFile = viewer.ReadWideCharsExt(ref startPos);
		}
		else
		{
			Vial = _rowInformation.VialName;
			CalibFile = string.Empty;
		}
		if (fileRevision >= 41)
		{
			Barcode = viewer.ReadWideCharsExt(ref startPos);
			BarcodeStatus = viewer.ReadIntExt(ref startPos);
		}
		else
		{
			Barcode = string.Empty;
		}
		if (fileRevision >= 58)
		{
			for (int j = 0; j < 15; j++)
			{
				ExtraUserColumns[j] = viewer.ReadWideCharsExt(ref startPos);
			}
		}
		else
		{
			for (int k = 0; k < 15; k++)
			{
				ExtraUserColumns[k] = string.Empty;
			}
		}
		return startPos - dataOffset;
	}
}
