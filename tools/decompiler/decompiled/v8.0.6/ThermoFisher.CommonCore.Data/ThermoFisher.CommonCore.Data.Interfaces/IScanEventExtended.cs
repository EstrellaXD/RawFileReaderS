using ThermoFisher.CommonCore.Data.FilterEnums;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Add new upper and lower character properties
/// </summary>
public interface IScanEventExtended
{
	/// <summary>
	/// gets the full set of lower case filter flags
	/// </summary>
	LowerCaseFilterFlags LowerCaseFlags { get; }

	/// <summary>
	/// gets the full set of upper case filter flags
	/// </summary>
	UpperCaseFilterFlags UpperCaseFlags { get; }

	/// <summary>
	/// Gets the state of a lower case flag
	/// </summary>
	/// <param name="flag">requested flag</param>
	/// <returns>state of flag</returns>
	TriState GetLowerCaseFlag(LowerCaseFilterFlags flag);

	/// <summary>
	/// Gets the state of an upper case flag
	/// </summary>
	/// <param name="flag">requested flag</param>
	/// <returns>state of flag</returns>
	TriState GetUpperCaseFlag(UpperCaseFilterFlags flag);
}
