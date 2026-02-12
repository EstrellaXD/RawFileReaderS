using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// Define an editable scan event interface
/// </summary>
internal interface IScanEventEdit : IRawFileReaderScanEvent
{
	/// <summary>
	/// Gets or sets the accurate mass type.
	/// </summary>
	new ScanFilterEnums.AccurateMassTypes AccurateMassType { get; set; }

	/// <summary>
	/// Gets or sets the scan data type.
	/// </summary>
	new ScanFilterEnums.ScanDataTypes ScanDataType { get; set; }

	/// <summary>
	/// Gets or sets the Polarity.
	/// </summary>
	new ScanFilterEnums.PolarityTypes Polarity { get; set; }

	/// <summary>
	/// Gets or sets the scan power setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Constants.ScanFilterEnums.MSOrderTypes" /> for possible values</value>
	new ScanFilterEnums.MSOrderTypes MsOrder { get; set; }

	/// <summary>
	/// Gets or sets the dependent scan setting.
	/// </summary>
	new ScanFilterEnums.IsDependent DependentDataFlag { get; set; }

	/// <summary>
	/// Gets or sets source fragmentation scan setting.
	/// </summary>
	new ScanFilterEnums.OnOffTypes SourceFragmentation { get; set; }

	/// <summary>
	/// Gets or sets the scan type.
	/// </summary>
	new ScanFilterEnums.ScanTypes ScanType { get; set; }

	/// <summary>
	/// Gets or sets the Detector value.
	/// </summary>
	new ScanFilterEnums.DetectorType Detector { get; set; }

	/// <summary>
	/// Gets or sets the mass analyzer type.
	/// </summary>
	new ScanFilterEnums.MassAnalyzerTypes MassAnalyzerType { get; set; }

	/// <summary>
	/// Gets or sets a value which indicates how source fragmentation values are interpreted.
	/// </summary>
	new ScanFilterEnums.VoltageTypes SourceFragmentationType { get; set; }

	/// <summary>
	/// Gets or sets the turbo scan.
	/// </summary>
	new ScanFilterEnums.OnOffTypes TurboScan { get; set; }

	/// <summary>
	/// Gets or sets the ionization mode.
	/// </summary>
	new ScanFilterEnums.IonizationModes IonizationMode { get; set; }

	/// <summary>
	/// Gets or sets the detector value.
	/// </summary>
	new double DetectorValue { get; set; }

	/// <summary>
	/// Gets or sets the corona value.
	/// </summary>
	new ScanFilterEnums.OnOffTypes Corona { get; set; }

	/// <summary>
	/// Gets or sets the wideband.
	/// </summary>
	new ScanFilterEnums.OffOnTypes Wideband { get; set; }

	/// <summary>
	/// Gets or sets the sector scan.
	/// </summary>
	new ScanFilterEnums.SectorScans SectorScan { get; set; }

	/// <summary>
	/// Gets or sets the ultra.
	/// </summary>
	new ScanFilterEnums.OnOffTypes Ultra { get; set; }

	/// <summary>
	/// Gets or sets the lock.
	/// </summary>
	new ScanFilterEnums.OnOffTypes Lock { get; set; }

	/// <summary>
	/// Gets or sets the enhanced.
	/// </summary>
	new ScanFilterEnums.OnOffTypes Enhanced { get; set; }

	/// <summary>
	/// Gets or sets the free region.
	/// </summary>
	new ScanFilterEnums.FreeRegions FreeRegion { get; set; }

	/// <summary>
	/// Gets or sets the multi photon dissociation.
	/// </summary>
	new double MultiPhotonDissociation { get; set; }

	/// <summary>
	/// Gets or sets the multi photon dissociation type.
	/// </summary>
	new ScanFilterEnums.OnAnyOffTypes MultiPhotonDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation.
	/// </summary>
	new double ElectronCaptureDissociation { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation type.
	/// </summary>
	new ScanFilterEnums.OnAnyOffTypes ElectronCaptureDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the pulsed q dissociation.
	/// </summary>
	new double PulsedQDissociation { get; set; }

	/// <summary>
	/// Gets or sets the pulsed q dissociation type.
	/// </summary>
	new ScanFilterEnums.OnOffTypes PulsedQDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the photo ionization.
	/// </summary>
	new ScanFilterEnums.OnOffTypes PhotoIonization { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation type.
	/// </summary>
	new ScanFilterEnums.OnOffTypes ElectronTransferDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the supplemental activation.
	/// </summary>
	new ScanFilterEnums.OffOnTypes SupplementalActivation { get; set; }

	/// <summary>
	/// Gets or sets SPS Multi notch (Synchronous Precursor Selection)
	/// </summary>
	new ScanFilterEnums.OffOnTypes SpsMultiNotch { get; set; }

	/// <summary>
	/// Gets or sets the parameter r.
	/// </summary>
	new ScanFilterEnums.OffOnTypes ParamR { get; set; }

	/// <summary>
	/// Gets or sets the parameter v.
	/// </summary>
	new ScanFilterEnums.OffOnTypes ParamV { get; set; }

	/// <summary>
	/// Gets or sets the parameter a.
	/// </summary>
	new ScanFilterEnums.OffOnTypes ParamA { get; set; }

	/// <summary>
	/// Gets or sets the parameter b.
	/// </summary>
	new ScanFilterEnums.OffOnTypes ParamB { get; set; }

	/// <summary>
	/// Gets or sets the parameter f.
	/// </summary>
	new ScanFilterEnums.OffOnTypes ParamF { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	new string Name { get; set; }

	/// <summary>
	/// Gets or sets the multi state activation.
	/// </summary>
	new ScanFilterEnums.OffOnTypes MultiStateActivation { get; set; }

	/// <summary>
	/// Gets or sets the multiplex.
	/// </summary>
	new ScanFilterEnums.OffOnTypes Multiplex { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID.
	/// </summary>
	new double HigherEnergyCid { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID type.
	/// </summary>
	new ScanFilterEnums.OnOffTypes HigherEnergyCidType { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation.
	/// </summary>
	new double ElectronTransferDissociation { get; set; }

	/// <summary>
	/// Gets or sets the compensation voltage.
	/// </summary>
	new ScanFilterEnums.OnOffTypes CompensationVoltage { get; set; }

	/// <summary>
	/// Gets or sets the compensation voltage type.
	/// </summary>
	new ScanFilterEnums.VoltageTypes CompensationVoltageType { get; set; }

	/// <summary>
	/// Extends flag getter for upper case flags with a setter
	/// </summary>
	new UpperCaseFilterFlags UpperCaseFlags { get; set; }

	/// <summary>
	/// Extends flag getter for lower case flags with a setter
	/// </summary>
	new LowerCaseFilterFlags LowerCaseFlags { get; set; }

	/// <summary>
	/// Extends "flag applied" getter for upper case flags with a setter
	/// </summary>
	new UpperCaseFilterFlags UpperCaseApplied { get; set; }

	/// <summary>
	/// Extends "flag applied" getter for lower case flags with a setter
	/// </summary>
	new LowerCaseFilterFlags LowerCaseApplied { get; set; }

	/// <summary>
	/// Gets or sets the mass ranges.
	/// </summary>
	new MassRangeStruct[] MassRanges { get; set; }
}
