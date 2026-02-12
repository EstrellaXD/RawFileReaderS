using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ScanFilter interface defines a set of rules for selecting scans.
/// For example: Testing if a scan should be included in a chromatogram.
/// Many if the rules include an "Any" choice, which implies that this rule
/// will not be tested.
/// Testing logic is included in the class <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanFilterHelper" />
/// The class <see cref="T:ThermoFisher.CommonCore.Data.Extensions" /> contains methods related to filters, the filter helper and raw data.
/// </summary>
public interface IScanFilter : IScanEventBase
{
	/// <summary>
	/// Gets the accurate mass filter rule.
	/// </summary>
	FilterAccurateMass AccurateMass { get; }

	/// <summary>
	/// Gets or sets the mass precision, which is used to format the filter (in ToString).
	/// </summary>
	int MassPrecision { get; set; }

	/// <summary>
	/// Gets or sets additional instrument defined filters (these are bit flags).
	/// See enum MetaFilterType.
	/// </summary>
	int MetaFilters { get; set; }

	/// <summary>
	/// Gets the number of unique masses, taking into account multiple activations.
	/// For example: If this is MS3 data, there are two  "parent masses",
	/// but they may have multiple reactions applied.
	/// If the first stage has two reactions, then there are a total of
	/// 3 reactions, for the 2 "unique masses"
	/// </summary>
	int UniqueMassCount { get; }

	/// <summary>
	/// Gets or sets an array of values which determines if the source fragmentation values are valid.
	/// </summary>
	SourceFragmentationInfoValidType[] SourceFragmentationInfoValid { get; set; }

	/// <summary>
	/// Gets or sets the locale name.
	/// ISO 639-1 standard language code.
	/// for example "en-us" for us English.
	/// This can be used to affect string conversion.
	/// </summary>
	string LocaleName { get; set; }

	/// <summary>
	/// Gets the number of compensation voltage values.
	/// This is the number of values related to <c>cv</c> mode.
	/// 1 for "single value", 2 for "ramp" and 1 per mass range for "SIM".
	/// </summary>
	int CompensationVoltageCount { get; }

	/// <summary>
	/// Gets the number of Source Fragmentation values.
	/// This is the number of values related to <c>sid</c> mode.
	/// 1 for "single value", 2 for "ramp" and 1 per mass range for "SIM".
	/// </summary>
	int SourceFragmentationValueCount { get; }

	/// <summary>
	/// Gets or sets the scan data type (centroid or profile) filtering rule.
	/// </summary>
	new ScanDataType ScanData { get; set; }

	/// <summary>
	/// Gets or sets the polarity (+/-) filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.PolarityType" /> for possible values</value>
	new PolarityType Polarity { get; set; }

	/// <summary>
	/// Gets or sets the scan power or MS/MS mode filtering rule, such as MS3 or Parent scan.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MSOrderType" /> for possible values</value>
	new MSOrderType MSOrder { get; set; }

	/// <summary>
	/// Gets or sets the dependent scan filtering rule.
	/// </summary>
	new TriState Dependent { get; set; }

	/// <summary>
	/// Gets or sets source fragmentation scan filtering rule.
	/// </summary>
	new TriState SourceFragmentation { get; set; }

	/// <summary>
	/// Gets or sets the source fragmentation type filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SourceFragmentationValueType" /> for possible values</value>
	new SourceFragmentationValueType SourceFragmentationType { get; set; }

	/// <summary>
	/// Gets or sets the scan type filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.ScanModeType" /> for possible values</value>
	new ScanModeType ScanMode { get; set; }

	/// <summary>
	/// Gets or sets the mass analyzer scan filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MassAnalyzerType" /> for possible values</value>
	new MassAnalyzerType MassAnalyzer { get; set; }

	/// <summary>
	/// Gets or sets the detector scan filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.DetectorType" /> for possible values</value>
	new DetectorType Detector { get; set; }

	/// <summary>
	/// Gets or sets the turbo scan filtering rule.
	/// </summary>
	new TriState TurboScan { get; set; }

	/// <summary>
	/// Gets or sets the ionization mode filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.IonizationModeType" /> for possible values</value>
	new IonizationModeType IonizationMode { get; set; }

	/// <summary>
	/// Gets or sets the corona scan filtering rule.
	/// </summary>
	new TriState Corona { get; set; }

	/// <summary>
	/// Gets or sets the detector value.
	/// This is used for filtering when the Detector filter is enabled.
	/// </summary>
	/// <value>Floating point detector value</value>
	new double DetectorValue { get; set; }

	/// <summary>
	/// Gets or sets the wideband filtering rule.
	/// </summary>
	new TriState Wideband { get; set; }

	/// <summary>
	/// Gets or sets the sector scan filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SectorScanType" /> for possible values</value>
	new SectorScanType SectorScan { get; set; }

	/// <summary>
	/// Gets or sets the lock scan filtering rule.
	/// </summary>
	new TriState Lock { get; set; }

	/// <summary>
	/// Gets or sets the field free region filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.FieldFreeRegionType" /> for possible values</value>
	new FieldFreeRegionType FieldFreeRegion { get; set; }

	/// <summary>
	/// Gets or sets the ultra scan filtering rule.
	/// </summary>
	new TriState Ultra { get; set; }

	/// <summary>
	/// Gets or sets the enhanced scan filtering rule.
	/// </summary>
	new TriState Enhanced { get; set; }

	/// <summary>
	/// Gets or sets the multi-photon dissociation filtering rule.
	/// </summary>
	new TriState MultiplePhotonDissociation { get; set; }

	/// <summary>
	/// Gets or sets the multi-photon dissociation value.
	/// </summary>
	/// <value>Floating point multi-photon dissociation value</value>
	new double MultiplePhotonDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation filtering rule.
	/// </summary>
	new TriState ElectronCaptureDissociation { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation value.
	/// </summary>
	/// <value>Floating point electron capture dissociation value</value>
	new double ElectronCaptureDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the photo ionization filtering rule.
	/// </summary>
	new TriState PhotoIonization { get; set; }

	/// <summary>
	/// Gets or sets pulsed dissociation filtering rule.
	/// </summary>
	new TriState PulsedQDissociation { get; set; }

	/// <summary>
	/// Gets or sets the pulsed dissociation value.
	/// Only applies when the PulsedQDissociation rule is used.
	/// </summary>
	/// <value>Floating point pulsed dissociation value</value>
	new double PulsedQDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation filtering rule.
	/// </summary>
	new TriState ElectronTransferDissociation { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation value.
	/// Only used when the "ElectronTransferDissociation" rule is used.
	/// </summary>
	/// <value>Floating point electron transfer dissociation value</value>
	new double ElectronTransferDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID filtering rule.
	/// </summary>
	new TriState HigherEnergyCiD { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID value.
	/// Only applies when the "HigherEnergyCiD" rule is used.
	/// </summary>
	new double HigherEnergyCiDValue { get; set; }

	/// <summary>
	/// Gets or sets the Multiplex type filtering rule.
	/// </summary>
	new TriState Multiplex { get; set; }

	/// <summary>
	/// Gets or sets the parameter a filtering rule..
	/// </summary>
	new TriState ParamA { get; set; }

	/// <summary>
	/// Gets or sets the parameter b filtering rule..
	/// </summary>
	new TriState ParamB { get; set; }

	/// <summary>
	/// Gets or sets the parameter f filtering rule..
	/// </summary>
	new TriState ParamF { get; set; }

	/// <summary>
	/// Gets or sets the SPS (Synchronous Precursor Selection) Multi notch filtering rule.
	/// </summary>
	new TriState MultiNotch { get; set; }

	/// <summary>
	/// Gets or sets the parameter r filtering rule..
	/// </summary>
	new TriState ParamR { get; set; }

	/// <summary>
	/// Gets or sets the parameter v filtering rule..
	/// </summary>
	new TriState ParamV { get; set; }

	/// <summary>
	/// Gets or sets the event Name. Used for "compound name" filtering.
	/// </summary>
	new string Name { get; set; }

	/// <summary>
	/// Gets or sets supplemental activation type filter rule.
	/// </summary>
	new TriState SupplementalActivation { get; set; }

	/// <summary>
	/// Gets or sets MultiStateActivation type filtering rule.
	/// </summary>
	new TriState MultiStateActivation { get; set; }

	/// <summary>
	/// Gets or sets Compensation Voltage filtering rule.
	/// </summary>
	new TriState CompensationVoltage { get; set; }

	/// <summary>
	/// Gets or sets Compensation Voltage type filtering rule.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.CompensationVoltageType" /> for possible values</value>
	new CompensationVoltageType CompensationVoltType { get; set; }

	/// <summary>
	/// Get source fragmentation info valid, at zero based index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.SourceFragmentationInfoValidType" />.
	/// </returns>
	SourceFragmentationInfoValidType GetSourceFragmentationInfoValid(int index);

	/// <summary>
	/// Convert an index to multiple activation index.
	/// Converts a simple mass index to the index to the unique mass,
	/// taking into account multiple activations.
	/// </summary>
	/// <param name="index">
	/// The index to convert.
	/// </param>
	/// <returns>
	/// The index of the unique mass.
	/// </returns>
	int IndexToMultipleActivationIndex(int index);

	/// <summary>
	/// Convert to string.
	/// Mass values are converted as per the precision.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	new string ToString();

	/// <summary>
	/// Retrieves a compensation voltage (cv) value at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanFilter.CompensationVoltageCount" /> to get the count of
	/// compensation voltage values.
	/// </remarks>
	/// <param name="index">
	/// Index of compensation voltage to be retrieved
	/// </param>
	/// <returns>
	/// Compensation voltage value (cv) at 0-based index
	/// </returns>
	double CompensationVoltageValue(int index);

	/// <summary>
	/// Retrieves a source fragmentation value (sid) at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanFilter.SourceFragmentationValueCount" /> to get the count of
	/// source fragmentation values.
	/// </remarks>
	/// <param name="index">
	/// Index of source fragmentation value to be retrieved
	/// </param>
	/// <returns>
	/// Source Fragmentation Value (sid) at 0-based index
	/// </returns>
	double SourceFragmentationValue(int index);

	/// <summary>
	/// Set the table of mass ranges
	/// </summary>
	/// <param name="ranges">List of mass ranges (which may be an empty list)</param>
	void SetMassRanges(IList<IRangeAccess> ranges)
	{
	}

	/// <summary>
	/// Sets the reaction table.
	/// </summary>
	/// <param name="reactions">
	/// The set of reactions for this filter.
	/// </param>
	void SetReactions(IList<IReaction> reactions)
	{
	}

	/// <summary>
	/// Set the state of a lower case flag.
	/// On: Flag applied On
	/// Off: Flag applied Off
	/// Any: Flag not used (not applied and off)
	/// </summary>
	/// <param name="flag">Flag to set</param>
	/// <param name="value">value of flag</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	void SetLowerFlag(LowerCaseFilterFlags flag, TriState value)
	{
	}

	/// <summary>
	/// Set the state of a upper case flag.
	/// On: Flag applied On
	/// Off: Flag applied Off
	/// Any: Flag not used (not applied and off)
	/// </summary>
	/// <param name="flag">flag to set</param>
	/// <param name="value">value of flag</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	void SetUpperFlag(UpperCaseFilterFlags flag, TriState value)
	{
	}

	/// <summary>
	/// Gets the state of a lower case filter flag
	/// </summary>
	/// <param name="flag">The flag to get</param>
	/// <returns>The state of the requested flag</returns>
	TriState GetLowerCaseFlag(LowerCaseFilterFlags flag)
	{
		return TriState.Any;
	}

	/// <summary>
	/// Gets the state of an upper case filter flag
	/// </summary>
	/// <param name="flag">The flag to get</param>
	/// <returns>state of the requested flag</returns>
	TriState GetUpperCaseFlag(UpperCaseFilterFlags flag)
	{
		return TriState.Any;
	}
}
