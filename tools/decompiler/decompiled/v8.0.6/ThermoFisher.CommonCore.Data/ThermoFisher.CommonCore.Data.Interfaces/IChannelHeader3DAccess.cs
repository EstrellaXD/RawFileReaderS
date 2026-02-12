namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Access to values in a 3d data channel header
/// </summary>
public interface IChannelHeader3DAccess : IChannelHeaderBase
{
	/// <summary>
	/// The unit of the scan axis. This is the device specific y-axis. It might be e.g. the wavelength axis in
	/// case of an UV detector.
	/// </summary>
	string ScanUnit { get; set; }

	/// <summary>
	/// The acquisition state of a signal or spectral field.
	///    Unknown,
	///    NotStarted,
	///    Acquiring,
	///    Finished,
	///    Error
	/// </summary>
	string State { get; set; }

	/// <summary>
	/// The kind of detector that is used to record the spectral field. It corresponds to the kind of measuring
	/// principle that is used by the detector.
	///     Unknown
	///     UV
	///     MS
	///     Amperometry
	/// </summary>
	string DetectorKind { get; set; }
}
