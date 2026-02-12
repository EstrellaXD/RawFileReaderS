namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
///     The virtual device types.
/// </summary>
internal enum VirtualDeviceTypes
{
	/// <summary>
	///     No device.
	/// </summary>
	NoDevice = -1,
	/// <summary>
	///     MS Device
	/// </summary>
	MsDevice,
	/// <summary>
	///     Basically same as UV_DEVICE but acquired through MS (not to be confused with UV)
	/// </summary>
	MsAnalogDevice,
	/// <summary>
	///     The Analog Device card in the PC
	/// </summary>
	AnalogDevice,
	/// <summary>
	///     PDA Device
	/// </summary>
	PdaDevice,
	/// <summary>
	///     UV Device
	/// </summary>
	UvDevice,
	/// <summary>
	///     Special device that has no scan data, but status log only (HN)
	/// </summary>
	StatusDevice
}
