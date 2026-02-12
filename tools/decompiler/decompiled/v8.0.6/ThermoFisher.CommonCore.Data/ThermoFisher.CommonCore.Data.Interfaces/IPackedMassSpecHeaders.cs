namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Binary (byte array) data from the generic headers of a mass spectrometer.
/// </summary>
public interface IPackedMassSpecHeaders
{
	/// <summary>
	/// Packed trailer extra headers
	/// </summary>
	byte[] TrailerExtraHeader { get; set; }

	/// <summary>
	///  Packed status log headers
	/// </summary>
	byte[] StatusLogHeader { get; set; }

	/// <summary>
	///  Packed tune headers
	/// </summary>
	byte[] TuneHeader { get; set; }
}
