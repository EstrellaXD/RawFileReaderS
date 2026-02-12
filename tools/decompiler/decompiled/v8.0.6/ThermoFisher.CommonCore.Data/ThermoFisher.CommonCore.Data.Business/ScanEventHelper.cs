using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class adds extended features to the Scan Event interface.
/// It acts as a "helper class" rather than "extension methods"
/// so that it can hold certain state information.
/// A common use case is:
/// Scan event information is read from a raw file. 
/// Multiple uses are made of this
/// (for example, testing if this scan should participate in 1000s chromatograms).
/// This class can organize information from the scan event,
/// such that "filter comparisons" or other work with events and filters
/// are more efficient. There is a very small overhead in constructing this object,
/// which still keeps the original interface reference in its construction factory.
/// The class is designed for thread safe testing.
/// TestScanAgainstFilter or TestScanAgainstNames may be called on multiple threads.
/// </summary>
public class ScanEventHelper
{
	/// <summary>
	/// The match tolerance for comparing two energy values
	/// </summary>
	private const double FilterEnergyTolerance = 0.01;

	private const int SfscanTypeNotSpecified = -1;

	/// <summary>
	/// The instrument mass resolution (used in determining if mass ranges match up or not)
	/// </summary>
	private const double DefaultFilterMassResolution = 0.4;

	private readonly IScanEventBase _baseEvent;

	private readonly bool _createdFromFilter;

	private readonly IScanEvent _scanEvent;

	private readonly IScanFilter _scanFilter;

	private readonly bool _dependentOn;

	private readonly TriState _dependent;

	private readonly int _scanSegmentNumber;

	private readonly int _scanEventNumber;

	private readonly bool _isFixed;

	private volatile bool _isInitialized;

	private MSOrderType _orderType;

	private bool _specificOrder;

	private TriState _multiplex;

	private bool _dependentOff;

	private bool _multiplexNotOff;

	private bool _multiplexIsOn;

	private int _reactions;

	private bool _singleReaction;

	/// <summary>
	/// Gets the scan event which was used to construct this object.
	/// If the object was created from IScanFilter, this will be null
	/// </summary>
	public IScanEvent ScanEvent => _scanEvent;

	/// <summary>
	/// Gets or sets the reactions.
	/// </summary>
	private IList<IReaction> Reactions { get; set; }

	/// <summary>
	/// Test if two tri-states are different.
	/// </summary>
	/// <param name="filterState">
	/// The state 1.
	/// </param>
	/// <param name="eventState">
	/// The state 2.
	/// </param>
	/// <returns>
	/// True if "filter failed to match"
	/// </returns>
	private bool DifferentTriState(TriState filterState, TriState eventState)
	{
		if (_createdFromFilter)
		{
			return filterState switch
			{
				TriState.On => eventState != TriState.On, 
				TriState.Off => eventState == TriState.On, 
				_ => false, 
			};
		}
		if (filterState != TriState.Any && eventState != TriState.Any)
		{
			return filterState != eventState;
		}
		return false;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventHelper" /> class.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	/// <param name="eventCode">Event code from scan index, -1 (default) if not known</param>
	private ScanEventHelper(IScanEvent scanEvent, int eventCode = -1)
	{
		_baseEvent = scanEvent;
		_scanEvent = scanEvent;
		_dependent = _baseEvent.Dependent;
		_dependentOn = _dependent == TriState.On;
		if (eventCode == -1)
		{
			_isFixed = false;
			return;
		}
		_isFixed = !_dependentOn && !scanEvent.IsCustom;
		_scanSegmentNumber = eventCode >> 16;
		_scanEventNumber = eventCode & 0xFFFF;
	}

	private ScanEventHelper(IScanFilter scanFilter)
	{
		_baseEvent = scanFilter;
		_scanFilter = scanFilter;
		_createdFromFilter = true;
		_dependent = _baseEvent.Dependent;
		_dependentOn = _dependent == TriState.On;
		_isFixed = false;
	}

	/// <summary>
	/// Thread safe initialize
	/// </summary>
	private void Initialize()
	{
		lock (_baseEvent)
		{
			if (!_isInitialized)
			{
				_dependentOff = _dependent == TriState.Off;
				_orderType = _baseEvent.MSOrder;
				_specificOrder = _orderType != MSOrderType.Any;
				_multiplex = _baseEvent.Multiplex;
				_multiplexNotOff = _multiplex != TriState.Off;
				_multiplexIsOn = _multiplex == TriState.On;
				CreateReactions();
				_isInitialized = true;
			}
		}
	}

	/// <summary>
	/// create reactions table
	/// </summary>
	private void CreateReactions()
	{
		_reactions = _baseEvent.MassCount;
		_singleReaction = _reactions == 1;
		IReaction[] array = new IReaction[_reactions];
		for (int i = 0; i < _reactions; i++)
		{
			array[i] = _baseEvent.GetReaction(i);
		}
		Reactions = array;
	}

	/// <summary>
	/// The scan event helper factory creates a ScanEventHelper from a scanEvent interface.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	/// <param name="eventCode">event code (form scan index). -1 for "not known"</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventHelper" />.
	/// </returns>
	public static ScanEventHelper ScanEventHelperFactory(IScanEvent scanEvent, int eventCode = -1)
	{
		return new ScanEventHelper(scanEvent, eventCode);
	}

	/// <summary>
	/// This scan event helper factory creates a ScanEventHelper from an IScanFilter interface.
	/// This constructor permits an application to test if a scan filter contains another filter.
	/// The contructed object from this method is considered a "scan" and another scan filter can be tested against it,
	/// using "TestScan". Example: Crreate this from "+ d". You can test that "+" or "d" filters match this
	/// (return true from TestScanAgainstFilter).
	/// </summary>
	/// <param name="scanFilter">
	/// The scan filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventHelper" />.
	/// </returns>
	public static ScanEventHelper ScanEventHelperFactory(IScanFilter scanFilter)
	{
		return new ScanEventHelper(scanFilter);
	}

	/// <summary>
	/// Test if activation is set for any reaction.
	/// </summary>
	/// <param name="activationType">
	/// The activation type.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool IsActivationSetForAnyReaction(ActivationType activationType)
	{
		for (int i = 0; i < _reactions; i++)
		{
			if (Reactions[i].ActivationType == activationType)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Test scan against component or compound names.
	/// </summary>
	/// <param name="names">
	/// The names.
	/// </param>
	/// <returns>
	/// True if the scan should be included
	/// </returns>
	public bool TestScanAgainstNames(IList<string> names)
	{
		string thisName = _baseEvent.Name;
		if (names != null && names.Count > 0)
		{
			return names.Where((string name) => !string.IsNullOrEmpty(name)).Any((string name) => thisName == name);
		}
		return true;
	}

	/// <summary>
	/// Test that this scan (event) passes the supplied scan filter.
	/// </summary>
	/// <param name="filterHelper">
	/// The filter Helper.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool TestScanAgainstFilter(ScanFilterHelper filterHelper)
	{
		if (_isFixed && filterHelper.KeepMatchedEventHistory)
		{
			ScanFilterHelper.EventTested eventTested;
			if ((eventTested = filterHelper.IsTestedAgainst(_scanSegmentNumber, _scanEventNumber)) != ScanFilterHelper.EventTested.EventNotTested)
			{
				return eventTested == ScanFilterHelper.EventTested.TestedAndPassed;
			}
			bool result = TestScan(filterHelper);
			filterHelper.AddMatchResult(result, _scanSegmentNumber, _scanEventNumber);
			return result;
		}
		return TestScan(filterHelper);
	}

	/// <summary>
	/// test a scan against a filter.
	/// </summary>
	/// <param name="filterHelper">
	/// The filter helper.
	/// </param>
	/// <returns>
	/// true, if the scan passes the filter
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown on invalid filter rule
	/// </exception>
	private bool TestScan(ScanFilterHelper filterHelper)
	{
		if (!_isInitialized)
		{
			Initialize();
		}
		IScanFilter filter = filterHelper.Filter;
		IScanFilterPlus scanFilterPlus = filter as IScanFilterPlus;
		IScanEventExtended extensions = _baseEvent.GetExtensions();
		bool flag = extensions != null && scanFilterPlus != null;
		FilterRule[] rules = filterHelper.Rules;
		int num = rules.Length;
		for (int i = 0; i < num; i++)
		{
			switch (rules[i])
			{
			case FilterRule.MetaFilter:
				return TestScanAgainstMetaFilter(filter);
			case FilterRule.DependentOn:
				if (_scanFilter != null)
				{
					if (!_dependentOn)
					{
						return false;
					}
				}
				else if (_dependentOff)
				{
					return false;
				}
				break;
			case FilterRule.DependentOff:
				if (_dependentOn)
				{
					return false;
				}
				break;
			case FilterRule.SupplementalActivation:
				if (DifferentTriState(filter.SupplementalActivation, _baseEvent.SupplementalActivation))
				{
					return false;
				}
				break;
			case FilterRule.MultiStateActivation:
				if (DifferentTriState(filter.MultiStateActivation, _baseEvent.MultiStateActivation))
				{
					return false;
				}
				break;
			case FilterRule.Wideband:
				if (DifferentTriState(filter.Wideband, _baseEvent.Wideband))
				{
					return false;
				}
				break;
			case FilterRule.Polarity:
				if (_baseEvent.Polarity != PolarityType.Any && filter.Polarity != _baseEvent.Polarity)
				{
					return false;
				}
				break;
			case FilterRule.ScanData:
				if (_baseEvent.ScanData != ScanDataType.Any && filter.ScanData != _baseEvent.ScanData)
				{
					return false;
				}
				break;
			case FilterRule.IonizationMode:
				if (_baseEvent.IonizationMode != IonizationModeType.Any && filter.IonizationMode != _baseEvent.IonizationMode)
				{
					return false;
				}
				break;
			case FilterRule.Corona:
				if (DifferentTriState(filter.Corona, _baseEvent.Corona))
				{
					return false;
				}
				break;
			case FilterRule.Lock:
				if (DifferentTriState(filter.Lock, _baseEvent.Lock))
				{
					return false;
				}
				break;
			case FilterRule.FieldFreeRegion:
				if (_baseEvent.FieldFreeRegion != FieldFreeRegionType.Any && filter.FieldFreeRegion != _baseEvent.FieldFreeRegion)
				{
					return false;
				}
				break;
			case FilterRule.Ultra:
				if (DifferentTriState(filter.Ultra, _baseEvent.Ultra))
				{
					return false;
				}
				break;
			case FilterRule.Enhanced:
				if (DifferentTriState(filter.Enhanced, _baseEvent.Enhanced))
				{
					return false;
				}
				break;
			case FilterRule.ParamA:
				if (DifferentTriState(filter.ParamA, _baseEvent.ParamA))
				{
					return false;
				}
				break;
			case FilterRule.ParamB:
				if (DifferentTriState(filter.ParamB, _baseEvent.ParamB))
				{
					return false;
				}
				break;
			case FilterRule.ParamF:
				if (DifferentTriState(filter.ParamF, _baseEvent.ParamF))
				{
					return false;
				}
				break;
			case FilterRule.MultiNotch:
				if (DifferentTriState(filter.MultiNotch, _baseEvent.MultiNotch))
				{
					return false;
				}
				break;
			case FilterRule.MultiplePhotonDissociation:
				if (!TestMultiPhotonDissociation(filter))
				{
					return false;
				}
				break;
			case FilterRule.ParamV:
				if (DifferentTriState(filter.ParamV, _baseEvent.ParamV))
				{
					return false;
				}
				break;
			case FilterRule.ParamR:
				if (DifferentTriState(filter.ParamR, _baseEvent.ParamR))
				{
					return false;
				}
				break;
			case FilterRule.ElectronCaptureDissociation:
				if (!TestElectronCaptureDissociation(filter))
				{
					return false;
				}
				break;
			case FilterRule.PhotoIonization:
				if (DifferentTriState(filter.PhotoIonization, _baseEvent.PhotoIonization))
				{
					return false;
				}
				break;
			case FilterRule.SourceFragmentation:
				if (DifferentTriState(filter.SourceFragmentation, _baseEvent.SourceFragmentation))
				{
					return false;
				}
				break;
			case FilterRule.SourceFragmentationType:
				if (!TestAgainstSourceFragmentation(filter))
				{
					return false;
				}
				break;
			case FilterRule.CompensationVoltage:
				if (DifferentTriState(filter.CompensationVoltage, _baseEvent.CompensationVoltage))
				{
					return false;
				}
				break;
			case FilterRule.CompensationVoltType:
				if (!TestAgainstCompensationVoltage(filter))
				{
					return false;
				}
				break;
			case FilterRule.Detector:
				if (_baseEvent.Detector != DetectorType.Any && (filter.Detector != _baseEvent.Detector || !(Math.Abs(filter.DetectorValue - _baseEvent.DetectorValue) <= 0.01)))
				{
					return false;
				}
				break;
			case FilterRule.MassAnalyzerType:
				if (_baseEvent.MassAnalyzer != MassAnalyzerType.Any && filter.MassAnalyzer != _baseEvent.MassAnalyzer)
				{
					return false;
				}
				break;
			case FilterRule.SectorScan:
				if (_baseEvent.SectorScan != SectorScanType.Any && filter.SectorScan != _baseEvent.SectorScan)
				{
					return false;
				}
				break;
			case FilterRule.TurboScan:
				if (DifferentTriState(filter.TurboScan, _baseEvent.TurboScan))
				{
					return false;
				}
				break;
			case FilterRule.ScanMode:
				if (_baseEvent.ScanMode != ScanModeType.Any && filter.ScanMode != _baseEvent.ScanMode && (filter.ScanMode != ScanModeType.Full || (_baseEvent.ScanMode != ScanModeType.Q1Ms && _baseEvent.ScanMode != ScanModeType.Q3Ms)))
				{
					return false;
				}
				break;
			case FilterRule.Multiplex:
				if (DifferentTriState(filter.Multiplex, _multiplex))
				{
					return false;
				}
				if ((filterHelper.IsMultiplex && _multiplex == TriState.Off) || (_multiplexIsOn && filter.Multiplex == TriState.Off))
				{
					return false;
				}
				if (filterHelper.IsMultiplex && (_orderType < MSOrderType.Any || _orderType > MSOrderType.Ms2))
				{
					return false;
				}
				break;
			case FilterRule.MsOrder:
				if (_specificOrder)
				{
					if (filterHelper.MsOrder != _orderType)
					{
						return false;
					}
					if (filterHelper.IsSpecialOrder && !CheckParentMasses(filter, filterHelper))
					{
						return false;
					}
				}
				break;
			case FilterRule.ScanTypeIndex:
				if (_baseEvent.ScanTypeIndex != -1 && filter.ScanTypeIndex != _baseEvent.ScanTypeIndex)
				{
					return false;
				}
				break;
			case FilterRule.AccurateMass:
				if (_scanEvent != null)
				{
					if ((filter.AccurateMass != FilterAccurateMass.Off || _scanEvent.AccurateMass != EventAccurateMass.Off) && (_scanEvent.AccurateMass != EventAccurateMass.Internal || (filter.AccurateMass != FilterAccurateMass.On && filter.AccurateMass != FilterAccurateMass.Internal)) && (_scanEvent.AccurateMass != EventAccurateMass.External || (filter.AccurateMass != FilterAccurateMass.On && filter.AccurateMass != FilterAccurateMass.External)))
					{
						return false;
					}
				}
				else if (_scanFilter != null && (filter.AccurateMass != FilterAccurateMass.Off || _scanFilter.AccurateMass != FilterAccurateMass.Off) && (_scanFilter.AccurateMass != FilterAccurateMass.Internal || (filter.AccurateMass != FilterAccurateMass.On && filter.AccurateMass != FilterAccurateMass.Internal)) && (_scanFilter.AccurateMass != FilterAccurateMass.External || (filter.AccurateMass != FilterAccurateMass.On && filter.AccurateMass != FilterAccurateMass.External)))
				{
					return false;
				}
				break;
			case FilterRule.LowerE:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerE))
				{
					return false;
				}
				break;
			case FilterRule.LowerG:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerG))
				{
					return false;
				}
				break;
			case FilterRule.LowerH:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerH))
				{
					return false;
				}
				break;
			case FilterRule.LowerI:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerI))
				{
					return false;
				}
				break;
			case FilterRule.LowerJ:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerJ))
				{
					return false;
				}
				break;
			case FilterRule.LowerK:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerK))
				{
					return false;
				}
				break;
			case FilterRule.LowerL:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerL))
				{
					return false;
				}
				break;
			case FilterRule.LowerM:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerM))
				{
					return false;
				}
				break;
			case FilterRule.LowerN:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerN))
				{
					return false;
				}
				break;
			case FilterRule.LowerO:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerO))
				{
					return false;
				}
				break;
			case FilterRule.LowerQ:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerQ))
				{
					return false;
				}
				break;
			case FilterRule.LowerS:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerS))
				{
					return false;
				}
				break;
			case FilterRule.LowerX:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerX))
				{
					return false;
				}
				break;
			case FilterRule.LowerY:
				if (flag && DifferentTriState(scanFilterPlus, extensions, LowerCaseFilterFlags.LowerI))
				{
					return false;
				}
				break;
			case FilterRule.UpperA:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperA))
				{
					return false;
				}
				break;
			case FilterRule.UpperB:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperB))
				{
					return false;
				}
				break;
			case FilterRule.UpperF:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperF))
				{
					return false;
				}
				break;
			case FilterRule.UpperG:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperG))
				{
					return false;
				}
				break;
			case FilterRule.UpperH:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperH))
				{
					return false;
				}
				break;
			case FilterRule.UpperI:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperI))
				{
					return false;
				}
				break;
			case FilterRule.UpperJ:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperJ))
				{
					return false;
				}
				break;
			case FilterRule.UpperK:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperK))
				{
					return false;
				}
				break;
			case FilterRule.UpperL:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperL))
				{
					return false;
				}
				break;
			case FilterRule.UpperM:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperM))
				{
					return false;
				}
				break;
			case FilterRule.UpperN:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperN))
				{
					return false;
				}
				break;
			case FilterRule.UpperO:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperO))
				{
					return false;
				}
				break;
			case FilterRule.UpperQ:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperQ))
				{
					return false;
				}
				break;
			case FilterRule.UpperR:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperR))
				{
					return false;
				}
				break;
			case FilterRule.UpperS:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperS))
				{
					return false;
				}
				break;
			case FilterRule.UpperT:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperT))
				{
					return false;
				}
				break;
			case FilterRule.UpperU:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperU))
				{
					return false;
				}
				break;
			case FilterRule.UpperV:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperV))
				{
					return false;
				}
				break;
			case FilterRule.UpperW:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperW))
				{
					return false;
				}
				break;
			case FilterRule.UpperX:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperX))
				{
					return false;
				}
				break;
			case FilterRule.UpperY:
				if (flag && DifferentTriState(scanFilterPlus, extensions, UpperCaseFilterFlags.UpperY))
				{
					return false;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		return CheckMassRanges();
		bool CheckMassRanges()
		{
			bool flag2 = filter.MassRangeCount == 0;
			if (!flag2 && filter.MassRangeCount == _baseEvent.MassRangeCount)
			{
				bool flag3 = true;
				double num2 = filterHelper.MassResolution;
				if (filterHelper.IsAccurateResolution)
				{
					num2 = ((!filterHelper.IsDependent && !_dependentOn) ? (num2 * 1.001) : 0.4);
				}
				for (int j = 0; j < filter.MassRangeCount && flag3; j++)
				{
					IRangeAccess massRange = filter.GetMassRange(j);
					IRangeAccess massRange2 = _baseEvent.GetMassRange(j);
					if (Math.Abs(massRange.Low - massRange2.Low) > num2 / 2.0 || Math.Abs(massRange.High - massRange2.High) > num2 / 2.0)
					{
						flag3 = false;
					}
				}
				flag2 = flag3;
			}
			if (!flag2 && filter.MassRangeCount != _baseEvent.MassRangeCount)
			{
				bool flag4 = true;
				double num3 = ((!filterHelper.IsAccurateResolution) ? filterHelper.MassResolution : ((!filterHelper.IsDependent && !_dependentOn) ? (filterHelper.MassResolution * 1.001) : 0.4));
				for (int k = 0; k < filter.MassRangeCount && flag4; k++)
				{
					IRangeAccess massRange3 = filter.GetMassRange(k);
					bool flag5 = false;
					for (int l = 0; l < _baseEvent.MassRangeCount; l++)
					{
						if (flag5)
						{
							break;
						}
						IRangeAccess massRange4 = _baseEvent.GetMassRange(l);
						if (Math.Abs(massRange3.Low - massRange4.Low) <= num3 / 2.0 || Math.Abs(massRange3.High - massRange4.High) <= num3 / 2.0)
						{
							flag5 = true;
						}
					}
					if (!flag5)
					{
						flag4 = false;
					}
				}
				flag2 = flag4;
			}
			return flag2;
		}
	}

	private bool DifferentTriState(IScanFilterPlus filterState, IScanEventExtended eventState, LowerCaseFilterFlags lowerFlag)
	{
		return DifferentTriState(filterState.GetLowerCaseFlag(lowerFlag), eventState.GetLowerCaseFlag(lowerFlag));
	}

	private bool DifferentTriState(IScanFilterPlus filterState, IScanEventExtended eventState, UpperCaseFilterFlags upperFlag)
	{
		return DifferentTriState(filterState.GetUpperCaseFlag(upperFlag), eventState.GetUpperCaseFlag(upperFlag));
	}

	/// <summary>
	/// Test electron capture dissociation.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool TestElectronCaptureDissociation(IScanFilter filter)
	{
		if (filter.ElectronCaptureDissociation != TriState.Any && _baseEvent.ElectronCaptureDissociation != TriState.Any)
		{
			if (filter.ElectronCaptureDissociation == _baseEvent.ElectronCaptureDissociation)
			{
				return Math.Abs(filter.ElectronCaptureDissociationValue - _baseEvent.ElectronCaptureDissociationValue) <= 0.01;
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Test multi photon dissociation.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool TestMultiPhotonDissociation(IScanFilter filter)
	{
		if (_baseEvent.MultiplePhotonDissociation != TriState.Any && (filter.MultiplePhotonDissociation != _baseEvent.MultiplePhotonDissociation || !(Math.Abs(filter.MultiplePhotonDissociationValue - _baseEvent.MultiplePhotonDissociationValue) <= 0.01)))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Test against compensation voltage.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool TestAgainstCompensationVoltage(IScanFilter filter)
	{
		if (_baseEvent.CompensationVoltType != CompensationVoltageType.Any && filter.CompensationVoltType != _baseEvent.CompensationVoltType)
		{
			return false;
		}
		if (filter.CompensationVoltType != CompensationVoltageType.NoValue)
		{
			int num = 0;
			switch (filter.CompensationVoltType)
			{
			case CompensationVoltageType.SingleValue:
				num = 1;
				break;
			case CompensationVoltageType.Ramp:
				num = 2;
				break;
			case CompensationVoltageType.SIM:
				num = filter.MassRangeCount;
				break;
			}
			int num2 = 0;
			if (_baseEvent.SourceFragmentation == TriState.On)
			{
				switch (_baseEvent.SourceFragmentationType)
				{
				case SourceFragmentationValueType.SingleValue:
					num2 = 1;
					break;
				case SourceFragmentationValueType.Ramp:
					num2 = 2;
					break;
				}
				if (num + num2 > _baseEvent.SourceFragmentationInfoCount)
				{
					num2 = 0;
				}
			}
			for (int i = 0; i < num; i++)
			{
				if (NumCompensationVoltageValues() > i && filter.CompensationVoltageValueIsValid(i) && Math.Abs(filter.CompensationVoltageValue(i) - CompensationVoltageValue(i, num2)) > 0.01)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Test against source fragmentation.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool TestAgainstSourceFragmentation(IScanFilter filter)
	{
		if (_baseEvent.SourceFragmentationType != SourceFragmentationValueType.Any && filter.SourceFragmentationType != _baseEvent.SourceFragmentationType)
		{
			return false;
		}
		if (filter.SourceFragmentationType != SourceFragmentationValueType.NoValue)
		{
			int num = 0;
			switch (filter.SourceFragmentationType)
			{
			case SourceFragmentationValueType.SingleValue:
				num = 1;
				break;
			case SourceFragmentationValueType.Ramp:
				num = 2;
				break;
			case SourceFragmentationValueType.SIM:
				num = filter.MassRangeCount;
				break;
			}
			for (int i = 0; i < num; i++)
			{
				if (filter.GetSourceFragmentationInfoValid(i) == SourceFragmentationInfoValidType.Energy && _baseEvent.SourceFragmentationInfoCount > i && Math.Abs(filter.GetSourceFragmentationInfo(i) - _baseEvent.GetSourceFragmentationInfo(i)) > 0.01)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Test scan against meta filter.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool TestScanAgainstMetaFilter(IScanFilter filter)
	{
		MetaFilterType metaFilters = (MetaFilterType)filter.MetaFilters;
		bool flag = false;
		bool flag2 = false;
		if (_orderType >= MSOrderType.Ms2)
		{
			if ((metaFilters & MetaFilterType.Hcd) != MetaFilterType.None)
			{
				flag2 = true;
				flag |= IsActivationSetForAnyReaction(ActivationType.HigherEnergyCollisionalDissociation);
			}
			if ((metaFilters & MetaFilterType.Etd) != MetaFilterType.None)
			{
				flag2 = true;
				flag |= IsActivationSetForAnyReaction(ActivationType.ElectronTransferDissociation);
			}
			if ((metaFilters & MetaFilterType.Cid) != MetaFilterType.None)
			{
				flag2 = true;
				flag |= IsActivationSetForAnyReaction(ActivationType.CollisionInducedDissociation);
			}
			if ((metaFilters & MetaFilterType.Uvpd) != MetaFilterType.None)
			{
				flag2 = true;
				flag |= IsActivationSetForAnyReaction(ActivationType.UltraVioletPhotoDissociation);
			}
			if ((metaFilters & MetaFilterType.Eid) != MetaFilterType.None)
			{
				flag2 = true;
				flag |= IsActivationSetForAnyReaction(ActivationType.ElectronInducedDissociation);
			}
			if ((metaFilters & MetaFilterType.Msn) == 0)
			{
				return flag;
			}
			int num = metaFilters.MSnCount();
			if ((metaFilters & MetaFilterType.MsndMask) != MetaFilterType.None && !_dependentOn)
			{
				return false;
			}
			if (num <= 2 || (int)_orderType >= num)
			{
				return !flag2 || flag;
			}
		}
		return false;
	}

	/// <summary>
	/// Check the parent masses.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <param name="helper">
	/// The filter helper.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool CheckParentMasses(IScanFilter filter, ScanFilterHelper helper)
	{
		if ((_multiplexNotOff && helper.IsMultiplex) || (_multiplexIsOn && filter.Multiplex != TriState.Off))
		{
			if (helper.HasMasses && !MultiplexPrecursorConditionsMatch(filter, helper))
			{
				return false;
			}
		}
		else if (helper.HasMasses)
		{
			int multiActivationCorrectedMassCount = helper.MultiActivationCorrectedMassCount;
			if (multiActivationCorrectedMassCount > _reactions)
			{
				return false;
			}
			for (int i = 0; i < multiActivationCorrectedMassCount; i++)
			{
				if (!PrecursorConditionsMatch(filter, helper, i))
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Get the compensation voltage value.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="sourceFragmentations">The number of source fragmentation values,
	/// (which are before CV values in the table)</param>
	/// <returns>
	/// The <see cref="T:System.Double" />.
	/// </returns>
	private double CompensationVoltageValue(int index, int sourceFragmentations)
	{
		return _baseEvent.GetSourceFragmentationInfo(index + sourceFragmentations);
	}

	/// <summary>
	/// Test of the multiplex precursor conditions match.
	/// </summary>
	/// <param name="scanFilter">
	/// The scan filter.
	/// </param>
	/// <param name="helper">
	/// The filter helper.
	/// </param>
	/// <returns>
	/// True if matched
	/// </returns>
	private bool MultiplexPrecursorConditionsMatch(IScanFilter scanFilter, ScanFilterHelper helper)
	{
		double num = ((!helper.IsAccurateResolution) ? helper.MassResolution : ((!helper.IsDependent && !_dependentOn) ? helper.MassResolution : 0.4));
		if (scanFilter.Multiplex != TriState.On && (helper.MsOrder < MSOrderType.Any || helper.MsOrder > MSOrderType.Ms2))
		{
			return false;
		}
		int num2 = scanFilter.NumMassesEx();
		if (num2 == scanFilter.MassCount && scanFilter.MassCount == 1 && _singleReaction)
		{
			IReaction reaction = Reactions[0];
			if (Math.Abs(scanFilter.GetMass(0) - reaction.PrecursorMass) > num / 2.0 || (scanFilter.GetEnergyValid(0) == EnergyType.Valid && reaction.CollisionEnergyValid && Math.Abs(scanFilter.GetEnergy(0) - reaction.CollisionEnergy) > 0.01))
			{
				return false;
			}
		}
		for (int i = 0; i < num2; i++)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			int num3 = scanFilter.IndexToMultipleActivationIndex(i);
			ActivationType activation = scanFilter.GetActivation(num3);
			EnergyType energyValid = scanFilter.GetEnergyValid(num3);
			bool flag4 = energyValid == EnergyType.Valid;
			double energy = scanFilter.GetEnergy(num3);
			for (int j = 0; j < _reactions; j++)
			{
				if (!(Math.Abs(scanFilter.GetMass(num3) - Reactions[j].PrecursorMass) <= num / 2.0))
				{
					continue;
				}
				flag = true;
				do
				{
					flag2 = false;
					flag3 = false;
					if (activation == ActivationType.Any)
					{
						flag2 = true;
						flag3 = true;
						break;
					}
					for (int k = j; k < _reactions; k++)
					{
						IReaction reaction2 = Reactions[k];
						if (k > j && !reaction2.MultipleActivation)
						{
							break;
						}
						if (activation == reaction2.ActivationType)
						{
							flag2 = true;
							if (flag4 && reaction2.CollisionEnergyValid && Math.Abs(energy - reaction2.CollisionEnergy) <= 0.01)
							{
								flag3 = true;
							}
							else if (energyValid == EnergyType.Any || !reaction2.CollisionEnergyValid)
							{
								flag3 = true;
							}
						}
						if (flag2 && flag3)
						{
							break;
						}
					}
				}
				while (flag2 && flag3 && ++num3 < scanFilter.MassCount && scanFilter.GetIsMultipleActivation(num3));
				if (flag2 && flag3)
				{
					break;
				}
			}
			if (!flag || !flag2 || !flag3)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Get the number of compensation voltage values.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	private int NumCompensationVoltageValues()
	{
		return _baseEvent.SourceFragmentationInfoCount;
	}

	/// <summary>
	/// Test if the precursor conditions match.
	/// </summary>
	/// <param name="scanFilter">
	///     The scan filter.
	/// </param>
	/// <param name="helper">The filter helper (optimized filter data)</param>
	/// <param name="precursorMass">
	///     The precursor mass.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool PrecursorConditionsMatch(IScanFilter scanFilter, ScanFilterHelper helper, int precursorMass)
	{
		double num = ((helper.IsAccurateResolution && (helper.IsDependent || _dependentOn)) ? 0.2 : helper.PrecursorTolerance);
		if (helper.IsSingleActivation && _singleReaction)
		{
			IReaction reaction = Reactions[precursorMass];
			if (Math.Abs(scanFilter.GetMass(precursorMass) - reaction.PrecursorMass) > num || (scanFilter.GetEnergyValid(precursorMass) == EnergyType.Valid && reaction.CollisionEnergyValid && Math.Abs(scanFilter.GetEnergy(precursorMass) - reaction.CollisionEnergy) > 0.01))
			{
				return false;
			}
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool result = true;
		int num2 = scanFilter.IndexToMultipleActivationIndex(precursorMass);
		for (int i = 0; i < _reactions; i++)
		{
			if (Math.Abs(scanFilter.GetMass(num2) - Reactions[num2].PrecursorMass) <= num)
			{
				flag = true;
				do
				{
					flag2 = false;
					flag3 = false;
					if (scanFilter.GetActivation(num2) == ActivationType.Any)
					{
						flag2 = true;
						flag3 = true;
						break;
					}
					for (int j = i; j < _reactions; j++)
					{
						IReaction reaction2 = Reactions[j];
						if (j > i && !reaction2.MultipleActivation)
						{
							break;
						}
						if (scanFilter.GetActivation(num2) == reaction2.ActivationType)
						{
							flag2 = true;
							EnergyType energyValid = scanFilter.GetEnergyValid(num2);
							if (energyValid == EnergyType.Valid && reaction2.CollisionEnergyValid && Math.Abs(scanFilter.GetEnergy(num2) - reaction2.CollisionEnergy) <= 0.01)
							{
								flag3 = true;
							}
							else if (energyValid == EnergyType.Any || !reaction2.CollisionEnergyValid)
							{
								flag3 = true;
							}
						}
						if (flag2 && flag3)
						{
							result = true;
							break;
						}
					}
					if (!flag2 || !flag3)
					{
						result = false;
						break;
					}
				}
				while (++num2 < scanFilter.MassCount && scanFilter.GetIsMultipleActivation(num2));
			}
			if (num2 >= scanFilter.MassCount)
			{
				break;
			}
		}
		if (!flag || !flag2 || !flag3)
		{
			result = false;
		}
		return result;
	}
}
