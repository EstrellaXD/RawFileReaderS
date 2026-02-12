using System;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The ScanEvent interface.
/// </summary>
internal interface IRawFileReaderScanEvent
{
	/// <summary>
	/// Gets the accurate mass type.
	/// </summary>
	ScanFilterEnums.AccurateMassTypes AccurateMassType { get; }

	/// <summary>
	/// Gets the compensation voltage.
	/// </summary>
	ScanFilterEnums.OnOffTypes CompensationVoltage { get; }

	/// <summary>
	/// Gets the compensation voltage type.
	/// </summary>
	ScanFilterEnums.VoltageTypes CompensationVoltageType { get; }

	/// <summary>
	/// Gets the corona value.
	/// </summary>
	ScanFilterEnums.OnOffTypes Corona { get; }

	/// <summary>
	/// Gets the dependent data.
	/// </summary>
	ScanFilterEnums.IsDependent DependentDataFlag { get; }

	/// <summary>
	/// Gets the Detector value.
	/// </summary>
	ScanFilterEnums.DetectorType Detector { get; }

	/// <summary>
	/// Gets the detector value.
	/// </summary>
	double DetectorValue { get; }

	/// <summary>
	/// Gets the electron capture dissociation.
	/// </summary>
	double ElectronCaptureDissociation { get; }

	/// <summary>
	/// Gets the electron capture dissociation type.
	/// </summary>
	ScanFilterEnums.OnAnyOffTypes ElectronCaptureDissociationType { get; }

	/// <summary>
	/// Gets the electron transfer dissociation.
	/// </summary>
	double ElectronTransferDissociation { get; }

	/// <summary>
	/// Gets the electron transfer dissociation type.
	/// </summary>
	ScanFilterEnums.OnOffTypes ElectronTransferDissociationType { get; }

	/// <summary>
	/// Gets the enhanced.
	/// </summary>
	ScanFilterEnums.OnOffTypes Enhanced { get; }

	/// <summary>
	/// Gets the free region.
	/// </summary>
	ScanFilterEnums.FreeRegions FreeRegion { get; }

	/// <summary>
	/// Gets the higher energy CID.
	/// </summary>
	double HigherEnergyCid { get; }

	/// <summary>
	/// Gets the higher energy CID type.
	/// </summary>
	ScanFilterEnums.OnOffTypes HigherEnergyCidType { get; }

	/// <summary>
	/// Gets the ionization mode.
	/// </summary>
	ScanFilterEnums.IonizationModes IonizationMode { get; }

	/// <summary>
	/// Gets a value indicating whether the scan event is custom - true if trailer
	///     scan event should be used.
	/// </summary>
	bool IsCustom { get; }

	/// <summary>
	/// Gets a value indicating whether the scan event is valid.
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	/// Gets the lock.
	/// </summary>
	ScanFilterEnums.OnOffTypes Lock { get; }

	/// <summary>
	/// Gets the MS order.
	/// </summary>
	ScanFilterEnums.MSOrderTypes MsOrder { get; }

	/// <summary>
	/// Gets the mass analyzer type.
	/// </summary>
	ScanFilterEnums.MassAnalyzerTypes MassAnalyzerType { get; }

	/// <summary>
	/// Gets the mass calibrators.
	/// </summary>
	double[] MassCalibrators { get; }

	/// <summary>
	/// Gets the mass ranges.
	/// </summary>
	MassRangeStruct[] MassRanges { get; }

	/// <summary>
	/// Gets the multi photon dissociation.
	/// </summary>
	double MultiPhotonDissociation { get; }

	/// <summary>
	/// Gets the multi photon dissociation type.
	/// </summary>
	ScanFilterEnums.OnAnyOffTypes MultiPhotonDissociationType { get; }

	/// <summary>
	/// Gets the multi state activation.
	/// </summary>
	ScanFilterEnums.OffOnTypes MultiStateActivation { get; }

	/// <summary>
	/// Gets the multiplex.
	/// </summary>
	ScanFilterEnums.OffOnTypes Multiplex { get; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the parameter a.
	/// </summary>
	ScanFilterEnums.OffOnTypes ParamA { get; }

	/// <summary>
	/// Gets the parameter b.
	/// </summary>
	ScanFilterEnums.OffOnTypes ParamB { get; }

	/// <summary>
	/// Gets the parameter f.
	/// </summary>
	ScanFilterEnums.OffOnTypes ParamF { get; }

	/// <summary>
	/// Gets the parameter k.
	/// </summary>
	ScanFilterEnums.OffOnTypes SpsMultiNotch { get; }

	/// <summary>
	/// Gets the parameter r.
	/// </summary>
	ScanFilterEnums.OffOnTypes ParamR { get; }

	/// <summary>
	/// Gets the parameter v.
	/// </summary>
	ScanFilterEnums.OffOnTypes ParamV { get; }

	/// <summary>
	/// Gets the photo ionization.
	/// </summary>
	ScanFilterEnums.OnOffTypes PhotoIonization { get; }

	/// <summary>
	/// Gets the polarity.
	/// </summary>
	ScanFilterEnums.PolarityTypes Polarity { get; }

	/// <summary>
	/// Gets the pulsed q dissociation.
	/// </summary>
	double PulsedQDissociation { get; }

	/// <summary>
	/// Gets the pulsed q dissociation type.
	/// </summary>
	ScanFilterEnums.OnOffTypes PulsedQDissociationType { get; }

	/// <summary>
	/// Gets the reactions.
	/// </summary>
	Reaction[] Reactions { get; }

	/// <summary>
	/// Gets the scan data type.
	/// </summary>
	ScanFilterEnums.ScanDataTypes ScanDataType { get; }

	/// <summary>
	/// Gets the scan type.
	/// </summary>
	ScanFilterEnums.ScanTypes ScanType { get; }

	/// <summary>
	/// Gets the scan type index. Scan Type Index indicates the segment/scan event for this filter scan event.
	///     HIWORD == segment, LOWORD == scan type
	/// </summary>
	int ScanTypeIndex { get; }

	/// <summary>
	/// Gets the sector scan.
	/// </summary>
	ScanFilterEnums.SectorScans SectorScan { get; }

	/// <summary>
	/// Gets the source fragmentation.
	/// </summary>
	ScanFilterEnums.OnOffTypes SourceFragmentation { get; }

	/// <summary>
	/// Gets the source fragmentation mass ranges.
	/// </summary>
	MassRangeStruct[] SourceFragmentationMassRanges { get; }

	/// <summary>
	/// Gets value to indicate how source fragmentation values are interpreted.
	/// </summary>
	ScanFilterEnums.VoltageTypes SourceFragmentationType { get; }

	/// <summary>
	/// Gets or sets the source fragmentations.
	/// </summary>
	double[] SourceFragmentations { get; set; }

	/// <summary>
	/// Gets the supplemental activation.
	/// </summary>
	ScanFilterEnums.OffOnTypes SupplementalActivation { get; }

	/// <summary>
	/// Gets the turbo scan.
	/// </summary>
	ScanFilterEnums.OnOffTypes TurboScan { get; }

	/// <summary>
	/// Gets the ultra.
	/// </summary>
	ScanFilterEnums.OnOffTypes Ultra { get; }

	/// <summary>
	/// Gets the wideband.
	/// </summary>
	ScanFilterEnums.OffOnTypes Wideband { get; }

	UpperCaseFilterFlags UpperCaseFlags { get; }

	LowerCaseFilterFlags LowerCaseFlags { get; }

	UpperCaseFilterFlags UpperCaseApplied { get; }

	LowerCaseFilterFlags LowerCaseApplied { get; }

	/// <summary>
	/// To the automatic filter string.
	/// </summary>
	/// <param name="scanEvent">The scan event.</param>
	/// <param name="massPrecision">The mass precision.</param>
	/// <param name="charsMax">The chars maximum.</param>
	/// <param name="energyPrecision">The energy precision.</param>
	/// <param name="formatProvider">numeric format (culture)</param>
	/// <param name="listSeparator">culture specific list separator (',' if not specified)</param>
	/// <returns>Auto filter string.</returns>
	string ToAutoFilterString(IRawFileReaderScanEvent scanEvent, int massPrecision = -1, int charsMax = -1, int energyPrecision = -1, IFormatProvider formatProvider = null, string listSeparator = null);

	/// <summary>
	/// Gets the run header filter mass precision.
	/// </summary>
	/// <returns>the run header filter mass precision</returns>
	int GetRunHeaderFilterMassPrecision();
}
