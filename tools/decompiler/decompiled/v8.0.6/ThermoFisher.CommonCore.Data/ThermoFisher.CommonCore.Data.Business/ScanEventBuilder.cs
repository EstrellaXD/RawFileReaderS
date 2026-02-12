using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The scan event builder.
/// This class permits a scan event to be built, by adding
/// MS/MS reactions, mass ranges etc.
/// It can be used as a helper to log MS data into a raw file.
/// This builder does not support parsing from a filter string,
/// or conversion to filter string format.
/// The "ToString" method provides basic .Net object formatting only.
/// The builder may be constructed as default or from IScanEvent or IScanFilter.
/// </summary>
public class ScanEventBuilder : IScanEvent, IScanEventBase, IScanEventExtended
{
	private readonly List<double> _massCalibrators = new List<double>();

	private readonly List<IRangeAccess> _massRanges = new List<IRangeAccess>();

	private readonly List<double> _sourceFragmentationValues = new List<double>();

	private readonly List<double> _compensationVoltageValues = new List<double>();

	private readonly List<Range> _sourceFragmentationRanges = new List<Range>();

	private readonly List<MsStage> _stages = new List<MsStage>();

	private LowerCaseFilterFlags LowerCaseFlagsApplied { get; set; }

	private UpperCaseFilterFlags UpperCaseFlagsApplied { get; set; }

	/// <inheritdoc />
	public LowerCaseFilterFlags LowerCaseFlags { get; set; }

	/// <inheritdoc />
	public UpperCaseFilterFlags UpperCaseFlags { get; set; }

	/// <summary>
	/// Gets or sets the accurate mass setting.
	/// </summary>
	public EventAccurateMass AccurateMass { get; set; }

	/// <summary>
	/// Gets or sets Compensation Voltage Option setting.
	/// Composition voltage is exclusive with source fragmentation,
	/// so only one may be active.
	/// </summary>
	public TriState CompensationVoltage { get; set; }

	/// <summary>
	/// Gets or sets Compensation Voltage type setting.
	/// Composition voltage is exclusive with source fragmentation,
	/// so only one may be active.
	/// When set to SingleValue a value must be added using AddSourceFragmentationInfo.
	/// When set to Ramp two values must be added using AddSourceFragmentationInfo.
	/// When set to SIM, there must be one value added per (scanned) mass range.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.CompensationVoltageType" /> for possible values</value>
	public CompensationVoltageType CompensationVoltType { get; set; }

	/// <summary>
	/// Gets or sets the corona scan setting.
	/// </summary>
	public TriState Corona { get; set; }

	/// <summary>
	/// Gets or sets the dependent scan setting.
	/// A scan is "dependent" if the scanning method is based
	/// on analysis of data from a previous scan.
	/// </summary>
	public TriState Dependent { get; set; }

	/// <summary>
	/// Gets or sets the detector scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.DetectorType" /> for possible values</value>
	public DetectorType Detector { get; set; }

	/// <summary>
	/// Gets or sets the detector value.
	/// </summary>
	/// <value>Floating point detector value</value>
	public double DetectorValue { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation setting.
	/// </summary>
	public TriState ElectronCaptureDissociation { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation value.
	/// </summary>
	/// <value>Floating point electron capture dissociation value</value>
	public double ElectronCaptureDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation setting.
	/// </summary>
	public TriState ElectronTransferDissociation { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation value.
	/// </summary>
	/// <value>Floating point electron transfer dissociation value</value>
	public double ElectronTransferDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the enhanced scan setting.
	/// </summary>
	public TriState Enhanced { get; set; }

	/// <summary>
	/// Gets or sets the field free region setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.FieldFreeRegionType" /> for possible values</value>
	public FieldFreeRegionType FieldFreeRegion { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID setting.
	/// </summary>
	public TriState HigherEnergyCiD { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID value.
	/// </summary>
	/// <value>Floating point higher energy CID value</value>
	public double HigherEnergyCiDValue { get; set; }

	/// <summary>
	/// Gets or sets the ionization mode scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.IonizationModeType" /> for possible values</value>
	public IonizationModeType IonizationMode { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is a custom event.
	/// A custom event implies that any scan derived from this event could be different.
	/// The scan type must be inspected to determine the scanning mode, and not the event.
	/// </summary>
	public bool IsCustom { get; set; }

	/// <summary>
	/// Gets a value indicating whether this event is valid.
	/// </summary>
	public bool IsValid
	{
		get
		{
			if (CompensationVoltage == TriState.On && _compensationVoltageValues.Count != ExpectedCv())
			{
				return false;
			}
			if (SourceFragmentation == TriState.On && _sourceFragmentationValues.Count != ExpectedSid())
			{
				return false;
			}
			if (Multiplex != TriState.On)
			{
				int mSOrder = (int)MSOrder;
				if (mSOrder > 1)
				{
					if (_stages.Count != mSOrder - 1)
					{
						return false;
					}
				}
				else if (mSOrder < 0 && _stages.Count != 1)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>
	/// Gets or sets the lock scan setting.
	/// </summary>
	public TriState Lock { get; set; }

	/// <summary>
	/// Gets or sets the mass analyzer scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MassAnalyzerType" /> for possible values</value>
	public MassAnalyzerType MassAnalyzer { get; set; }

	/// <summary>
	/// Gets the mass calibrator count.
	/// </summary>
	public int MassCalibratorCount => _massCalibrators.Count;

	/// <summary>
	/// Gets the number of (precursor) masses
	/// </summary>
	/// <value>The size of mass array</value>
	public int MassCount
	{
		get
		{
			int num = 0;
			foreach (MsStage stage in _stages)
			{
				num += stage.Reactions.Count;
			}
			return num;
		}
	}

	/// <summary>
	/// Gets the number of mass ranges for final scan
	/// </summary>
	/// <value>The size of mass range array</value>
	public int MassRangeCount => _massRanges.Count;

	/// <summary>
	/// Gets or sets the scan MS/MS power setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.MSOrderType" /> for possible values</value>
	public MSOrderType MSOrder { get; set; }

	/// <summary>
	/// Gets or sets the Multi notch (Synchronous Precursor Selection) type
	/// </summary>
	public TriState MultiNotch { get; set; }

	/// <summary>
	/// Gets or sets the multi-photon dissociation setting.
	/// </summary>
	public TriState MultiplePhotonDissociation { get; set; }

	/// <summary>
	/// Gets or sets the multi-photon dissociation value.
	/// </summary>
	/// <value>Floating point multi-photon dissociation value</value>
	public double MultiplePhotonDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the Multiplex type
	/// </summary>
	public TriState Multiplex { get; set; }

	/// <summary>
	/// Gets or sets MultiStateActivation type setting.
	/// </summary>
	public TriState MultiStateActivation { get; set; }

	/// <summary>
	/// Gets or sets the event Name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the parameter a.
	/// </summary>
	public TriState ParamA { get; set; }

	/// <summary>
	/// Gets or sets the parameter b.
	/// </summary>
	public TriState ParamB { get; set; }

	/// <summary>
	/// Gets or sets the parameter f.
	/// </summary>
	public TriState ParamF { get; set; }

	/// <summary>
	/// Gets or sets the parameter r.
	/// </summary>
	public TriState ParamR { get; set; }

	/// <summary>
	/// Gets or sets the parameter v.
	/// </summary>
	public TriState ParamV { get; set; }

	/// <summary>
	/// Gets or sets the photo ionization setting.
	/// </summary>
	public TriState PhotoIonization { get; set; }

	/// <summary>
	/// Gets or sets the polarity of the scan.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.PolarityType" /> for possible values</value>
	public PolarityType Polarity { get; set; }

	/// <summary>
	/// Gets or sets the pulsed dissociation setting.
	/// </summary>
	public TriState PulsedQDissociation { get; set; }

	/// <summary>
	/// Gets or sets the pulsed dissociation value.
	/// </summary>
	/// <value>Floating point pulsed dissociation value</value>
	public double PulsedQDissociationValue { get; set; }

	/// <summary>
	/// Gets or sets the scan data format (profile or centroid).
	/// </summary>
	public ScanDataType ScanData { get; set; }

	/// <summary>
	/// Gets or sets the scan type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.ScanModeType" /> for possible values</value>
	public ScanModeType ScanMode { get; set; }

	/// <summary>
	/// Gets or sets encoded form of segment and scan event number.
	/// </summary>
	/// <value>HIWORD == segment, LOWORD == scan type</value>
	public long ScanTypeIndex { get; set; }

	/// <summary>
	/// Gets or sets the sector scan setting. Applies to 2 sector (Magnetic, electrostatic) Mass spectrometers, or hybrids.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SectorScanType" /> for possible values</value>
	public SectorScanType SectorScan { get; set; }

	/// <summary>
	/// Gets or sets source fragmentation scan setting.
	/// </summary>
	public TriState SourceFragmentation { get; set; }

	/// <summary>
	/// Gets the total amount of source fragmentation information.
	/// This is all data for SID and CV, and is internally needed to
	/// support IScanEvent.
	/// To separately count the number of SID vales and CV values, use
	/// SourceFragmentationValueCount and CompensationVoltageValueCount
	/// </summary>
	/// <value>The size of source fragmentation info array</value>
	public int SourceFragmentationInfoCount => _sourceFragmentationValues.Count + _compensationVoltageValues.Count;

	/// <summary>
	/// Gets the number of Source Fragmentation values.
	/// </summary>
	/// <value>The size of source fragmentation values array</value>
	public int SourceFragmentationValueCount => _sourceFragmentationValues.Count;

	/// <summary>
	/// Gets the number of Compensation Voltage Fragmentation values.
	/// </summary>
	/// <value>The size of Compensation Voltage values array</value>
	public int CompensationVoltageValueCount => _compensationVoltageValues.Count;

	/// <summary>
	/// Gets the source fragmentation mass range count.
	/// </summary>
	public int SourceFragmentationMassRangeCount => _sourceFragmentationRanges.Count;

	/// <summary>
	/// Gets or sets the source fragmentation type.
	/// source fragmentation is exclusive with Composition voltage,
	/// so only one may be active.
	/// When set to SingleValue a value must be added using AddSourceFragmentationInfo.
	/// When set to Ramp two values must be added using AddSourceFragmentationInfo.
	/// When set to SIM, there must be one value added per (scanned) mass range.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SourceFragmentationValueType" /> for possible values</value>
	public SourceFragmentationValueType SourceFragmentationType { get; set; }

	/// <summary>
	/// Gets or sets supplemental activation type setting.
	/// </summary>
	public TriState SupplementalActivation { get; set; }

	/// <summary>
	/// Gets or sets the turbo scan setting.
	/// </summary>
	public TriState TurboScan { get; set; }

	/// <summary>
	/// Gets or sets the ultra scan setting.
	/// </summary>
	public TriState Ultra { get; set; }

	/// <summary>
	/// Gets or sets the wide band scan setting.
	/// </summary>
	public TriState Wideband { get; set; }

	/// <summary>
	/// Gets the MS stage count.
	/// </summary>
	public int MsStageCount => _stages.Count;

	/// <summary>
	/// Gets or sets the lower case E flag
	/// </summary>
	public bool LowerE
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerE);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerE, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case G flag
	/// </summary>
	public bool LowerG
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerG);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerG, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case H flag
	/// </summary>
	public bool LowerH
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerH);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerH, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case I flag
	/// </summary>
	public bool LowerI
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerI);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerI, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case J flag
	/// </summary>
	public bool LowerJ
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerJ);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerJ, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case IK flag
	/// </summary>
	public bool LowerK
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerK);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerK, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case L flag
	/// </summary>
	public bool LowerL
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerL);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerL, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case M flag
	/// </summary>
	public bool LowerM
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerM);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerM, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case N flag
	/// </summary>
	public bool LowerN
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerN);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerN, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case O flag
	/// </summary>
	public bool LowerO
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerO);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerO, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case Q flag
	/// </summary>
	public bool LowerQ
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerQ);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerQ, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case S flag
	/// </summary>
	public bool LowerS
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerS);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerS, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case X flag
	/// </summary>
	public bool LowerX
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerX);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerX, value);
		}
	}

	/// <summary>
	/// Gets or sets the lower case Y flag
	/// </summary>
	public bool LowerY
	{
		get
		{
			return GetLowerCaseSetting(LowerCaseFilterFlags.LowerY);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerY, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case A flag
	/// </summary>
	public bool UpperA
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperA);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperA, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case B flag
	/// </summary>
	public bool UpperB
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperB);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperB, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case F flag
	/// </summary>
	public bool UpperF
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperF);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperF, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case G flag
	/// </summary>
	public bool UpperG
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperG);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperG, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case H flag
	/// </summary>
	public bool UpperH
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperH);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperH, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case I flag
	/// </summary>
	public bool UpperI
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperI);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperI, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case J flag
	/// </summary>
	public bool UpperJ
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperJ);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperJ, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case K flag
	/// </summary>
	public bool UpperK
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperK);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperK, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case L flag
	/// </summary>
	public bool UpperL
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperL);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperL, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case M flag
	/// </summary>
	public bool UpperM
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperM);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperM, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case N flag
	/// </summary>
	public bool UpperN
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperN);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperN, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case O flag
	/// </summary>
	public bool UpperO
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperO);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperO, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case Q flag
	/// </summary>
	public bool UpperQ
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperQ);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperQ, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case R flag
	/// </summary>
	public bool UpperR
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperR);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperR, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case S flag
	/// </summary>
	public bool UpperS
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperS);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperS, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case T flag
	/// </summary>
	public bool UpperT
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperT);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperT, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case U flag
	/// </summary>
	public bool UpperU
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperU);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperU, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case V flag
	/// </summary>
	public bool UpperV
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperV);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperV, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case W flag
	/// </summary>
	public bool UpperW
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperW);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperW, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case X flag
	/// </summary>
	public bool UpperX
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperX);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperX, value);
		}
	}

	/// <summary>
	/// Gets or sets the upper case Y flag
	/// </summary>
	public bool UpperY
	{
		get
		{
			return GetUpperCaseSetting(UpperCaseFilterFlags.UpperY);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperY, value);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventBuilder" /> class.
	/// </summary>
	/// <param name="other">
	/// Copies data from the supplied event.
	/// </param>
	public ScanEventBuilder(IScanEvent other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (!other.IsValid)
		{
			throw new ArgumentException("Cannot copy from an event which is not valid", "other");
		}
		CopyBaseItems(other);
		AccurateMass = other.AccurateMass;
		IsCustom = other.IsCustom;
		CopyTables(other);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventBuilder" /> class.
	/// Note that this constructor makes a best case mapping between filter and event
	/// Some items specific to filter (such as "MassPrecision") cannot be preserved.
	/// Event only features (such as "IsCustom") are initialized to defaults.
	/// applications using this feature should test use cases.
	/// </summary>
	/// <param name="other">
	/// Copies data from the supplied filter.
	/// </param>
	public ScanEventBuilder(IScanFilter other)
		: this()
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		CopyBaseItems(other);
		switch (other.AccurateMass)
		{
		case FilterAccurateMass.Off:
			AccurateMass = EventAccurateMass.Off;
			break;
		case FilterAccurateMass.On:
			AccurateMass = EventAccurateMass.Internal;
			break;
		case FilterAccurateMass.Internal:
			AccurateMass = EventAccurateMass.Internal;
			break;
		case FilterAccurateMass.External:
			AccurateMass = EventAccurateMass.External;
			break;
		case FilterAccurateMass.Any:
			AccurateMass = EventAccurateMass.Off;
			break;
		}
	}

	/// <summary>
	/// copy base interface items.
	/// </summary>
	/// <param name="other">
	/// The other.
	/// </param>
	private void CopyBaseItems(IScanEventBase other)
	{
		CompensationVoltage = other.CompensationVoltage;
		CompensationVoltType = other.CompensationVoltType;
		Corona = other.Corona;
		Dependent = other.Dependent;
		Detector = other.Detector;
		DetectorValue = other.DetectorValue;
		ElectronCaptureDissociation = other.ElectronCaptureDissociation;
		ElectronCaptureDissociationValue = other.ElectronCaptureDissociationValue;
		ElectronTransferDissociation = other.ElectronTransferDissociation;
		ElectronTransferDissociationValue = other.ElectronTransferDissociationValue;
		Enhanced = other.Enhanced;
		FieldFreeRegion = other.FieldFreeRegion;
		HigherEnergyCiD = other.HigherEnergyCiD;
		HigherEnergyCiDValue = other.HigherEnergyCiDValue;
		IonizationMode = other.IonizationMode;
		Lock = other.Lock;
		MSOrder = other.MSOrder;
		MassAnalyzer = other.MassAnalyzer;
		MultiNotch = other.MultiNotch;
		MultiStateActivation = other.MultiStateActivation;
		MultiplePhotonDissociation = other.MultiplePhotonDissociation;
		MultiplePhotonDissociationValue = other.MultiplePhotonDissociationValue;
		Multiplex = other.Multiplex;
		Name = other.Name;
		ParamA = other.ParamA;
		ParamB = other.ParamB;
		ParamF = other.ParamF;
		ParamR = other.ParamR;
		ParamV = other.ParamV;
		PhotoIonization = other.PhotoIonization;
		Polarity = other.Polarity;
		PulsedQDissociation = other.PulsedQDissociation;
		PulsedQDissociationValue = other.PulsedQDissociationValue;
		ScanData = other.ScanData;
		ScanMode = other.ScanMode;
		ScanTypeIndex = other.ScanTypeIndex;
		SectorScan = other.SectorScan;
		SourceFragmentation = other.SourceFragmentation;
		SourceFragmentationType = other.SourceFragmentationType;
		SupplementalActivation = other.SupplementalActivation;
		TurboScan = other.TurboScan;
		Ultra = other.Ultra;
		Wideband = other.Wideband;
		CopyMsMsStages(other);
		CopyBaseTables(other);
		if (other is IScanFilterPlus scanFilterPlus)
		{
			UpperCaseFlags = scanFilterPlus.UpperCaseFlags;
			LowerCaseFlags = scanFilterPlus.LowerCaseFlags;
			UpperCaseFlagsApplied = scanFilterPlus.AllUpperCaseFilterApplied;
			LowerCaseFlagsApplied = scanFilterPlus.AllLowerCaseFiltersApplied;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventBuilder" /> class.
	/// Most properties are initialized to "Any" (feature not used).
	/// Numeric values have defaults (0), apart from <c>ScanTypeIndex</c>
	/// which is set to "-1" for undefined.
	/// </summary>
	public ScanEventBuilder()
	{
		Dependent = TriState.Any;
		Wideband = TriState.Any;
		SupplementalActivation = TriState.Any;
		MultiStateActivation = TriState.Any;
		AccurateMass = EventAccurateMass.Off;
		Detector = DetectorType.Any;
		SourceFragmentation = TriState.Any;
		SourceFragmentationType = SourceFragmentationValueType.Any;
		CompensationVoltage = TriState.Any;
		CompensationVoltType = CompensationVoltageType.Any;
		TurboScan = TriState.Any;
		Lock = TriState.Any;
		Multiplex = TriState.Any;
		ParamA = TriState.Any;
		ParamB = TriState.Any;
		ParamF = TriState.Any;
		MultiNotch = TriState.Any;
		ParamR = TriState.Any;
		ParamV = TriState.Any;
		Ultra = TriState.Any;
		Enhanced = TriState.Any;
		ElectronCaptureDissociation = TriState.Any;
		MultiplePhotonDissociation = TriState.Any;
		Corona = TriState.Any;
		ElectronTransferDissociation = TriState.Any;
		FieldFreeRegion = FieldFreeRegionType.Any;
		HigherEnergyCiD = TriState.Any;
		IonizationMode = IonizationModeType.Any;
		MSOrder = MSOrderType.Any;
		MassAnalyzer = MassAnalyzerType.Any;
		PhotoIonization = TriState.Any;
		PulsedQDissociation = TriState.Any;
		ScanData = ScanDataType.Any;
		ScanMode = ScanModeType.Any;
		SectorScan = SectorScanType.Any;
		ElectronCaptureDissociation = TriState.Any;
		Polarity = PolarityType.Any;
		ScanTypeIndex = -1L;
	}

	/// <summary>
	/// Gets the number expected SID values.
	/// </summary>
	/// <returns>
	/// The number of values
	/// </returns>
	private int ExpectedSid()
	{
		int result = 0;
		switch (SourceFragmentationType)
		{
		case SourceFragmentationValueType.SingleValue:
			result = 1;
			break;
		case SourceFragmentationValueType.Ramp:
			result = 2;
			break;
		case SourceFragmentationValueType.SIM:
			result = _massRanges.Count;
			break;
		}
		return result;
	}

	/// <summary>
	/// Gets the number expected CV values.
	/// </summary>
	/// <returns>
	/// The number of values
	/// </returns>
	private int ExpectedCv()
	{
		int result = 0;
		switch (CompensationVoltType)
		{
		case CompensationVoltageType.SingleValue:
			result = 1;
			break;
		case CompensationVoltageType.Ramp:
			result = 2;
			break;
		case CompensationVoltageType.SIM:
			result = _massRanges.Count;
			break;
		}
		return result;
	}

	/// <summary>
	/// add a mass calibrator.
	/// </summary>
	/// <param name="calibrator">
	/// The calibrator to add.
	/// </param>
	public void AddMassCalibrator(double calibrator)
	{
		_massCalibrators.Add(calibrator);
	}

	/// <summary>
	/// Adds a set of mass calibrators.
	/// </summary>
	/// <param name="calibrators">
	/// The calibrators to add.
	/// </param>
	public void AddMassCalibrators(IEnumerable<double> calibrators)
	{
		if (calibrators == null)
		{
			throw new ArgumentNullException("calibrators");
		}
		foreach (double calibrator in calibrators)
		{
			_massCalibrators.Add(calibrator);
		}
	}

	/// <summary>
	/// Adds a mass range.
	/// </summary>
	/// <param name="massRange">
	/// The mass range.
	/// </param>
	public void AddMassRange(Range massRange)
	{
		if (massRange == null)
		{
			throw new ArgumentNullException("massRange");
		}
		_massRanges.Add(massRange);
	}

	/// <summary>
	/// Adds a set of mass ranges.
	/// </summary>
	/// <param name="massRanges">
	/// The mass ranges to add.
	/// </param>
	public void AddMassRanges(IEnumerable<Range> massRanges)
	{
		if (massRanges == null)
		{
			throw new ArgumentNullException("massRanges");
		}
		foreach (Range massRange in massRanges)
		{
			AddMassRange(massRange);
		}
	}

	/// <summary>
	/// Adds an MS stage.
	/// For example: for an MS3 scan, 2 MS stages should be added,
	/// with the 2 required precursor masses.
	/// Each stage may have one or more reactions.
	/// Neutral Loss, Neutral Gain and Parent have 1 stage.
	/// For multiplex mode, each multiplexed precursor should be added as
	/// a separate stage. 
	/// </summary>
	/// <param name="stage">
	/// The stage.
	/// </param>
	public void AddMsStage(MsStage stage)
	{
		if (stage == null)
		{
			throw new ArgumentNullException("stage");
		}
		_stages.Add(stage);
	}

	/// <summary>
	/// Gets an MS stage.
	/// For example: for an MS3 scan, there will be 2 MS stages
	/// with the 2 required precursor masses.
	/// Each stage may have one or more reactions.
	/// Neutral Loss, Neutral Gain and Parent have 1 stage. 
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The stage at the given index
	/// </returns>
	public MsStage GetMsStage(int index)
	{
		return _stages[index];
	}

	/// <summary>
	/// Add a source fragmentation value.
	/// This is for "source fragmentation (<c>sid</c>)
	/// and is only used when "SourceFragmentation" has
	/// been enabled. Add one value for "Single value" mode or two values
	/// for "Ramp" mode of the enabled feature.
	/// For SIM mode, add one value per target mass range.
	/// </summary>
	/// <param name="value">
	/// The value.
	/// </param>
	public void AddSourceFragmentationValue(double value)
	{
		_sourceFragmentationValues.Add(value);
	}

	/// <summary>
	/// Add a compensation voltage value. This is for "compensation voltage (<c>cv</c>)"
	/// and used only when "CompensationVoltage" has
	/// been enabled. Add one value for "Single value" mode or two values
	/// for "Ramp" mode of the enabled feature.
	/// For SIM mode, add one value per target mass range.
	/// </summary>
	/// <param name="value">
	/// The value.
	/// </param>
	public void AddCompensationVoltageValue(double value)
	{
		_compensationVoltageValues.Add(value);
	}

	/// <summary>
	/// Adds a source fragmentation mass range.
	/// </summary>
	/// <param name="range">
	/// The mass range to add.
	/// </param>
	public void AddSourceFragmentationMassRange(Range range)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		_sourceFragmentationRanges.Add(range);
	}

	/// <summary>
	/// Clears the mass calibrators list.
	/// </summary>
	public void ClearMassCalibrators()
	{
		_massCalibrators.Clear();
	}

	/// <summary>
	/// clear the list of mass ranges.
	/// </summary>
	public void ClearMassRanges()
	{
		_massRanges.Clear();
	}

	/// <summary>
	/// Resets the list of MS/MS stages.
	/// This may be needed if an event is cloned from another event,
	/// but the new object needs different MS/MS stages.
	/// </summary>
	public void ClearMsStages()
	{
		_stages.Clear();
	}

	/// <summary>
	/// Clears the source fragmentation values.
	/// </summary>
	public void ClearSourceFragmentationValues()
	{
		_sourceFragmentationValues.Clear();
	}

	/// <summary>
	/// Clears the Compensation Voltage values.
	/// </summary>
	public void ClearCompensationVoltageValues()
	{
		_compensationVoltageValues.Clear();
	}

	/// <summary>
	/// Clears the source fragmentation mass ranges list.
	/// </summary>
	public void ClearSourceFragmentationMassRanges()
	{
		_sourceFragmentationRanges.Clear();
	}

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
	public ActivationType GetActivation(int index)
	{
		return GetReaction(index).ActivationType;
	}

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
	public double GetEnergy(int index)
	{
		return GetReaction(index).CollisionEnergy;
	}

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
	public EnergyType GetEnergyValid(int index)
	{
		if (!GetReaction(index).CollisionEnergyValid)
		{
			return EnergyType.Any;
		}
		return EnergyType.Valid;
	}

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
	public double GetFirstPrecursorMass(int index)
	{
		return GetReaction(index).FirstPrecursorMass;
	}

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
	public bool GetIsMultipleActivation(int index)
	{
		return GetReaction(index).MultipleActivation;
	}

	/// <summary>
	/// Gets the isolation width.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The isolation width
	/// </returns>
	public double GetIsolationWidth(int index)
	{
		return GetReaction(index).IsolationWidth;
	}

	/// <summary>
	/// Gets the isolation width offset.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The isolation width offset
	/// </returns>
	public double GetIsolationWidthOffset(int index)
	{
		return GetReaction(index).IsolationWidthOffset;
	}

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
	public double GetLastPrecursorMass(int index)
	{
		return GetReaction(index).LastPrecursorMass;
	}

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
	public double GetMass(int index)
	{
		return GetReaction(index).PrecursorMass;
	}

	/// <summary>
	/// Gets the mass calibrator, at a given index.
	/// </summary>
	/// <param name="index">
	/// The index, which should be from 0 to MassCalibratorCount -1
	/// </param>
	/// <returns>
	/// The mass calibrator.
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Thrown when requesting calibrator above count</exception>
	public double GetMassCalibrator(int index)
	{
		return _massCalibrators[index];
	}

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
	public IRangeAccess GetMassRange(int index)
	{
		return _massRanges[index];
	}

	/// <summary>
	/// Determine if a precursor range is valid.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// true if valid
	/// </returns>
	public bool GetPrecursorRangeValidity(int index)
	{
		return GetReaction(index).PrecursorRangeIsValid;
	}

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
	public IReaction GetReaction(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "index must be positive");
		}
		int num = 0;
		foreach (MsStage stage in _stages)
		{
			List<IReaction> reactions = stage.Reactions;
			int count = reactions.Count;
			if (index < num + count)
			{
				return reactions[index - num];
			}
			num += count;
		}
		throw new ArgumentOutOfRangeException("index", "index must be less than the number of reactions");
	}

	/// <summary>
	/// Retrieves a source fragmentation value at 0-based index.
	/// </summary>
	/// <param name="index">
	/// Index of source fragmentation value to be retrieved
	/// </param>
	/// <returns>
	/// Source Fragmentation value at 0-based index
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= SourceFragmentationValueCount</exception>
	public double GetSourceFragmentationValue(int index)
	{
		return _sourceFragmentationValues[index];
	}

	/// <summary>
	/// Retrieves a compensation voltage value at 0-based index.
	/// </summary>
	/// <param name="index">
	/// Index of compensation voltage value to be retrieved
	/// </param>
	/// <returns>
	/// Compensation Voltage value at 0-based index
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Will be thrown when index &gt;= CompensationVoltageValueCount</exception>
	public double GetCompensationVoltageValue(int index)
	{
		return _compensationVoltageValues[index];
	}

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
	public double GetSourceFragmentationInfo(int index)
	{
		int sourceFragmentationValueCount = SourceFragmentationValueCount;
		if (index < sourceFragmentationValueCount)
		{
			return _sourceFragmentationValues[index];
		}
		return _compensationVoltageValues[index - sourceFragmentationValueCount];
	}

	/// <inheritdoc />
	public IScanEventExtended GetExtensions()
	{
		return this;
	}

	/// <summary>
	/// Get the source fragmentation mass range, at a given index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The mass range.
	/// </returns>
	public Range GetSourceFragmentationMassRange(int index)
	{
		return _sourceFragmentationRanges[index];
	}

	/// <summary>
	/// copy mass calibrators from another event.
	/// </summary>
	/// <param name="other">
	/// The other event.
	/// </param>
	private void CopyMassCalibrators(IScanEvent other)
	{
		int massCalibratorCount = other.MassCalibratorCount;
		for (int i = 0; i < massCalibratorCount; i++)
		{
			AddMassCalibrator(other.GetMassCalibrator(i));
		}
	}

	/// <summary>
	/// copy MS/MS stages from another event.
	/// </summary>
	/// <param name="other">
	/// The event from which data is copied from.
	/// </param>
	private void CopyMsMsStages(IScanEventBase other)
	{
		IEnumerable<MsStage> stages = new MsOrderTable(other).Stages;
		AddMsStages(stages);
	}

	/// <summary>
	/// Adds a set of MS stages.
	/// </summary>
	/// <param name="stages">
	/// The stages.
	/// </param>
	public void AddMsStages(IEnumerable<MsStage> stages)
	{
		if (stages == null)
		{
			throw new ArgumentNullException("stages");
		}
		foreach (MsStage stage in stages)
		{
			AddMsStage(stage);
		}
	}

	/// <summary>
	/// copy scanned mass ranges from another event.
	/// </summary>
	/// <param name="other">
	/// The other event.
	/// </param>
	private void CopyScannedMassRanges(IScanEventBase other)
	{
		int massRangeCount = other.MassRangeCount;
		for (int i = 0; i < massRangeCount; i++)
		{
			_massRanges.Add(other.GetMassRange(i));
		}
	}

	/// <summary>
	/// copy source fragmentation info from another event.
	/// </summary>
	/// <param name="other">
	/// The other event.
	/// </param>
	private void CopySourceFragmentationInfo(IScanEventBase other)
	{
		int num = ExpectedSid();
		int sourceFragmentationInfoCount = other.SourceFragmentationInfoCount;
		for (int i = 0; i < sourceFragmentationInfoCount; i++)
		{
			double sourceFragmentationInfo = other.GetSourceFragmentationInfo(i);
			if (i < num)
			{
				_sourceFragmentationValues.Add(sourceFragmentationInfo);
			}
			else
			{
				_compensationVoltageValues.Add(sourceFragmentationInfo);
			}
		}
	}

	/// <summary>
	/// copy source fragmentation mass ranges from another event.
	/// </summary>
	/// <param name="other">
	/// The other event.
	/// </param>
	private void CopySourceFragmentationMassRanges(IScanEvent other)
	{
		int sourceFragmentationMassRangeCount = other.SourceFragmentationMassRangeCount;
		for (int i = 0; i < sourceFragmentationMassRangeCount; i++)
		{
			_sourceFragmentationRanges.Add(other.GetSourceFragmentationMassRange(i));
		}
	}

	/// <summary>
	/// copy various tables from another event.
	/// </summary>
	/// <param name="other">
	/// The other event.
	/// </param>
	private void CopyTables(IScanEvent other)
	{
		CopyMassCalibrators(other);
		CopySourceFragmentationMassRanges(other);
	}

	/// <summary>
	/// copy various tables from another event base.
	/// </summary>
	/// <param name="other">
	/// The other event.
	/// </param>
	private void CopyBaseTables(IScanEventBase other)
	{
		CopyScannedMassRanges(other);
		CopySourceFragmentationInfo(other);
	}

	/// <inheritdoc />
	public TriState GetLowerCaseFlag(LowerCaseFilterFlags flag)
	{
		if ((LowerCaseFlagsApplied & flag) == 0)
		{
			return TriState.Any;
		}
		if ((LowerCaseFlags & flag) == 0)
		{
			return TriState.Off;
		}
		return TriState.On;
	}

	/// <summary>
	/// Gets a value indicating whether a scan was performed with the indicated feature enabled.
	/// These features appear as lower case letters, when converted to string (as a scan event). 
	/// </summary>
	/// <param name="flag">The flag to inspect</param>
	/// <returns>value of the flag</returns>
	public bool GetLowerCaseSetting(LowerCaseFilterFlags flag)
	{
		if ((LowerCaseFlagsApplied & flag) != 0)
		{
			return (LowerCaseFlags & flag) != 0;
		}
		return false;
	}

	/// <inheritdoc />
	public TriState GetUpperCaseFlag(UpperCaseFilterFlags flag)
	{
		if ((UpperCaseFlagsApplied & flag) == 0)
		{
			return TriState.Any;
		}
		if ((UpperCaseFlags & flag) == 0)
		{
			return TriState.Off;
		}
		return TriState.On;
	}

	/// <summary>
	/// Gets a value indicating whether a scan was performed with the indicated feature enabled.
	/// These features appear as upper case letters, when converted to string (as a scan event). 
	/// </summary>
	/// <param name="flag">The flag to inspect</param>
	/// <returns>value of the flag</returns>
	public bool GetUpperCaseSetting(UpperCaseFilterFlags flag)
	{
		if ((UpperCaseFlagsApplied & flag) != 0)
		{
			return (UpperCaseFlags & flag) != 0;
		}
		return false;
	}

	/// <summary>
	/// Set the state of a upper case flag. (true, if this scan uses the indicated feature)
	/// </summary>
	/// <param name="flag">Flag set set</param>
	/// <param name="value">value of flag</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	private void SetUpperFlag(UpperCaseFilterFlags flag, bool value)
	{
		if (value)
		{
			UpperCaseFlags |= flag;
			UpperCaseFlagsApplied |= flag;
		}
		else
		{
			UpperCaseFlags &= ~flag;
			UpperCaseFlagsApplied |= flag;
		}
	}

	/// <summary>
	/// Set the state of a upper case flag. (true, if this scan uses the indicated feature)
	/// </summary>
	/// <param name="flag">Flag to set</param>
	/// <param name="value">value of flag</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	private void SetLowerFlag(LowerCaseFilterFlags flag, bool value)
	{
		if (value)
		{
			LowerCaseFlags |= flag;
			LowerCaseFlagsApplied |= flag;
		}
		else
		{
			LowerCaseFlags &= ~flag;
			LowerCaseFlagsApplied |= flag;
		}
	}
}
