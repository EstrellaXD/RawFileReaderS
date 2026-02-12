using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The SampleInformation interface.
/// </summary>
public interface ISampleInformation
{
	/// <summary>
	/// Gets or sets the comment about sample (from user).
	/// </summary>
	[DataMember]
	string Comment { get; set; }

	/// <summary>
	/// Gets or sets the Code to identify sample.
	/// </summary>
	[DataMember]
	string SampleId { get; set; }

	/// <summary>
	/// Gets or sets the description of sample.
	/// </summary>
	[DataMember]
	string SampleName { get; set; }

	/// <summary>
	/// Gets or sets the vial or well form auto sampler.
	/// </summary>
	[DataMember]
	string Vial { get; set; }

	/// <summary>
	/// Gets or sets the amount of sample injected.
	/// </summary>
	[DataMember]
	double InjectionVolume { get; set; }

	/// <summary>
	/// Gets or sets bar code from scanner (if attached).
	/// </summary>
	[DataMember]
	string Barcode { get; set; }

	/// <summary>
	/// Gets or sets the bar code status.
	/// </summary>
	[DataMember]
	BarcodeStatusType BarcodeStatus { get; set; }

	/// <summary>
	/// Gets or sets a name to identify the Calibration or QC level associated with this sample.
	/// Empty if this sample does not contain any calibration compound.
	/// </summary>
	[DataMember]
	string CalibrationLevel { get; set; }

	/// <summary>
	/// Gets or sets the bulk dilution factor (volume correction) of this sequence row.
	/// </summary>
	[DataMember]
	double DilutionFactor { get; set; }

	/// <summary>
	/// Gets or sets the instrument method filename of this sequence row.
	/// </summary>
	[DataMember]
	string InstrumentMethodFile { get; set; }

	/// <summary>
	/// Gets or sets the name of acquired file (excluding path).
	/// </summary>
	[DataMember]
	string RawFileName { get; set; }

	/// <summary>
	/// Gets or sets the name of calibration file.
	/// </summary>
	[DataMember]
	string CalibrationFile { get; set; }

	/// <summary>
	/// Gets or sets the ISTD amount of this sequence row.
	/// </summary>
	[DataMember]
	double IstdAmount { get; set; }

	/// <summary>
	/// Gets or sets the row number.
	/// </summary>
	[DataMember]
	int RowNumber { get; set; }

	/// <summary>
	/// Gets or sets the path to original data.
	/// </summary>
	[DataMember]
	string Path { get; set; }

	/// <summary>
	/// Gets or sets the processing method filename of this sequence row.
	/// </summary>
	[DataMember]
	string ProcessingMethodFile { get; set; }

	/// <summary>
	/// Gets or sets the type of the sample.
	/// </summary>
	[DataMember]
	SampleType SampleType { get; set; }

	/// <summary>
	/// Gets or sets the sample volume of this sequence row.
	/// </summary>
	[DataMember]
	double SampleVolume { get; set; }

	/// <summary>
	/// Gets or sets the sample weight of this sequence row.
	/// </summary>
	[DataMember]
	double SampleWeight { get; set; }

	/// <summary>
	/// Gets or sets the collection of user text.
	/// </summary>
	[DataMember]
	string[] UserText { get; set; }
}
