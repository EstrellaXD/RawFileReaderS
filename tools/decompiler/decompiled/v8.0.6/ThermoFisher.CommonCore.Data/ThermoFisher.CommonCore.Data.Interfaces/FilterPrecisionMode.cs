using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Determines how precision is handled when comparing filters
/// </summary>
[Flags]
public enum FilterPrecisionMode
{
	/// <summary>
	/// Precision is based on rules, such:
	/// Instruments with a lower precursor isolation ability have a wider
	/// tolerance when matching precursors (Legacy Xcalibur mode)
	/// </summary>
	Auto = 0,
	/// <summary>
	/// Use value suggested by the instrument
	/// RunHeader.FilterMassPrecision
	/// </summary>
	Instrument = 1,
	/// <summary>
	/// A precision must be specified (decimal places)
	/// </summary>
	Specified = 2,
	/// <summary>
	/// Mask, to check if precision is any value other than auto
	/// </summary>
	SpecifiedPrecisionMask = 3,
	/// <summary>
	/// For data dependent scans, use a wider scan range match window
	/// </summary>
	ExtendedDataDependentMatch = 4,
	/// <summary>
	/// For a range of precursors, select a value near the center of the band.
	/// </summary>
	FindPrecursorMidPoint = 8,
	/// <summary>
	/// Mass limits at matched at 10% of the parent mass match per count for values (0-10).
	/// So: MatchExpansion 8 will have an 80% larger tolerance for the target mass window
	/// than the DD parent mass.
	/// Values 11-15: Extend the tolerance by 1,2,5,10,20 Amu respectively.
	/// </summary>
	ScanRangeMatchExpansionMask = 0xF0
}
