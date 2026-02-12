using System;
using System.Collections.Generic;
using System.Text;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped scan filter.
/// Adds a public interface to an internal class.
/// </summary>
internal class WrappedScanFilter : IScanFilterPlus, IScanFilter, IScanEventBase, IScanEventExtended
{
	private readonly IFilterScanEvent _filterScanEvent;

	private int _numReactions;

	private int _numMassRanges;

	private int _numSrcFragInfo;

	private Reaction[] _reactions;

	private SourceFragmentationInfoValidType[] _sourceFragmentationInfoValid;

	/// <summary>
	/// Gets the filter.
	/// </summary>
	/// <value>
	/// The filter.
	/// </value>
	public IFilterScanEvent Filter => _filterScanEvent;

	/// <summary>
	/// Gets the accurate mass setting.
	/// </summary>
	public FilterAccurateMass AccurateMass => (FilterAccurateMass)_filterScanEvent.AccurateMassType;

	/// <summary>
	/// Gets the number of compensation voltage values
	/// </summary>
	/// <value>
	/// The size of compensation voltage array
	/// </value>
	public int CompensationVoltageCount { get; }

	/// <summary>
	/// Gets the number of Source Fragmentation values
	/// </summary>
	public int SourceFragmentationValueCount => SourceFragmentationInfoCount - CompensationVoltageCount;

	/// <summary>
	/// Gets or sets the locale name.
	/// This can be used to affect string conversion.
	/// </summary>
	public string LocaleName
	{
		get
		{
			return _filterScanEvent.LocaleName;
		}
		set
		{
			_filterScanEvent.LocaleName = value;
		}
	}

	/// <summary>
	/// Gets or sets the mass precision, which is used to format the filter (in ToString).
	/// </summary>
	public int MassPrecision
	{
		get
		{
			return _filterScanEvent.FilterMassPrecision;
		}
		set
		{
			_filterScanEvent.FilterMassPrecision = value;
		}
	}

	/// <summary>
	/// Gets or sets additional instrument defined filters (can be bit flags). No general definition.
	/// </summary>
	public int MetaFilters
	{
		get
		{
			return (int)_filterScanEvent.MetaFilters;
		}
		set
		{
			_filterScanEvent.MetaFilters = (MetaFilterType)value;
		}
	}

	/// <summary>
	/// Gets or sets an array of values which determines if the source fragmentation values are valid.
	/// </summary>
	public SourceFragmentationInfoValidType[] SourceFragmentationInfoValid
	{
		get
		{
			return _sourceFragmentationInfoValid;
		}
		set
		{
			_sourceFragmentationInfoValid = value;
		}
	}

	/// <summary>
	/// Gets or sets the ScanData setting.
	/// </summary>
	public ScanDataType ScanData
	{
		get
		{
			return (ScanDataType)_filterScanEvent.ScanDataType;
		}
		set
		{
			_filterScanEvent.ScanDataType = (ScanFilterEnums.ScanDataTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the Polarity setting.
	/// </summary>
	public PolarityType Polarity
	{
		get
		{
			return (PolarityType)_filterScanEvent.Polarity;
		}
		set
		{
			_filterScanEvent.Polarity = (ScanFilterEnums.PolarityTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the SourceFragmentation setting.
	/// </summary>
	public TriState SourceFragmentation
	{
		get
		{
			return (TriState)_filterScanEvent.SourceFragmentation;
		}
		set
		{
			_filterScanEvent.SourceFragmentation = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the MSOrder setting.
	/// </summary>
	public MSOrderType MSOrder
	{
		get
		{
			return (MSOrderType)_filterScanEvent.MsOrder;
		}
		set
		{
			_filterScanEvent.MsOrder = (ScanFilterEnums.MSOrderTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the Mass Analyzer setting.
	/// </summary>
	public MassAnalyzerType MassAnalyzer
	{
		get
		{
			return (MassAnalyzerType)_filterScanEvent.MassAnalyzerType;
		}
		set
		{
			_filterScanEvent.MassAnalyzerType = (ScanFilterEnums.MassAnalyzerTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the Detector setting.
	/// </summary>
	public DetectorType Detector
	{
		get
		{
			return _filterScanEvent.Detector.ToDetectorType();
		}
		set
		{
			_filterScanEvent.Detector = (ScanFilterEnums.DetectorType)value;
		}
	}

	/// <summary>
	/// Gets or sets the Dependent setting.
	/// </summary>
	public TriState Dependent
	{
		get
		{
			return _filterScanEvent.DependentDataFlag.ToTriState();
		}
		set
		{
			_filterScanEvent.DependentDataFlag = value switch
			{
				TriState.Off => ScanFilterEnums.IsDependent.No, 
				TriState.On => ScanFilterEnums.IsDependent.Yes, 
				_ => ScanFilterEnums.IsDependent.Any, 
			};
		}
	}

	/// <summary>
	/// Gets or sets the Scan Mode setting.
	/// </summary>
	public ScanModeType ScanMode
	{
		get
		{
			return (ScanModeType)_filterScanEvent.ScanType;
		}
		set
		{
			_filterScanEvent.ScanType = (ScanFilterEnums.ScanTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the Source Fragmentation Type setting.
	/// </summary>
	public SourceFragmentationValueType SourceFragmentationType
	{
		get
		{
			return (SourceFragmentationValueType)_filterScanEvent.SourceFragmentationType;
		}
		set
		{
			_filterScanEvent.SourceFragmentationType = (ScanFilterEnums.VoltageTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the Turbo Scan setting.
	/// </summary>
	public TriState TurboScan
	{
		get
		{
			return (TriState)_filterScanEvent.TurboScan;
		}
		set
		{
			_filterScanEvent.TurboScan = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the Ionization Mode setting.
	/// </summary>
	public IonizationModeType IonizationMode
	{
		get
		{
			return (IonizationModeType)_filterScanEvent.IonizationMode;
		}
		set
		{
			_filterScanEvent.IonizationMode = (ScanFilterEnums.IonizationModes)value;
		}
	}

	/// <summary>
	/// Gets or sets the corona scan setting.
	/// </summary>
	public TriState Corona
	{
		get
		{
			return (TriState)_filterScanEvent.Corona;
		}
		set
		{
			_filterScanEvent.Corona = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the detector value.
	/// </summary>
	/// <value>Floating point detector value</value>
	public double DetectorValue
	{
		get
		{
			return _filterScanEvent.DetectorValue;
		}
		set
		{
			_filterScanEvent.DetectorValue = value;
		}
	}

	/// <summary>
	/// Gets or sets the wideband scan setting.
	/// </summary>
	public TriState Wideband
	{
		get
		{
			return _filterScanEvent.Wideband.ToTriState();
		}
		set
		{
			_filterScanEvent.Wideband = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the sector scan setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.SectorScanType" /> for possible values</value>
	public SectorScanType SectorScan
	{
		get
		{
			return (SectorScanType)_filterScanEvent.SectorScan;
		}
		set
		{
			_filterScanEvent.SectorScan = (ScanFilterEnums.SectorScans)value;
		}
	}

	/// <summary>
	/// Gets or sets the lock scan setting.
	/// </summary>
	public TriState Lock
	{
		get
		{
			return (TriState)_filterScanEvent.Lock;
		}
		set
		{
			_filterScanEvent.Lock = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the field free region setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.FieldFreeRegionType" /> for possible values</value>
	public FieldFreeRegionType FieldFreeRegion
	{
		get
		{
			return (FieldFreeRegionType)_filterScanEvent.FreeRegion;
		}
		set
		{
			_filterScanEvent.FreeRegion = (ScanFilterEnums.FreeRegions)value;
		}
	}

	/// <summary>
	/// Gets or sets the ultra scan setting.
	/// </summary>
	public TriState Ultra
	{
		get
		{
			return (TriState)_filterScanEvent.Ultra;
		}
		set
		{
			_filterScanEvent.Ultra = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the enhanced scan setting.
	/// </summary>
	public TriState Enhanced
	{
		get
		{
			return (TriState)_filterScanEvent.Enhanced;
		}
		set
		{
			_filterScanEvent.Enhanced = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the multi-photon dissociation setting.
	/// </summary>
	public TriState MultiplePhotonDissociation
	{
		get
		{
			return _filterScanEvent.MultiPhotonDissociationType.ToTriState();
		}
		set
		{
			_filterScanEvent.MultiPhotonDissociationType = value.ToOnAnyOffType();
		}
	}

	/// <summary>
	/// Gets or sets the multi-photon dissociation value.
	/// </summary>
	/// <value>Floating point multi-photon dissociation value</value>
	public double MultiplePhotonDissociationValue
	{
		get
		{
			return _filterScanEvent.MultiPhotonDissociation;
		}
		set
		{
			_filterScanEvent.MultiPhotonDissociation = value;
		}
	}

	/// <summary>
	/// Gets or sets the electron capture dissociation setting.
	/// </summary>
	public TriState ElectronCaptureDissociation
	{
		get
		{
			return _filterScanEvent.ElectronCaptureDissociationType.ToTriState();
		}
		set
		{
			_filterScanEvent.ElectronCaptureDissociationType = value.ToOnAnyOffType();
		}
	}

	/// <summary>
	/// Gets or sets the electron capture dissociation value.
	/// </summary>
	/// <value>Floating point electron capture dissociation value</value>
	public double ElectronCaptureDissociationValue
	{
		get
		{
			return _filterScanEvent.ElectronCaptureDissociation;
		}
		set
		{
			_filterScanEvent.ElectronCaptureDissociation = value;
		}
	}

	/// <summary>
	/// Gets or sets the photo ionization setting.
	/// </summary>
	public TriState PhotoIonization
	{
		get
		{
			return (TriState)_filterScanEvent.PhotoIonization;
		}
		set
		{
			_filterScanEvent.PhotoIonization = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the dissociation setting.
	/// </summary>
	public TriState PulsedQDissociation
	{
		get
		{
			return (TriState)_filterScanEvent.PulsedQDissociationType;
		}
		set
		{
			_filterScanEvent.PulsedQDissociationType = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the pulsed dissociation value.
	/// </summary>
	/// <value>Floating point pulsed dissociation value</value>
	public double PulsedQDissociationValue
	{
		get
		{
			return _filterScanEvent.PulsedQDissociation;
		}
		set
		{
			_filterScanEvent.PulsedQDissociation = value;
		}
	}

	/// <summary>
	/// Gets or sets the electron transfer dissociation setting.
	/// </summary>
	public TriState ElectronTransferDissociation
	{
		get
		{
			return (TriState)_filterScanEvent.ElectronTransferDissociationType;
		}
		set
		{
			_filterScanEvent.ElectronTransferDissociationType = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the electron transfer dissociation value.
	/// </summary>
	/// <value>Floating point electron transfer dissociation value</value>
	public double ElectronTransferDissociationValue
	{
		get
		{
			return _filterScanEvent.ElectronTransferDissociation;
		}
		set
		{
			_filterScanEvent.ElectronTransferDissociation = value;
		}
	}

	/// <summary>
	/// Gets or sets the higher energy cid setting.
	/// </summary>
	public TriState HigherEnergyCiD
	{
		get
		{
			return (TriState)_filterScanEvent.HigherEnergyCidType;
		}
		set
		{
			_filterScanEvent.HigherEnergyCidType = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets the higher energy cid value.
	/// </summary>
	/// <value>Floating point higher energy cid value</value>
	public double HigherEnergyCiDValue
	{
		get
		{
			return _filterScanEvent.HigherEnergyCid;
		}
		set
		{
			_filterScanEvent.HigherEnergyCid = value;
		}
	}

	/// <summary>
	/// Gets or sets the Multiplex type
	/// </summary>
	public TriState Multiplex
	{
		get
		{
			return _filterScanEvent.Multiplex.ToTriState();
		}
		set
		{
			_filterScanEvent.Multiplex = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the parameter a.
	/// </summary>
	public TriState ParamA
	{
		get
		{
			return _filterScanEvent.ParamA.ToTriState();
		}
		set
		{
			_filterScanEvent.ParamA = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the parameter b.
	/// </summary>
	public TriState ParamB
	{
		get
		{
			return _filterScanEvent.ParamB.ToTriState();
		}
		set
		{
			_filterScanEvent.ParamB = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the parameter f.
	/// </summary>
	public TriState ParamF
	{
		get
		{
			return _filterScanEvent.ParamF.ToTriState();
		}
		set
		{
			_filterScanEvent.ParamF = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the Multi notch (Synchronous Precursor Selection) type
	/// </summary>
	public TriState MultiNotch
	{
		get
		{
			return _filterScanEvent.SpsMultiNotch.ToTriState();
		}
		set
		{
			_filterScanEvent.SpsMultiNotch = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the parameter r.
	/// </summary>
	public TriState ParamR
	{
		get
		{
			return _filterScanEvent.ParamR.ToTriState();
		}
		set
		{
			_filterScanEvent.ParamR = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the parameter v.
	/// </summary>
	public TriState ParamV
	{
		get
		{
			return _filterScanEvent.ParamV.ToTriState();
		}
		set
		{
			_filterScanEvent.ParamV = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets the event Name.
	/// </summary>
	public string Name
	{
		get
		{
			return _filterScanEvent.Name;
		}
		set
		{
			_filterScanEvent.Name = value;
		}
	}

	/// <summary>
	/// Gets or sets supplemental activation type setting.
	/// </summary>
	public TriState SupplementalActivation
	{
		get
		{
			return _filterScanEvent.SupplementalActivation.ToTriState();
		}
		set
		{
			_filterScanEvent.SupplementalActivation = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets MultiStateActivation type setting.
	/// </summary>
	public TriState MultiStateActivation
	{
		get
		{
			return _filterScanEvent.MultiStateActivation.ToTriState();
		}
		set
		{
			_filterScanEvent.MultiStateActivation = value.ToOffOnType();
		}
	}

	/// <summary>
	/// Gets or sets Compensation Voltage Option setting.
	/// </summary>
	public TriState CompensationVoltage
	{
		get
		{
			return (TriState)_filterScanEvent.CompensationVoltage;
		}
		set
		{
			_filterScanEvent.CompensationVoltage = (ScanFilterEnums.OnOffTypes)value;
		}
	}

	/// <summary>
	/// Gets or sets compensation Voltage type setting.
	/// </summary>
	/// <value>See <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.CompensationVoltageType" /> for possible values</value>
	public CompensationVoltageType CompensationVoltType
	{
		get
		{
			return (CompensationVoltageType)_filterScanEvent.CompensationVoltageType;
		}
		set
		{
			_filterScanEvent.CompensationVoltageType = (ScanFilterEnums.VoltageTypes)value;
		}
	}

	/// <summary>
	/// Gets the number of unique masses, taking into account multiple activations.
	/// </summary>
	public int UniqueMassCount
	{
		get
		{
			int num = MassCount;
			for (int i = 0; i < MassCount; i++)
			{
				if (GetIsMultipleActivation(i))
				{
					num--;
				}
			}
			return num;
		}
	}

	/// <summary>
	/// Gets the scan type index.
	/// </summary>
	public long ScanTypeIndex => _filterScanEvent.ScanTypeIndex;

	/// <summary>
	/// Gets the mass count.
	/// </summary>
	public int MassCount => _numReactions;

	/// <summary>
	/// Gets the mass range count.
	/// </summary>
	public int MassRangeCount => _numMassRanges;

	/// <summary>
	/// Gets the source fragmentation info count.
	/// </summary>
	public int SourceFragmentationInfoCount => _numSrcFragInfo;

	public LowerCaseFilterFlags LowerCaseFlags => _filterScanEvent.LowerCaseFlags;

	public UpperCaseFilterFlags UpperCaseFlags => _filterScanEvent.UpperCaseFlags;

	public LowerCaseFilterFlags AllLowerCaseFiltersApplied => _filterScanEvent.LowerCaseApplied;

	public UpperCaseFilterFlags AllUpperCaseFilterApplied => _filterScanEvent.UpperCaseApplied;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedScanFilter" /> class.
	/// </summary>
	/// <param name="filterScanEvent">
	/// The filter scan event.
	/// </param>
	public WrappedScanFilter(IFilterScanEvent filterScanEvent)
	{
		if (filterScanEvent != null)
		{
			_filterScanEvent = filterScanEvent;
			_sourceFragmentationInfoValid = new SourceFragmentationInfoValidType[0];
			_reactions = filterScanEvent.Reactions;
			Reaction[] reactions = _reactions;
			_numReactions = ((reactions != null) ? reactions.Length : 0);
			MassRangeStruct[] massRanges = filterScanEvent.MassRanges;
			_numMassRanges = ((massRanges != null) ? massRanges.Length : 0);
			double[] sourceFragmentations = filterScanEvent.SourceFragmentations;
			_numSrcFragInfo = ((sourceFragmentations != null) ? sourceFragmentations.Length : 0);
			CompensationVoltageCount = filterScanEvent.NumCompensationVoltageValues();
			int totalSourceValues = _filterScanEvent.TotalSourceValues;
			int num = filterScanEvent.NumCompensationVoltageValues();
			int num2 = filterScanEvent.NumSourceCidInfoValues();
			int num3 = 0;
			Array.Resize(ref _sourceFragmentationInfoValid, totalSourceValues);
			for (int i = 0; i < num2; i++)
			{
				_sourceFragmentationInfoValid[num3++] = _filterScanEvent.SourceCidVoltagesValid[i].ToSourceFragmentationInfoValidType();
			}
			for (int j = 0; j < num; j++)
			{
				_sourceFragmentationInfoValid[num3++] = _filterScanEvent.CompensationVoltagesValid[j].ToSourceFragmentationInfoValidType();
			}
		}
	}

	/// <summary>
	/// Retrieves a source fragmentation value at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedScanFilter.SourceFragmentationValueCount" /> to get the count of
	/// source fragmentation values.
	/// </remarks>
	/// <param name="index">
	/// Index of source fragmentation value to be retrieved
	/// </param>
	/// <returns>
	/// Source Fragmentation Value (sid) at 0-based index
	/// </returns>
	public double SourceFragmentationValue(int index)
	{
		return GetSourceFragmentationInfo(index);
	}

	/// <summary>
	/// Retrieves a compensation voltage value at 0-based index.
	/// </summary>
	/// <param name="index">
	/// Index of compensation voltage to be retrieved
	/// </param>
	/// <returns>
	/// Compensation voltage value at 0-based index
	/// </returns>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedScanFilter.CompensationVoltageCount" /> to get the count of
	/// compensation voltage values.
	/// </remarks>
	public double CompensationVoltageValue(int index)
	{
		return GetSourceFragmentationInfo(index + SourceFragmentationValueCount);
	}

	/// <summary>
	/// Get source fragmentation info valid, at zero based index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.SourceFragmentationInfoValidType" />.
	/// </returns>
	public SourceFragmentationInfoValidType GetSourceFragmentationInfoValid(int index)
	{
		if (index < 0 || index >= _sourceFragmentationInfoValid.Length)
		{
			return SourceFragmentationInfoValidType.Any;
		}
		return _sourceFragmentationInfoValid[index];
	}

	/// <summary>
	/// Convert a simple mass index to an index to the unique mass, taking into account multiple activations
	/// </summary>
	/// <param name="index">
	/// Simple mass index to convert
	/// </param>
	/// <returns>
	/// Corresponding index of the unique mass
	/// </returns>
	public int IndexToMultipleActivationIndex(int index)
	{
		int num = -1;
		int i;
		for (i = 0; i < MassCount; i++)
		{
			if (!GetIsMultipleActivation(i))
			{
				num++;
			}
			if (num >= index)
			{
				break;
			}
		}
		return i;
	}

	public IScanFilterPlus GetExtensions()
	{
		return this;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents this instance.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.String" /> that represents this instance.
	/// </returns>
	string IScanFilter.ToString()
	{
		StringBuilder metaString;
		if (MetaFilters != 0)
		{
			metaString = new StringBuilder(50);
			MetaFilterType metaFilters = (MetaFilterType)MetaFilters;
			if ((metaFilters & MetaFilterType.Hcd) != MetaFilterType.None)
			{
				AddMeta("HCD");
			}
			if ((metaFilters & MetaFilterType.Etd) != MetaFilterType.None)
			{
				AddMeta("ETD");
			}
			if ((metaFilters & MetaFilterType.Cid) != MetaFilterType.None)
			{
				AddMeta("CID");
			}
			if ((metaFilters & MetaFilterType.Uvpd) != MetaFilterType.None)
			{
				AddMeta("UVPD");
			}
			if ((metaFilters & MetaFilterType.Eid) != MetaFilterType.None)
			{
				AddMeta("EID");
			}
			if ((metaFilters & MetaFilterType.Msn) != MetaFilterType.None)
			{
				AddMeta("MSn");
				int num = metaFilters.MSnCount();
				if (num > 0)
				{
					string text = num.ToString();
					if ((metaFilters & MetaFilterType.MsndMask) != MetaFilterType.None)
					{
						text += "d";
					}
					AddMeta(text);
				}
			}
			return metaString.ToString();
		}
		return _filterScanEvent.ToString();
		void AddMeta(string s)
		{
			if (metaString.Length > 0)
			{
				metaString.Append(' ');
			}
			metaString.Append(s);
		}
	}

	/// <summary>
	/// Gets a reaction.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.IReaction" />.
	/// </returns>
	public IReaction GetReaction(int index)
	{
		return _reactions[index];
	}

	/// <summary>
	/// Sets the reaction table.
	/// </summary>
	/// <param name="reactions">
	/// The set of reactions for this filter.
	/// </param>
	public void SetReactions(IList<IReaction> reactions)
	{
		if (reactions == null)
		{
			throw new ArgumentNullException("reactions");
		}
		_reactions = new Reaction[reactions.Count];
		for (int i = 0; i < reactions.Count; i++)
		{
			_reactions[i] = new Reaction(reactions[i]);
		}
		_numReactions = _reactions.Length;
	}

	/// <summary>
	/// The get mass.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Double" />.
	/// </returns>
	public double GetMass(int index)
	{
		return _reactions[index].PrecursorMass;
	}

	/// <summary>
	/// The get energy.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Double" />.
	/// </returns>
	public double GetEnergy(int index)
	{
		return _reactions[index].CollisionEnergy;
	}

	/// <summary>
	/// Get the precursor range validity.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// true if this item is valid.
	/// </returns>
	public bool GetPrecursorRangeValidity(int index)
	{
		return _reactions[index].PrecursorRangeIsValid;
	}

	/// <summary>
	/// Get the first precursor mass, at a given index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The first precursor mass for the selected index.
	/// </returns>
	public double GetFirstPrecursorMass(int index)
	{
		return _reactions[index].FirstPrecursorMass;
	}

	/// <summary>
	/// Get the last precursor mass, at a given index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The last precursor mass for the selected index.
	/// </returns>
	public double GetLastPrecursorMass(int index)
	{
		return _reactions[index].LastPrecursorMass;
	}

	/// <summary>
	/// The get isolation width.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Double" />.
	/// </returns>
	public double GetIsolationWidth(int index)
	{
		return _reactions[index].IsolationWidth;
	}

	/// <summary>
	/// Get the isolation width offset.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The isolation width at the requested index.
	/// </returns>
	public double GetIsolationWidthOffset(int index)
	{
		return _reactions[index].IsolationWidthOffset;
	}

	/// <summary>
	/// The get energy valid.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.EnergyType" />.
	/// </returns>
	public EnergyType GetEnergyValid(int index)
	{
		if (!_reactions[index].CollisionEnergyValid)
		{
			return EnergyType.Any;
		}
		return EnergyType.Valid;
	}

	/// <summary>
	/// The get activation.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.FilterEnums.ActivationType" />.
	/// </returns>
	public ActivationType GetActivation(int index)
	{
		return _reactions[index].ActivationType;
	}

	/// <summary>
	/// The get is multiple activation.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool GetIsMultipleActivation(int index)
	{
		return _reactions[index].MultipleActivation;
	}

	/// <summary>
	/// The get mass range.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IRangeAccess" />.
	/// </returns>
	public IRangeAccess GetMassRange(int index)
	{
		MassRangeStruct massRangeStruct = _filterScanEvent.MassRanges[index];
		return RangeFactory.Create(massRangeStruct.LowMass, massRangeStruct.HighMass);
	}

	/// <summary>
	/// Set the state of a lower case flag.
	/// On: Flag applied On
	/// Off: Flag applied Off
	/// Any: Flag not used (not applied and off)
	/// </summary>
	/// <param name="flag">The lower case flag to set</param>
	/// <param name="value">New value of the flag</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	public void SetLowerFlag(LowerCaseFilterFlags flag, TriState value)
	{
		switch (value)
		{
		case TriState.Off:
			_filterScanEvent.LowerCaseFlags &= ~flag;
			_filterScanEvent.LowerCaseApplied |= flag;
			break;
		case TriState.On:
			_filterScanEvent.LowerCaseFlags |= flag;
			_filterScanEvent.LowerCaseApplied |= flag;
			break;
		case TriState.Any:
			_filterScanEvent.LowerCaseFlags &= ~flag;
			_filterScanEvent.LowerCaseApplied &= ~flag;
			break;
		default:
			throw new ArgumentOutOfRangeException("value", value, null);
		}
	}

	/// <summary>
	/// Get the state of a lower case flag, from the lower case flag bits and the active flag bits.
	/// If it is not active, return "Any"
	/// </summary>
	/// <param name="flag">The lower case flag to check</param>
	/// <returns>current values of the flag</returns>
	public TriState GetLowerCaseFlag(LowerCaseFilterFlags flag)
	{
		if ((_filterScanEvent.LowerCaseApplied & flag) == 0)
		{
			return TriState.Any;
		}
		if ((_filterScanEvent.LowerCaseFlags & flag) == 0)
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
	/// <param name="flag">The upper case flag to set</param>
	/// <param name="value">New value of the flag</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
	public void SetUpperFlag(UpperCaseFilterFlags flag, TriState value)
	{
		switch (value)
		{
		case TriState.Off:
			_filterScanEvent.UpperCaseFlags &= ~flag;
			_filterScanEvent.UpperCaseApplied |= flag;
			break;
		case TriState.On:
			_filterScanEvent.UpperCaseFlags |= flag;
			_filterScanEvent.UpperCaseApplied |= flag;
			break;
		case TriState.Any:
			_filterScanEvent.UpperCaseFlags &= ~flag;
			_filterScanEvent.UpperCaseApplied &= ~flag;
			break;
		default:
			throw new ArgumentOutOfRangeException("value", value, null);
		}
	}

	/// <summary>
	/// Get the state of a upper case flag, from the upper case flag bits
	/// and the active upper case flag bits.
	/// If it is not active, return "Any"
	/// </summary>
	/// <param name="flag">Flag who's state is needed</param>
	/// <returns>State of the requested flag</returns>
	public TriState GetUpperCaseFlag(UpperCaseFilterFlags flag)
	{
		if ((_filterScanEvent.UpperCaseApplied & flag) == 0)
		{
			return TriState.Any;
		}
		if ((_filterScanEvent.UpperCaseFlags & flag) == 0)
		{
			return TriState.Off;
		}
		return TriState.On;
	}

	/// <summary>
	/// Set the table of mass ranges
	/// </summary>
	/// <param name="ranges">new ranges</param>
	/// <exception cref="T:System.ArgumentNullException">Thrown on null parameter</exception>
	public void SetMassRanges(IList<IRangeAccess> ranges)
	{
		if (ranges == null)
		{
			throw new ArgumentNullException("ranges");
		}
		MassRangeStruct[] array = new MassRangeStruct[ranges.Count];
		for (int i = 0; i < ranges.Count; i++)
		{
			array[i] = new MassRangeStruct(ranges[i]);
		}
		_filterScanEvent.MassRanges = array;
		_numMassRanges = array.Length;
	}

	/// <summary>
	/// The get source fragmentation info.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The source fragmentation value at the given index.
	/// </returns>
	public double GetSourceFragmentationInfo(int index)
	{
		return _filterScanEvent.SourceFragmentations[index];
	}

	/// <summary>
	/// Gets extended properties
	/// </summary>
	/// <returns>extended methods</returns>
	IScanEventExtended IScanEventBase.GetExtensions()
	{
		return this;
	}
}
