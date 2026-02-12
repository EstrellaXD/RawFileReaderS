using System;
using System.ComponentModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Meta Filter codes (32 bit flags).
/// The lower 7 bits are for "activation type meta filters" (5 used, 2 reserved).
/// This is defined as flags, as multiple features may be combined.
/// </summary>
[Flags]
public enum MetaFilterType
{
	/// <summary>
	/// No meta filter.
	/// </summary>
	[Description("NoMetaFilter")]
	None = 0,
	/// <summary>
	/// HCD meta filter.
	/// </summary>
	[Description("hcd")]
	Hcd = 1,
	/// <summary>
	/// ETD meta filter.
	/// </summary>
	[Description("etd")]
	Etd = 2,
	/// <summary>
	/// CID meta filter.
	/// </summary>
	[Description("cid")]
	Cid = 4,
	/// <summary>
	/// UVPD meta filter.
	/// </summary>
	[Description("uvpd")]
	Uvpd = 8,
	/// <summary>
	/// EID meta filter.
	/// </summary>
	[Description("eid")]
	Eid = 0x10,
	/// <summary>
	/// Msn meta filter. (ms2 or higher)
	/// </summary>
	[Description("msn")]
	Msn = 0x80,
	/// <summary>
	/// These bits contain an ms order for an MSn filter request
	/// </summary>
	[Description("")]
	MSnCountMask = 0xF00,
	/// <summary>
	/// If set, MSn + order must be data dependent (such as MSn 3d, for order&gt;= 3 and d ).
	/// </summary>
	[Description("")]
	MsndMask = 0x1000
}
