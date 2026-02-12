using System.ComponentModel;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
///     The scan filter enumerations.
/// </summary>
internal static class ScanFilterEnums
{
	/// <summary>
	///     The accurate mass types.
	/// </summary>
	public enum AccurateMassTypes
	{
		/// <summary>
		///     Accurate Mass off.
		/// </summary>
		[Description("!AM")]
		Off,
		/// <summary>
		///     Accurate Mass on.
		/// </summary>
		[Description("AM")]
		On,
		/// <summary>
		///     Accurate Mass internal.
		/// </summary>
		[Description("AMI")]
		Internal,
		/// <summary>
		///     Accurate Mass external.
		/// </summary>
		[Description("AME")]
		External,
		/// <summary>
		///     Accept any accurate mass.
		/// </summary>
		[Description("AnyAccurateMass")]
		AcceptAnyAccurateMass
	}

	/// <summary>
	///     The detector type.
	/// </summary>
	public enum DetectorType
	{
		/// <summary>
		///     The is valid.
		/// </summary>
		[Description("")]
		IsValid,
		/// <summary>
		///     The any.
		/// </summary>
		[Description("AnyDector")]
		Any,
		/// <summary>
		///     The is in valid.
		/// </summary>
		[Description("!")]
		IsInValid
	}

	/// <summary>
	/// The filter source high val.
	/// </summary>
	public enum FilterSourceHighVal
	{
		/// <summary>
		/// Apply source CID high filter.
		/// </summary>
		SourceCIDHigh,
		/// <summary>
		/// accept any source CID high.
		/// </summary>
		AcceptAnySourceCIDHigh
	}

	/// <summary>
	/// The filter source low val.
	/// </summary>
	public enum FilterSourceLowVal
	{
		/// <summary>
		/// Apply source CID low filter.
		/// </summary>
		SourceCIDLow,
		/// <summary>
		/// accept any source CID low.
		/// </summary>
		AcceptAnySourceCIDLow
	}

	/// <summary>
	///     Determines how filter parsing is going
	/// </summary>
	public enum FilterStringParseState
	{
		/// <summary>
		/// Filter is incomplete
		/// </summary>
		Incomplete,
		/// <summary>
		/// Filter good
		/// </summary>
		Good,
		/// <summary>
		/// Filter is badly formatted
		/// </summary>
		Bad,
		/// <summary>
		/// Next token must be inspected
		/// </summary>
		Next
	}

	/// <summary>
	/// The field free regions.
	/// </summary>
	public enum FreeRegions
	{
		/// <summary>
		///     The free region 1.
		/// </summary>
		[Description("ffr1")]
		FreeRegion1,
		/// <summary>
		///     The free region 2.
		/// </summary>
		[Description("ffr2")]
		FreeRegion2,
		/// <summary>
		///     The any free region.
		/// </summary>
		[Description("AnyFreeRegion")]
		AnyFreeRegion
	}

	/// <summary>
	/// The ionization modes.
	/// </summary>
	public enum IonizationModes
	{
		/// <summary>
		///     The electron impact.
		/// </summary>
		[Description("EI")]
		ElectronImpact,
		/// <summary>
		///     The chemical ionization.
		/// </summary>
		[Description("CI")]
		ChemicalIonization,
		/// <summary>
		///     The fast atom bombardment.
		/// </summary>
		[Description("FAB")]
		FastAtomBombardment,
		/// <summary>
		///     The electrospray.
		/// </summary>
		[Description("ESI")]
		Electrospray,
		/// <summary>
		///     The atmospheric pressure chemical ionization.
		/// </summary>
		[Description("APCI")]
		AtmosphericPressureChemicalIonization,
		/// <summary>
		/// <c>nano-spray</c>.
		/// </summary>
		[Description("NSI")]
		Nanospray,
		/// <summary>
		/// thermo-spray ionization.
		/// </summary>
		[Description("TSP")]
		Thermospray,
		/// <summary>
		/// field desorption.
		/// </summary>
		[Description("FD")]
		FieldDesorption,
		/// <summary>
		/// matrix assisted laser desorption ionization.
		/// </summary>
		[Description("MALDI")]
		MatrixAssistedLaserDesorptionIonization,
		/// <summary>
		/// glow discharge.
		/// </summary>
		[Description("GD")]
		GlowDischarge,
		/// <summary>
		/// The accept any ionization mode.
		/// Only applies to filtering.
		/// If this appears in a file, no mode string is shown.
		/// </summary>
		[Description("")]
		AcceptAnyIonizationMode,
		/// <summary>
		///  paper spray ionization.
		/// </summary>
		[Description("PSI")]
		PaperSprayIonization,
		/// <summary>
		/// Card <c>nanospray</c> ionization.
		/// </summary>
		[Description("cNSI")]
		CardNanoSprayIonization,
		/// <summary>
		/// The extension ionization mode 1.
		/// </summary>
		[Description("IM1")]
		IM1,
		/// <summary>
		/// The extension ionization mode 2.
		/// </summary>
		[Description("IM2")]
		IM2,
		/// <summary>
		/// The extension ionization mode 3.
		/// </summary>
		[Description("IM3")]
		IM3,
		/// <summary>
		/// The extension ionization mode 4.
		/// </summary>
		[Description("IM4")]
		IM4,
		/// <summary>
		/// The extension ionization mode 5.
		/// </summary>
		[Description("IM5")]
		IM5,
		/// <summary>
		/// The extension ionization mode 6.
		/// </summary>
		[Description("IM6")]
		IM6,
		/// <summary>
		/// The extension ionization mode 7.
		/// </summary>
		[Description("IM7")]
		IM7,
		/// <summary>
		/// The extension ionization mode 8.
		/// </summary>
		[Description("IM8")]
		IM8,
		/// <summary>
		/// The extension ionization mode 9.
		/// </summary>
		[Description("IM9")]
		IM9,
		/// <summary>
		/// The ion mode is beyond known types.
		/// </summary>
		IonModeBeyondKnown
	}

	/// <summary>
	///     The possible values for scan.
	/// </summary>
	public enum IsDependent
	{
		/// <summary>
		///     Not Dependent.
		/// </summary>
		[Description("!")]
		No,
		/// <summary>
		///     Is Dependent.
		/// </summary>
		[Description("")]
		Yes,
		/// <summary>
		///     Any Type.
		/// </summary>
		Any
	}

	/// <summary>
	///     The mass analyzer types.
	/// </summary>
	public enum MassAnalyzerTypes
	{
		/// <summary>
		///     Ion Trap MS.
		/// </summary>
		[Description("ITMS")]
		ITMS,
		/// <summary>
		/// Triple quad MS.
		/// </summary>
		[Description("TQMS")]
		TQMS,
		/// <summary>
		///     Single Quad MS.
		/// </summary>
		[Description("SQMS")]
		SQMS,
		/// <summary>
		///     TOF MS.
		/// </summary>
		[Description("TOFMS")]
		TOFMS,
		/// <summary>
		///     FTMS analyzer.
		/// </summary>
		[Description("FTMS")]
		FTMS,
		/// <summary>
		///     Sector analyzer.
		/// </summary>
		[Description("Sector")]
		Sector,
		/// <summary>
		///     Any analyzer.
		/// </summary>
		[Description("AnyMassAnalyzer")]
		Any,
		/// <summary>
		/// Asymmetric Track Lossless (ASTRAL)
		/// AS         T
		/// </summary>
		[Description("ASTMS")]
		ASTMS
	}

	/// <summary>
	///     For raw file types whose "Off" value is 0.
	/// </summary>
	public enum OffOnTypes
	{
		/// <summary>
		///     The off. Used in these modes
		/// <c>
		///     SFWidebandOff
		///     SFSupplementalActivationOff
		///     SFMultiStateActivationOff
		///     SFMultiplexOff
		///     SFParam_A_Off
		///     SFParam_B_Off
		///     SFParam_F_Off
		///     SFParam_K_Off
		///     SFParam_R_Off
		///     SFParam_V_Off
		/// </c>
		/// </summary>
		[Description("!")]
		Off,
		/// <summary>
		/// The feature is on.
		/// Used by these filter codes.
		/// <c>
		///     SFWidebandOn
		///     SFSupplementalActivationOn
		///     SFMultiStateActivationOn
		///     SFMultiplexOn
		///     SFParam_A_On
		///     SFParam_B_On
		///     SFParam_F_On
		///     SFParam_K_On
		///     SFParam_R_On
		///     SFParam_V_On
		/// </c>
		/// </summary>
		[Description("")]
		On,
		/// <summary>
		///     The any.
		/// Used by these filter codes.
		/// <c>
		///     SFAcceptAnyWideband
		///     SFAcceptAnySupplementalActivation
		///     SFAcceptAnyMultiStateActivation
		///     SFAcceptAnyMultiplex
		///     SFAcceptAnyParam_A
		///     SFAcceptAnyParam_B
		///     SFAcceptAnyParam_F
		///     SFAcceptAnyParam_K
		///     SFAcceptAnyParam_R
		///     SFAcceptAnyParam_V
		/// </c>
		/// </summary>
		[Description("AnyOffOn")]
		Any
	}

	/// <summary>
	///     The on any off types.
	/// </summary>
	public enum OnAnyOffTypes
	{
		/// <summary>
		///     The on.
		///     SFDetectorValid
		///     SFMultiPhotonDissociationOn
		///     SFElectronCaptureDissociationOn
		/// </summary>
		[Description("")]
		On,
		/// <summary>
		///     The any.
		///     SFAcceptAnyDetector
		///     SFAcceptAnyMultiPhotonDissociation
		///     SFAcceptAnyElectronCaptureDissociation
		/// </summary>
		[Description("AnyOnAnyOff")]
		Any,
		/// <summary>
		///     The off.
		///     SFDetectorNotValid
		///     SFMultiPhotonDissociationOff
		///     SFElectronCaptureDissociationOff
		/// </summary>
		[Description("!")]
		Off
	}

	/// <summary>
	///     For raw file types whose "On" value is 0.
	/// </summary>
	public enum OnOffTypes
	{
		/// <summary>
		///     Feature is on. Used for these features:
		/// <c>
		///     SFSourceCIDon
		///     SFTurboScanOn
		///     SFCoronaOn
		///     SFLockOn
		///     SFUltraOn
		///     SFEnhancedOn
		///     SFPhotoIonizationOn
		///     SFPulsedQDissociationOn
		///     SFElectronTransferDissociationOn
		///     SFHigherenergyCiDOn
		///     SFCompensationVoltageOn
		/// </c>
		/// </summary>
		[Description("")]
		On,
		/// <summary>
		///     The feature is off.
		/// Used by these filter codes
		/// <c>
		///     SFSourceCIDoff
		///     SFTurboScanOff
		///     SFCoronaOff
		///     SFLockOff
		///     SFUltraOff
		///     SFEnhancedOff
		///     SFPhotoIonizationOff
		///     SFPulsedQDissociationOff
		///     SFElectronTransferDissociationOff
		///     SFHigherenergyCiDOff
		///     SFCompensationVoltageOff
		/// </c>
		/// </summary>
		[Description("!")]
		Off,
		/// <summary>
		///     any mode accepted. Used for these features:
		/// <c>
		///     SFAcceptAnySourceCID
		///     SFAcceptAnyTurboScan
		///     SFAcceptAnyCorona
		///     SFAcceptAnyLock
		///     SFAcceptAnyUltra
		///     SFAcceptAnyEnhanced
		///     SFAcceptAnyPhotoIonization
		///     SFAcceptAnyPulsedQDissociation
		///     SFAcceptAnyElectronTransferDissociation
		///     SFAcceptAnyHigherenergyCiD
		///     SFAcceptAnyCompensationVoltage
		/// </c>
		/// </summary>
		[Description("AnyOnOff")]
		Any
	}

	/// <summary>
	///     The polarity types.
	/// </summary>
	public enum PolarityTypes
	{
		/// <summary>
		///     The negative.
		/// </summary>
		[Description("-")]
		Negative,
		/// <summary>
		///     The positive.
		/// </summary>
		[Description("+")]
		Positive,
		/// <summary>
		///     The any.
		/// </summary>
		Any
	}

	/// <summary>
	///     The precursor energy.
	/// </summary>
	public enum PrecursorEnergy
	{
		/// <summary>
		///     The is valid.
		/// </summary>
		IsValid,
		/// <summary>
		///     The any.
		/// </summary>
		Any
	}

	/// <summary>
	///     The scan data types.
	/// </summary>
	public enum ScanDataTypes
	{
		/// <summary>
		///     The centroid.
		/// </summary>
		[Description("c")]
		Centroid,
		/// <summary>
		///     The profile.
		/// </summary>
		[Description("p")]
		Profile,
		/// <summary>
		///     The any.
		/// </summary>
		[Description("AnyScanData")]
		Any
	}

	/// <summary>
	///     The scan event accurate mass enumeration saved in the raw file. This should not be
	///     exposed outside of the reader.
	/// </summary>
	public enum ScanEventAccurateMassTypes
	{
		/// <summary>
		///     The public.
		/// </summary>
		Internal,
		/// <summary>
		///     The external.
		/// </summary>
		External,
		/// <summary>
		///     The off.
		/// </summary>
		Off,
		/// <summary>
		///     Any: Not supported in raw files, only used for filtering
		/// </summary>
		Any
	}

	/// <summary>
	///     The scan types.
	/// </summary>
	public enum ScanTypes
	{
		/// <summary>
		/// Type is not specified.
		/// </summary>
		[Description("NotSpecified")]
		NotSpecified = -1,
		/// <summary>
		/// full scan.
		/// </summary>
		[Description("full")]
		Full,
		/// <summary>
		/// Scan type zoom.
		/// </summary>
		[Description("z")]
		Zoom,
		/// <summary>
		/// Scan type SIM.
		/// </summary>
		[Description("sim")]
		SIM,
		/// <summary>
		/// Scan type SRM.
		/// </summary>
		[Description("srm")]
		SRM,
		/// <summary>
		/// Scan type CRM.
		/// </summary>
		[Description("crm")]
		CRM,
		/// <summary>
		/// Any scan type.
		/// </summary>
		[Description("AnyScanType")]
		Any,
		/// <summary>
		/// quad 1 MS.
		/// </summary>
		[Description("q1ms")]
		Q1MS,
		/// <summary>
		/// quad 3 MS.
		/// </summary>
		[Description("q3ms")]
		Q3MS
	}

	/// <summary>
	///     The sector scans.
	/// </summary>
	public enum SectorScans
	{
		/// <summary>
		/// Magnet scan.
		/// </summary>
		[Description("BSCAN")]
		BSCAN,
		/// <summary>
		/// Electrostatic scan.
		/// </summary>
		[Description("ESCAN")]
		ESCAN,
		/// <summary>
		/// Any sector scanned.
		/// </summary>
		[Description("AnySectorScans")]
		Any
	}

	/// <summary>
	///     Determines if a scan uses segment and event numbers
	/// </summary>
	public enum SegmentScanEventType
	{
		/// <summary>
		///     Segment and scan event is set
		/// </summary>
		SegmentScanEventSet,
		/// <summary>
		///     Accept any segment scan event
		/// </summary>
		AcceptAnySegmentScanEvent
	}

	/// <summary>
	/// The SIM compensation voltage energy modes.
	/// </summary>
	public enum SIMCompensationVoltageEnergy
	{
		/// <summary>
		/// SIM compensation voltage energy set
		/// </summary>
		SIMCompensationVoltageEnergySet,
		/// <summary>
		/// Accept any SIM compensation voltage energy
		/// </summary>
		AcceptAnySIMCompensationVoltageEnergy
	}

	/// <summary>
	/// The SIM source CID energy.
	/// </summary>
	public enum SIMSourceCIDEnergy
	{
		/// <summary>
		/// The SIM source CID energy has been set.
		/// </summary>
		SIMSourceCIDEnergySet,
		/// <summary>
		/// accept any SIM source CID energy.
		/// </summary>
		AcceptAnySIMSourceCIDEnergy
	}

	/// <summary>
	///     The source cid valid types.
	/// </summary>
	public enum SourceCIDValidTypes
	{
		/// <summary>
		///     The source cid energy valid.
		/// </summary>
		SourceCIDEnergyValid,
		/// <summary>
		///     The accept any source cid energy.
		/// </summary>
		AcceptAnySourceCIDEnergy,
		/// <summary>
		///     The 'SourceCIDEnergyValid" array is now doing double duty
		///     for the CVEnergyValid values as well (binary compatibility
		///     issue). So I added this value to the enumeration to indicate that
		///     the energy value is for CV, not SID
		/// </summary>
		CompensationVoltageEnergyValid
	}

	/// <summary>
	///     The voltage types.
	/// </summary>
	public enum VoltageTypes
	{
		/// <summary>
		///     No Value - e.g. CV or SID
		///     SFSourceCIDTypeNoValue
		///     SFCompensationVoltageNoValue
		/// </summary>
		NoValue,
		/// <summary>
		///     Single Value - example: sid=40 or cv=40
		///     SFSourceCIDTypeSingleValue
		///     SFCompensationVoltageSingleValue
		/// </summary>
		SingleValue,
		/// <summary>
		///     Ramp - example: sid=40-50
		///     SFSourceCIDTypeRamp
		///     SFCompensationVoltageRamp
		/// </summary>
		Ramp,
		/// <summary>
		///     SIM - example : SIM [100@40, 200@50]
		///     SFSourceCIDTypeSIM
		///     SFCompensationVoltageSIM
		/// </summary>
		SIM,
		/// <summary>
		///     Accept any type.
		///     SFAcceptAnySourceCIDType
		///     SFAcceptAnyCompensationVoltageType
		/// </summary>
		Any
	}

	/// <summary>
	///     The MS order types.
	/// </summary>
	internal enum MSOrderTypes
	{
		/// <summary>
		///     The neutral gain.
		/// </summary>
		NeutralGain = -3,
		/// <summary>
		///     The neutral loss.
		/// </summary>
		NeutralLoss,
		/// <summary>
		///     The parent scan.
		/// </summary>
		ParentScan,
		/// <summary>
		///     Accept any MS order.
		/// </summary>
		AcceptAnyMSorder,
		/// <summary>
		///  MS data.
		/// </summary>
		MS,
		/// <summary>
		///  MS/MS data
		/// </summary>
		MS2,
		/// <summary>
		///     The m s 3.
		/// </summary>
		MS3,
		/// <summary>
		///     The m s 4.
		/// </summary>
		MS4,
		/// <summary>
		///     The m s 5.
		/// </summary>
		MS5,
		/// <summary>
		///     The m s 6.
		/// </summary>
		MS6,
		/// <summary>
		///     The m s 7.
		/// </summary>
		MS7,
		/// <summary>
		///     The m s 8.
		/// </summary>
		MS8,
		/// <summary>
		///     The m s 9.
		/// </summary>
		MS9,
		/// <summary>
		///     The m s 10.
		/// </summary>
		MS10,
		/// <summary>
		///     The m s 11.
		/// </summary>
		MS11,
		/// <summary>
		///     The m s 12.
		/// </summary>
		MS12,
		/// <summary>
		///     The m s 13.
		/// </summary>
		MS13,
		/// <summary>
		///     The m s 14.
		/// </summary>
		MS14,
		/// <summary>
		///     The m s 15.
		/// </summary>
		MS15,
		/// <summary>
		///     The m s 16.
		/// </summary>
		MS16,
		/// <summary>
		///     The m s 17.
		/// </summary>
		MS17,
		/// <summary>
		///     The m s 18.
		/// </summary>
		MS18,
		/// <summary>
		///     The m s 19.
		/// </summary>
		MS19,
		/// <summary>
		///     The m s 20.
		/// </summary>
		MS20,
		/// <summary>
		///     The m s 21.
		/// </summary>
		MS21,
		/// <summary>
		///     The m s 22.
		/// </summary>
		MS22,
		/// <summary>
		///     The m s 23.
		/// </summary>
		MS23,
		/// <summary>
		///     The m s 24.
		/// </summary>
		MS24,
		/// <summary>
		///     The m s 25.
		/// </summary>
		MS25,
		/// <summary>
		///     The m s 26.
		/// </summary>
		MS26,
		/// <summary>
		///     The m s 27.
		/// </summary>
		MS27,
		/// <summary>
		///     The m s 28.
		/// </summary>
		MS28,
		/// <summary>
		///     The m s 29.
		/// </summary>
		MS29,
		/// <summary>
		///     The m s 30.
		/// </summary>
		MS30,
		/// <summary>
		///     The m s 31.
		/// </summary>
		MS31,
		/// <summary>
		///     The m s 32.
		/// </summary>
		MS32,
		/// <summary>
		///     The m s 33.
		/// </summary>
		MS33,
		/// <summary>
		///     The m s 34.
		/// </summary>
		MS34,
		/// <summary>
		///     The m s 35.
		/// </summary>
		MS35,
		/// <summary>
		///     The m s 36.
		/// </summary>
		MS36,
		/// <summary>
		///     The m s 37.
		/// </summary>
		MS37,
		/// <summary>
		///     The m s 38.
		/// </summary>
		MS38,
		/// <summary>
		///     The m s 39.
		/// </summary>
		MS39,
		/// <summary>
		///     The m s 40.
		/// </summary>
		MS40,
		/// <summary>
		///     The m s 41.
		/// </summary>
		MS41,
		/// <summary>
		///     The m s 42.
		/// </summary>
		MS42,
		/// <summary>
		///     The m s 43.
		/// </summary>
		MS43,
		/// <summary>
		///     The m s 44.
		/// </summary>
		MS44,
		/// <summary>
		///     The m s 45.
		/// </summary>
		MS45,
		/// <summary>
		///     The m s 46.
		/// </summary>
		MS46,
		/// <summary>
		///     The m s 47.
		/// </summary>
		MS47,
		/// <summary>
		///     The m s 48.
		/// </summary>
		MS48,
		/// <summary>
		///     The m s 49.
		/// </summary>
		MS49,
		/// <summary>
		///     The m s 50.
		/// </summary>
		MS50,
		/// <summary>
		///     The m s 51.
		/// </summary>
		MS51,
		/// <summary>
		///     The m s 52.
		/// </summary>
		MS52,
		/// <summary>
		///     The m s 53.
		/// </summary>
		MS53,
		/// <summary>
		///     The m s 54.
		/// </summary>
		MS54,
		/// <summary>
		///     The m s 55.
		/// </summary>
		MS55,
		/// <summary>
		///     The m s 56.
		/// </summary>
		MS56,
		/// <summary>
		///     The m s 57.
		/// </summary>
		MS57,
		/// <summary>
		///     The m s 58.
		/// </summary>
		MS58,
		/// <summary>
		///     The m s 59.
		/// </summary>
		MS59,
		/// <summary>
		///     The m s 60.
		/// </summary>
		MS60,
		/// <summary>
		///     The m s 61.
		/// </summary>
		MS61,
		/// <summary>
		///     The m s 62.
		/// </summary>
		MS62,
		/// <summary>
		///     The m s 63.
		/// </summary>
		MS63,
		/// <summary>
		///     The m s 64.
		/// </summary>
		MS64,
		/// <summary>
		///     The m s 65.
		/// </summary>
		MS65,
		/// <summary>
		///     The m s 66.
		/// </summary>
		MS66,
		/// <summary>
		///     The m s 67.
		/// </summary>
		MS67,
		/// <summary>
		///     The m s 68.
		/// </summary>
		MS68,
		/// <summary>
		///     The m s 69.
		/// </summary>
		MS69,
		/// <summary>
		///     The m s 70.
		/// </summary>
		MS70,
		/// <summary>
		///     The m s 71.
		/// </summary>
		MS71,
		/// <summary>
		///     The m s 72.
		/// </summary>
		MS72,
		/// <summary>
		///     The m s 73.
		/// </summary>
		MS73,
		/// <summary>
		///     The m s 74.
		/// </summary>
		MS74,
		/// <summary>
		///     The m s 75.
		/// </summary>
		MS75,
		/// <summary>
		///     The m s 76.
		/// </summary>
		MS76,
		/// <summary>
		///     The m s 77.
		/// </summary>
		MS77,
		/// <summary>
		///     The m s 78.
		/// </summary>
		MS78,
		/// <summary>
		///     The m s 79.
		/// </summary>
		MS79,
		/// <summary>
		///     The m s 80.
		/// </summary>
		MS80,
		/// <summary>
		///     The m s 81.
		/// </summary>
		MS81,
		/// <summary>
		///     The m s 82.
		/// </summary>
		MS82,
		/// <summary>
		///     The m s 83.
		/// </summary>
		MS83,
		/// <summary>
		///     The m s 84.
		/// </summary>
		MS84,
		/// <summary>
		///     The m s 85.
		/// </summary>
		MS85,
		/// <summary>
		///     The m s 86.
		/// </summary>
		MS86,
		/// <summary>
		///     The m s 87.
		/// </summary>
		MS87,
		/// <summary>
		///     The m s 88.
		/// </summary>
		MS88,
		/// <summary>
		///     The m s 89.
		/// </summary>
		MS89,
		/// <summary>
		///     The m s 90.
		/// </summary>
		MS90,
		/// <summary>
		///     The m s 91.
		/// </summary>
		MS91,
		/// <summary>
		///     The m s 92.
		/// </summary>
		MS92,
		/// <summary>
		///     The m s 93.
		/// </summary>
		MS93,
		/// <summary>
		///     The m s 94.
		/// </summary>
		MS94,
		/// <summary>
		///     The m s 95.
		/// </summary>
		MS95,
		/// <summary>
		///     The m s 96.
		/// </summary>
		MS96,
		/// <summary>
		///     The m s 97.
		/// </summary>
		MS97,
		/// <summary>
		///     The m s 98.
		/// </summary>
		MS98,
		/// <summary>
		///     The m s 99.
		/// </summary>
		MS99,
		/// <summary>
		///     The m s 100.
		/// </summary>
		MS100
	}

	/// <summary>
	/// The SIM energy type.
	/// </summary>
	internal enum SIMenergyType
	{
		/// <summary>
		/// SID energy
		/// </summary>
		SID,
		/// <summary>
		/// CV energy
		/// </summary>
		Cv
	}

	/// <summary>
	///     The method converts from scan event accurate mass enumeration read from the raw file to
	///     the Scan Filter (public) Accurate Mass enumeration.
	/// </summary>
	/// <param name="massTypesType">
	///     The scan event's mass type read from the raw file.
	/// </param>
	/// <returns>
	///     The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Constants.ScanFilterEnums.AccurateMassTypes" /> enumeration.
	/// </returns>
	public static AccurateMassTypes FromScanEventAccurateMass(ScanEventAccurateMassTypes massTypesType)
	{
		return massTypesType switch
		{
			ScanEventAccurateMassTypes.Internal => AccurateMassTypes.Internal, 
			ScanEventAccurateMassTypes.External => AccurateMassTypes.External, 
			ScanEventAccurateMassTypes.Any => AccurateMassTypes.AcceptAnyAccurateMass, 
			_ => AccurateMassTypes.Off, 
		};
	}
}
