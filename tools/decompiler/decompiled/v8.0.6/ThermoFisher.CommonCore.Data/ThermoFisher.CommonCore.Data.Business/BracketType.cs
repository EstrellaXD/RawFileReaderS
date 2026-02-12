namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Specifies a sequence bracket type.
/// This determines which groups of samples use the same calibration curve.
/// </summary>
public enum BracketType
{
	/// <summary>
	/// No bracket type specified.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Standards are Overlapped with adjacent brackets.
	/// </summary>
	Overlapped,
	/// <summary>
	/// There is no bracketing. All samples in a sequence are a single group with one calibration curve.
	/// </summary>
	None,
	/// <summary>
	/// Multiple groups which are not overlapped (do not share standards).
	/// </summary>
	NonOverlapped,
	/// <summary>
	/// Groups of samples are automatically determined, based on sample types of each row.
	/// </summary>
	Open
}
