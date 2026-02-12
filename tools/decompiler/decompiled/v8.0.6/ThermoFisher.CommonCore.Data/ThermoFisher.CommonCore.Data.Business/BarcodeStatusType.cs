namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Enumeration of possible bar code status values
/// </summary>
public enum BarcodeStatusType
{
	/// <summary>
	/// NotRead status.
	/// </summary>
	NotRead,
	/// <summary>
	/// Read status. 
	/// </summary>
	Read,
	/// <summary>
	/// Unreadable status. 
	/// </summary>
	Unreadable,
	/// <summary>
	/// Error status.
	/// </summary>
	Error,
	/// <summary>
	/// Wait status.
	/// </summary>
	Wait
}
