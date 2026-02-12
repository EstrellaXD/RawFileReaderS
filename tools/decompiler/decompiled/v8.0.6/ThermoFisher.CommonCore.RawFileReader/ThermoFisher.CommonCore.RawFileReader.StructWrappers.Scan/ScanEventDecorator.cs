using System;
using System.Globalization;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// The scan event decorator.
/// </summary>
internal abstract class ScanEventDecorator : IScanEventEdit, IRawFileReaderScanEvent
{
	/// <summary>
	/// Gets the (decorated) scan event
	/// </summary>
	protected IRawFileReaderScanEvent ReaderScanEvent { get; }

	/// <summary>
	/// Gets the base scan event.
	/// </summary>
	/// <value>
	/// The base scan event.
	/// </value>
	public IRawFileReaderScanEvent BaseScanEvent => ReaderScanEvent;

	/// <summary>
	/// Gets or sets the dependent data flag.
	/// </summary>
	public ScanFilterEnums.IsDependent DependentDataFlag { get; set; }

	/// <summary>
	/// Gets or sets the wide band.
	/// </summary>
	public ScanFilterEnums.OffOnTypes Wideband { get; set; }

	public LowerCaseFilterFlags LowerCaseFlags { get; set; }

	public LowerCaseFilterFlags LowerCaseApplied { get; set; }

	public UpperCaseFilterFlags UpperCaseFlags { get; set; }

	public UpperCaseFilterFlags UpperCaseApplied { get; set; }

	/// <summary>
	/// Gets or sets the supplemental activation.
	/// </summary>
	public ScanFilterEnums.OffOnTypes SupplementalActivation { get; set; }

	/// <summary>
	/// Gets or sets the multi state activation.
	/// </summary>
	public ScanFilterEnums.OffOnTypes MultiStateActivation { get; set; }

	/// <summary>
	/// Gets or sets the accurate mass type.
	/// </summary>
	public ScanFilterEnums.AccurateMassTypes AccurateMassType { get; set; }

	/// <summary>
	/// Gets or sets the detector.
	/// </summary>
	public ScanFilterEnums.DetectorType Detector { get; set; }

	/// <summary>
	/// Gets or sets the source fragmentation.
	/// </summary>
	public ScanFilterEnums.OnOffTypes SourceFragmentation { get; set; }

	/// <summary>
	/// Gets or sets a value which indicates how source fragmentation values are interpreted.
	/// </summary>
	public ScanFilterEnums.VoltageTypes SourceFragmentationType { get; set; }

	/// <summary>
	/// Gets or sets the compensation voltage.
	/// </summary>
	public ScanFilterEnums.OnOffTypes CompensationVoltage { get; set; }

	/// <summary>
	/// Gets or sets the compensation voltage type.
	/// </summary>
	public ScanFilterEnums.VoltageTypes CompensationVoltageType { get; set; }

	/// <summary>
	/// Gets or sets the turbo scan.
	/// </summary>
	public ScanFilterEnums.OnOffTypes TurboScan { get; set; }

	/// <summary>
	/// Gets or sets the lock.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Lock { get; set; }

	/// <summary>
	/// Gets or sets the multiplex.
	/// </summary>
	public ScanFilterEnums.OffOnTypes Multiplex { get; set; }

	/// <summary>
	/// Gets or sets the parameter a.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamA { get; set; }

	/// <summary>
	/// Gets or sets the parameter b.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamB { get; set; }

	/// <summary>
	/// Gets or sets the parameter f.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamF { get; set; }

	/// <summary>
	/// Gets or sets SPS Multi notch (Synchronous Precursor Selection)
	/// </summary>
	public ScanFilterEnums.OffOnTypes SpsMultiNotch { get; set; }

	/// <summary>
	/// Gets or sets the parameter r.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamR { get; set; }

	/// <summary>
	/// Gets or sets the parameter v.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamV { get; set; }

	/// <summary>
	/// Gets or sets the ultra.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Ultra { get; set; }

	/// <summary>
	/// Gets or sets the enhanced.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Enhanced { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation type.
	/// </summary>
	public ScanFilterEnums.OnAnyOffTypes ElectronCaptureDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the multi photon dissociation type.
	/// </summary>
	public ScanFilterEnums.OnAnyOffTypes MultiPhotonDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the corona value.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Corona { get; set; }

	/// <summary>
	/// Gets or sets the detector value.
	/// </summary>
	public double DetectorValue { get; set; }

	/// <summary>
	/// Gets or sets the electron capture dissociation.
	/// </summary>
	public double ElectronCaptureDissociation { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation.
	/// </summary>
	public double ElectronTransferDissociation { get; set; }

	/// <summary>
	/// Gets or sets the electron transfer dissociation type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes ElectronTransferDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the free region.
	/// </summary>
	public ScanFilterEnums.FreeRegions FreeRegion { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID.
	/// </summary>
	public double HigherEnergyCid { get; set; }

	/// <summary>
	/// Gets or sets the higher energy CID type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes HigherEnergyCidType { get; set; }

	/// <summary>
	/// Gets or sets the ionization mode.
	/// </summary>
	public ScanFilterEnums.IonizationModes IonizationMode { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the scan event is custom - true if trailer
	///     scan event should be used.
	/// </summary>
	public bool IsCustom { get; protected set; }

	/// <summary>
	/// Gets or sets a value indicating whether the scan event is valid.
	/// </summary>
	public bool IsValid { get; protected set; }

	/// <summary>
	/// Gets or sets the MS order.
	/// </summary>
	public ScanFilterEnums.MSOrderTypes MsOrder { get; set; }

	/// <summary>
	/// Gets or sets the mass analyzer type.
	/// </summary>
	public ScanFilterEnums.MassAnalyzerTypes MassAnalyzerType { get; set; }

	/// <summary>
	/// Gets or sets the mass calibrators.
	/// </summary>
	public double[] MassCalibrators { get; protected set; }

	/// <summary>
	/// Gets or sets the mass ranges.
	/// </summary>
	public MassRangeStruct[] MassRanges { get; set; }

	/// <summary>
	/// Gets or sets the multi photon dissociation.
	/// </summary>
	public double MultiPhotonDissociation { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the photo ionization.
	/// </summary>
	public ScanFilterEnums.OnOffTypes PhotoIonization { get; set; }

	/// <summary>
	/// Gets or sets the Polarity.
	/// </summary>
	public ScanFilterEnums.PolarityTypes Polarity { get; set; }

	/// <summary>
	/// Gets or sets the pulsed q dissociation.
	/// </summary>
	public double PulsedQDissociation { get; set; }

	/// <summary>
	/// Gets or sets the pulsed q dissociation type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes PulsedQDissociationType { get; set; }

	/// <summary>
	/// Gets or sets the reactions.
	/// </summary>
	public Reaction[] Reactions { get; protected set; }

	/// <summary>
	/// Gets or sets the scan data type.
	/// </summary>
	public ScanFilterEnums.ScanDataTypes ScanDataType { get; set; }

	/// <summary>
	/// Gets or sets the scan type.
	/// </summary>
	public ScanFilterEnums.ScanTypes ScanType { get; set; }

	/// <summary>
	/// Gets or sets the scan type index. Scan Type Index indicates the segment/scan event for this filter scan event.
	///     HIWORD == segment, LOWORD == scan type
	/// </summary>
	public int ScanTypeIndex { get; set; }

	/// <summary>
	/// Gets or sets the sector scan.
	/// </summary>
	public ScanFilterEnums.SectorScans SectorScan { get; set; }

	/// <summary>
	/// Gets or sets the source fragmentation mass ranges.
	/// </summary>
	public MassRangeStruct[] SourceFragmentationMassRanges { get; protected set; }

	/// <summary>
	/// Gets or sets the source fragmentations.
	/// </summary>
	public double[] SourceFragmentations { get; set; }

	public TriState LowerE
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerE);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerE, value);
		}
	}

	public TriState LowerG
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerG);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerG, value);
		}
	}

	public TriState LowerH
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerH);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerH, value);
		}
	}

	public TriState LowerI
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerI);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerI, value);
		}
	}

	public TriState LowerJ
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerJ);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerJ, value);
		}
	}

	public TriState LowerK
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerK);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerK, value);
		}
	}

	public TriState LowerL
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerL);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerL, value);
		}
	}

	public TriState LowerM
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerM);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerM, value);
		}
	}

	public TriState LowerN
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerN);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerN, value);
		}
	}

	public TriState LowerO
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerO);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerO, value);
		}
	}

	public TriState LowerQ
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerQ);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerQ, value);
		}
	}

	public TriState LowerS
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerS);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerS, value);
		}
	}

	public TriState LowerX
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerX);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerX, value);
		}
	}

	public TriState LowerY
	{
		get
		{
			return GetLowerFlag(LowerCaseFilterFlags.LowerY);
		}
		set
		{
			SetLowerFlag(LowerCaseFilterFlags.LowerY, value);
		}
	}

	public TriState UpperA
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperA);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperA, value);
		}
	}

	public TriState UpperB
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperB);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperB, value);
		}
	}

	public TriState UpperF
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperF);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperF, value);
		}
	}

	public TriState UpperG
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperG);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperG, value);
		}
	}

	public TriState UpperH
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperH);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperH, value);
		}
	}

	public TriState UpperI
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperI);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperI, value);
		}
	}

	public TriState UpperJ
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperJ);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperJ, value);
		}
	}

	public TriState UpperK
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperK);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperK, value);
		}
	}

	public TriState UpperL
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperL);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperL, value);
		}
	}

	public TriState UpperM
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperM);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperM, value);
		}
	}

	public TriState UpperN
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperN);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperN, value);
		}
	}

	public TriState UpperO
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperO);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperO, value);
		}
	}

	public TriState UpperQ
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperQ);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperQ, value);
		}
	}

	public TriState UpperR
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperR);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperR, value);
		}
	}

	public TriState UpperS
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperS);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperS, value);
		}
	}

	public TriState UpperT
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperT);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperT, value);
		}
	}

	public TriState UpperU
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperU);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperU, value);
		}
	}

	public TriState UpperV
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperV);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperV, value);
		}
	}

	public TriState UpperW
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperW);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperW, value);
		}
	}

	public TriState UpperX
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperX);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperX, value);
		}
	}

	public TriState UpperY
	{
		get
		{
			return GetUpperFlag(UpperCaseFilterFlags.UpperY);
		}
		set
		{
			SetUpperFlag(UpperCaseFilterFlags.UpperY, value);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEventDecorator" /> class.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	protected ScanEventDecorator(IRawFileReaderScanEvent scanEvent)
	{
		ReaderScanEvent = scanEvent;
	}

	/// <summary>
	/// To the automatic filter string.
	/// </summary>
	/// <param name="scanEvent">The scan event.</param>
	/// <param name="massPrecision">The mass precision.</param>
	/// <param name="charsMax">The chars maximum.</param>
	/// <param name="energyPrecision">The energy precision.</param>
	/// <param name="formatProvider">localized number format</param>
	/// <param name="listSeparator"></param>
	/// <returns>Auto filter string.</returns>
	public string ToAutoFilterString(IRawFileReaderScanEvent scanEvent, int massPrecision = -1, int charsMax = -1, int energyPrecision = -1, IFormatProvider formatProvider = null, string listSeparator = ",")
	{
		return ReaderScanEvent.ToAutoFilterString(scanEvent, massPrecision, charsMax, energyPrecision, formatProvider ?? CultureInfo.InvariantCulture, listSeparator);
	}

	/// <summary>
	/// Gets the run header filter mass precision.
	/// </summary>
	/// <returns>the run header filter mass precision</returns>
	public int GetRunHeaderFilterMassPrecision()
	{
		return ReaderScanEvent.GetRunHeaderFilterMassPrecision();
	}

	/// <summary>
	/// Set the state of a lower case flag.
	/// On: Flag applied On
	/// Off: Flag applied Off
	/// Any: Flag not used (not applied and off)
	/// </summary>
	/// <param name="flag"></param>
	/// <param name="value"></param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	private void SetLowerFlag(LowerCaseFilterFlags flag, TriState value)
	{
		switch (value)
		{
		case TriState.Off:
			LowerCaseFlags &= ~flag;
			LowerCaseApplied |= flag;
			break;
		case TriState.On:
			LowerCaseFlags |= flag;
			LowerCaseApplied |= flag;
			break;
		case TriState.Any:
			LowerCaseFlags &= ~flag;
			LowerCaseApplied &= ~flag;
			break;
		default:
			throw new ArgumentOutOfRangeException("value", value, null);
		}
	}

	/// <summary>
	/// Get the state of a lower case flag, from the lower case flag bits and the active flag bits.
	/// If it is not active, return "Any"
	/// </summary>
	/// <param name="flag"></param>
	/// <returns></returns>
	private TriState GetLowerFlag(LowerCaseFilterFlags flag)
	{
		if ((LowerCaseApplied & flag) == 0)
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
	/// Set the state of a upper case flag.
	/// On: Flag applied On
	/// Off: Flag applied Off
	/// Any: Flag not used (not applied and off)
	/// </summary>
	/// <param name="flag"></param>
	/// <param name="value"></param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	private void SetUpperFlag(UpperCaseFilterFlags flag, TriState value)
	{
		switch (value)
		{
		case TriState.Off:
			UpperCaseFlags &= ~flag;
			UpperCaseApplied |= flag;
			break;
		case TriState.On:
			UpperCaseFlags |= flag;
			UpperCaseApplied |= flag;
			break;
		case TriState.Any:
			UpperCaseFlags &= ~flag;
			UpperCaseApplied &= ~flag;
			break;
		default:
			throw new ArgumentOutOfRangeException("value", value, null);
		}
	}

	/// <summary>
	/// Get the state of a upper case flag, from the lower case flag bits and the active flag bits.
	/// If it is not active, return "Any"
	/// </summary>
	/// <param name="flag"></param>
	/// <returns></returns>
	private TriState GetUpperFlag(UpperCaseFilterFlags flag)
	{
		if ((UpperCaseApplied & flag) == 0)
		{
			return TriState.Any;
		}
		if ((UpperCaseFlags & flag) == 0)
		{
			return TriState.Off;
		}
		return TriState.On;
	}
}
