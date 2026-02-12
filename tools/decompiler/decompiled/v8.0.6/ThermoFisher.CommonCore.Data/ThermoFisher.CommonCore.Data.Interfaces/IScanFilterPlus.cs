namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Extended properties for scan filters
/// Adds a range of upper and lower case letter flags.
/// </summary>
public interface IScanFilterPlus : IScanFilter, IScanEventBase
{
	/// <summary>
	/// Gets the set of lower case filter flags
	/// </summary>
	LowerCaseFilterFlags LowerCaseFlags { get; }

	/// <summary>
	/// Gets the set of upper case filter flags
	/// </summary>
	UpperCaseFilterFlags UpperCaseFlags { get; }

	/// <summary>
	/// Gets the set of all "flag applied" flags (filter in use) for lower case flags.
	/// If a specific enum bit is set, then the flag in "AllLowerCaseFilterFlags" can be used for filtering.
	/// Otherwise: the filter flags are ignored.
	/// </summary>
	LowerCaseFilterFlags AllLowerCaseFiltersApplied { get; }

	/// <summary>
	/// Gets the set of all "flag applied" flags (filter in use) for upper case flags.
	/// If a specific enum bit is set, then the flag in "AllUpperCaseFilterFlags" can be used for filtering.
	/// Otherwise: the filter flags are ignored.
	/// </summary>
	UpperCaseFilterFlags AllUpperCaseFilterApplied { get; }
}
