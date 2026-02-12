using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The scan filter helper.
/// Analyses a filter to save time on filter to scan comparisons.
/// This is done by making a list of rules to apply.
/// Uses properties, which can be inlined for high performance.
/// Does not implement any interfaces, to avoid slowdown of virtual calls.
/// (properties may be use billions of times for filter against scan comparisons).
/// Used for high performance (parallel) chromatogram generation.
/// </summary>
public class ScanFilterHelper
{
	/// <summary>
	/// Determines if an event was tested, and the test results.
	/// </summary>
	internal enum EventTested : byte
	{
		/// <summary>
		/// This has never been tested
		/// </summary>
		EventNotTested,
		/// <summary>
		/// This event has been tested, but failed
		/// </summary>
		TestedAndFailed,
		/// <summary>
		/// Event tested, and passed the filter
		/// </summary>
		TestedAndPassed
	}

	/// <summary>
	/// The no meta filter.
	/// </summary>
	private const long NoMetaFilter = 0L;

	/// <summary>
	/// The instrument mass resolution (used in determining if mass ranges match up or not)
	/// </summary>
	private const double DefaultFilterMassResolution = 0.4;

	private const double DefaultPrecursorTolerance = 0.2;

	private EventTested[][] _resultsCache;

	/// <summary>
	/// Gets the filter.
	/// </summary>
	public IScanFilter Filter { get; private set; }

	/// <summary>
	/// Gets the rules, which have been found in the supplied filter.
	/// For example, a filter text of <c>"ms !d"</c> includes 2 rules
	/// 1: The MS order must be one.
	/// 2: The scan must not be dependent
	/// </summary>
	public FilterRule[] Rules { get; private set; }

	/// <summary>
	/// Gets the MS order.
	/// </summary>
	public MSOrderType MsOrder { get; }

	/// <summary>
	/// Gets a value indicating whether "multiplex" is in the "On" state
	/// </summary>
	public bool IsMultiplex { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the MS order is any value other than "MS".
	/// </summary>
	public bool IsSpecialOrder { get; private set; }

	/// <summary>
	/// Gets the mass resolution.
	/// </summary>
	public double MassResolution { get; }

	/// <summary>
	/// Gets the multi activation corrected mass count.
	/// </summary>
	public int MultiActivationCorrectedMassCount { get; }

	/// <summary>
	/// Gets a value indicating whether the filter has multiple reactions.
	/// </summary>
	public bool IsSingleActivation { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the filter's "dependent" flag is in the "on" tristate.
	/// </summary>
	public bool IsDependent { get; }

	/// <summary>
	/// Gets a value indicating whether the filter has any masses.
	/// </summary>
	public bool HasMasses { get; private set; }

	/// <summary>
	/// Gets a value indicating whether this object can keep
	/// track of "already tested" scan events.
	/// To enable it: Call InitializeAvailableEvents,
	/// with the result of "IRawDataPlus.ScanEvents"
	/// </summary>
	public bool KeepMatchedEventHistory { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the filter resolution was set to "Accurate precursor value".
	/// Else it will be "DefaultFilterMassResolution"
	/// </summary>
	public bool IsAccurateResolution { get; private set; }

	/// <summary>
	/// Gets the precursor tolerance (half of MassResolution)
	/// </summary>
	public double PrecursorTolerance { get; private set; }

	/// <summary>
	/// Sets the table of "known scan events"
	/// This can be used to optimize filtering
	/// </summary>
	/// <param name="allEvents">Planned scan events. This interface may be obtained from IRawDataPlus (ScanEvents property)</param>
	public void InitializeAvailableEvents(IScanEvents allEvents)
	{
		int segments = allEvents.Segments;
		EventTested[][] array = new EventTested[segments][];
		for (int i = 0; i < segments; i++)
		{
			int eventCount = allEvents.GetEventCount(i);
			array[i] = new EventTested[eventCount];
		}
		_resultsCache = array;
		KeepMatchedEventHistory = true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanFilterHelper" /> class.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <param name="accuratePrecursors">In this mode a tolerance matching for precursors is "accurate" (based on the precision value),
	/// except for data dependent scans.
	/// For all "non accurate" precursor selections, and for all data dependent scans a default (wide) tolerance is used.</param>
	/// <param name="filterMassPrecision">number of decimal places for masses in filter</param>
	public ScanFilterHelper(IScanFilter filter, bool accuratePrecursors, int filterMassPrecision)
	{
		TriState dependent = filter.Dependent;
		IsDependent = dependent == TriState.On;
		HasMasses = filter.MassCount != 0;
		IsMultiplex = filter.Multiplex == TriState.On;
		MsOrder = filter.MSOrder;
		IsSpecialOrder = MsOrder != MSOrderType.Ms;
		Filter = filter;
		List<FilterRule> list = new List<FilterRule>(10);
		if (filter.MetaFilters != 0L)
		{
			list.Add(FilterRule.MetaFilter);
		}
		if (dependent != TriState.Any)
		{
			list.Add(IsDependent ? FilterRule.DependentOn : FilterRule.DependentOff);
		}
		if (MsOrder != MSOrderType.Any)
		{
			list.Add(FilterRule.MsOrder);
		}
		if (filter.SupplementalActivation != TriState.Any)
		{
			list.Add(FilterRule.SupplementalActivation);
		}
		if (filter.MultiStateActivation != TriState.Any)
		{
			list.Add(FilterRule.MultiStateActivation);
		}
		if (filter.Wideband != TriState.Any)
		{
			list.Add(FilterRule.Wideband);
		}
		if (filter.Polarity != PolarityType.Any)
		{
			list.Add(FilterRule.Polarity);
		}
		if (filter.ScanData != ScanDataType.Any)
		{
			list.Add(FilterRule.ScanData);
		}
		if (filter.IonizationMode != IonizationModeType.Any)
		{
			list.Add(FilterRule.IonizationMode);
		}
		if (filter.Corona != TriState.Any)
		{
			list.Add(FilterRule.Corona);
		}
		if (filter.Lock != TriState.Any)
		{
			list.Add(FilterRule.Lock);
		}
		if (filter.FieldFreeRegion != FieldFreeRegionType.Any)
		{
			list.Add(FilterRule.FieldFreeRegion);
		}
		if (filter.Ultra != TriState.Any)
		{
			list.Add(FilterRule.Ultra);
		}
		if (filter.Enhanced != TriState.Any)
		{
			list.Add(FilterRule.Enhanced);
		}
		if (filter.ParamA != TriState.Any)
		{
			list.Add(FilterRule.ParamA);
		}
		if (filter.ParamB != TriState.Any)
		{
			list.Add(FilterRule.ParamB);
		}
		if (filter.ParamF != TriState.Any)
		{
			list.Add(FilterRule.ParamF);
		}
		if (filter.MultiNotch != TriState.Any)
		{
			list.Add(FilterRule.MultiNotch);
		}
		if (filter.ParamR != TriState.Any)
		{
			list.Add(FilterRule.ParamR);
		}
		if (filter.ParamV != TriState.Any)
		{
			list.Add(FilterRule.ParamV);
		}
		if (filter.MultiplePhotonDissociation != TriState.Any)
		{
			list.Add(FilterRule.MultiplePhotonDissociation);
		}
		if (filter.ElectronCaptureDissociation != TriState.Any)
		{
			list.Add(FilterRule.ElectronCaptureDissociation);
		}
		if (filter.PhotoIonization != TriState.Any)
		{
			list.Add(FilterRule.PhotoIonization);
		}
		if (filter.SourceFragmentation != TriState.Any)
		{
			list.Add(FilterRule.SourceFragmentation);
		}
		if (filter.SourceFragmentationType != SourceFragmentationValueType.Any)
		{
			list.Add(FilterRule.SourceFragmentationType);
		}
		if (filter.CompensationVoltage != TriState.Any)
		{
			list.Add(FilterRule.CompensationVoltage);
		}
		if (filter.CompensationVoltType != CompensationVoltageType.Any)
		{
			list.Add(FilterRule.CompensationVoltType);
		}
		if (filter.Detector != DetectorType.Any)
		{
			list.Add(FilterRule.Detector);
		}
		if (filter.MassAnalyzer != MassAnalyzerType.Any)
		{
			list.Add(FilterRule.MassAnalyzerType);
		}
		if (filter.SectorScan != SectorScanType.Any)
		{
			list.Add(FilterRule.SectorScan);
		}
		if (filter.TurboScan != TriState.Any)
		{
			list.Add(FilterRule.TurboScan);
		}
		if (filter.ScanMode != ScanModeType.Any)
		{
			list.Add(FilterRule.ScanMode);
		}
		if (filter.Multiplex != TriState.Any)
		{
			list.Add(FilterRule.Multiplex);
		}
		if (filter.ScanTypeIndex != -1)
		{
			list.Add(FilterRule.ScanTypeIndex);
		}
		if (filter.AccurateMass != FilterAccurateMass.Any)
		{
			list.Add(FilterRule.AccurateMass);
		}
		if (filter is IScanFilterPlus { AllLowerCaseFiltersApplied: var allLowerCaseFiltersApplied } scanFilterPlus)
		{
			if (allLowerCaseFiltersApplied != 0)
			{
				if ((allLowerCaseFiltersApplied & (LowerCaseFilterFlags.LowerE | LowerCaseFilterFlags.LowerG | LowerCaseFilterFlags.LowerH | LowerCaseFilterFlags.LowerI)) != 0)
				{
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerE) != TriState.Any)
					{
						list.Add(FilterRule.LowerE);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerG) != TriState.Any)
					{
						list.Add(FilterRule.LowerG);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerH) != TriState.Any)
					{
						list.Add(FilterRule.LowerH);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerI) != TriState.Any)
					{
						list.Add(FilterRule.LowerI);
					}
				}
				if ((allLowerCaseFiltersApplied & (LowerCaseFilterFlags.LowerJ | LowerCaseFilterFlags.LowerK | LowerCaseFilterFlags.LowerL | LowerCaseFilterFlags.LowerM)) != 0)
				{
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerJ) != TriState.Any)
					{
						list.Add(FilterRule.LowerJ);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerK) != TriState.Any)
					{
						list.Add(FilterRule.LowerK);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerL) != TriState.Any)
					{
						list.Add(FilterRule.LowerL);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerM) != TriState.Any)
					{
						list.Add(FilterRule.LowerM);
					}
				}
				if ((allLowerCaseFiltersApplied & (LowerCaseFilterFlags.LowerQ | LowerCaseFilterFlags.LowerN | LowerCaseFilterFlags.LowerO)) != 0)
				{
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerN) != TriState.Any)
					{
						list.Add(FilterRule.LowerN);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerO) != TriState.Any)
					{
						list.Add(FilterRule.LowerO);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerQ) != TriState.Any)
					{
						list.Add(FilterRule.LowerQ);
					}
				}
				if ((allLowerCaseFiltersApplied & (LowerCaseFilterFlags.LowerS | LowerCaseFilterFlags.LowerX | LowerCaseFilterFlags.LowerY)) != 0)
				{
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerS) != TriState.Any)
					{
						list.Add(FilterRule.LowerS);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerX) != TriState.Any)
					{
						list.Add(FilterRule.LowerX);
					}
					if (scanFilterPlus.GetLowerCaseFlag(LowerCaseFilterFlags.LowerY) != TriState.Any)
					{
						list.Add(FilterRule.LowerY);
					}
				}
			}
			UpperCaseFilterFlags allUpperCaseFilterApplied = scanFilterPlus.AllUpperCaseFilterApplied;
			if (allUpperCaseFilterApplied != 0)
			{
				if ((allUpperCaseFilterApplied & (UpperCaseFilterFlags.UpperA | UpperCaseFilterFlags.UpperB | UpperCaseFilterFlags.UpperF)) != 0)
				{
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperA) != TriState.Any)
					{
						list.Add(FilterRule.UpperA);
					}
					TriState upperCaseFlag = scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperB);
					if (upperCaseFlag != TriState.Any)
					{
						list.Add(FilterRule.UpperB);
					}
					scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperF);
					if (upperCaseFlag != TriState.Any)
					{
						list.Add(FilterRule.UpperF);
					}
				}
				if ((allUpperCaseFilterApplied & (UpperCaseFilterFlags.UpperG | UpperCaseFilterFlags.UpperH | UpperCaseFilterFlags.UpperI)) != 0)
				{
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperG) != TriState.Any)
					{
						list.Add(FilterRule.UpperG);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperH) != TriState.Any)
					{
						list.Add(FilterRule.UpperH);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperI) != TriState.Any)
					{
						list.Add(FilterRule.UpperI);
					}
				}
				if ((allUpperCaseFilterApplied & (UpperCaseFilterFlags.UpperJ | UpperCaseFilterFlags.UpperK | UpperCaseFilterFlags.UpperL)) != 0)
				{
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperJ) != TriState.Any)
					{
						list.Add(FilterRule.UpperJ);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperK) != TriState.Any)
					{
						list.Add(FilterRule.UpperK);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperL) != TriState.Any)
					{
						list.Add(FilterRule.UpperL);
					}
				}
				if ((allUpperCaseFilterApplied & (UpperCaseFilterFlags.UpperM | UpperCaseFilterFlags.UpperN | UpperCaseFilterFlags.UpperO)) != 0)
				{
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperM) != TriState.Any)
					{
						list.Add(FilterRule.UpperM);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperN) != TriState.Any)
					{
						list.Add(FilterRule.UpperN);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperO) != TriState.Any)
					{
						list.Add(FilterRule.UpperO);
					}
				}
				if ((allUpperCaseFilterApplied & (UpperCaseFilterFlags.UpperQ | UpperCaseFilterFlags.UpperR | UpperCaseFilterFlags.UpperS)) != 0)
				{
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperQ) != TriState.Any)
					{
						list.Add(FilterRule.UpperQ);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperR) != TriState.Any)
					{
						list.Add(FilterRule.UpperR);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperS) != TriState.Any)
					{
						list.Add(FilterRule.UpperS);
					}
				}
				if ((allUpperCaseFilterApplied & (UpperCaseFilterFlags.UpperT | UpperCaseFilterFlags.UpperU | UpperCaseFilterFlags.UpperV)) != 0)
				{
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperT) != TriState.Any)
					{
						list.Add(FilterRule.UpperT);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperU) != TriState.Any)
					{
						list.Add(FilterRule.UpperU);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperV) != TriState.Any)
					{
						list.Add(FilterRule.UpperV);
					}
				}
				if ((allUpperCaseFilterApplied & (UpperCaseFilterFlags.UpperW | UpperCaseFilterFlags.UpperX | UpperCaseFilterFlags.UpperY)) != 0)
				{
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperW) != TriState.Any)
					{
						list.Add(FilterRule.UpperW);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperX) != TriState.Any)
					{
						list.Add(FilterRule.UpperX);
					}
					if (scanFilterPlus.GetUpperCaseFlag(UpperCaseFilterFlags.UpperY) != TriState.Any)
					{
						list.Add(FilterRule.UpperY);
					}
				}
			}
		}
		Rules = list.ToArray();
		MultiActivationCorrectedMassCount = filter.NumMassesEx();
		int massCount = filter.MassCount;
		IsSingleActivation = MultiActivationCorrectedMassCount == massCount && massCount == 1;
		IsAccurateResolution = accuratePrecursors;
		if (accuratePrecursors)
		{
			filter.MassPrecision = filterMassPrecision;
			MassResolution = filter.FilterMassResolution();
			PrecursorTolerance = MassResolution / 2.0;
		}
		else
		{
			MassResolution = 0.4;
			PrecursorTolerance = 0.2;
		}
	}

	/// <summary>
	/// Record the match result for a scan type segment/event)
	/// </summary>
	/// <param name="result">
	/// Result for this segment and event number
	/// </param>
	/// <param name="segment">
	/// Segment number for this scan event
	/// </param>
	/// <param name="eventNumber">
	/// Event number for this scan event
	/// </param>
	internal void AddMatchResult(bool result, int segment, int eventNumber)
	{
		_resultsCache[segment][eventNumber] = ((!result) ? EventTested.TestedAndFailed : EventTested.TestedAndPassed);
	}

	/// <summary>
	/// Determine if this scan type has already been tested, and find the result
	/// </summary>
	/// <param name="segment">Segment number for this scan event</param>
	/// <param name="eventNumber">Event number for this scan event</param>
	/// <returns>state of the scan type test</returns>
	internal EventTested IsTestedAgainst(int segment, int eventNumber)
	{
		return _resultsCache[segment][eventNumber];
	}
}
