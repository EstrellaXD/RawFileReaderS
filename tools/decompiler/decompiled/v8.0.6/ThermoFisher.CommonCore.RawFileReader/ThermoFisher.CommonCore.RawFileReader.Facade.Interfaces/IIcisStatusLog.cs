namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// Defines status from ICIS system instruments.
/// </summary>
internal interface IIcisStatusLog : IRawObjectBase
{
	/// <summary>
	/// Gets the status.
	/// </summary>
	/// <value>
	/// The status.
	/// </value>
	string Status { get; }
}
