using System;
using System.Globalization;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.FilterInfo;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// Defines filter data, as may be embedded in a processing method, or chromatogram settings
/// </summary>
internal sealed class Filter : IRawFileReaderScanEvent, IRawObjectBase
{
	private static readonly int[,] MarshalledSizes;

	private FilterInfoStruct _filterInfoStructInfo;

	/// <summary>
	/// Gets the masses. Masses for each MS step
	/// </summary>
	/// <value>
	/// The masses.
	/// </value>
	private double[] _masses;

	/// <summary>
	/// Gets the precursor mass ranges. Precursor mass range(s)
	/// </summary>
	/// <value>
	/// The precursor mass ranges.
	/// </value>
	private MassRangeStruct[] _precursorMassRanges;

	private uint[] _precursorMassRangesValid;

	private double[] _precursorEnergies;

	private uint[] _precursorEnergiesValid;

	/// <summary>
	/// Gets the name. event name - for named filters as an alternative to scan filters
	/// </summary>
	/// <value>
	/// The name.
	/// </value>
	public string Name { get; private set; }

	/// <summary>
	/// Gets or sets the mass ranges. Mass range(s) for final scan
	/// </summary>
	/// <value>
	/// The mass ranges.
	/// </value>
	public MassRangeStruct[] MassRanges { get; set; }

	/// <summary>
	/// Gets or sets the source fragmentations.
	/// </summary>
	private double[] SourceFragmentations { get; set; }

	/// <summary>
	/// Gets a value used to indicate which m_SourceFragmentationInfo values are valid.
	/// </summary>
	public ScanFilterEnums.SourceCIDValidTypes[] SourceFragmentationInfoValid { get; private set; }

	/// <summary>
	/// Gets the source fragmentation mass ranges.
	/// </summary>
	public MassRangeStruct[] SourceFragmentationMassRanges { get; private set; }

	/// <summary>
	/// Gets the accurate mass type.
	/// </summary>
	public ScanFilterEnums.AccurateMassTypes AccurateMassType => _filterInfoStructInfo.AccurateMass;

	/// <summary>
	/// Gets the compensation voltage.
	/// </summary>
	public ScanFilterEnums.OnOffTypes CompensationVoltage => _filterInfoStructInfo.CompensationVoltage;

	/// <summary>
	/// Gets the compensation voltage type.
	/// </summary>
	public ScanFilterEnums.VoltageTypes CompensationVoltageType => _filterInfoStructInfo.CompensationVoltageType;

	/// <summary>
	/// Gets the corona value.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Corona => _filterInfoStructInfo.Corona;

	/// <summary>
	/// Gets the dependent data.
	/// </summary>
	public ScanFilterEnums.IsDependent DependentDataFlag => _filterInfoStructInfo.Dependent;

	/// <summary>
	/// Gets the Detector value.
	/// </summary>
	public ScanFilterEnums.DetectorType Detector => (ScanFilterEnums.DetectorType)_filterInfoStructInfo.DetectorState;

	/// <summary>
	/// Gets the detector value.
	/// </summary>
	public double DetectorValue => _filterInfoStructInfo.DetectorValue;

	/// <summary>
	/// Gets the electron capture dissociation.
	/// </summary>
	public double ElectronCaptureDissociation => _filterInfoStructInfo.ElectronCaptureDissociationValue;

	/// <summary>
	/// Gets the electron capture dissociation type.
	/// </summary>
	public ScanFilterEnums.OnAnyOffTypes ElectronCaptureDissociationType => _filterInfoStructInfo.ElectronCaptureDissociationState;

	/// <summary>
	/// Gets the electron transfer dissociation.
	/// </summary>
	public double ElectronTransferDissociation => _filterInfoStructInfo.ElectronTransferDissociationValue;

	/// <summary>
	/// Gets the electron transfer dissociation type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes ElectronTransferDissociationType => _filterInfoStructInfo.ElectronTransferDissociationState;

	/// <summary>
	/// Gets the enhanced.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Enhanced => _filterInfoStructInfo.Enhanced;

	/// <summary>
	/// Gets the free region.
	/// </summary>
	public ScanFilterEnums.FreeRegions FreeRegion => _filterInfoStructInfo.FreeRegion;

	/// <summary>
	/// Gets the higher energy CID.
	/// </summary>
	public double HigherEnergyCid => _filterInfoStructInfo.HigherenergyCiDValue;

	/// <summary>
	/// Gets the higher energy cid type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes HigherEnergyCidType => _filterInfoStructInfo.HigherenergyCiDState;

	/// <summary>
	/// Gets the ionization mode.
	/// </summary>
	public ScanFilterEnums.IonizationModes IonizationMode => _filterInfoStructInfo.IonizationMode;

	/// <summary>
	/// Gets a value indicating whether the scan event is custom - true if trailer
	///     scan event should be used.
	/// </summary>
	public bool IsCustom => false;

	/// <summary>
	/// Gets a value indicating whether the scan event is valid.
	/// </summary>
	public bool IsValid => true;

	/// <summary>
	/// Gets the lock.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Lock => _filterInfoStructInfo.Lock;

	/// <summary>
	/// Gets the MS order.
	/// </summary>
	public ScanFilterEnums.MSOrderTypes MsOrder => _filterInfoStructInfo.MSOrder;

	/// <summary>
	/// Gets the mass analyzer type.
	/// </summary>
	public ScanFilterEnums.MassAnalyzerTypes MassAnalyzerType => _filterInfoStructInfo.MassAnalyzer;

	/// <summary>
	/// Gets the mass calibrators.
	/// </summary>
	public double[] MassCalibrators => Array.Empty<double>();

	/// <summary>
	/// Gets the multi photon dissociation.
	/// </summary>
	public double MultiPhotonDissociation => _filterInfoStructInfo.MultiPhotonDissociationValue;

	/// <summary>
	/// Gets the multi photon dissociation type.
	/// </summary>
	public ScanFilterEnums.OnAnyOffTypes MultiPhotonDissociationType => _filterInfoStructInfo.MultiPhotonDissociationState;

	/// <summary>
	/// Gets the multi state activation.
	/// </summary>
	public ScanFilterEnums.OffOnTypes MultiStateActivation => _filterInfoStructInfo.MultiStateActivation;

	/// <summary>
	/// Gets the multiplex.
	/// </summary>
	public ScanFilterEnums.OffOnTypes Multiplex => _filterInfoStructInfo.Multiplex;

	/// <summary>
	/// Gets the parameter a.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamA => _filterInfoStructInfo.ParamA;

	/// <summary>
	/// Gets the parameter b.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamB => _filterInfoStructInfo.ParamB;

	/// <summary>
	/// Gets the parameter f.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamF => _filterInfoStructInfo.ParamF;

	/// <summary>
	/// Gets the parameter k.
	/// </summary>
	public ScanFilterEnums.OffOnTypes SpsMultiNotch => _filterInfoStructInfo.SpsMultiNotch;

	/// <summary>
	/// Gets the parameter r.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamR => _filterInfoStructInfo.ParamR;

	/// <summary>
	/// Gets the parameter v.
	/// </summary>
	public ScanFilterEnums.OffOnTypes ParamV => _filterInfoStructInfo.ParamV;

	/// <summary>
	/// Gets the photo ionization.
	/// </summary>
	public ScanFilterEnums.OnOffTypes PhotoIonization => _filterInfoStructInfo.PhotoIonization;

	/// <summary>
	/// Gets the polarity.
	/// </summary>
	public ScanFilterEnums.PolarityTypes Polarity => _filterInfoStructInfo.Polarity;

	/// <summary>
	/// Gets the pulsed q dissociation.
	/// </summary>
	public double PulsedQDissociation => _filterInfoStructInfo.PulsedQDissociationValue;

	/// <summary>
	/// Gets the pulsed q dissociation type.
	/// </summary>
	public ScanFilterEnums.OnOffTypes PulsedQDissociationType => _filterInfoStructInfo.PulsedQDissociationState;

	/// <summary>
	/// Gets the reactions.
	/// </summary>
	public Reaction[] Reactions
	{
		get
		{
			Reaction[] array = new Reaction[_masses.Length];
			for (int i = 0; i < _masses.Length; i++)
			{
				array[i] = CreateReaction(i);
			}
			return array;
		}
	}

	/// <summary>
	/// Gets the scan data type.
	/// </summary>
	public ScanFilterEnums.ScanDataTypes ScanDataType => _filterInfoStructInfo.ScanData;

	/// <summary>
	/// Gets the scan type.
	/// </summary>
	public ScanFilterEnums.ScanTypes ScanType => _filterInfoStructInfo.ScanType;

	/// <summary>
	/// Gets the scan type index. Scan Type Index indicates the segment/scan event for this filter scan event.
	///     HIWORD == segment, LOWORD == scan type
	/// </summary>
	public int ScanTypeIndex => -1;

	/// <summary>
	/// Gets the sector scan.
	/// </summary>
	public ScanFilterEnums.SectorScans SectorScan => _filterInfoStructInfo.SectorScan;

	/// <summary>
	/// Gets the source fragmentation.
	/// </summary>
	public ScanFilterEnums.OnOffTypes SourceFragmentation => _filterInfoStructInfo.SourceCID;

	/// <summary>
	/// Gets the source fragmentation mass ranges.
	/// </summary>
	MassRangeStruct[] IRawFileReaderScanEvent.SourceFragmentationMassRanges => SourceFragmentationMassRanges;

	/// <summary>
	/// Gets value to indicate how source fragmentation values are interpreted.
	/// </summary>
	public ScanFilterEnums.VoltageTypes SourceFragmentationType => _filterInfoStructInfo.SourceCIDType;

	/// <summary>
	/// Gets or sets the source fragmentations.
	/// </summary>
	double[] IRawFileReaderScanEvent.SourceFragmentations
	{
		get
		{
			return SourceFragmentations;
		}
		set
		{
			SourceFragmentations = value;
		}
	}

	/// <summary>
	/// Gets the supplemental activation.
	/// </summary>
	public ScanFilterEnums.OffOnTypes SupplementalActivation => _filterInfoStructInfo.SupplementalActivation;

	/// <summary>
	/// Gets the turbo scan.
	/// </summary>
	public ScanFilterEnums.OnOffTypes TurboScan => _filterInfoStructInfo.TurboScan;

	/// <summary>
	/// Gets the ultra.
	/// </summary>
	public ScanFilterEnums.OnOffTypes Ultra => _filterInfoStructInfo.Ultra;

	/// <summary>
	/// Gets the wideband.
	/// </summary>
	public ScanFilterEnums.OffOnTypes Wideband => _filterInfoStructInfo.Wideband;

	public UpperCaseFilterFlags UpperCaseFlags { get; set; }

	public LowerCaseFilterFlags LowerCaseFlags { get; set; }

	public UpperCaseFilterFlags UpperCaseApplied { get; set; }

	public LowerCaseFilterFlags LowerCaseApplied { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Filter" /> class.
	/// </summary>
	public Filter()
	{
		Name = string.Empty;
		_masses = Array.Empty<double>();
		_precursorMassRanges = Array.Empty<MassRangeStruct>();
		_precursorMassRangesValid = Array.Empty<uint>();
		_precursorEnergies = Array.Empty<double>();
		_precursorEnergiesValid = Array.Empty<uint>();
		SourceFragmentations = Array.Empty<double>();
		SourceFragmentationInfoValid = Array.Empty<ScanFilterEnums.SourceCIDValidTypes>();
		SourceFragmentationMassRanges = Array.Empty<MassRangeStruct>();
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_filterInfoStructInfo = Utilities.ReadStructure<FilterInfoStruct>(viewer, ref startPos, fileRevision, MarshalledSizes);
		FixUpFilterDefaults(fileRevision);
		_masses = viewer.ReadDoublesExt(ref startPos);
		MassRanges = MassRangeStruct.LoadArray(viewer, ref startPos);
		if (fileRevision >= 25)
		{
			SourceFragmentations = viewer.ReadDoublesExt(ref startPos);
			SourceFragmentationMassRanges = MassRangeStruct.LoadArray(viewer, ref startPos);
		}
		if (fileRevision < 31)
		{
			_filterInfoStructInfo.SourceCIDType = ScanFilterEnums.VoltageTypes.Any;
			_filterInfoStructInfo.ScanTypeIndex = -1;
			SetNumMasses(_masses.Length);
		}
		else
		{
			_precursorEnergies = viewer.ReadDoublesExt(ref startPos);
			_precursorEnergiesValid = viewer.ReadUnsignedIntsExt(ref startPos);
			int num = viewer.ReadIntExt(ref startPos);
			SourceFragmentationInfoValid = new ScanFilterEnums.SourceCIDValidTypes[num];
			for (int i = 0; i < num; i++)
			{
				SourceFragmentationInfoValid[i] = (ScanFilterEnums.SourceCIDValidTypes)viewer.ReadIntExt(ref startPos);
			}
		}
		if (fileRevision >= 65)
		{
			Name = viewer.ReadStringExt(ref startPos);
			_precursorMassRanges = MassRangeStruct.LoadArray(viewer, ref startPos);
			_precursorMassRangesValid = viewer.ReadUnsignedIntsExt(ref startPos);
		}
		return startPos - dataOffset;
	}

	/// <summary>
	/// fix up filter defaults, for a given raw file version.
	/// </summary>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	private void FixUpFilterDefaults(int fileRevision)
	{
		if (fileRevision < 65)
		{
			_filterInfoStructInfo.Multiplex = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.ParamA = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.ParamB = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.ParamF = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.SpsMultiNotch = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.ParamR = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.ParamV = ScanFilterEnums.OffOnTypes.Any;
			Name = string.Empty;
			_precursorMassRanges = Array.Empty<MassRangeStruct>();
		}
		if (fileRevision < 63)
		{
			_filterInfoStructInfo.SupplementalActivation = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.MultiStateActivation = ScanFilterEnums.OffOnTypes.Any;
			_filterInfoStructInfo.CompensationVoltage = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.CompensationVoltageType = ScanFilterEnums.VoltageTypes.Any;
		}
		if (fileRevision < 62)
		{
			_filterInfoStructInfo.PulsedQDissociationState = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.PulsedQDissociationValue = 0.0;
			_filterInfoStructInfo.ElectronTransferDissociationState = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.ElectronTransferDissociationValue = 0.0;
			_filterInfoStructInfo.HigherenergyCiDState = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.HigherenergyCiDValue = 0.0;
		}
		if (fileRevision < 54)
		{
			_filterInfoStructInfo.MassAnalyzer = ScanFilterEnums.MassAnalyzerTypes.Any;
			_filterInfoStructInfo.SectorScan = ScanFilterEnums.SectorScans.Any;
			_filterInfoStructInfo.Lock = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.FreeRegion = ScanFilterEnums.FreeRegions.AnyFreeRegion;
			_filterInfoStructInfo.Ultra = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.Enhanced = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.MultiPhotonDissociationState = ScanFilterEnums.OnAnyOffTypes.Any;
			_filterInfoStructInfo.MultiPhotonDissociationValue = 0.0;
			_filterInfoStructInfo.ElectronCaptureDissociationState = ScanFilterEnums.OnAnyOffTypes.Any;
			_filterInfoStructInfo.ElectronCaptureDissociationValue = 0.0;
			_filterInfoStructInfo.PhotoIonization = ScanFilterEnums.OnOffTypes.Any;
		}
		if (fileRevision < 51)
		{
			_filterInfoStructInfo.AccurateMass = ScanFilterEnums.AccurateMassTypes.AcceptAnyAccurateMass;
		}
		if (fileRevision < 48)
		{
			_filterInfoStructInfo.Wideband = ScanFilterEnums.OffOnTypes.Any;
		}
		if (fileRevision < 31)
		{
			_filterInfoStructInfo.SourceCIDType = ScanFilterEnums.VoltageTypes.Any;
			_filterInfoStructInfo.ScanTypeIndex = -1;
		}
		if (fileRevision < 25)
		{
			_filterInfoStructInfo.IonizationMode = ScanFilterEnums.IonizationModes.AcceptAnyIonizationMode;
			_filterInfoStructInfo.Corona = ScanFilterEnums.OnOffTypes.Any;
			_filterInfoStructInfo.DetectorState = ScanFilterEnums.OnAnyOffTypes.Any;
			_filterInfoStructInfo.DetectorValue = 0.0;
		}
		if (fileRevision < 14)
		{
			_filterInfoStructInfo.TurboScan = ScanFilterEnums.OnOffTypes.Any;
		}
	}

	/// <summary>
	/// Sets the number of (precursor) masses.
	/// </summary>
	/// <param name="n">
	/// The number of masses.
	/// </param>
	private void SetNumMasses(int n)
	{
		int num = _precursorEnergiesValid.Length;
		Array.Resize(ref _masses, n);
		Array.Resize(ref _precursorEnergies, n);
		Array.Resize(ref _precursorEnergiesValid, n);
		for (int i = num; i < n; i++)
		{
			_precursorEnergiesValid[i] = 1u;
		}
	}

	/// <summary>
	/// Convert to automatic filter string.
	/// </summary>
	/// <param name="scanEvent">The scan event.</param>
	/// <param name="massPrecision">The mass precision.</param>
	/// <param name="charsMax">The chars maximum.</param>
	/// <param name="energyPrecision">The energy precision.</param>
	/// <param name="formatProvider">The (number) format for the current culture</param>
	/// <param name="listSeparator">list separator for localization</param>
	/// <returns>The filter string</returns>
	public string ToAutoFilterString(IRawFileReaderScanEvent scanEvent, int massPrecision = -1, int charsMax = -1, int energyPrecision = -1, IFormatProvider formatProvider = null, string listSeparator = ", ")
	{
		return ScanEvent.FormulateAutoFilterString(this, "F4", massPrecision, charsMax, energyPrecision, formatProvider ?? CultureInfo.InvariantCulture, listSeparator);
	}

	/// <summary>
	/// get the run header filter mass precision.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	public int GetRunHeaderFilterMassPrecision()
	{
		return 6;
	}

	/// <summary>
	/// create reaction.
	/// </summary>
	/// <param name="reaction">
	/// The reaction number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction" />.
	/// </returns>
	private Reaction CreateReaction(int reaction)
	{
		bool flag = _precursorMassRangesValid != null && _precursorMassRangesValid.Length > reaction && _precursorMassRangesValid[reaction] != 0;
		MassRangeStruct massRangeStruct = (flag ? _precursorMassRanges[reaction] : new MassRangeStruct(0.0, 0.0));
		uint collisionEnergyValid = _precursorEnergiesValid[reaction] ^ 1;
		return new Reaction(_masses[reaction], 1.0, _precursorEnergies[reaction], collisionEnergyValid, flag, massRangeStruct.LowMass, massRangeStruct.HighMass);
	}

	static Filter()
	{
		int[,] obj = new int[10, 2]
		{
			{ 65, 0 },
			{ 63, 0 },
			{ 62, 0 },
			{ 54, 0 },
			{ 51, 0 },
			{ 48, 0 },
			{ 31, 0 },
			{ 25, 0 },
			{ 14, 0 },
			{ 0, 0 }
		};
		obj[0, 1] = Marshal.SizeOf(typeof(FilterInfoStruct));
		obj[1, 1] = Marshal.SizeOf(typeof(FilterInfoStruct63));
		obj[2, 1] = Marshal.SizeOf(typeof(FilterInfoStruct62));
		obj[3, 1] = Marshal.SizeOf(typeof(FilterInfoStruct54));
		obj[4, 1] = Marshal.SizeOf(typeof(FilterInfoStruct51));
		obj[5, 1] = Marshal.SizeOf(typeof(FilterInfoStruct50));
		obj[6, 1] = Marshal.SizeOf(typeof(FilterInfoStruct4));
		obj[7, 1] = Marshal.SizeOf(typeof(FilterInfoStruct3));
		obj[8, 1] = Marshal.SizeOf(typeof(FilterInfoStruct2));
		obj[9, 1] = Marshal.SizeOf(typeof(FilterInfoStruct1));
		MarshalledSizes = obj;
	}
}
