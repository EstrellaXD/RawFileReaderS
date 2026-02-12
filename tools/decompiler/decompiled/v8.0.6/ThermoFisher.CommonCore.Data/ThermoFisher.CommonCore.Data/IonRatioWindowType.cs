namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determines if ion ratio checks are based on an absolute or relative window, from the expected percentage.
/// </summary>
public enum IonRatioWindowType
{
	/// <summary>
	/// Window is an absolute % range. For example: 50% +/- absolute 10% gives 40-60%
	/// </summary>
	Absolute,
	/// <summary>
	/// Window is a relative range, for example: 50% +/ relative 10% gives 45%-55%.
	/// </summary>
	Relative
}
