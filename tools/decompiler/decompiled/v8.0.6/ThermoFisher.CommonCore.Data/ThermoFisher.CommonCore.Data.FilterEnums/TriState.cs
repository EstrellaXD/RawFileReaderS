namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// The feature state.
/// By default: On.
/// This tri-state enum is designed for filtering 
/// </summary>
public enum TriState
{
	/// <summary>
	/// The on state.
	/// The feature is used
	/// </summary>
	On,
	/// <summary>
	/// The off state. The feature is not used.
	/// </summary>
	Off,
	/// <summary>
	/// The any state. When filtering, match any state of this feature.
	/// </summary>
	Any
}
