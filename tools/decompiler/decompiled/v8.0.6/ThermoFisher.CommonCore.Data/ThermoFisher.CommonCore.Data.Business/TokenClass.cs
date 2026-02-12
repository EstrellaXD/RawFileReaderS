namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// category of tokens with a scan filter
/// </summary>
internal enum TokenClass
{
	/// <summary>
	/// A mass range token.
	/// </summary>
	RangeToken,
	/// <summary>
	/// A generic token (a string, such as "Full").
	/// </summary>
	Generic,
	/// <summary>
	/// A data format ("p" or "c").
	/// </summary>
	DataFormat,
	/// <summary>
	/// A polarity token ("+" or "-").
	/// </summary>
	Polarity,
	/// <summary>
	/// An MS order token.
	/// </summary>
	MsOrder,
	/// <summary>
	/// A parent mass.
	/// </summary>
	ParentMass,
	/// <summary>
	/// A data dependent token "d".
	/// </summary>
	DataDependent
}
