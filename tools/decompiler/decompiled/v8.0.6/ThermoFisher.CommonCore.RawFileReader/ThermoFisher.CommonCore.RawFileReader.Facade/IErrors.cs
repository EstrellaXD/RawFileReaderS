namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// The Errors interface.
/// </summary>
internal interface IErrors
{
	/// <summary>
	/// Gets the error message.
	/// </summary>
	string ErrorMessage { get; }

	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	bool HasError { get; }
}
