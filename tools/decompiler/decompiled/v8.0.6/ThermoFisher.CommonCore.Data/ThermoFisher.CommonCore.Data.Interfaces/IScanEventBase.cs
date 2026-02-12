using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The IScanEventBase interface defines scanning features
/// which are common to "scan events" and "scan filters".
/// </summary>
public interface IScanEventBase
{
	/// <summary>
	/// Gets the scan data format (profile or centroid).
	/// </summary>
	ScanDataType ScanData { get; }

	/// <summary>
	/// Gets the polarity of the scan.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.PolarityType" /> for possible values</value>
	PolarityType Polarity { get; }

	/// <summary>
	/// Gets the scan MS/MS power setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MSOrderType" /> for possible values</value>
	MSOrderType MSOrder { get; }

	/// <summary>
	/// Gets the dependent scan setting.
	/// A scan is "dependent" if the scanning method is based
	/// on analysis of data from a previous scan.
	/// </summary>
	TriState Dependent { get; }

	/// <summary>
	/// Gets source fragmentation scan setting.
	/// </summary>
	TriState SourceFragmentation { get; }

	/// <summary>
	/// Gets the source fragmentation type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SourceFragmentationValueType" /> for possible values</value>
	SourceFragmentationValueType SourceFragmentationType { get; }

	/// <summary>
	/// Gets the scan type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.ScanModeType" /> for possible values</value>
	ScanModeType ScanMode { get; }

	/// <summary>
	/// Gets the turbo scan setting.
	/// </summary>
	TriState TurboScan { get; }

	/// <summary>
	/// Gets the ionization mode scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.IonizationModeType" /> for possible values</value>
	IonizationModeType IonizationMode { get; }

	/// <summary>
	/// Gets the corona scan setting.
	/// </summary>
	TriState Corona { get; }

	/// <summary>
	/// Gets the detector validity setting.
	/// The property <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.DetectorValue" /> only contains valid information
	/// when this is set to "DetectorType.Valid"
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.DetectorType" /> for possible values</value>
	DetectorType Detector { get; }

	/// <summary>
	/// Gets the detector value.
	/// This should only be used when valid. <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.Detector" />
	/// </summary>
	/// <value>Floating point detector value</value>
	double DetectorValue { get; }

	/// <summary>
	/// Gets the wideband scan setting.
	/// </summary>
	TriState Wideband { get; }

	/// <summary>
	/// Gets the mass analyzer scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MassAnalyzerType" /> for possible values</value>
	MassAnalyzerType MassAnalyzer { get; }

	/// <summary>
	/// Gets the sector scan setting. Applies to 2 sector (Magnetic, electrostatic) Mass spectrometers, or hybrids.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SectorScanType" /> for possible values</value>
	SectorScanType SectorScan { get; }

	/// <summary>
	/// Gets the lock scan setting.
	/// </summary>
	TriState Lock { get; }

	/// <summary>
	/// Gets the field free region setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.FieldFreeRegionType" /> for possible values</value>
	FieldFreeRegionType FieldFreeRegion { get; }

	/// <summary>
	/// Gets the ultra scan setting.
	/// </summary>
	TriState Ultra { get; }

	/// <summary>
	/// Gets the enhanced scan setting.
	/// </summary>
	TriState Enhanced { get; }

	/// <summary>
	/// Gets the multi-photon dissociation setting.
	/// </summary>
	TriState MultiplePhotonDissociation { get; }

	/// <summary>
	/// Gets the multi-photon dissociation value.
	/// </summary>
	/// <value>Floating point multi-photon dissociation value</value>
	double MultiplePhotonDissociationValue { get; }

	/// <summary>
	/// Gets the electron capture dissociation setting.
	/// </summary>
	TriState ElectronCaptureDissociation { get; }

	/// <summary>
	/// Gets the electron capture dissociation value.
	/// </summary>
	/// <value>Floating point electron capture dissociation value</value>
	double ElectronCaptureDissociationValue { get; }

	/// <summary>
	/// Gets the photo ionization setting.
	/// </summary>
	TriState PhotoIonization { get; }

	/// <summary>
	/// Gets pulsed dissociation setting.
	/// </summary>
	TriState PulsedQDissociation { get; }

	/// <summary>
	/// Gets the pulsed dissociation value.
	/// </summary>
	/// <value>Floating point pulsed dissociation value</value>
	double PulsedQDissociationValue { get; }

	/// <summary>
	/// Gets the electron transfer dissociation setting.
	/// </summary>
	TriState ElectronTransferDissociation { get; }

	/// <summary>
	/// Gets the electron transfer dissociation value.
	/// </summary>
	/// <value>Floating point electron transfer dissociation value</value>
	double ElectronTransferDissociationValue { get; }

	/// <summary>
	/// Gets the higher energy CID setting.
	/// </summary>
	TriState HigherEnergyCiD { get; }

	/// <summary>
	/// Gets the higher energy CID value.
	/// </summary>
	/// <value>Floating point higher energy CID value</value>
	double HigherEnergyCiDValue { get; }

	/// <summary>
	/// Gets the Multiplex type
	/// </summary>
	TriState Multiplex { get; }

	/// <summary>
	/// Gets the parameter a.
	/// </summary>
	TriState ParamA { get; }

	/// <summary>
	/// Gets the parameter b.
	/// </summary>
	TriState ParamB { get; }

	/// <summary>
	/// Gets the parameter f.
	/// </summary>
	TriState ParamF { get; }

	/// <summary>
	/// Gets the Multi notch (Synchronous Precursor Selection) type
	/// </summary>
	TriState MultiNotch { get; }

	/// <summary>
	/// Gets the parameter r.
	/// </summary>
	TriState ParamR { get; }

	/// <summary>
	/// Gets the parameter v.
	/// </summary>
	TriState ParamV { get; }

	/// <summary>
	/// Gets the event Name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets supplemental activation type setting.
	/// </summary>
	TriState SupplementalActivation { get; }

	/// <summary>
	/// Gets MultiStateActivation type setting.
	/// </summary>
	TriState MultiStateActivation { get; }

	/// <summary>
	/// Gets Compensation Voltage Option setting.
	/// </summary>
	TriState CompensationVoltage { get; }

	/// <summary>
	/// Gets Compensation Voltage type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.CompensationVoltageType" /> for possible values</value>
	CompensationVoltageType CompensationVoltType { get; }

	/// <summary>
	/// Gets encoded form of segment and scan event number.
	/// </summary>
	/// <value>HIWORD == segment, LOWORD == scan type</value>
	long ScanTypeIndex { get; }

	/// <summary>
	/// Gets number of (precursor) masses
	/// </summary>
	/// <value>The size of mass array</value>
	int MassCount { get; }

	/// <summary>
	/// Gets the number of mass ranges for final scan
	/// </summary>
	/// <value>The size of mass range array</value>
	int MassRangeCount { get; }

	/// <summary>
	/// Gets the number of source fragmentation info values
	/// </summary>
	/// <value>The size of source fragmentation info array</value>
	int SourceFragmentationInfoCount { get; }

	/// <summary>
	/// Gets the reaction data for the mass at 0 based index.
	/// Descries how a particular MS/MS precursor mass is fragmented.
	/// Equivalent to calling GetMass, GetEnergy, GetPrecursorRangeValidity, GetFirstPrecursorMass
	/// GetLastPrecursorMass, GetIsolationWidth, GetIsolationWidthOffset, GetEnergyValid
	/// GetActivation, GetIsMultipleActivation.
	/// Depending on the implementation of the interface, this call may be more efficient
	/// than calling several of the methods listed.
	/// </summary>
	/// <param name="index">index of reaction</param>
	/// <returns>reaction details</returns>
	IReaction GetReaction(int index);

	/// <summary>
	/// Retrieves mass value for MS step at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of mass values.
	/// </remarks>
	/// <param name="index">
	/// Index of mass value to be retrieved
	/// </param>
	/// <returns>
	/// Mass value of MS step
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= MassCount</exception>
	double GetMass(int index);

	/// <summary>
	/// Retrieves precursor(collision) energy value for MS step at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of energies.
	/// </remarks>
	/// <param name="index">
	/// Index of precursor(collision) energy to be retrieved
	/// </param>
	/// <returns>
	/// precursor(collision) energy of MS step
	/// </returns>
	double GetEnergy(int index);

	/// <summary>
	/// Determine if a precursor range is valid.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// true if valid
	/// </returns>
	bool GetPrecursorRangeValidity(int index);

	/// <summary>
	/// Gets the first precursor mass.
	/// This is only valid data where "GetPrecursorRangeValidity" returns true for the same index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The first mass
	/// </returns>
	double GetFirstPrecursorMass(int index);

	/// <summary>
	/// Gets the last precursor mass.
	/// This is only valid data where "GetPrecursorRangeValidity" returns true for the same index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The last mass
	/// </returns>
	double GetLastPrecursorMass(int index);

	/// <summary>
	/// Gets the isolation width.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The isolation width
	/// </returns>
	double GetIsolationWidth(int index);

	/// <summary>
	/// Gets the isolation width offset.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The isolation width offset
	/// </returns>
	double GetIsolationWidthOffset(int index);

	/// <summary>
	/// Retrieves precursor(collision) energy validation flag at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of precursor(collision) energy validations.
	/// </remarks>
	/// <param name="index">
	/// Index of precursor(collision) energy validation to be retrieved
	/// </param>
	/// <returns>
	/// precursor(collision) energy validation of MS step;
	/// See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.EnergyType" /> for possible values
	/// </returns>
	EnergyType GetEnergyValid(int index);

	/// <summary>
	/// Retrieves activation type at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of activations.
	/// </remarks>
	/// <param name="index">
	/// Index of activation to be retrieved
	/// </param>
	/// <returns>
	/// activation of MS step;
	/// See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.ActivationType" /> for possible values
	/// </returns>
	ActivationType GetActivation(int index);

	/// <summary>
	/// Retrieves multiple activations flag at 0-based index of masses.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of masses.
	/// </remarks>
	/// <param name="index">
	/// Index of flag to be retrieved
	/// </param>
	/// <returns>
	/// true if mass at given index has multiple activations;  false otherwise
	/// </returns>
	bool GetIsMultipleActivation(int index);

	/// <summary>
	/// Retrieves mass range for final scan at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassRangeCount" /> to get the count of mass ranges.
	/// </remarks>
	/// <param name="index">
	/// Index of mass range to be retrieved
	/// </param>
	/// <returns>
	/// Mass range for final scan at 0-based index
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= MassRangeCount</exception>
	IRangeAccess GetMassRange(int index);

	/// <summary>
	/// Retrieves a source fragmentation info value at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.SourceFragmentationInfoCount" /> to get the count of source
	/// fragmentation info values.
	/// </remarks>
	/// <param name="index">
	/// Index of source fragmentation info to be retrieved
	/// </param>
	/// <returns>
	/// Source Fragmentation info value at 0-based index
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= SourceFragmentationInfoCount</exception>
	double GetSourceFragmentationInfo(int index);

	/// <summary>
	/// Get extended interface.
	/// </summary>
	/// <returns>extended event properties</returns>
	IScanEventExtended GetExtensions()
	{
		return null;
	}
}
