namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Data acquisition device
/// </summary>
public enum Device
{
	/// <summary>
	/// No instrument
	/// </summary>
	None = -1,
	/// <summary>
	/// Mass spectrometer
	/// </summary>
	MS,
	/// <summary>
	/// Data collected from an analog input connected to a mass spectrometer.
	/// </summary>
	MSAnalog,
	/// <summary>
	/// An A to D device
	/// </summary>
	Analog,
	/// <summary>
	/// UV/Vis detector
	/// </summary>
	UV,
	/// <summary>
	/// PDA (UV detector)
	/// </summary>
	Pda,
	/// <summary>
	/// Unknown detector type or instrument which collects only status
	/// </summary>
	Other
}
