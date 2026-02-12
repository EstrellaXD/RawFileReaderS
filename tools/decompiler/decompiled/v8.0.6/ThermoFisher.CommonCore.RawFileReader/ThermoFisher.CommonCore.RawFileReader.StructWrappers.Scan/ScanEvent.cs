using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanEventInfo;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
///     The scan event.
/// </summary>
internal class ScanEvent : IComparable<ScanEvent>, IEquatable<ScanEvent>, IRawFileReaderScanEvent, IRawObjectBase, IScanEvent, IScanEventBase, IScanEventExtended
{
	private static readonly ScanEventInfoStruct DefaultInfo = new ScanEventInfoStruct
	{
		DependentData = 2,
		Wideband = 2,
		SupplementalActivation = 2,
		MultiStateActivation = 2,
		AccurateMassType = ScanFilterEnums.ScanEventAccurateMassTypes.Any,
		Detector = 1,
		SourceFragmentation = 2,
		SourceFragmentationType = 4,
		CompensationVoltage = 2,
		CompensationVoltageType = 4,
		TurboScan = 2,
		Lock = 2,
		Multiplex = 2,
		ParamA = 2,
		ParamB = 2,
		ParamF = 2,
		SpsMultiNotch = 2,
		ParamR = 2,
		ParamV = 2,
		Ultra = 2,
		Enhanced = 2,
		ElectronCaptureDissociationType = 1,
		MultiPhotonDissociationType = 1,
		Corona = 2,
		ElectronTransferDissociationType = 2,
		FreeRegion = 2,
		HigherEnergyCIDType = 2,
		IonizationMode = 10,
		MSOrder = 0,
		MassAnalyzerType = 6,
		PhotoIonization = 2,
		PulsedQDissociationType = 2,
		ScanDataType = 2,
		ScanType = 5,
		SectorScan = 2,
		MultiPhotonDissociation = 0.0,
		ElectronCaptureDissociation = 0.0,
		Polarity = 2,
		DetectorValue = 0.0,
		ScanTypeIndex = -1
	};

	internal static readonly int ScanEventInfoStructSize = Utilities.StructSizeLookup.Value[18];

	private static readonly string[] ActivationTypes = new string[39]
	{
		"cid", "mpd", "ecd", "pqd", "etd", "hcd", "Any", "sa", "ptr", "netd",
		"nptr", "uvpd", "eid", "ee", "modeC", "modeD", "modeE", "modeF", "modeG", "modeG",
		"modeI", "modeJ", "modeK", "modeL", "modeM", "modeN", "modeO", "modeP", "modeQ", "modeR",
		"modeS", "modeT", "modeU", "modeV", "modeW", "modeX", "modeY", "modeZ", "LastActivation"
	};

	private readonly int _massPrecisionDecimals;

	private long _hash1;

	private long _hash2;

	private long _hash3;

	private long _hash4;

	private bool _hasSourceFragmentationRanges;

	private bool _hasDissociationValues;

	private string _massPrecisionFormat;

	private ScanEventInfoStruct _scanEventInfo;

	private static readonly MassRangeStruct[] NoMassRanges = Array.Empty<MassRangeStruct>();

	private static readonly double[] NoDoubles = Array.Empty<double>();

	private const LowerCaseFilterFlags AllLowerCase = (LowerCaseFilterFlags)65535;

	private const UpperCaseFilterFlags AllUpperCase = (UpperCaseFilterFlags)2147483647;

	/// <summary>
	///     Sets the dependent data as byte.
	///     (Used in conversion of legacy LCQ files)
	/// </summary>
	public byte DependentDataAsByte
	{
		set
		{
			_scanEventInfo.DependentData = value;
		}
	}

	/// <summary>
	///     Gets the header filter mass precision.
	/// </summary>
	public int HeaderFilterMassPrecision { get; }

	/// <summary>
	///     Sets the scan data type. Internal method for converting old file revisions
	/// </summary>
	public byte ScanDataTypeAsByte
	{
		set
		{
			_scanEventInfo.ScanDataType = value;
		}
	}

	/// <summary>
	///     Gets or sets the scan event info.
	///     Note that this is a struct, so code which needs
	///     to modify needs to follow a get, modify, set pattern
	/// </summary>
	public ScanEventInfoStruct ScanEventInfo
	{
		get
		{
			return _scanEventInfo;
		}
		set
		{
			_scanEventInfo = value;
		}
	}

	/// <summary>
	///     Gets or sets the index of the unique scan event.
	/// </summary>
	/// <value>
	///     The index of the unique scan event.
	/// </value>
	public int UniqueScanEventIndex { get; internal set; }

	/// <summary>
	///     Gets or sets the scan type Location.
	///     This determines where the scan was found within the
	///     segment and event table.
	///     This is similar data to "ScanTypeIndex"
	///     but not shown in "ToString"
	///     HIWORD == segment, LOWORD == event number
	/// </summary>
	internal int ScanTypeLocation { get; set; }

	/// <summary>
	///     Gets the accurate mass type.
	/// </summary>
	public ScanFilterEnums.AccurateMassTypes AccurateMassType { get; private set; }

	/// <summary>
	///     Gets the compensation voltage.
	/// </summary>
	public ScanFilterEnums.OnOffTypes CompensationVoltage => (ScanFilterEnums.OnOffTypes)_scanEventInfo.CompensationVoltage;

	/// <summary>
	///     Gets the compensation voltage type.
	/// </summary>
	public ScanFilterEnums.VoltageTypes CompensationVoltageType => (ScanFilterEnums.VoltageTypes)_scanEventInfo.CompensationVoltageType;

	/// <summary>
	///     Gets the corona value.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Corona => (ScanFilterEnums.OnOffTypes)_scanEventInfo.Corona;

	/// <summary>
	///     Gets the dependent data.
	/// </summary>
	public ScanFilterEnums.IsDependent DependentDataFlag => (ScanFilterEnums.IsDependent)_scanEventInfo.DependentData;

	/// <summary>
	///     Gets the Detector value.
	/// </summary>
	public ScanFilterEnums.DetectorType Detector => (ScanFilterEnums.DetectorType)_scanEventInfo.Detector;

	/// <summary>
	///     Gets the detector value.
	/// </summary>
	public double DetectorValue => _scanEventInfo.DetectorValue;

	/// <summary>
	///     Gets the electron capture dissociation.
	/// </summary>
	public double ElectronCaptureDissociation => _scanEventInfo.ElectronCaptureDissociation;

	/// <summary>
	///     Gets the electron capture dissociation type.
	/// </summary>
	public ScanFilterEnums.OnAnyOffTypes ElectronCaptureDissociationType => (ScanFilterEnums.OnAnyOffTypes)_scanEventInfo.ElectronCaptureDissociationType;

	/// <summary>
	///     Gets the electron transfer dissociation.
	/// </summary>
	public double ElectronTransferDissociation => _scanEventInfo.ElectronTransferDissociation;

	/// <summary>
	///     Gets the electron transfer dissociation type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes ElectronTransferDissociationType => (ScanFilterEnums.OnOffTypes)_scanEventInfo.ElectronTransferDissociationType;

	/// <summary>
	///     Gets the enhanced.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Enhanced => (ScanFilterEnums.OnOffTypes)_scanEventInfo.Enhanced;

	/// <summary>
	///     Gets the free region.
	/// </summary>
	public ScanFilterEnums.FreeRegions FreeRegion => (ScanFilterEnums.FreeRegions)_scanEventInfo.FreeRegion;

	/// <summary>
	///     Gets the higher energy CID.
	/// </summary>
	public double HigherEnergyCid => _scanEventInfo.HigherEnergyCID;

	/// <summary>
	///     Gets the higher energy CID type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes HigherEnergyCidType => (ScanFilterEnums.OnOffTypes)_scanEventInfo.HigherEnergyCIDType;

	/// <summary>
	///     Gets the ionization mode.
	/// </summary>
	public ScanFilterEnums.IonizationModes IonizationMode => (ScanFilterEnums.IonizationModes)_scanEventInfo.IonizationMode;

	/// <summary>
	///     Gets a value indicating whether the scan event is custom - true if trailer
	///     scan event should be used.
	/// </summary>
	public bool IsCustom => _scanEventInfo.IsCustom != 0;

	/// <summary>
	///     Gets a value indicating whether the scan event is valid.
	/// </summary>
	public bool IsValid => _scanEventInfo.IsValid != 0;

	/// <summary>
	///     Gets the lock.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Lock => (ScanFilterEnums.OnOffTypes)_scanEventInfo.Lock;

	/// <summary>
	///     Gets the mass analyzer type.
	/// </summary>
	public ScanFilterEnums.MassAnalyzerTypes MassAnalyzerType => (ScanFilterEnums.MassAnalyzerTypes)_scanEventInfo.MassAnalyzerType;

	/// <summary>
	///     Gets the mass calibrators.
	/// </summary>
	public double[] MassCalibrators { get; private set; }

	/// <summary>
	///     Gets or sets the mass ranges.
	/// </summary>
	public MassRangeStruct[] MassRanges { get; set; }

	/// <summary>
	///     Gets the MS order.
	/// </summary>
	public ScanFilterEnums.MSOrderTypes MsOrder => (ScanFilterEnums.MSOrderTypes)_scanEventInfo.MSOrder;

	/// <summary>
	///     Gets the multi photon dissociation.
	/// </summary>
	public double MultiPhotonDissociation => _scanEventInfo.MultiPhotonDissociation;

	/// <summary>
	///     Gets the multi photon dissociation type.
	/// </summary>
	public ScanFilterEnums.OnAnyOffTypes MultiPhotonDissociationType => (ScanFilterEnums.OnAnyOffTypes)_scanEventInfo.MultiPhotonDissociationType;

	/// <summary>
	///     Gets the multiplex.
	/// </summary>
	public ScanFilterEnums.OffOnTypes Multiplex => (ScanFilterEnums.OffOnTypes)_scanEventInfo.Multiplex;

	/// <summary>
	///     Gets the multi state activation.
	/// </summary>
	public ScanFilterEnums.OffOnTypes MultiStateActivation => (ScanFilterEnums.OffOnTypes)_scanEventInfo.MultiStateActivation;

	/// <summary>
	///     Gets or sets the name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///     Gets the parameter a.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamA => (ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamA;

	/// <summary>
	///     Gets the parameter b.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamB => (ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamB;

	/// <summary>
	///     Gets the parameter f.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamF => (ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamF;

	/// <summary>
	///     Gets the parameter r.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamR => (ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamR;

	/// <summary>
	///     Gets the parameter v.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamV => (ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamV;

	/// <summary>
	///     Gets the photo ionization.
	/// </summary>
	public ScanFilterEnums.OnOffTypes PhotoIonization => (ScanFilterEnums.OnOffTypes)_scanEventInfo.PhotoIonization;

	/// <summary>
	///     Gets the polarity.
	/// </summary>
	public ScanFilterEnums.PolarityTypes Polarity => (ScanFilterEnums.PolarityTypes)_scanEventInfo.Polarity;

	/// <summary>
	///     Gets the pulsed q dissociation.
	/// </summary>
	public double PulsedQDissociation => _scanEventInfo.PulsedQDissociation;

	/// <summary>
	///     Gets the pulsed q dissociation type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes PulsedQDissociationType => (ScanFilterEnums.OnOffTypes)_scanEventInfo.PulsedQDissociationType;

	/// <summary>
	///     Gets or sets the reactions.
	/// </summary>
	public Reaction[] Reactions { get; set; }

	/// <summary>
	///     Gets the scan data type.
	/// </summary>
	public ScanFilterEnums.ScanDataTypes ScanDataType => (ScanFilterEnums.ScanDataTypes)_scanEventInfo.ScanDataType;

	/// <summary>
	///     Gets the scan type.
	/// </summary>
	public ScanFilterEnums.ScanTypes ScanType => (ScanFilterEnums.ScanTypes)_scanEventInfo.ScanType;

	/// <summary>
	///     Gets the scan type index.
	///     When specified, it will show as {segment,event} in a filter string.
	///     Scan Type Index indicates the segment/scan event for this
	///     scan event.
	///     HIWORD == segment, LOWORD == scan type
	///     -1 for "not in fixed table"
	/// </summary>
	public int ScanTypeIndex => _scanEventInfo.ScanTypeIndex;

	/// <summary>
	///     Gets the sector scan.
	/// </summary>
	public ScanFilterEnums.SectorScans SectorScan => (ScanFilterEnums.SectorScans)_scanEventInfo.SectorScan;

	/// <summary>
	///     Gets the source fragmentation.
	/// </summary>
	public ScanFilterEnums.OnOffTypes SourceFragmentation => (ScanFilterEnums.OnOffTypes)_scanEventInfo.SourceFragmentation;

	/// <summary>
	///     Gets the source fragmentation mass ranges.
	/// </summary>
	public MassRangeStruct[] SourceFragmentationMassRanges { get; private set; }

	/// <summary>
	///     Gets or sets the source fragmentations.
	/// </summary>
	public double[] SourceFragmentations { get; set; }

	/// <summary>
	///     Gets value to indicate how source fragmentation values are interpreted.
	/// </summary>
	public ScanFilterEnums.VoltageTypes SourceFragmentationType => (ScanFilterEnums.VoltageTypes)_scanEventInfo.SourceFragmentationType;

	/// <summary>
	///     Gets the parameter k.
	/// </summary>
	public ScanFilterEnums.OffOnTypes SpsMultiNotch => (ScanFilterEnums.OffOnTypes)_scanEventInfo.SpsMultiNotch;

	/// <summary>
	///     Gets the supplemental activation.
	/// </summary>
	public ScanFilterEnums.OffOnTypes SupplementalActivation => (ScanFilterEnums.OffOnTypes)_scanEventInfo.SupplementalActivation;

	/// <summary>
	///     Gets the turbo scan.
	/// </summary>
	public ScanFilterEnums.OnOffTypes TurboScan => (ScanFilterEnums.OnOffTypes)_scanEventInfo.TurboScan;

	/// <summary>
	///     Gets the ultra.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Ultra => (ScanFilterEnums.OnOffTypes)_scanEventInfo.Ultra;

	/// <summary>
	///     Gets the wideband.
	/// </summary>
	public ScanFilterEnums.OffOnTypes Wideband => (ScanFilterEnums.OffOnTypes)_scanEventInfo.Wideband;

	public UpperCaseFilterFlags UpperCaseFlags => _scanEventInfo.UpperFlags;

	public LowerCaseFilterFlags LowerCaseFlags => (LowerCaseFilterFlags)_scanEventInfo.LowerFlags;

	public UpperCaseFilterFlags UpperCaseApplied => (UpperCaseFilterFlags)2147483647;

	public LowerCaseFilterFlags LowerCaseApplied => (LowerCaseFilterFlags)65535;

	/// <summary>
	///     Gets the accurate mass setting.
	/// </summary>
	EventAccurateMass IScanEvent.AccurateMass => AccurateMassType.ToEventAccurateMass();

	/// <summary>
	///     Gets Compensation Voltage Option setting.
	/// </summary>
	TriState IScanEventBase.CompensationVoltage => (TriState)_scanEventInfo.CompensationVoltage;

	/// <summary>
	///     Gets Compensation Voltage type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.CompensationVoltageType" /> for possible values</value>
	CompensationVoltageType IScanEventBase.CompensationVoltType => (CompensationVoltageType)_scanEventInfo.CompensationVoltageType;

	/// <summary>
	///     Gets the corona scan setting.
	/// </summary>
	TriState IScanEventBase.Corona => (TriState)_scanEventInfo.Corona;

	/// <summary>
	///     Gets the dependent scan setting.
	/// </summary>
	TriState IScanEventBase.Dependent => ((ScanFilterEnums.IsDependent)_scanEventInfo.DependentData).ToTriState();

	/// <summary>
	///     Gets the detector scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.DetectorType" /> for possible values</value>
	DetectorType IScanEventBase.Detector => ((ScanFilterEnums.DetectorType)_scanEventInfo.Detector).ToDetectorType();

	/// <summary>
	///     Gets the detector value.
	/// </summary>
	/// <value>Floating point detector value</value>
	double IScanEventBase.DetectorValue => _scanEventInfo.DetectorValue;

	/// <summary>
	///     Gets the electron capture dissociation setting.
	/// </summary>
	TriState IScanEventBase.ElectronCaptureDissociation => ((ScanFilterEnums.OnAnyOffTypes)_scanEventInfo.ElectronCaptureDissociationType).ToTriState();

	/// <summary>
	///     Gets the electron capture dissociation value.
	/// </summary>
	/// <value>Floating point electron capture dissociation value</value>
	double IScanEventBase.ElectronCaptureDissociationValue => _scanEventInfo.ElectronCaptureDissociation;

	/// <summary>
	///     Gets the electron transfer dissociation setting.
	/// </summary>
	TriState IScanEventBase.ElectronTransferDissociation => (TriState)_scanEventInfo.ElectronTransferDissociationType;

	/// <summary>
	///     Gets the electron transfer dissociation value.
	/// </summary>
	/// <value>Floating point electron transfer dissociation value</value>
	double IScanEventBase.ElectronTransferDissociationValue => _scanEventInfo.ElectronTransferDissociation;

	/// <summary>
	///     Gets the enhanced scan setting.
	/// </summary>
	TriState IScanEventBase.Enhanced => (TriState)_scanEventInfo.Enhanced;

	/// <summary>
	///     Gets the field free region setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.FieldFreeRegionType" /> for possible values</value>
	FieldFreeRegionType IScanEventBase.FieldFreeRegion => (FieldFreeRegionType)_scanEventInfo.FreeRegion;

	/// <summary>
	///     Gets the higher energy cid setting.
	/// </summary>
	TriState IScanEventBase.HigherEnergyCiD => (TriState)_scanEventInfo.HigherEnergyCIDType;

	/// <summary>
	///     Gets the higher energy cid value.
	/// </summary>
	/// <value>Floating point higher energy cid value</value>
	double IScanEventBase.HigherEnergyCiDValue => _scanEventInfo.HigherEnergyCID;

	/// <summary>
	///     Gets the ionization mode scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.IonizationModeType" /> for possible values</value>
	IonizationModeType IScanEventBase.IonizationMode => (IonizationModeType)_scanEventInfo.IonizationMode;

	/// <summary>
	///     Gets a value indicating whether this is a custom event.
	///     A custom event implies that any scan derived from this event could be different.
	///     The scan type must be inspected to determine the scanning mode, and not the event.
	/// </summary>
	bool IScanEvent.IsCustom => _scanEventInfo.IsCustom != 0;

	/// <summary>
	///     Gets a value indicating whether this event is valid.
	/// </summary>
	bool IScanEvent.IsValid => _scanEventInfo.IsValid != 0;

	/// <summary>
	///     Gets the lock scan setting.
	/// </summary>
	TriState IScanEventBase.Lock => (TriState)_scanEventInfo.Lock;

	/// <summary>
	///     Gets the mass analyzer scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MassAnalyzerType" /> for possible values</value>
	MassAnalyzerType IScanEventBase.MassAnalyzer => (MassAnalyzerType)_scanEventInfo.MassAnalyzerType;

	/// <summary>
	///     Gets the mass calibrator count.
	/// </summary>
	public int MassCalibratorCount => MassCalibrators.Length;

	/// <summary>
	///     Gets number of masses
	/// </summary>
	/// <value>The size of mass array</value>
	int IScanEventBase.MassCount => Reactions.Length;

	/// <summary>
	///     Gets the number of mass ranges for final scan
	/// </summary>
	/// <value>The size of mass range array</value>
	int IScanEventBase.MassRangeCount => MassRanges.Length;

	/// <summary>
	///     Gets the scan power setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MSOrderType" /> for possible values</value>
	MSOrderType IScanEventBase.MSOrder => (MSOrderType)_scanEventInfo.MSOrder;

	/// <summary>
	///     Gets the Multi notch (Synchronous Precursor Selection) type
	/// </summary>
	TriState IScanEventBase.MultiNotch => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.SpsMultiNotch).ToTriState();

	/// <summary>
	///     Gets the multi-photon dissociation setting.
	/// </summary>
	TriState IScanEventBase.MultiplePhotonDissociation => ((ScanFilterEnums.OnAnyOffTypes)_scanEventInfo.MultiPhotonDissociationType).ToTriState();

	/// <summary>
	///     Gets the multi-photon dissociation value.
	/// </summary>
	/// <value>Floating point multi-photon dissociation value</value>
	double IScanEventBase.MultiplePhotonDissociationValue => _scanEventInfo.MultiPhotonDissociation;

	/// <summary>
	///     Gets the Multiplex type
	/// </summary>
	TriState IScanEventBase.Multiplex => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.Multiplex).ToTriState();

	/// <summary>
	///     Gets MultiStateActivation type setting.
	/// </summary>
	TriState IScanEventBase.MultiStateActivation => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.MultiStateActivation).ToTriState();

	/// <summary>
	///     Gets the event Name.
	/// </summary>
	string IScanEventBase.Name => Name;

	/// <summary>
	///     Gets the parameter a.
	/// </summary>
	TriState IScanEventBase.ParamA => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamA).ToTriState();

	/// <summary>
	///     Gets the parameter b.
	/// </summary>
	TriState IScanEventBase.ParamB => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamB).ToTriState();

	/// <summary>
	///     Gets the parameter f.
	/// </summary>
	TriState IScanEventBase.ParamF => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamF).ToTriState();

	/// <summary>
	///     Gets the parameter r.
	/// </summary>
	TriState IScanEventBase.ParamR => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamR).ToTriState();

	/// <summary>
	///     Gets the parameter v.
	/// </summary>
	TriState IScanEventBase.ParamV => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.ParamV).ToTriState();

	/// <summary>
	///     Gets the photo ionization setting.
	/// </summary>
	TriState IScanEventBase.PhotoIonization => (TriState)_scanEventInfo.PhotoIonization;

	/// <summary>
	///     Gets the polarity scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.PolarityType" /> for possible values</value>
	PolarityType IScanEventBase.Polarity => (PolarityType)_scanEventInfo.Polarity;

	/// <summary>
	///     Gets pulsed dissociation setting.
	/// </summary>
	TriState IScanEventBase.PulsedQDissociation => (TriState)_scanEventInfo.PulsedQDissociationType;

	/// <summary>
	///     Gets the pulsed dissociation value.
	/// </summary>
	/// <value>Floating point pulsed dissociation value</value>
	double IScanEventBase.PulsedQDissociationValue => _scanEventInfo.PulsedQDissociation;

	/// <summary>
	///     Gets the scan data.
	/// </summary>
	ScanDataType IScanEventBase.ScanData => (ScanDataType)_scanEventInfo.ScanDataType;

	/// <summary>
	///     Gets the scan type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.ScanModeType" /> for possible values</value>
	ScanModeType IScanEventBase.ScanMode => ((ScanFilterEnums.ScanTypes)_scanEventInfo.ScanType).ToScanModeType();

	/// <summary>
	///     Gets encoded form of segment and scan event number.
	/// </summary>
	/// <value>HIWORD == segment, LOWORD == scan type</value>
	long IScanEventBase.ScanTypeIndex => _scanEventInfo.ScanTypeIndex;

	/// <summary>
	///     Gets the sector scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SectorScanType" /> for possible values</value>
	SectorScanType IScanEventBase.SectorScan => (SectorScanType)_scanEventInfo.SectorScan;

	/// <summary>
	///     Gets source fragmentation scan setting.
	/// </summary>
	TriState IScanEventBase.SourceFragmentation => (TriState)_scanEventInfo.SourceFragmentation;

	/// <summary>
	///     Gets the number of source fragmentation info values
	/// </summary>
	/// <value>The size of source fragmentation info array</value>
	int IScanEventBase.SourceFragmentationInfoCount => SourceFragmentations.Length;

	/// <summary>
	///     Gets the source fragmentation mass range count.
	/// </summary>
	int IScanEvent.SourceFragmentationMassRangeCount => SourceFragmentationMassRanges.Length;

	/// <summary>
	///     Gets the source fragmentation type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SourceFragmentationValueType" /> for possible values</value>
	SourceFragmentationValueType IScanEventBase.SourceFragmentationType => (SourceFragmentationValueType)_scanEventInfo.SourceFragmentationType;

	/// <summary>
	///     Gets supplemental activation type setting.
	/// </summary>
	TriState IScanEventBase.SupplementalActivation => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.SupplementalActivation).ToTriState();

	/// <summary>
	///     Gets the turbo scan setting.
	/// </summary>
	TriState IScanEventBase.TurboScan => (TriState)_scanEventInfo.TurboScan;

	/// <summary>
	///     Gets the ultra scan setting.
	/// </summary>
	TriState IScanEventBase.Ultra => (TriState)_scanEventInfo.Ultra;

	/// <summary>
	///     Gets the wideband scan setting.
	/// </summary>
	TriState IScanEventBase.Wideband => ((ScanFilterEnums.OffOnTypes)_scanEventInfo.Wideband).ToTriState();

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" /> class.
	/// </summary>
	/// <param name="scanEvent">
	///     The scan event.
	/// </param>
	public ScanEvent(ScanEvent scanEvent)
	{
		_massPrecisionDecimals = scanEvent._massPrecisionDecimals;
		_scanEventInfo = scanEvent._scanEventInfo;
		HeaderFilterMassPrecision = scanEvent.HeaderFilterMassPrecision;
		AccurateMassType = scanEvent.AccurateMassType;
		Name = scanEvent.Name;
		ScanTypeLocation = scanEvent.ScanTypeLocation;
		_hash1 = scanEvent._hash1;
		_hash2 = scanEvent._hash2;
		_hash3 = scanEvent._hash3;
		_hash4 = scanEvent._hash4;
		_hasSourceFragmentationRanges = scanEvent._hasSourceFragmentationRanges;
		_hasDissociationValues = scanEvent._hasDissociationValues;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" /> class.
	/// </summary>
	internal ScanEvent()
	{
		MassRanges = NoMassRanges;
		SourceFragmentationMassRanges = Array.Empty<MassRangeStruct>();
		MassCalibrators = NoDoubles;
		Reactions = Array.Empty<Reaction>();
		SourceFragmentations = NoDoubles;
		_massPrecisionDecimals = 2;
		HeaderFilterMassPrecision = 2;
		UniqueScanEventIndex = -1;
		_scanEventInfo = DefaultInfo;
		AccurateMassType = ScanFilterEnums.AccurateMassTypes.AcceptAnyAccurateMass;
		ScanTypeLocation = -1;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" /> class.
	/// </summary>
	/// <param name="header">The header.</param>
	/// <param name="uniqueScanEventIndex">Index of the unique scan event.</param>
	internal ScanEvent(ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader header, int uniqueScanEventIndex = -1)
		: this()
	{
		HeaderFilterMassPrecision = header.FilterMassPrecision;
		_massPrecisionDecimals = HeaderFilterMassPrecision;
		UniqueScanEventIndex = uniqueScanEventIndex;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" /> class.
	/// </summary>
	/// <param name="filterMassPrecision">The filter mass precision.</param>
	/// <param name="uniqueScanEventIndex">Index of the unique scan event.</param>
	internal ScanEvent(int filterMassPrecision, int uniqueScanEventIndex = -1)
		: this()
	{
		HeaderFilterMassPrecision = filterMassPrecision;
		_massPrecisionDecimals = filterMassPrecision;
		UniqueScanEventIndex = uniqueScanEventIndex;
	}

	/// <summary>
	///     compare to.
	/// </summary>
	/// <param name="other">
	///     The other.
	/// </param>
	/// <returns>
	///     A 32-bit signed integer that indicates the relative order of the objects being compared.
	///     The return value has the following meanings:
	///     Value              Meaning
	///     Less than zero     This object is less than the <paramref name="other" /> parameter.
	///     Zero               This object is equal to <paramref name="other" />.
	///     Greater than zero  This object is greater than <paramref name="other" />.
	/// </returns>
	public int CompareTo(ScanEvent other)
	{
		if (this == other)
		{
			return 0;
		}
		int num = ((_hash1 != other._hash1) ? _scanEventInfo.ComparePart1(other._scanEventInfo) : ((_hash2 != other._hash2) ? _scanEventInfo.ComparePart1Hash1(other._scanEventInfo) : _scanEventInfo.ComparePart1Hash1Hash2(other._scanEventInfo)));
		if (num != 0)
		{
			return num;
		}
		num = ComparePart2(other, compareNames: true);
		if (num != 0)
		{
			return num;
		}
		ScanEventInfoStruct scanEventInfo = _scanEventInfo;
		ScanEventInfoStruct scanEventInfo2 = other._scanEventInfo;
		num = scanEventInfo.SourceFragmentation - scanEventInfo2.SourceFragmentation;
		if (num != 0)
		{
			return num;
		}
		num = CompareReactions(Reactions, other.Reactions);
		if (num != 0)
		{
			return num;
		}
		num = CompareMassRanges(MassRanges, other.MassRanges);
		if (num != 0)
		{
			return num;
		}
		num = CompareListsOfDoubles(MassCalibrators, other.MassCalibrators, 0.01);
		if (num != 0)
		{
			return num;
		}
		num = scanEventInfo.Lock - scanEventInfo2.Lock;
		if (num != 0)
		{
			return num;
		}
		num = scanEventInfo.TurboScan - scanEventInfo2.TurboScan;
		if (num != 0)
		{
			return num;
		}
		num = scanEventInfo.UpperFlags - scanEventInfo2.UpperFlags;
		if (num != 0)
		{
			return num;
		}
		num = scanEventInfo.LowerFlags - scanEventInfo2.LowerFlags;
		if (num != 0)
		{
			return num;
		}
		UniqueScanEventIndex = other.UniqueScanEventIndex;
		return num;
	}

	/// <summary>
	///     The method compares two <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" />objects.
	/// </summary>
	/// <param name="other">
	///     The other <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" /> object.
	/// </param>
	/// <returns>
	///     True if equal.
	/// </returns>
	public virtual bool Equals(ScanEvent other)
	{
		return CompareTo(other) == 0;
	}

	/// <summary>
	///     get run header filter mass precision.
	/// </summary>
	/// <returns>
	///     The mass precision
	/// </returns>
	public int GetRunHeaderFilterMassPrecision()
	{
		return HeaderFilterMassPrecision;
	}

	/// <summary>
	///     Convert to auto filter string.
	/// </summary>
	/// <param name="scanEvent">
	///     The scan event.
	/// </param>
	/// <param name="massPrecision">
	///     The mass precision.
	/// </param>
	/// <param name="charsMax">
	///     The chars max.
	/// </param>
	/// <param name="energyPrecision">
	///     The energy precision.
	/// </param>
	/// <param name="formatProvider">format for current culture</param>
	/// <param name="listSeparator">list separator for localization</param>
	/// <returns>
	///     The <see cref="T:System.String" />.
	/// </returns>
	public string ToAutoFilterString(IRawFileReaderScanEvent scanEvent, int massPrecision = -1, int charsMax = -1, int energyPrecision = -1, IFormatProvider formatProvider = null, string listSeparator = ",")
	{
		if (_massPrecisionFormat == null)
		{
			_massPrecisionFormat = "f" + _massPrecisionDecimals;
		}
		return FormulateAutoFilterString(scanEvent, _massPrecisionFormat, massPrecision, charsMax, energyPrecision, formatProvider ?? CultureInfo.InvariantCulture, listSeparator);
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">The viewer (memory map into file).</param>
	/// <param name="dataOffset">The data offset (into the memory map).</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long num = dataOffset;
		num += ReadStructure(viewer, num, fileRevision);
		ScanTypeLocation = -1;
		FixUpDefaults(fileRevision);
		CalculateHash();
		num += ReadReactionsArray(viewer, num, fileRevision);
		int num2 = viewer.ReadIntExt(ref num);
		MassRanges = new MassRangeStruct[num2];
		for (int i = 0; i < num2; i++)
		{
			double lowMass = viewer.ReadDoubleExt(ref num);
			double highMass = viewer.ReadDoubleExt(ref num);
			MassRanges[i] = new MassRangeStruct(lowMass, highMass);
		}
		MassCalibrators = viewer.ReadDoublesExt(ref num);
		SourceFragmentations = viewer.ReadDoublesExt(ref num);
		SourceFragmentationMassRanges = MassRangeStruct.LoadArray(viewer, ref num);
		Name = ((fileRevision >= 65) ? viewer.ReadStringExt(ref num) : string.Empty);
		return num - dataOffset;
	}

	/// <summary>
	///     Retrieves activation type at 0-based index.
	/// </summary>
	/// <remarks>
	///     Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of activations.
	/// </remarks>
	/// <param name="index">
	///     Index of activation to be retrieved
	/// </param>
	/// <returns>
	///     activation of MS step;
	///     See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.ActivationType" /> for possible values
	/// </returns>
	ActivationType IScanEventBase.GetActivation(int index)
	{
		return Reactions[index].ActivationType;
	}

	/// <summary>
	///     Retrieves precursor(collision) energy value for MS step at 0-based index.
	/// </summary>
	/// <remarks>
	///     Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of energies.
	/// </remarks>
	/// <param name="index">
	///     Index of precursor(collision) energy to be retrieved
	/// </param>
	/// <returns>
	///     precursor(collision) energy of MS step
	/// </returns>
	double IScanEventBase.GetEnergy(int index)
	{
		return Reactions[index].CollisionEnergy;
	}

	/// <summary>
	///     Retrieves precursor(collision) energy validation flag at 0-based index.
	/// </summary>
	/// <remarks>
	///     Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of precursor(collision) energy validations.
	/// </remarks>
	/// <param name="index">
	///     Index of precursor(collision) energy validation to be retrieved
	/// </param>
	/// <returns>
	///     precursor(collision) energy validation of MS step;
	///     See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.EnergyType" /> for possible values
	/// </returns>
	EnergyType IScanEventBase.GetEnergyValid(int index)
	{
		if (!Reactions[index].CollisionEnergyValid)
		{
			return EnergyType.Any;
		}
		return EnergyType.Valid;
	}

	/// <summary>
	///     Gets the first precursor mass.
	/// </summary>
	/// <param name="index">
	///     The index.
	/// </param>
	/// <returns>
	///     The first mass
	/// </returns>
	double IScanEventBase.GetFirstPrecursorMass(int index)
	{
		return Reactions[index].FirstPrecursorMass;
	}

	/// <summary>
	///     Retrieves multiple activations flag at 0-based index of masses.
	/// </summary>
	/// <remarks>
	///     Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of masses.
	/// </remarks>
	/// <param name="index">
	///     Index of flag to be retrieved
	/// </param>
	/// <returns>
	///     true if mass at given index has multiple activations;  false otherwise
	/// </returns>
	bool IScanEventBase.GetIsMultipleActivation(int index)
	{
		return Reactions[index].MultipleActivation;
	}

	/// <summary>
	///     Get the isolation width.
	/// </summary>
	/// <param name="index">
	///     The index.
	/// </param>
	/// <returns>
	///     The isolation width
	/// </returns>
	double IScanEventBase.GetIsolationWidth(int index)
	{
		return Reactions[index].IsolationWidth;
	}

	/// <summary>
	///     Get the isolation width offset.
	/// </summary>
	/// <param name="index">
	///     The index.
	/// </param>
	/// <returns>
	///     The isolation width offset
	/// </returns>
	double IScanEventBase.GetIsolationWidthOffset(int index)
	{
		return Reactions[index].IsolationWidthOffset;
	}

	/// <summary>
	///     Gets the last precursor mass.
	/// </summary>
	/// <param name="index">
	///     The index.
	/// </param>
	/// <returns>
	///     The last mass
	/// </returns>
	double IScanEventBase.GetLastPrecursorMass(int index)
	{
		return Reactions[index].LastPrecursorMass;
	}

	/// <summary>
	///     Retrieves mass value for MS step at 0-based index.
	/// </summary>
	/// <remarks>
	///     Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassCount" /> to get the count of mass values.
	/// </remarks>
	/// <param name="index">
	///     Index of mass value to be retrieved
	/// </param>
	/// <returns>
	///     Mass value of MS step
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= MassCount</exception>
	double IScanEventBase.GetMass(int index)
	{
		return Reactions[index].PrecursorMass;
	}

	/// <summary>
	///     Get the mass calibrator, at a given index.
	/// </summary>
	/// <param name="index">
	///     The index, which should be from 0 to MassCalibratorCount -1
	/// </param>
	/// <returns>
	///     The mass calibrator.
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Thrown when requesting calibrator above count</exception>
	double IScanEvent.GetMassCalibrator(int index)
	{
		return MassCalibrators[index];
	}

	/// <summary>
	///     Retrieves mass range for final scan at 0-based index.
	/// </summary>
	/// <remarks>
	///     Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.MassRangeCount" /> to get the count of mass ranges.
	/// </remarks>
	/// <param name="index">
	///     Index of mass range to be retrieved
	/// </param>
	/// <returns>
	///     Mass range for final scan at 0-based index
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= MassRangeCount</exception>
	IRangeAccess IScanEventBase.GetMassRange(int index)
	{
		MassRangeStruct massRangeStruct = MassRanges[index];
		return new ThermoFisher.CommonCore.Data.Business.Range(massRangeStruct.LowMass, massRangeStruct.HighMass);
	}

	/// <summary>
	///     Determine if a precursor range is valid.
	/// </summary>
	/// <param name="index">
	///     The index.
	/// </param>
	/// <returns>
	///     true if valid
	/// </returns>
	bool IScanEventBase.GetPrecursorRangeValidity(int index)
	{
		return Reactions[index].PrecursorRangeIsValid;
	}

	/// <summary>
	///     Gets the reaction data for the mass at 0 based index
	///     Equivalent to calling GetMass, GetEnergy, GetPrecursorRangeValidity, GetFirstPrecursorMass
	///     GetLastPrecursorMass,GetIsolationWidth,GetIsolationWidthOffset,GetEnergyValid
	///     GetActivation, GetIsMultipleActivation.
	///     Depending on the implementation of the interface, this call may be more efficient
	///     that calling several of the methods listed.
	/// </summary>
	/// <param name="index">index of reaction</param>
	/// <returns>reaction details</returns>
	IReaction IScanEventBase.GetReaction(int index)
	{
		return Reactions[index];
	}

	/// <summary>
	///     Retrieves a source fragmentation info value at 0-based index.
	/// </summary>
	/// <remarks>
	///     Use <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanEventBase.SourceFragmentationInfoCount" /> to get the count of source
	///     fragmentation info values.
	/// </remarks>
	/// <param name="index">
	///     Index of source fragmentation info to be retrieved
	/// </param>
	/// <returns>
	///     Source Fragmentation info value at 0-based index
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= SourceFragmentationInfoCount</exception>
	double IScanEventBase.GetSourceFragmentationInfo(int index)
	{
		return SourceFragmentations[index];
	}

	public IScanEventExtended GetExtensions()
	{
		return this;
	}

	/// <summary>
	///     Get the source fragmentation mass range, at a give index.
	/// </summary>
	/// <param name="index">
	///     The index.
	/// </param>
	/// <returns>
	///     The mass range.
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= SourceFragmentationMassRangeCount</exception>
	ThermoFisher.CommonCore.Data.Business.Range IScanEvent.GetSourceFragmentationMassRange(int index)
	{
		MassRangeStruct massRangeStruct = SourceFragmentationMassRanges[index];
		return new ThermoFisher.CommonCore.Data.Business.Range(massRangeStruct.LowMass, massRangeStruct.HighMass);
	}

	/// <summary>
	///     Convert to string.
	/// </summary>
	/// <returns>
	///     The converted scanning method.
	/// </returns>
	string IScanEvent.ToString()
	{
		return ToString();
	}

	/// <summary>
	///     Pack the "non array" items from an event into
	///     the "scan event information" structure.
	/// </summary>
	/// <param name="scanEvent">
	///     The scan event.
	/// </param>
	/// <returns>
	///     The <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanEventInfo.ScanEventInfoStruct" />.
	/// </returns>
	internal static ScanEventInfoStruct CreateEventInfo(IScanEvent scanEvent)
	{
		ScanEventInfoStruct defaultInfo = DefaultInfo;
		defaultInfo.AccurateMassType = scanEvent.AccurateMass.ToAccurateMass();
		defaultInfo.ParamB = (byte)scanEvent.ParamB.ToOffOnType();
		defaultInfo.ParamA = (byte)scanEvent.ParamA.ToOffOnType();
		defaultInfo.ParamF = (byte)scanEvent.ParamF.ToOffOnType();
		defaultInfo.ParamR = (byte)scanEvent.ParamR.ToOffOnType();
		defaultInfo.ParamV = (byte)scanEvent.ParamV.ToOffOnType();
		defaultInfo.SpsMultiNotch = (byte)scanEvent.MultiNotch.ToOffOnType();
		defaultInfo.CompensationVoltage = (byte)scanEvent.CompensationVoltage;
		defaultInfo.CompensationVoltageType = (byte)scanEvent.CompensationVoltType;
		defaultInfo.Corona = (byte)scanEvent.Corona;
		defaultInfo.DetectorValue = scanEvent.DetectorValue;
		defaultInfo.DependentData = (byte)scanEvent.Dependent.ToOffOnType();
		defaultInfo.Detector = (byte)scanEvent.Detector.ToDetectorType();
		defaultInfo.ElectronCaptureDissociation = scanEvent.ElectronCaptureDissociationValue;
		defaultInfo.ElectronCaptureDissociationType = (byte)scanEvent.ElectronCaptureDissociation.ToOnAnyOffType();
		defaultInfo.ElectronTransferDissociation = scanEvent.ElectronTransferDissociationValue;
		defaultInfo.ElectronTransferDissociationType = (byte)scanEvent.ElectronTransferDissociation;
		defaultInfo.Enhanced = (byte)scanEvent.Enhanced;
		defaultInfo.FreeRegion = (byte)scanEvent.FieldFreeRegion;
		defaultInfo.HigherEnergyCID = scanEvent.HigherEnergyCiDValue;
		defaultInfo.HigherEnergyCIDType = (byte)scanEvent.HigherEnergyCiD;
		defaultInfo.IsValid = 1;
		defaultInfo.IonizationMode = (byte)scanEvent.IonizationMode;
		defaultInfo.IsCustom = (scanEvent.IsCustom ? ((byte)1) : ((byte)0));
		defaultInfo.Lock = (byte)scanEvent.Lock;
		defaultInfo.MultiStateActivation = (byte)scanEvent.MultiStateActivation.ToOffOnType();
		defaultInfo.MSOrder = (sbyte)scanEvent.MSOrder;
		defaultInfo.MassAnalyzerType = (byte)scanEvent.MassAnalyzer;
		defaultInfo.MultiPhotonDissociation = scanEvent.MultiplePhotonDissociationValue;
		defaultInfo.MultiPhotonDissociationType = (byte)scanEvent.MultiplePhotonDissociation.ToOnAnyOffType();
		defaultInfo.Multiplex = (byte)scanEvent.Multiplex.ToOffOnType();
		defaultInfo.PhotoIonization = (byte)scanEvent.PhotoIonization;
		defaultInfo.PulsedQDissociation = scanEvent.PulsedQDissociationValue;
		defaultInfo.PulsedQDissociationType = (byte)scanEvent.PulsedQDissociation;
		defaultInfo.Polarity = (byte)scanEvent.Polarity;
		defaultInfo.SourceFragmentation = (byte)scanEvent.SourceFragmentation;
		defaultInfo.SourceFragmentationType = (byte)scanEvent.SourceFragmentationType;
		defaultInfo.ScanDataType = (byte)scanEvent.ScanData;
		defaultInfo.ScanType = (byte)scanEvent.ScanMode.ToScanType();
		defaultInfo.ScanTypeIndex = (int)scanEvent.ScanTypeIndex;
		defaultInfo.SectorScan = (byte)scanEvent.SectorScan;
		defaultInfo.SupplementalActivation = (byte)scanEvent.SupplementalActivation.ToOffOnType();
		defaultInfo.TurboScan = (byte)scanEvent.TurboScan;
		defaultInfo.Ultra = (byte)scanEvent.Ultra;
		defaultInfo.Wideband = (byte)scanEvent.Wideband.ToOffOnType();
		IScanEventExtended extensions = scanEvent.GetExtensions();
		if (extensions != null)
		{
			defaultInfo.LowerFlags = (ushort)extensions.LowerCaseFlags;
			defaultInfo.UpperFlags = extensions.UpperCaseFlags;
		}
		return defaultInfo;
	}

	/// <summary>
	///     Construct from event, from an interface.
	///     This can permit an application to build a scan filter
	///     Use "ScanEventBuilder" to make an event
	///     the use an IRawDataPlus method "event to filter"
	///     which internally calls this, then uses code already done
	///     to map "ScanEvent" to "WrappedScanFilter"
	/// </summary>
	/// <param name="oldEvent">
	///     The old event.
	/// </param>
	/// <returns>
	///     The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" />.
	/// </returns>
	internal static ScanEvent FromEvent(IScanEvent oldEvent)
	{
		ScanEvent scanEvent = new ScanEvent
		{
			_scanEventInfo = CreateEventInfo(oldEvent)
		};
		int massCalibratorCount = oldEvent.MassCalibratorCount;
		double[] array = new double[massCalibratorCount];
		for (int i = 0; i < massCalibratorCount; i++)
		{
			array[i] = oldEvent.GetMassCalibrator(i);
		}
		scanEvent.MassCalibrators = array;
		int massRangeCount = oldEvent.MassRangeCount;
		MassRangeStruct[] array2 = (scanEvent.MassRanges = new MassRangeStruct[massRangeCount]);
		MassRangeStruct[] array4 = array2;
		for (int j = 0; j < massRangeCount; j++)
		{
			array4[j] = new MassRangeStruct(oldEvent.GetMassRange(j));
		}
		scanEvent.SourceFragmentationMassRanges = CopyRangeTable(oldEvent.GetSourceFragmentationMassRange, oldEvent.SourceFragmentationMassRangeCount);
		int massCount = oldEvent.MassCount;
		if (massCount > 0)
		{
			Reaction[] array5 = new Reaction[massCount];
			for (int k = 0; k < massCount; k++)
			{
				array5[k] = new Reaction(oldEvent.GetReaction(k));
			}
			scanEvent.Reactions = array5;
		}
		int sourceFragmentationInfoCount = oldEvent.SourceFragmentationInfoCount;
		double[] array6 = new double[sourceFragmentationInfoCount];
		for (int l = 0; l < sourceFragmentationInfoCount; l++)
		{
			array6[l] = oldEvent.GetSourceFragmentationInfo(l);
		}
		scanEvent.SourceFragmentations = array6;
		scanEvent.ScanTypeLocation = (int)oldEvent.ScanTypeIndex;
		scanEvent.CalculateHash();
		return scanEvent;
	}

	/// <summary>
	///     Calculate hash codes, for faster sort
	/// </summary>
	internal void CalculateHash()
	{
		_hash1 = _scanEventInfo.GetHash1();
		_hash2 = _scanEventInfo.GetHash2();
		_hash3 = _scanEventInfo.GetHash3();
		_hash4 = _scanEventInfo.GetHash4();
		MassRangeStruct[] sourceFragmentationMassRanges = SourceFragmentationMassRanges;
		_hasSourceFragmentationRanges = sourceFragmentationMassRanges != null && sourceFragmentationMassRanges.Length != 0;
		_hasDissociationValues = _scanEventInfo.MultiPhotonDissociation > 0.0 || _scanEventInfo.ElectronCaptureDissociation > 0.0 || _scanEventInfo.PulsedQDissociation > 0.0 || _scanEventInfo.ElectronTransferDissociation > 0.0 || _scanEventInfo.HigherEnergyCID > 0.0;
	}

	/// <summary>
	///     table of mass range
	/// </summary>
	/// <param name="fromFunc">
	///     Function to get item to copy.
	/// </param>
	/// <param name="length">
	///     The length of the table.
	/// </param>
	private static MassRangeStruct[] CopyRangeTable(Func<int, ThermoFisher.CommonCore.Data.Business.Range> fromFunc, int length)
	{
		if (length == 0)
		{
			return Array.Empty<MassRangeStruct>();
		}
		MassRangeStruct[] array = new MassRangeStruct[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = new MassRangeStruct(fromFunc(i));
		}
		return array;
	}

	/// <summary>
	///     Compares items in this event, up to the reactions tests.
	///     Reaction tests are done differently by various callers.
	/// </summary>
	/// <param name="source">The source.</param>
	/// <param name="other">The other.</param>
	/// <returns>
	///     A 32-bit signed integer that indicates the relative order of the objects being compared.
	///     The return value has the following meanings:
	///     Value              Meaning
	///     Less than zero     This object is less than the <paramref name="other" /> parameter.
	///     Zero               This object is equal to <paramref name="other" />.
	///     Greater than zero  This object is greater than <paramref name="other" />.
	/// </returns>
	public int ComparePart1(ScanEvent source, ScanEvent other)
	{
		if (source == other)
		{
			return 0;
		}
		if (source._hash1 == other._hash1)
		{
			if (source._hash2 == other._hash2)
			{
				return source._scanEventInfo.ComparePart1Hash1Hash2(other._scanEventInfo);
			}
			return source._scanEventInfo.ComparePart1Hash1(other._scanEventInfo);
		}
		return source._scanEventInfo.ComparePart1(other._scanEventInfo);
	}

	/// <summary>
	///     Compares items in this event, after the reactions tests.
	///     Reaction tests are done differently by various callers.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <param name="compareNames">compare names as final test (not needed for auto filter)</param>
	/// <returns>
	///     A 32-bit signed integer that indicates the relative order of the objects being compared.
	///     The return value has the following meanings:
	///     Value              Meaning
	///     Less than zero     This object is less than the <paramref name="other" /> parameter.
	///     Zero               This object is equal to <paramref name="other" />.
	///     Greater than zero  This object is greater than <paramref name="other" />.
	/// </returns>
	public int ComparePart2(ScanEvent other, bool compareNames)
	{
		if (_hasSourceFragmentationRanges || other._hasSourceFragmentationRanges)
		{
			int num = CompareMassRanges(SourceFragmentationMassRanges, other.SourceFragmentationMassRanges);
			if (num != 0)
			{
				return num;
			}
		}
		if (_hash3 == other._hash3)
		{
			return ComparePart2Hash3(other, compareNames);
		}
		ScanEventInfoStruct scanEventInfo = _scanEventInfo;
		ScanEventInfoStruct scanEventInfo2 = other._scanEventInfo;
		int num2 = scanEventInfo.SourceFragmentationType - scanEventInfo2.SourceFragmentationType;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.CompensationVoltageType - scanEventInfo2.CompensationVoltageType;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = CompareListsOfDoubles(SourceFragmentations, other.SourceFragmentations, 0.01);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.MassAnalyzerType - scanEventInfo2.MassAnalyzerType;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.SectorScan - scanEventInfo2.SectorScan;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.FreeRegion - scanEventInfo2.FreeRegion;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.Ultra - scanEventInfo2.Ultra;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.Enhanced - scanEventInfo2.Enhanced;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.MultiPhotonDissociationType - scanEventInfo2.MultiPhotonDissociationType;
		if (num2 != 0)
		{
			return num2;
		}
		if (scanEventInfo.MultiPhotonDissociationType == 0)
		{
			num2 = Utilities.CompareDoubles(scanEventInfo.MultiPhotonDissociation, scanEventInfo2.MultiPhotonDissociation, 0.01);
			if (num2 != 0)
			{
				return num2;
			}
		}
		num2 = scanEventInfo.ElectronCaptureDissociationType - scanEventInfo2.ElectronCaptureDissociationType;
		if (num2 != 0)
		{
			return num2;
		}
		if (scanEventInfo.ElectronCaptureDissociationType == 0)
		{
			num2 = Utilities.CompareDoubles(scanEventInfo.ElectronCaptureDissociation, scanEventInfo2.ElectronCaptureDissociation, 0.01);
			if (num2 != 0)
			{
				return num2;
			}
		}
		num2 = scanEventInfo.PulsedQDissociationType - scanEventInfo2.PulsedQDissociationType;
		if (num2 != 0)
		{
			return num2;
		}
		if (scanEventInfo.PulsedQDissociationType == 0)
		{
			num2 = Utilities.CompareDoubles(scanEventInfo.PulsedQDissociation, scanEventInfo2.PulsedQDissociation, 0.01);
			if (num2 != 0)
			{
				return num2;
			}
		}
		num2 = scanEventInfo.ElectronTransferDissociationType - scanEventInfo2.ElectronTransferDissociationType;
		if (num2 != 0)
		{
			return num2;
		}
		if (scanEventInfo.ElectronTransferDissociationType == 0)
		{
			num2 = Utilities.CompareDoubles(scanEventInfo.ElectronTransferDissociation, scanEventInfo2.ElectronTransferDissociation, 0.01);
			if (num2 != 0)
			{
				return num2;
			}
		}
		num2 = scanEventInfo.HigherEnergyCIDType - scanEventInfo2.HigherEnergyCIDType;
		if (num2 != 0)
		{
			return num2;
		}
		if (scanEventInfo.HigherEnergyCIDType == 0)
		{
			num2 = Utilities.CompareDoubles(scanEventInfo.HigherEnergyCID, scanEventInfo2.HigherEnergyCID, 0.01);
			if (num2 != 0)
			{
				return num2;
			}
		}
		num2 = scanEventInfo.PhotoIonization - scanEventInfo2.PhotoIonization;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEventInfo.ScanTypeIndex - scanEventInfo2.ScanTypeIndex;
		if (compareNames && num2 == 0)
		{
			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}
		return num2;
	}

	/// <summary>
	///     Compares items in this event, after the reactions tests.
	///     Reaction tests are done differently by various callers.
	///     Skips items which are found equal in hash3
	/// </summary>
	/// <param name="other">The other.</param>
	/// <param name="compareNames">compare names as final test (not needed for auto filter)</param>
	/// <returns>
	///     A 32-bit signed integer that indicates the relative order of the objects being compared.
	///     The return value has the following meanings:
	///     Value              Meaning
	///     Less than zero     This object is less than the <paramref name="other" /> parameter.
	///     Zero               This object is equal to <paramref name="other" />.
	///     Greater than zero  This object is greater than <paramref name="other" />.
	/// </returns>
	public int ComparePart2Hash3(ScanEvent other, bool compareNames)
	{
		int num = CompareListsOfDoubles(SourceFragmentations, other.SourceFragmentations, 0.01);
		if (num != 0)
		{
			return num;
		}
		if (_hash4 == other._hash4)
		{
			return ComparePart2Hash3Hash4(other, compareNames);
		}
		ScanEventInfoStruct scanEventInfo = _scanEventInfo;
		ScanEventInfoStruct scanEventInfo2 = other._scanEventInfo;
		if (scanEventInfo.MultiPhotonDissociationType == 0)
		{
			num = Utilities.CompareDoubles(scanEventInfo.MultiPhotonDissociation, scanEventInfo2.MultiPhotonDissociation, 0.01);
			if (num != 0)
			{
				return num;
			}
		}
		num = scanEventInfo.ElectronCaptureDissociationType - scanEventInfo2.ElectronCaptureDissociationType;
		if (num != 0)
		{
			return num;
		}
		if (scanEventInfo.ElectronCaptureDissociationType == 0)
		{
			num = Utilities.CompareDoubles(scanEventInfo.ElectronCaptureDissociation, scanEventInfo2.ElectronCaptureDissociation, 0.01);
			if (num != 0)
			{
				return num;
			}
		}
		num = scanEventInfo.PulsedQDissociationType - scanEventInfo2.PulsedQDissociationType;
		if (num != 0)
		{
			return num;
		}
		if (scanEventInfo.PulsedQDissociationType == 0)
		{
			num = Utilities.CompareDoubles(scanEventInfo.PulsedQDissociation, scanEventInfo2.PulsedQDissociation, 0.01);
			if (num != 0)
			{
				return num;
			}
		}
		num = scanEventInfo.ElectronTransferDissociationType - scanEventInfo2.ElectronTransferDissociationType;
		if (num != 0)
		{
			return num;
		}
		if (scanEventInfo.ElectronTransferDissociationType == 0)
		{
			num = Utilities.CompareDoubles(scanEventInfo.ElectronTransferDissociation, scanEventInfo2.ElectronTransferDissociation, 0.01);
			if (num != 0)
			{
				return num;
			}
		}
		num = scanEventInfo.HigherEnergyCIDType - scanEventInfo2.HigherEnergyCIDType;
		if (num != 0)
		{
			return num;
		}
		if (scanEventInfo.HigherEnergyCIDType == 0)
		{
			num = Utilities.CompareDoubles(scanEventInfo.HigherEnergyCID, scanEventInfo2.HigherEnergyCID, 0.01);
			if (num != 0)
			{
				return num;
			}
		}
		num = scanEventInfo.PhotoIonization - scanEventInfo2.PhotoIonization;
		if (num != 0)
		{
			return num;
		}
		num = scanEventInfo.ScanTypeIndex - scanEventInfo2.ScanTypeIndex;
		if (compareNames && num == 0)
		{
			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}
		return num;
	}

	/// <summary>
	///     Compares items in this event, after the reactions tests.
	///     Reaction tests are done differently by various callers.
	///     Skips items which are fouud equal in hash3 and hash4
	/// </summary>
	/// <param name="other">The other.</param>
	/// <param name="compareNames">compare names as final test (not needed for auto filter)</param>
	/// <returns>
	///     A 32-bit signed integer that indicates the relative order of the objects being compared.
	///     The return value has the following meanings:
	///     Value              Meaning
	///     Less than zero     This object is less than the <paramref name="other" /> parameter.
	///     Zero               This object is equal to <paramref name="other" />.
	///     Greater than zero  This object is greater than <paramref name="other" />.
	/// </returns>
	public int ComparePart2Hash3Hash4(ScanEvent other, bool compareNames)
	{
		if (_hasDissociationValues || other._hasDissociationValues)
		{
			ScanEventInfoStruct scanEventInfo = _scanEventInfo;
			ScanEventInfoStruct scanEventInfo2 = other._scanEventInfo;
			if (scanEventInfo.MultiPhotonDissociationType == 0)
			{
				int num = Utilities.CompareDoubles(scanEventInfo.MultiPhotonDissociation, scanEventInfo2.MultiPhotonDissociation, 0.01);
				if (num != 0)
				{
					return num;
				}
			}
			if (scanEventInfo.ElectronCaptureDissociationType == 0)
			{
				int num = Utilities.CompareDoubles(scanEventInfo.ElectronCaptureDissociation, scanEventInfo2.ElectronCaptureDissociation, 0.01);
				if (num != 0)
				{
					return num;
				}
			}
			if (scanEventInfo.PulsedQDissociationType == 0)
			{
				int num = Utilities.CompareDoubles(scanEventInfo.PulsedQDissociation, scanEventInfo2.PulsedQDissociation, 0.01);
				if (num != 0)
				{
					return num;
				}
			}
			if (scanEventInfo.ElectronTransferDissociationType == 0)
			{
				int num = Utilities.CompareDoubles(scanEventInfo.ElectronTransferDissociation, scanEventInfo2.ElectronTransferDissociation, 0.01);
				if (num != 0)
				{
					return num;
				}
			}
			if (scanEventInfo.HigherEnergyCIDType == 0)
			{
				int num = Utilities.CompareDoubles(scanEventInfo.HigherEnergyCID, scanEventInfo2.HigherEnergyCID, 0.01);
				if (num != 0)
				{
					return num;
				}
			}
		}
		if (compareNames)
		{
			return string.Compare(Name, other.Name, StringComparison.Ordinal);
		}
		return 0;
	}

	/// <summary>
	///     create editable (mutable) scan event.
	/// </summary>
	/// <returns>
	///     The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEventEditor" />.
	/// </returns>
	public ScanEventEditor CreateEditor()
	{
		return new ScanEventEditor(this);
	}

	/// <summary>
	///     Convert the internal scan event info struct to byte array.
	///     <para />
	///     This method is intended to return the internal scan event info struct as "byte array", such that
	///     we don't have to be encoded from the properties, when exporting to a new file.
	/// </summary>
	/// <returns>
	///     The byte array from the internal scan event info struct
	/// </returns>
	public byte[] GetBytesFromScanEventInfo()
	{
		return WriterHelper.StructToByteArray(_scanEventInfo, ScanEventInfoStructSize);
	}

	/// <summary>
	///     The method returns the string representation of the scan event object.
	/// </summary>
	/// <returns>
	///     The <see cref="T:System.String" /> representation of the scan event object.
	/// </returns>
	public override string ToString()
	{
		return new FilterScanEvent(this)
		{
			FilterMassPrecision = HeaderFilterMassPrecision
		}.ToString();
	}

	/// <summary>
	///     The method compares two lists of doubles.
	/// </summary>
	/// <param name="list1">
	///     The first list of doubles.
	/// </param>
	/// <param name="list2">
	///     The second list of doubles.
	/// </param>
	/// <param name="tolerance">
	///     The tolerance for equivalence.
	/// </param>
	/// <returns>
	///     Return an <see cref="T:System.Int32" /> that has one of three values:
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <description>Meaning</description>
	///         </listheader>
	///         <term>Less than zero</term>
	///         <description>The current instance precedes the object specified by the CompareTo method in the sort order.</description>
	///         <term>Zero</term>
	///         <description>
	///             This current instance occurs in the same position in the sort order as the object specified by the
	///             CompareTo method.
	///         </description>
	///         <term>Greater than zero</term>
	///         <description>This current instance follows the object specified by the CompareTo method in the sort order.</description>
	///     </list>
	/// </returns>
	private static int CompareListsOfDoubles(double[] list1, double[] list2, double tolerance)
	{
		int i = list1.Length - list2.Length;
		if (i != 0)
		{
			return i;
		}
		for (; i < list1.Length; i++)
		{
			int num = Utilities.CompareDoubles(list1[i], list2[i], tolerance);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	/// <summary>
	///     The method compares two lists of mass ranges.
	///     This version uses "default" tolerance
	/// </summary>
	/// <param name="list1">
	///     The first list of mass ranges.
	/// </param>
	/// <param name="list2">
	///     The second list of mass ranges.
	/// </param>
	/// <returns>
	///     Return an <see cref="T:System.Int32" /> that has one of three values:
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <description>Meaning</description>
	///         </listheader>
	///         <term>Less than zero</term>
	///         <description>The current instance precedes the object specified by the CompareTo method in the sort order.</description>
	///         <term>Zero</term>
	///         <description>
	///             This current instance occurs in the same position in the sort order as the object specified by the
	///             CompareTo method.
	///         </description>
	///         <term>Greater than zero</term>
	///         <description>This current instance follows the object specified by the CompareTo method in the sort order.</description>
	///     </list>
	/// </returns>
	private static int CompareMassRanges(IReadOnlyList<MassRangeStruct> list1, IReadOnlyList<MassRangeStruct> list2)
	{
		int i = list1.Count - list2.Count;
		if (i != 0)
		{
			return i;
		}
		for (; i < list1.Count; i++)
		{
			int num = Utilities.CompareDoubles(list1[i].LowMass, list2[i].LowMass, 5E-07);
			if (num != 0)
			{
				return num;
			}
			num = Utilities.CompareDoubles(list1[i].HighMass, list2[i].HighMass, 5E-07);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	/// <summary>
	///     The method compares two lists of mass ranges.
	/// </summary>
	/// <param name="list1">
	///     The first list of mass ranges.
	/// </param>
	/// <param name="list2">
	///     The second list of mass ranges.
	/// </param>
	/// <param name="tolerance">Resolution (tolerance) for comparison</param>
	/// <returns>
	///     Return an <see cref="T:System.Int32" /> that has one of three values:
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <description>Meaning</description>
	///         </listheader>
	///         <term>Less than zero</term>
	///         <description>The current instance precedes the object specified by the CompareTo method in the sort order.</description>
	///         <term>Zero</term>
	///         <description>
	///             This current instance occurs in the same position in the sort order as the object specified by the
	///             CompareTo method.
	///         </description>
	///         <term>Greater than zero</term>
	///         <description>This current instance follows the object specified by the CompareTo method in the sort order.</description>
	///     </list>
	/// </returns>
	internal static int CompareMassRanges(IReadOnlyList<MassRangeStruct> list1, IReadOnlyList<MassRangeStruct> list2, double tolerance)
	{
		int num = list1.Count - list2.Count;
		if (num != 0)
		{
			return num;
		}
		for (int i = 0; i < list1.Count; i++)
		{
			MassRangeStruct massRangeStruct = list1[i];
			MassRangeStruct massRangeStruct2 = list2[i];
			num = Utilities.CompareDoubles(massRangeStruct.LowMass, massRangeStruct2.LowMass, tolerance);
			if (num != 0)
			{
				return num;
			}
			num = Utilities.CompareDoubles(massRangeStruct.HighMass, massRangeStruct2.HighMass, tolerance);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	/// <summary>
	///     The method compares two lists of reactions.
	/// </summary>
	/// <param name="reactions">
	///     The first list of reaction.
	/// </param>
	/// <param name="otherReactions">
	///     The other list of reactions.
	/// </param>
	/// <returns>
	///     Return an <see cref="T:System.Int32" /> that has one of three values:
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <description>Meaning</description>
	///         </listheader>
	///         <term>Less than zero</term>
	///         <description>The current instance precedes the object specified by the CompareTo method in the sort order.</description>
	///         <term>Zero</term>
	///         <description>
	///             This current instance occurs in the same position in the sort order as the object specified by the
	///             CompareTo method.
	///         </description>
	///         <term>Greater than zero</term>
	///         <description>This current instance follows the object specified by the CompareTo method in the sort order.</description>
	///     </list>
	/// </returns>
	private static int CompareReactions(IReadOnlyList<Reaction> reactions, IReadOnlyList<Reaction> otherReactions)
	{
		int count = reactions.Count;
		int i = count - otherReactions.Count;
		if (i != 0)
		{
			return i;
		}
		for (; i < count; i++)
		{
			int num = reactions[i].CompareTo(otherReactions[i]);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	/// <summary>
	///     formulate auto filter string.
	/// </summary>
	/// <param name="scanEvent">
	///     The scan event.
	/// </param>
	/// <param name="defaultMassPrecisionFormat">
	///     The default mass precision format.
	/// </param>
	/// <param name="massPrecision">
	///     The mass precision.
	/// </param>
	/// <param name="charsMax">
	///     The max number of chars
	/// </param>
	/// <param name="energyPrecision">
	///     The energy precision.
	/// </param>
	/// <param name="formatProvider">numeric formatting culture</param>
	/// <param name="listSeparator">list separator for localization</param>
	/// <returns>
	///     The filter as a string
	/// </returns>
	internal static string FormulateAutoFilterString(IRawFileReaderScanEvent scanEvent, string defaultMassPrecisionFormat, int massPrecision, int charsMax, int energyPrecision, IFormatProvider formatProvider, string listSeparator)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		string massPrecisionFmt = defaultMassPrecisionFormat;
		string energyPrecisionFmt = "f2";
		if (massPrecision >= 0)
		{
			massPrecisionFmt = stringBuilder2.AppendFormat("f{0}", massPrecision).ToString();
		}
		if (energyPrecision >= 0)
		{
			energyPrecisionFmt = $"f{energyPrecision}";
		}
		string massAnalyzerTypeFilterString = GetMassAnalyzerTypeFilterString(scanEvent.MassAnalyzerType);
		if (!string.IsNullOrEmpty(massAnalyzerTypeFilterString))
		{
			stringBuilder.Append(massAnalyzerTypeFilterString);
			stringBuilder.Append(' ');
		}
		FormatScanTypeIndex(stringBuilder, scanEvent.ScanTypeIndex);
		FormatPolarity(stringBuilder, scanEvent.Polarity);
		FormulateScanDataType(stringBuilder, scanEvent.ScanDataType);
		FormatIonizationMode(stringBuilder, scanEvent.IonizationMode);
		AppendString(stringBuilder, scanEvent.Corona, "corona");
		AppendString(stringBuilder, scanEvent.PhotoIonization, "pi");
		FormatSourceFragmentation(stringBuilder, scanEvent, energyPrecisionFmt, formatProvider);
		FormatCompensationVoltage(stringBuilder, scanEvent, energyPrecisionFmt, formatProvider);
		FormatDetector(stringBuilder, scanEvent, energyPrecisionFmt, formatProvider);
		AppendString(stringBuilder, scanEvent.TurboScan, "t");
		AppendString(stringBuilder, scanEvent.Enhanced, "E");
		AppendString(stringBuilder, scanEvent.ParamA, "a");
		AppendString(stringBuilder, scanEvent.ParamB, "b");
		AppendString(stringBuilder, scanEvent.ParamF, "f");
		AppendString(stringBuilder, scanEvent.SpsMultiNotch, "sps");
		AppendString(stringBuilder, scanEvent.ParamR, "r");
		AppendString(stringBuilder, scanEvent.ParamV, "v");
		FormatDependentDataFlag(stringBuilder, scanEvent.DependentDataFlag);
		AppendString(stringBuilder, scanEvent.Wideband, "w");
		if (scanEvent is IFilterExtensions filterExtensions)
		{
			FormatFlagsFromFilter(filterExtensions, stringBuilder);
		}
		else
		{
			FormatFlagsFromEvent(stringBuilder, scanEvent.LowerCaseFlags, scanEvent.UpperCaseFlags);
		}
		AppendString(stringBuilder, scanEvent.SupplementalActivation, "sa");
		AppendString(stringBuilder, scanEvent.MultiStateActivation, "msa");
		FormatAccurateMassType(stringBuilder, scanEvent.AccurateMassType);
		AppendString(stringBuilder, scanEvent.Ultra, "u");
		FormatScanType(stringBuilder, scanEvent.ScanType);
		FormatSectorScan(stringBuilder, scanEvent.SectorScan);
		AppendString(stringBuilder, scanEvent.Lock, "lock");
		AppendString(stringBuilder, scanEvent.Multiplex, "msx");
		FormatMsOrder(stringBuilder, scanEvent, massPrecisionFmt, energyPrecisionFmt, formatProvider);
		FormatMultiPhotonDissociationType(stringBuilder, scanEvent, energyPrecisionFmt, formatProvider);
		FormatElectronCaptureDissociationType(stringBuilder, scanEvent, energyPrecisionFmt, formatProvider);
		FormatFreeRegion(stringBuilder, scanEvent.FreeRegion);
		FormatMassRanges(stringBuilder, scanEvent, massPrecisionFmt, energyPrecisionFmt, formatProvider, listSeparator);
		if (charsMax >= 3)
		{
			_ = stringBuilder.Length;
		}
		int num = stringBuilder.Length;
		if (num > 0 && stringBuilder[num - 1] == ' ')
		{
			num--;
		}
		return stringBuilder.ToString(0, num);
		static void FormatFlagsFromEvent(StringBuilder result, LowerCaseFilterFlags allLower, UpperCaseFilterFlags allUpper)
		{
			if (allLower != 0)
			{
				if ((allLower & (LowerCaseFilterFlags.LowerE | LowerCaseFilterFlags.LowerG | LowerCaseFilterFlags.LowerH | LowerCaseFilterFlags.LowerI)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerE, "e");
					AddTriState(LowerCaseFilterFlags.LowerG, "g");
					AddTriState(LowerCaseFilterFlags.LowerH, "h");
					AddTriState(LowerCaseFilterFlags.LowerI, "i");
				}
				if ((allLower & (LowerCaseFilterFlags.LowerJ | LowerCaseFilterFlags.LowerK | LowerCaseFilterFlags.LowerL | LowerCaseFilterFlags.LowerM)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerJ, "j");
					AddTriState(LowerCaseFilterFlags.LowerK, "k");
					AddTriState(LowerCaseFilterFlags.LowerL, "l");
					AddTriState(LowerCaseFilterFlags.LowerM, "m");
				}
				if ((allLower & (LowerCaseFilterFlags.LowerQ | LowerCaseFilterFlags.LowerN | LowerCaseFilterFlags.LowerO)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerN, "n");
					AddTriState(LowerCaseFilterFlags.LowerO, "o");
					AddTriState(LowerCaseFilterFlags.LowerQ, "q");
				}
				if ((allLower & (LowerCaseFilterFlags.LowerS | LowerCaseFilterFlags.LowerX | LowerCaseFilterFlags.LowerY)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerS, "s");
					AddTriState(LowerCaseFilterFlags.LowerX, "x");
					AddTriState(LowerCaseFilterFlags.LowerY, "y");
				}
			}
			if (allUpper != 0)
			{
				if ((allUpper & (UpperCaseFilterFlags.UpperA | UpperCaseFilterFlags.UpperB | UpperCaseFilterFlags.UpperF)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperA, "A");
					AddTriStateUpper(UpperCaseFilterFlags.UpperB, "B");
					AddTriStateUpper(UpperCaseFilterFlags.UpperF, "F");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperG | UpperCaseFilterFlags.UpperH | UpperCaseFilterFlags.UpperI)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperG, "G");
					AddTriStateUpper(UpperCaseFilterFlags.UpperH, "H");
					AddTriStateUpper(UpperCaseFilterFlags.UpperI, "I");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperJ | UpperCaseFilterFlags.UpperK | UpperCaseFilterFlags.UpperL)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperJ, "J");
					AddTriStateUpper(UpperCaseFilterFlags.UpperK, "K");
					AddTriStateUpper(UpperCaseFilterFlags.UpperL, "L");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperM | UpperCaseFilterFlags.UpperN | UpperCaseFilterFlags.UpperO)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperM, "M");
					AddTriStateUpper(UpperCaseFilterFlags.UpperN, "N");
					AddTriStateUpper(UpperCaseFilterFlags.UpperO, "O");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperQ | UpperCaseFilterFlags.UpperR | UpperCaseFilterFlags.UpperS)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperQ, "Q");
					AddTriStateUpper(UpperCaseFilterFlags.UpperR, "R");
					AddTriStateUpper(UpperCaseFilterFlags.UpperS, "S");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperT | UpperCaseFilterFlags.UpperU | UpperCaseFilterFlags.UpperV)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperT, "T");
					AddTriStateUpper(UpperCaseFilterFlags.UpperU, "U");
					AddTriStateUpper(UpperCaseFilterFlags.UpperV, "V");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperW | UpperCaseFilterFlags.UpperX | UpperCaseFilterFlags.UpperY)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperW, "W");
					AddTriStateUpper(UpperCaseFilterFlags.UpperX, "X");
					AddTriStateUpper(UpperCaseFilterFlags.UpperY, "Y");
				}
			}
			void AddTriState(LowerCaseFilterFlags flag, string token)
			{
				AppendString(result, ((allLower & flag) == 0) ? TriState.Any : TriState.On, token);
			}
			void AddTriStateUpper(UpperCaseFilterFlags flag, string token)
			{
				AppendString(result, ((allUpper & flag) == 0) ? TriState.Any : TriState.On, token);
			}
		}
		static void FormatFlagsFromFilter(IFilterExtensions filterExtensions2, StringBuilder result)
		{
			LowerCaseFilterFlags allLower = filterExtensions2.LowerCaseApplied;
			if (allLower != 0)
			{
				if ((allLower & (LowerCaseFilterFlags.LowerE | LowerCaseFilterFlags.LowerG | LowerCaseFilterFlags.LowerH | LowerCaseFilterFlags.LowerI)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerE, "e");
					AddTriState(LowerCaseFilterFlags.LowerG, "g");
					AddTriState(LowerCaseFilterFlags.LowerH, "h");
					AddTriState(LowerCaseFilterFlags.LowerI, "i");
				}
				if ((allLower & (LowerCaseFilterFlags.LowerJ | LowerCaseFilterFlags.LowerK | LowerCaseFilterFlags.LowerL | LowerCaseFilterFlags.LowerM)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerJ, "j");
					AddTriState(LowerCaseFilterFlags.LowerK, "k");
					AddTriState(LowerCaseFilterFlags.LowerL, "l");
					AddTriState(LowerCaseFilterFlags.LowerM, "m");
				}
				if ((allLower & (LowerCaseFilterFlags.LowerQ | LowerCaseFilterFlags.LowerN | LowerCaseFilterFlags.LowerO)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerN, "n");
					AddTriState(LowerCaseFilterFlags.LowerO, "o");
					AddTriState(LowerCaseFilterFlags.LowerQ, "q");
				}
				if ((allLower & (LowerCaseFilterFlags.LowerS | LowerCaseFilterFlags.LowerX | LowerCaseFilterFlags.LowerY)) != 0)
				{
					AddTriState(LowerCaseFilterFlags.LowerS, "s");
					AddTriState(LowerCaseFilterFlags.LowerX, "x");
					AddTriState(LowerCaseFilterFlags.LowerY, "y");
				}
			}
			UpperCaseFilterFlags allUpper = filterExtensions2.UpperCaseApplied;
			if (allUpper != 0)
			{
				if ((allUpper & (UpperCaseFilterFlags.UpperA | UpperCaseFilterFlags.UpperB | UpperCaseFilterFlags.UpperF)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperA, "A");
					AddTriStateUpper(UpperCaseFilterFlags.UpperB, "B");
					AddTriStateUpper(UpperCaseFilterFlags.UpperF, "F");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperG | UpperCaseFilterFlags.UpperH | UpperCaseFilterFlags.UpperI)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperG, "G");
					AddTriStateUpper(UpperCaseFilterFlags.UpperH, "H");
					AddTriStateUpper(UpperCaseFilterFlags.UpperI, "I");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperJ | UpperCaseFilterFlags.UpperK | UpperCaseFilterFlags.UpperL)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperJ, "J");
					AddTriStateUpper(UpperCaseFilterFlags.UpperK, "K");
					AddTriStateUpper(UpperCaseFilterFlags.UpperL, "L");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperM | UpperCaseFilterFlags.UpperN | UpperCaseFilterFlags.UpperO)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperM, "M");
					AddTriStateUpper(UpperCaseFilterFlags.UpperN, "N");
					AddTriStateUpper(UpperCaseFilterFlags.UpperO, "O");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperQ | UpperCaseFilterFlags.UpperR | UpperCaseFilterFlags.UpperS)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperQ, "Q");
					AddTriStateUpper(UpperCaseFilterFlags.UpperR, "R");
					AddTriStateUpper(UpperCaseFilterFlags.UpperS, "S");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperT | UpperCaseFilterFlags.UpperU | UpperCaseFilterFlags.UpperV)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperT, "T");
					AddTriStateUpper(UpperCaseFilterFlags.UpperU, "U");
					AddTriStateUpper(UpperCaseFilterFlags.UpperV, "V");
				}
				if ((allUpper & (UpperCaseFilterFlags.UpperW | UpperCaseFilterFlags.UpperX | UpperCaseFilterFlags.UpperY)) != 0)
				{
					AddTriStateUpper(UpperCaseFilterFlags.UpperW, "W");
					AddTriStateUpper(UpperCaseFilterFlags.UpperX, "X");
					AddTriStateUpper(UpperCaseFilterFlags.UpperY, "Y");
				}
			}
			void AddTriState(LowerCaseFilterFlags flag, string token)
			{
				AppendString(result, ((allLower & flag) == 0) ? TriState.Any : (((filterExtensions2.LowerCaseFlags & flag) == 0) ? TriState.Off : TriState.On), token);
			}
			void AddTriStateUpper(UpperCaseFilterFlags flag, string token)
			{
				AppendString(result, ((allUpper & flag) == 0) ? TriState.Any : (((filterExtensions2.UpperCaseFlags & flag) == 0) ? TriState.Off : TriState.On), token);
			}
		}
	}

	/// <summary>
	///     copy arrays from this to "copy"
	/// </summary>
	/// <param name="copy">
	///     The copy.
	/// </param>
	internal void CopyArrays(ScanEvent copy)
	{
		if (MassCalibrators.Length == 0)
		{
			copy.MassCalibrators = Array.Empty<double>();
		}
		else
		{
			copy.MassCalibrators = (double[])MassCalibrators.Clone();
		}
		copy.MassRanges = MassRanges.Clone() as MassRangeStruct[];
		if (SourceFragmentationMassRanges.Length == 0)
		{
			copy.SourceFragmentationMassRanges = Array.Empty<MassRangeStruct>();
		}
		else
		{
			copy.SourceFragmentationMassRanges = DuplicateMassRangeList(SourceFragmentationMassRanges);
		}
		int num = ListSafeCount(Reactions);
		if (num <= 0)
		{
			copy.Reactions = Array.Empty<Reaction>();
		}
		else
		{
			copy.Reactions = new Reaction[num];
			for (int i = 0; i < num; i++)
			{
				copy.Reactions[i] = Reactions[i].DeepClone();
			}
		}
		copy.SourceFragmentations = SourceFragmentations.Clone() as double[];
	}

	/// <summary>
	///     The method appends the specified string to the result.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="parameter">
	///     The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Constants.ScanFilterEnums.OnOffTypes" /> parameter.
	/// </param>
	/// <param name="append">
	///     The string to append.
	/// </param>
	private static void AppendString(StringBuilder result, ScanFilterEnums.OnOffTypes parameter, string append)
	{
		switch (parameter)
		{
		case ScanFilterEnums.OnOffTypes.On:
			result.AppendFormat("{0} ", append);
			break;
		case ScanFilterEnums.OnOffTypes.Off:
			result.AppendFormat("!{0} ", append);
			break;
		case ScanFilterEnums.OnOffTypes.Any:
			break;
		}
	}

	/// <summary>
	///     The method appends the specified string to the result.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="parameter">
	///     The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Constants.ScanFilterEnums.OffOnTypes" /> parameter.
	/// </param>
	/// <param name="append">
	///     The string to append.
	/// </param>
	private static void AppendString(StringBuilder result, ScanFilterEnums.OffOnTypes parameter, string append)
	{
		switch (parameter)
		{
		case ScanFilterEnums.OffOnTypes.On:
			result.AppendFormat("{0} ", append);
			break;
		case ScanFilterEnums.OffOnTypes.Off:
			result.AppendFormat("!{0} ", append);
			break;
		case ScanFilterEnums.OffOnTypes.Any:
			break;
		}
	}

	/// <summary>
	///     The method appends the specified string to the result.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="parameter">
	///     The value of the parameter, such as "X is off" to show "!x"
	/// </param>
	/// <param name="append">
	///     The string to append. (such as "x' if the parameter if for the token 'x'
	/// </param>
	private static void AppendString(StringBuilder result, TriState parameter, string append)
	{
		switch (parameter)
		{
		case TriState.On:
			result.AppendFormat("{0} ", append);
			break;
		case TriState.Off:
			result.AppendFormat("!{0} ", append);
			break;
		case TriState.Any:
			break;
		}
	}

	/// <summary>
	///     duplicate a list of mass range.
	/// </summary>
	/// <param name="from">
	///     The data to copy.
	/// </param>
	/// <returns>
	///     A new list which is a copy of the old list.
	///     If the old list is null: a new empty list.
	/// </returns>
	private static MassRangeStruct[] DuplicateMassRangeList(MassRangeStruct[] from)
	{
		if (from == null || from.Length == 0)
		{
			return Array.Empty<MassRangeStruct>();
		}
		MassRangeStruct[] array = new MassRangeStruct[from.Length];
		for (int i = 0; i < from.Length; i++)
		{
			array[i] = from[i];
		}
		return array;
	}

	/// <summary>
	///     The format accurate mass type.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="accMassType">Type to format</param>
	private static void FormatAccurateMassType(StringBuilder result, ScanFilterEnums.AccurateMassTypes accMassType)
	{
		switch (accMassType)
		{
		case ScanFilterEnums.AccurateMassTypes.Off:
			result.Append("!AM ");
			break;
		case ScanFilterEnums.AccurateMassTypes.On:
			result.Append("AM ");
			break;
		case ScanFilterEnums.AccurateMassTypes.Internal:
			result.Append("AMI ");
			break;
		case ScanFilterEnums.AccurateMassTypes.External:
			result.Append("AME ");
			break;
		case ScanFilterEnums.AccurateMassTypes.AcceptAnyAccurateMass:
			break;
		}
	}

	/// <summary>
	///     The format compensation voltage.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanEvent">The scan event to format</param>
	/// <param name="energyPrecisionFmt">Precision for energy values</param>
	/// <param name="formatProvider">format for localization</param>
	private static void FormatCompensationVoltage(StringBuilder result, IRawFileReaderScanEvent scanEvent, string energyPrecisionFmt, IFormatProvider formatProvider)
	{
		int fragmentationsOffset = 0;
		if (scanEvent.SourceFragmentation == ScanFilterEnums.OnOffTypes.On)
		{
			switch (scanEvent.SourceFragmentationType)
			{
			case ScanFilterEnums.VoltageTypes.SingleValue:
				fragmentationsOffset = 1;
				break;
			case ScanFilterEnums.VoltageTypes.Ramp:
				fragmentationsOffset = 2;
				break;
			}
		}
		FormatVoltageFeature(result, energyPrecisionFmt, scanEvent.SourceFragmentations, fragmentationsOffset, scanEvent.CompensationVoltage, scanEvent.CompensationVoltageType, "cv", formatProvider);
	}

	/// <summary>
	///     The method formats the dependent data flag.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="dependent">The value to format</param>
	private static void FormatDependentDataFlag(StringBuilder result, ScanFilterEnums.IsDependent dependent)
	{
		switch (dependent)
		{
		case ScanFilterEnums.IsDependent.Yes:
			result.Append("d ");
			break;
		case ScanFilterEnums.IsDependent.No:
			result.Append("!d ");
			break;
		case ScanFilterEnums.IsDependent.Any:
			break;
		}
	}

	/// <summary>
	///     The method formats the detector string.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanEvent">Scan event to format</param>
	/// <param name="energyPrecisionFmt">Precision for energy values</param>
	/// <param name="formatProvider">format for localization</param>
	/// Indicates that the string is for an Auto Filter.
	private static void FormatDetector(StringBuilder result, IRawFileReaderScanEvent scanEvent, string energyPrecisionFmt, IFormatProvider formatProvider)
	{
		switch (scanEvent.Detector)
		{
		case ScanFilterEnums.DetectorType.IsValid:
			result.AppendFormat("det={0} ", scanEvent.DetectorValue.ToString(energyPrecisionFmt, formatProvider));
			break;
		case ScanFilterEnums.DetectorType.IsInValid:
			result.Append("!det ");
			break;
		case ScanFilterEnums.DetectorType.Any:
			break;
		}
	}

	/// <summary>
	///     The method format electron capture dissociation type.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanEvent">event to format</param>
	/// <param name="energyPrecisionFmt">precision for energy values</param>
	/// <param name="formatProvider">format for localization</param>
	private static void FormatElectronCaptureDissociationType(StringBuilder result, IRawFileReaderScanEvent scanEvent, string energyPrecisionFmt, IFormatProvider formatProvider)
	{
		switch (scanEvent.ElectronCaptureDissociationType)
		{
		case ScanFilterEnums.OnAnyOffTypes.On:
			result.AppendFormat("ecd@{0} ", scanEvent.ElectronCaptureDissociation.ToString(energyPrecisionFmt, formatProvider));
			break;
		case ScanFilterEnums.OnAnyOffTypes.Off:
			result.Append("!ecd ");
			break;
		}
	}

	/// <summary>
	///     The method formats the free region.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="freeRegion">Free region code</param>
	private static void FormatFreeRegion(StringBuilder result, ScanFilterEnums.FreeRegions freeRegion)
	{
		switch (freeRegion)
		{
		case ScanFilterEnums.FreeRegions.FreeRegion1:
			result.Append("ffr1 ");
			break;
		case ScanFilterEnums.FreeRegions.FreeRegion2:
			result.Append("ffr2 ");
			break;
		case ScanFilterEnums.FreeRegions.AnyFreeRegion:
			break;
		}
	}

	/// <summary>
	///     The method formats the ionization mode.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="ionizationMode">The ionization mode</param>
	private static void FormatIonizationMode(StringBuilder result, ScanFilterEnums.IonizationModes ionizationMode)
	{
		if (ionizationMode < ScanFilterEnums.IonizationModes.IonModeBeyondKnown)
		{
			string value = FilterStringTokens.IonizationModeTokenNames[(int)ionizationMode];
			if (!string.IsNullOrEmpty(value))
			{
				result.Append(value);
				result.Append(' ');
			}
		}
	}

	/// <summary>
	///     The method formats the mass ranges.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanEvent">The event to format</param>
	/// <param name="massPrecisionFmt">Precision for precursor mass</param>
	/// <param name="energyPrecisionFmt">Precision for energy</param>
	/// <param name="formatProvider">culture specific number formatting</param>
	/// <param name="listSeparator">list separator for localization</param>
	private static void FormatMassRanges(StringBuilder result, IRawFileReaderScanEvent scanEvent, string massPrecisionFmt, string energyPrecisionFmt, IFormatProvider formatProvider, string listSeparator)
	{
		string text = ", ";
		if (!string.IsNullOrEmpty(listSeparator))
		{
			text = listSeparator;
			if (text[text.Length - 1] != ' ')
			{
				text += " ";
			}
		}
		int num = scanEvent.MassRanges.Length;
		if (num <= 0)
		{
			return;
		}
		result.Append("[");
		bool isDissociation = (scanEvent.SourceFragmentation == ScanFilterEnums.OnOffTypes.On && scanEvent.SourceFragmentationType == ScanFilterEnums.VoltageTypes.SIM) || (scanEvent.CompensationVoltage == ScanFilterEnums.OnOffTypes.On && scanEvent.CompensationVoltageType == ScanFilterEnums.VoltageTypes.SIM);
		for (int i = 0; i < num; i++)
		{
			MassRangeStruct range = scanEvent.MassRanges[i];
			result.Append(GetMassRangeString(i, range, isDissociation, scanEvent.SourceFragmentations, massPrecisionFmt, energyPrecisionFmt, formatProvider));
			if (i < num - 1)
			{
				result.Append(text);
			}
		}
		result.Append("]");
	}

	/// <summary>
	///     The method formats the MS order.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanEvent">The event to format</param>
	/// <param name="massPrecisionFmt">Mass precision to use</param>
	/// <param name="energyPrecisionFmt">Precision for activation energy</param>
	/// <param name="formatProvider">format for localization</param>
	private static void FormatMsOrder(StringBuilder result, IRawFileReaderScanEvent scanEvent, string massPrecisionFmt, string energyPrecisionFmt, IFormatProvider formatProvider)
	{
		if (scanEvent.ScanType == ScanFilterEnums.ScanTypes.Q1MS || scanEvent.ScanType == ScanFilterEnums.ScanTypes.Q3MS)
		{
			return;
		}
		int msOrder = (int)scanEvent.MsOrder;
		if (msOrder <= 1)
		{
			switch (scanEvent.MsOrder)
			{
			default:
				return;
			case ScanFilterEnums.MSOrderTypes.MS:
				result.Append("ms ");
				return;
			case ScanFilterEnums.MSOrderTypes.ParentScan:
				result.Append("pr ");
				break;
			case ScanFilterEnums.MSOrderTypes.NeutralLoss:
				result.Append("cnl ");
				break;
			case ScanFilterEnums.MSOrderTypes.NeutralGain:
				result.Append("cng ");
				break;
			case ScanFilterEnums.MSOrderTypes.AcceptAnyMSorder:
				return;
			}
		}
		else
		{
			result.AppendFormat("ms{0} ", msOrder);
		}
		StringBuilder stringBuilder = new StringBuilder(50);
		Reaction[] reactions = scanEvent.Reactions;
		foreach (Reaction reaction in reactions)
		{
			FormulateAutoFilterReaction(stringBuilder, reaction, massPrecisionFmt, energyPrecisionFmt, formatProvider);
		}
		if (stringBuilder.Length > 0)
		{
			result.Append(stringBuilder);
			if (stringBuilder[stringBuilder.Length - 1] != ' ')
			{
				result.Append(' ');
			}
		}
	}

	/// <summary>
	///     The method formats the multi photon dissociation type.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanEvent">Event to format</param>
	/// <param name="energyPrecisionFmt">Precision for energy</param>
	/// <param name="formatProvider">Format for localization</param>
	private static void FormatMultiPhotonDissociationType(StringBuilder result, IRawFileReaderScanEvent scanEvent, string energyPrecisionFmt, IFormatProvider formatProvider)
	{
		switch (scanEvent.MultiPhotonDissociationType)
		{
		case ScanFilterEnums.OnAnyOffTypes.On:
			result.AppendFormat("mpd@{0} ", scanEvent.MultiPhotonDissociation.ToString(energyPrecisionFmt, formatProvider));
			break;
		case ScanFilterEnums.OnAnyOffTypes.Off:
			result.Append("!mpd ");
			break;
		}
	}

	/// <summary>
	///     The method formats the polarity.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="polarity">Polarity to format</param>
	private static void FormatPolarity(StringBuilder result, ScanFilterEnums.PolarityTypes polarity)
	{
		switch (polarity)
		{
		case ScanFilterEnums.PolarityTypes.Negative:
			result.Append("- ");
			break;
		case ScanFilterEnums.PolarityTypes.Positive:
			result.Append("+ ");
			break;
		case ScanFilterEnums.PolarityTypes.Any:
			break;
		}
	}

	/// <summary>
	///     The method formats the scan type.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanType">Type to format</param>
	private static void FormatScanType(StringBuilder result, ScanFilterEnums.ScanTypes scanType)
	{
		switch (scanType)
		{
		case ScanFilterEnums.ScanTypes.Full:
			result.Append("Full ");
			break;
		case ScanFilterEnums.ScanTypes.Zoom:
			result.Append("Z ");
			break;
		case ScanFilterEnums.ScanTypes.SIM:
			result.Append("SIM ");
			break;
		case ScanFilterEnums.ScanTypes.SRM:
			result.Append("SRM ");
			break;
		case ScanFilterEnums.ScanTypes.CRM:
			result.Append("CRM ");
			break;
		case ScanFilterEnums.ScanTypes.Q1MS:
			result.Append("Q1MS ");
			break;
		case ScanFilterEnums.ScanTypes.Q3MS:
			result.Append("Q3MS ");
			break;
		case ScanFilterEnums.ScanTypes.Any:
			break;
		}
	}

	/// <summary>
	///     The method formats the scan type index.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanTypeIndex">Type to format</param>
	private static void FormatScanTypeIndex(StringBuilder result, int scanTypeIndex)
	{
		if (scanTypeIndex != -1)
		{
			result.AppendFormat("{{{0},{1}}} ", scanTypeIndex >> 16, scanTypeIndex & 0xFFFF);
		}
	}

	/// <summary>
	///     The method formats the sector scan.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="sectorScan">Value to format</param>
	private static void FormatSectorScan(StringBuilder result, ScanFilterEnums.SectorScans sectorScan)
	{
		switch (sectorScan)
		{
		case ScanFilterEnums.SectorScans.BSCAN:
			result.Append("BSCAN ");
			break;
		case ScanFilterEnums.SectorScans.ESCAN:
			result.Append("ESCAN ");
			break;
		case ScanFilterEnums.SectorScans.Any:
			break;
		}
	}

	/// <summary>
	///     The method formats the source fragmentation.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanEvent">Event to format</param>
	/// <param name="energyPrecisionFmt">Format for energy values</param>
	/// <param name="format">format for localization</param>
	private static void FormatSourceFragmentation(StringBuilder result, IRawFileReaderScanEvent scanEvent, string energyPrecisionFmt, IFormatProvider format)
	{
		FormatVoltageFeature(result, energyPrecisionFmt, scanEvent.SourceFragmentations, 0, scanEvent.SourceFragmentation, scanEvent.SourceFragmentationType, "sid", format);
	}

	/// <summary>
	///     format a voltage feature.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="energyPrecisionFmt">
	///     The energy precision format.
	/// </param>
	/// <param name="srcFrags">
	///     The source fragmentations.
	/// </param>
	/// <param name="fragmentationsOffset">offet from start of fragmentations array</param>
	/// <param name="voltEnable">
	///     The volt enable.
	/// </param>
	/// <param name="voltType">
	///     The volt type.
	/// </param>
	/// <param name="code">
	///     The code.
	/// </param>
	/// <param name="formatProvider">format for localization</param>
	private static void FormatVoltageFeature(StringBuilder result, string energyPrecisionFmt, IList<double> srcFrags, int fragmentationsOffset, ScanFilterEnums.OnOffTypes voltEnable, ScanFilterEnums.VoltageTypes voltType, string code, IFormatProvider formatProvider)
	{
		switch (voltEnable)
		{
		case ScanFilterEnums.OnOffTypes.On:
			result.Append(code);
			switch (voltType)
			{
			case ScanFilterEnums.VoltageTypes.SingleValue:
			case ScanFilterEnums.VoltageTypes.Ramp:
				result.Append('=');
				if (fragmentationsOffset >= srcFrags.Count)
				{
					fragmentationsOffset = srcFrags.Count - 1;
				}
				result.Append(srcFrags[fragmentationsOffset].ToString(energyPrecisionFmt, formatProvider));
				if (voltType == ScanFilterEnums.VoltageTypes.Ramp)
				{
					result.AppendFormat("-{0}", srcFrags[fragmentationsOffset + 1].ToString(energyPrecisionFmt, formatProvider));
				}
				break;
			}
			result.Append(' ');
			break;
		case ScanFilterEnums.OnOffTypes.Off:
			result.Append('!');
			result.Append(code);
			result.Append(' ');
			break;
		case ScanFilterEnums.OnOffTypes.Any:
			break;
		}
	}

	/// <summary>
	///     The method formulates auto filter reaction.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="reaction">
	///     The reaction.
	/// </param>
	/// <param name="massPrecisionFmt">
	///     Format for precursor mass
	/// </param>
	/// <param name="energyPrecisionFmt">
	///     Format for collision energy
	/// </param>
	/// <param name="formatProvider">format for localization</param>
	private static void FormulateAutoFilterReaction(StringBuilder result, Reaction reaction, string massPrecisionFmt, string energyPrecisionFmt, IFormatProvider formatProvider)
	{
		bool flag = result.Length > 0;
		if (!reaction.UseNamedActivation)
		{
			if (flag)
			{
				result.Append(' ');
			}
			result.AppendFormat("{0}@cid", reaction.PrecursorMass.ToString(massPrecisionFmt, formatProvider));
			if (reaction.IsPrecursorEnergiesValid)
			{
				result.Append(reaction.CollisionEnergy.ToString(energyPrecisionFmt, formatProvider));
			}
		}
		else if (reaction.ActivationType == ActivationType.Any)
		{
			if (flag)
			{
				result.Append(' ');
			}
			result.AppendFormat("{0}", reaction.PrecursorMass.ToString(massPrecisionFmt, formatProvider));
		}
		else if (reaction.MultipleActivation)
		{
			result.Append('@');
			result.Append(ActivationTypes[(int)reaction.ActivationType]);
			if (reaction.IsPrecursorEnergiesValid)
			{
				result.Append(reaction.CollisionEnergy.ToString(energyPrecisionFmt, formatProvider));
			}
		}
		else
		{
			if (flag)
			{
				result.Append(' ');
			}
			result.AppendFormat("{0}@{1}", reaction.PrecursorMass.ToString(massPrecisionFmt, formatProvider), ActivationTypes[(int)reaction.ActivationType]);
			if (reaction.IsPrecursorEnergiesValid)
			{
				result.Append(reaction.CollisionEnergy.ToString(energyPrecisionFmt, formatProvider));
			}
		}
	}

	/// <summary>
	///     The method formats the scan data type.
	/// </summary>
	/// <param name="result">
	///     The result.
	/// </param>
	/// <param name="scanDataType">Value to format</param>
	private static void FormulateScanDataType(StringBuilder result, ScanFilterEnums.ScanDataTypes scanDataType)
	{
		switch (scanDataType)
		{
		case ScanFilterEnums.ScanDataTypes.Centroid:
			result.Append("c ");
			break;
		case ScanFilterEnums.ScanDataTypes.Profile:
			result.Append("p ");
			break;
		}
	}

	/// <summary>
	///     Gets the mass analyzer type filter string.
	/// </summary>
	/// <param name="massAnalyzerTypes">The mass analyzer types.</param>
	/// <returns>the mass analyzer type filter string</returns>
	private static string GetMassAnalyzerTypeFilterString(ScanFilterEnums.MassAnalyzerTypes massAnalyzerTypes)
	{
		if ((uint)massAnalyzerTypes > 5u && massAnalyzerTypes != ScanFilterEnums.MassAnalyzerTypes.ASTMS)
		{
			return string.Empty;
		}
		return massAnalyzerTypes.ToString();
	}

	/// <summary>
	///     Convert the mass range to string.
	/// </summary>
	/// <param name="i">
	///     The index into the table of ranges.
	/// </param>
	/// <param name="range">
	///     The range.
	/// </param>
	/// <param name="isDissociation">
	///     The is dissociation.
	/// </param>
	/// <param name="srcFragValues">
	///     The source fragmentation values.
	/// </param>
	/// <param name="massPrecisionFmt">
	///     The mass precision format.
	/// </param>
	/// <param name="energyPrecisionFmt">
	///     The energy precision format.
	/// </param>
	/// <param name="formatProvider">numeric format localization</param>
	/// <returns>
	///     The <see cref="T:System.String" />.
	/// </returns>
	private static string GetMassRangeString(int i, MassRangeStruct range, bool isDissociation, IList<double> srcFragValues, string massPrecisionFmt, string energyPrecisionFmt, IFormatProvider formatProvider)
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		stringBuilder.Append(range.LowMass.ToString(massPrecisionFmt, formatProvider));
		if (Math.Abs(range.LowMass - range.HighMass) >= double.Epsilon)
		{
			stringBuilder.Append('-');
			stringBuilder.Append(range.HighMass.ToString(massPrecisionFmt, formatProvider));
		}
		if (isDissociation && i < srcFragValues.Count)
		{
			stringBuilder.Append('@');
			stringBuilder.Append(srcFragValues[i].ToString(energyPrecisionFmt, formatProvider));
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	///     Safely Count items in a list. Count is zero for a null list.
	/// </summary>
	/// <param name="list">List to count</param>
	/// <returns>Count of items in the list</returns>
	private static int ListSafeCount(IList list)
	{
		return list?.Count ?? 0;
	}

	/// <summary>
	///     This method fixes up defaults.
	/// </summary>
	/// <param name="fileVersion">
	///     The file version.
	/// </param>
	private void FixUpDefaults(int fileVersion)
	{
		if (fileVersion < 65)
		{
			if (fileVersion < 63)
			{
				if (fileVersion < 62)
				{
					if (fileVersion < 54)
					{
						if (fileVersion < 51)
						{
							if (fileVersion < 48)
							{
								if (fileVersion < 31)
								{
									_scanEventInfo.ScanTypeIndex = -1;
									_scanEventInfo.SourceFragmentationType = 1;
								}
								_scanEventInfo.Wideband = 2;
							}
							_scanEventInfo.AccurateMassType = ScanFilterEnums.ScanEventAccurateMassTypes.Off;
						}
						_scanEventInfo.MassAnalyzerType = 6;
						_scanEventInfo.SectorScan = 2;
						_scanEventInfo.Lock = 2;
						_scanEventInfo.FreeRegion = 2;
						_scanEventInfo.Ultra = 2;
						_scanEventInfo.Enhanced = 2;
						_scanEventInfo.MultiPhotonDissociationType = 1;
						_scanEventInfo.ElectronCaptureDissociationType = 1;
						_scanEventInfo.PhotoIonization = 2;
					}
					_scanEventInfo.PulsedQDissociationType = 2;
					_scanEventInfo.ElectronTransferDissociationType = 2;
					_scanEventInfo.HigherEnergyCIDType = 2;
				}
				_scanEventInfo.SupplementalActivation = 2;
				_scanEventInfo.MultiStateActivation = 2;
				_scanEventInfo.CompensationVoltage = 2;
				_scanEventInfo.CompensationVoltageType = 4;
			}
			_scanEventInfo.Multiplex = 2;
			_scanEventInfo.ParamA = 2;
			_scanEventInfo.ParamB = 2;
			_scanEventInfo.ParamF = 2;
			_scanEventInfo.SpsMultiNotch = 2;
			_scanEventInfo.ParamR = 2;
			_scanEventInfo.ParamV = 2;
		}
		AccurateMassType = ScanFilterEnums.FromScanEventAccurateMass(_scanEventInfo.AccurateMassType);
	}

	/// <summary>
	///     The method reads the reactions array.
	/// </summary>
	/// <param name="viewer">
	///     The viewer.
	/// </param>
	/// <param name="dataOffset">
	///     Offset into memory map
	/// </param>
	/// <param name="fileRevision">
	///     The file version.
	/// </param>
	/// <returns>
	///     The number of bytes read.
	/// </returns>
	private long ReadReactionsArray(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		int num = viewer.ReadIntExt(ref startPos);
		if (num <= 0)
		{
			return startPos - dataOffset;
		}
		Reactions = new Reaction[num];
		for (int i = 0; i < num; i++)
		{
			Reaction reaction = viewer.LoadRawFileObjectExt(() => new Reaction(), fileRevision, ref startPos);
			Reactions[i] = reaction;
		}
		return startPos - dataOffset;
	}

	/// <summary>
	///     The method reads the structure.
	/// </summary>
	/// <param name="viewer">
	///     The viewer.
	/// </param>
	/// <param name="dataOffset">
	///     Offset into memory map
	/// </param>
	/// <param name="fileVersion">
	///     The file version.
	/// </param>
	/// <returns>
	///     The number of bytes read.
	/// </returns>
	private long ReadStructure(IMemoryReader viewer, long dataOffset, int fileVersion)
	{
		long startPos = dataOffset;
		if (fileVersion >= 65)
		{
			_scanEventInfo = viewer.ReadSimpleStructure<ScanEventInfoStruct>(startPos);
			startPos += ScanEventInfoStructSize;
			_ = _scanEventInfo.IsValid;
			return startPos - dataOffset;
		}
		if (fileVersion >= 63)
		{
			_scanEventInfo = viewer.ReadPreviousRevisionAndConvertExt<ScanEventInfoStruct, ScanEventInfoStruct63>(ref startPos);
		}
		else if (fileVersion >= 62)
		{
			_scanEventInfo = viewer.ReadPreviousRevisionAndConvertExt<ScanEventInfoStruct, ScanEventInfoStruct62>(ref startPos);
		}
		else if (fileVersion >= 54)
		{
			_scanEventInfo = viewer.ReadPreviousRevisionAndConvertExt<ScanEventInfoStruct, ScanEventInfoStruct54>(ref startPos);
		}
		else if (fileVersion >= 51)
		{
			_scanEventInfo = viewer.ReadPreviousRevisionAndConvertExt<ScanEventInfoStruct, ScanEventInfoStruct51>(ref startPos);
		}
		else if (fileVersion >= 48)
		{
			_scanEventInfo = viewer.ReadPreviousRevisionAndConvertExt<ScanEventInfoStruct, ScanEventInfoStruct50>(ref startPos);
		}
		else if (fileVersion >= 31)
		{
			_scanEventInfo = viewer.ReadPreviousRevisionAndConvertExt<ScanEventInfoStruct, ScanEventInfoStruct3>(ref startPos);
		}
		else if (fileVersion < 30)
		{
			int count = Marshal.SizeOf(typeof(ScanEventInfoStruct2));
			byte[] array = viewer.ReadBytesExt(ref startPos, count);
			_scanEventInfo.IsValid = (byte)(array[0] & 1);
			_scanEventInfo.IsCustom = (byte)(array[0] & 2);
			_scanEventInfo.Corona = 2;
			_scanEventInfo.Detector = 1;
			_scanEventInfo.Polarity = array[1];
			_scanEventInfo.ScanDataType = array[2];
			_scanEventInfo.MSOrder = (sbyte)array[3];
			_scanEventInfo.ScanType = array[4];
			_scanEventInfo.SourceFragmentation = array[5];
			_scanEventInfo.TurboScan = array[6];
			_scanEventInfo.DependentData = array[7];
			_scanEventInfo.IonizationMode = array[8];
			_scanEventInfo.DetectorValue = BitConverter.ToDouble(array, 16);
		}
		else
		{
			_scanEventInfo = viewer.ReadPreviousRevisionAndConvertExt<ScanEventInfoStruct, ScanEventInfoStruct2>(ref startPos);
		}
		return startPos - dataOffset;
	}

	public TriState GetLowerCaseFlag(LowerCaseFilterFlags flag)
	{
		if (((uint)_scanEventInfo.LowerFlags & (uint)flag) == 0)
		{
			return TriState.Any;
		}
		return TriState.On;
	}

	public TriState GetUpperCaseFlag(UpperCaseFilterFlags flag)
	{
		if ((_scanEventInfo.UpperFlags & flag) == 0)
		{
			return TriState.Any;
		}
		return TriState.On;
	}
}
