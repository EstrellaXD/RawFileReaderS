using System;
using System.Linq;
using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Encapsulates various information about sample.
/// </summary>
[Serializable]
[DataContract]
public class SampleInformation : CommonCoreDataObject, ISampleInformation
{
	/// <summary>
	/// Max number of user text column count (20)
	/// </summary>
	public const int MaxUserTextColumnCount = 20;

	/// <summary>
	/// Gets or sets the comment about sample (from user).
	/// </summary>
	[DataMember]
	public string Comment { get; set; }

	/// <summary>
	/// Gets or sets the Code to identify sample.
	/// </summary>
	[DataMember]
	public string SampleId { get; set; }

	/// <summary>
	/// Gets or sets the description of sample.
	/// </summary>
	[DataMember]
	public string SampleName { get; set; }

	/// <summary>
	/// Gets or sets the vial or well form auto sampler.
	/// </summary>
	[DataMember]
	public string Vial { get; set; }

	/// <summary>
	/// Gets or sets the amount of sample injected.
	/// </summary>
	[DataMember]
	public double InjectionVolume { get; set; }

	/// <summary>
	/// Gets or sets bar code from scanner (if attached).
	/// </summary>
	[DataMember]
	public string Barcode { get; set; }

	/// <summary>
	/// Gets or sets the bar code status.
	/// </summary>
	[DataMember]
	public BarcodeStatusType BarcodeStatus { get; set; }

	/// <summary>
	/// Gets or sets a name to identify the Calibration or QC level associated with this sample.
	/// Empty if this sample does not contain any calibration compound.
	/// </summary>
	[DataMember]
	public string CalibrationLevel { get; set; }

	/// <summary>
	/// Gets or sets the bulk dilution factor (volume correction) of this sequence row.
	/// </summary>
	[DataMember]
	public double DilutionFactor { get; set; }

	/// <summary>
	/// Gets or sets the instrument method filename of this sequence row.
	/// </summary>
	[DataMember]
	public string InstrumentMethodFile { get; set; }

	/// <summary>
	/// Gets or sets the name of acquired file (excluding path).
	/// </summary>
	[DataMember]
	public string RawFileName { get; set; }

	/// <summary>
	/// Gets or sets the name of calibration file.
	/// </summary>
	[DataMember]
	public string CalibrationFile { get; set; }

	/// <summary>
	/// Gets or sets the ISTD amount of this sequence row.
	/// </summary>
	[DataMember]
	public double IstdAmount { get; set; }

	/// <summary>
	/// Gets or sets the row number.
	/// </summary>
	[DataMember]
	public int RowNumber { get; set; }

	/// <summary>
	/// Gets or sets the path to original data.
	/// </summary>
	[DataMember]
	public string Path { get; set; }

	/// <summary>
	/// Gets or sets the processing method filename of this sequence row.
	/// </summary>
	[DataMember]
	public string ProcessingMethodFile { get; set; }

	/// <summary>
	/// Gets or sets the type of the sample.
	/// </summary>
	[DataMember]
	public SampleType SampleType { get; set; }

	/// <summary>
	/// Gets or sets the sample volume of this sequence row.
	/// </summary>
	[DataMember]
	public double SampleVolume { get; set; }

	/// <summary>
	/// Gets or sets the sample weight of this sequence row.
	/// </summary>
	[DataMember]
	public double SampleWeight { get; set; }

	/// <summary>
	/// Gets or sets the collection of user text.
	/// </summary>
	[DataMember]
	public string[] UserText { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.SampleInformation" /> class.
	/// </summary>
	public SampleInformation()
	{
		UserText = Enumerable.Repeat(string.Empty, 20).ToArray();
	}

	/// <summary>
	/// Create a deep copy of the current object.
	/// </summary>
	/// <returns>A deep copy of the current object.</returns>
	public SampleInformation DeepCopy()
	{
		SampleInformation sampleInformation = (SampleInformation)MemberwiseClone();
		sampleInformation.UserText = new string[20];
		UserText.CopyTo(sampleInformation.UserText, 0);
		return sampleInformation;
	}
}
