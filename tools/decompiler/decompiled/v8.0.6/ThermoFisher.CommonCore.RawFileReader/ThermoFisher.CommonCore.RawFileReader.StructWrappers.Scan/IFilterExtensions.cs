using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

internal interface IFilterExtensions
{
	/// <summary>
	/// On off state for upper case flags
	/// </summary>
	UpperCaseFilterFlags UpperCaseFlags { get; set; }

	/// <summary>
	/// On off state for lower case flags
	/// </summary>
	LowerCaseFilterFlags LowerCaseFlags { get; set; }

	/// <summary>
	/// When constructed from a filter, features are "applied" when the code appears
	/// for example: there is Q or !Q in the filter
	/// The "on off" state would be on for "Q" and off for "!Q".
	/// The "on/off" state has no meaning for a filter when not applied.
	/// </summary>
	UpperCaseFilterFlags UpperCaseApplied { get; set; }

	/// <summary>
	/// When constructed from a filter, features are "applied" when the code appears
	/// for example: there is q or !q in the filter
	/// The "on off" state would be on for "q" and off for "!q".
	/// The "on/off" state has no meaning for a filter when not applied.
	/// </summary>
	LowerCaseFilterFlags LowerCaseApplied { get; set; }
}
