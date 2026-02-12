namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Specifies scan power in scans.
/// </summary>
public enum MSOrderType
{
	/// <summary>Constant Neutral Gain scan(ICIS Data Only)</summary>
	Ng = -3,
	/// <summary>Constant Neutral Loss scan(ICIS Data Only)</summary>
	Nl,
	/// <summary>Parent scan(ICIS Data Only)</summary>
	Par,
	/// <summary>Any scan power</summary>
	Any,
	/// <summary>basic MS</summary>
	Ms,
	/// <summary>MS^2 (MS/MS)</summary>
	Ms2,
	/// <summary>order MS^3</summary>
	Ms3,
	/// <summary>order MS^4</summary>
	Ms4,
	/// <summary>order MS^5</summary>
	Ms5,
	/// <summary>order MS^6</summary>
	Ms6,
	/// <summary>order MS^7</summary>
	Ms7,
	/// <summary>order MS^8</summary>
	Ms8,
	/// <summary>order MS^9</summary>
	Ms9,
	/// <summary>order MS^10</summary>
	Ms10
}
