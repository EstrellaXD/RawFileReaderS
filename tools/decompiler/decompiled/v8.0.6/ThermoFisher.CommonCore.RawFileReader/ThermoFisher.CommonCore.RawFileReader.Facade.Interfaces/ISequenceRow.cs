namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
///     The Sequence Row interface.
/// </summary>
internal interface ISequenceRow
{
	/// <summary>
	/// Gets the barcode.
	/// </summary>
	string Barcode { get; }

	/// <summary>
	/// Gets the barcode status.
	/// </summary>
	int BarcodeStatus { get; }

	/// <summary>
	///     Gets the calibration level
	/// </summary>
	string CalLevel { get; }

	/// <summary>
	/// Gets the calibration file.
	/// </summary>
	string CalibFile { get; }

	/// <summary>
	///     Gets the comment
	/// </summary>
	string Comment { get; }

	/// <summary>
	///     Gets the concentration or dilution factor
	/// </summary>
	double ConcentrationDilutionFactor { get; }

	/// <summary>
	/// Gets the extra user columns.
	/// </summary>
	string[] ExtraUserColumns { get; }

	/// <summary>
	///     Gets the injection volume
	/// </summary>
	double InjectionVolume { get; }

	/// <summary>
	/// Gets the instrument.
	/// </summary>
	string Inst { get; }

	/// <summary>
	///     Gets the internal standard amount
	/// </summary>
	double InternalStandardAmount { get; }

	/// <summary>
	/// Gets the method.
	/// </summary>
	string Method { get; }

	/// <summary>
	/// Gets the path.
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Gets the raw file name.
	/// </summary>
	string RawFileName { get; }

	/// <summary>
	///     Gets the format revision of this object
	/// </summary>
	int Revision { get; }

	/// <summary>
	///     Gets the sequence row number
	/// </summary>
	int RowNumber { get; }

	/// <summary>
	///     Gets the sample id
	/// </summary>
	string SampleId { get; }

	/// <summary>
	///     Gets the sample name
	/// </summary>
	string SampleName { get; }

	/// <summary>
	///     Gets the (application specific) sample type
	/// </summary>
	int SampleType { get; }

	/// <summary>
	///     Gets the sample volume
	/// </summary>
	double SampleVolume { get; }

	/// <summary>
	///     Gets the sample weight
	/// </summary>
	double SampleWeight { get; }

	/// <summary>
	/// Gets the user texts.
	/// </summary>
	string[] UserTexts { get; }

	/// <summary>
	///     Gets the short vial string (obsolete?)
	/// </summary>
	string Vial { get; }
}
